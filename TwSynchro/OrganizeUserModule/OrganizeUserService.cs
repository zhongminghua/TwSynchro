using Dapper;
using DapperFactory;
using DapperFactory.Enum;
using Entity;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TwSynchro.Utils;
using Utils;

namespace TwSynchro.OrganizeUserModule
{
    public class OrganizeUserService
    {
        const string TS_KEY = "Key_OrganizeUserService";

        public static void Synchro(ILogger<Worker> _logger)
        {
            _logger.LogInformation($"------同步人员绑定岗位数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();



            var timestamp = UtilsSynchroTimestamp.GetTimestamp(TS_KEY);


            StringBuilder sql = new($@"SELECT Id,OrganizeId,UserId,time_stamp FROM rf_organizeuser 
                                       WHERE b.time_stamp > '{timestamp}'");

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            var data = (mySqlConn.Query<OrganizeUser>(sql.ToString())).ToList();

            _logger.LogInformation($"读取人员绑定岗位数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            sql.Clear();

            sql.AppendLine("SELECT UserRoleCode,UserCode,RoleCode FROM Tb_Sys_UserRole WHERE 1<>1;");

            var reader = sqlServerConn.ExecuteReader(sql.ToString());

            DataTable dt = new DataTable("Tb_Sys_UserRole");

            dt.Load(reader);

            DataRow dr;

            sql.Clear();

            foreach (var itemMenu in data)
            {
                sql.AppendLine($@"DELETE Tb_Sys_UserRole WHERE PNodeCode='{itemMenu.Id}';");
                if (itemMenu.Is_Delete == 1) continue;
                dr = dt.NewRow();
                dr["UserRoleCode"] = itemMenu.Id;
                dr["UserCode"] = itemMenu.UserId;
                dr["RoleCode"] = itemMenu.OrganizeId;
                dt.Rows.Add(dr);

            }
            _logger.LogInformation($"生成人员绑定岗位数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var trans = sqlServerConn.OpenTransaction();
            try
            {
                int rowsAffected = sqlServerConn.Execute(sql.ToString(), transaction: trans);

                _logger.LogInformation($"删除人员绑定岗位数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                stopwatch.Restart();

                DbBatch.InsertSingleTable(sqlServerConn, dt, "Tb_Sys_UserRole", trans);

                stopwatch.Stop();

                _logger.LogInformation($"插入人员绑定岗位数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                trans.Commit();

                UtilsSynchroTimestamp.SetTimestamp(TS_KEY, data.Max(c => c.time_stamp));

            }
            catch (Exception ex)
            {
                trans.Rollback();

                _logger.LogInformation($"插入人员绑定岗位数据失败:{ex.Message}{ex.StackTrace}");

            }
            _logger.LogInformation($"------同步人员绑定岗位数据结束------");

        }
    }
}
