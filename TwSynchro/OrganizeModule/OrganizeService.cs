﻿using Dapper;
using DapperFactory;
using DapperFactory.Enum;
using Entity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TwSynchro.OrganizeModule
{
    public class OrganizeService
    {
        public async static void Synchro(ILogger<Worker> _logger)
        {
         
            _logger.LogInformation($"------同步项目机构岗位数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            StringBuilder sql = new("SELECT * FROM rf_organize;");

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, "erp_base");

            _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            var result = await mySqlConn.QueryPagerAsync<List<Organize>>(DBType.MySql,sql.ToString(),"ID",10,1);

            var s = result.HasNext;

            _logger.LogInformation($"读取用户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, "PMS_Base");

            sql.Clear();

            sql.AppendLine("SELECT UserCode,UserName,LoginCode,PassWord,Sex,MobileTel,Email,IsFirstLogin,IsUse FROM Tb_Sys_User WHERE 1<>1;");

            var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

            DataTable dt = new DataTable("Tb_Sys_User");

            dt.Load(reader);

            DataRow dr;

            sql.Clear();

            foreach (var itemUser in result.Data)
            {
                dr = dt.NewRow();

                //dr["UserCode"] = itemUser.ID;
                //dr["UserName"] = itemUser.Name;
                //dr["LoginCode"] = itemUser.Account;
                //dr["PassWord"] = itemUser.Password;
                //dr["Sex"] = itemUser.Sex;
                //dr["MobileTel"] = itemUser.Mobile;
                //dr["Email"] = itemUser.Email;
                //dr["IsFirstLogin"] = 1;
                //dr["IsUse"] = 1;

                dt.Rows.Add(dr);

                //sql.AppendLine($@"DELETE Tb_Sys_User WHERE UserCode='{itemUser.ID.ToString()}';");
            }

            _logger.LogInformation($"生成用户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var trans = sqlServerConn.OpenTransaction();
            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                _logger.LogInformation($"删除用户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                stopwatch.Restart();

                DbBatch.InsertSingleTable(sqlServerConn, dt, "Tb_Sys_User", trans);

                stopwatch.Stop();

                _logger.LogInformation($"插入用户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");
                trans.Commit();
            }
            catch (Exception)
            {
                trans.Rollback();
                throw;
            }
            _logger.LogInformation($"------同步项目机构岗位结束------");
        }
    }
}
