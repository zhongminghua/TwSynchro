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
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace TwSynchro.UserModule
{
    public class UserService
    {
        public async static Task Synchro(ILogger<Worker> _logger, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"------同步用户数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            StringBuilder sql = new("SELECT ID,Name,Account,Password,(CASE Sex WHEN 0 THEN '女' ELSE '男' END) as Sex,Email,Mobile FROM rf_user;");

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            var result = (await mySqlConn.QueryAsync<User>(sql.ToString())).ToList();

            _logger.LogInformation($"读取用户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            sql.Clear();

            sql.AppendLine("SELECT UserCode,UserName,LoginCode,PassWord,Sex,MobileTel,Email,IsFirstLogin,IsUse FROM Tb_Sys_User WITH(NOLOCK) WHERE 1<>1;");

            var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

            DataTable dt = new DataTable("Tb_Sys_User");

            dt.Load(reader);

            DataRow dr;

            sql.Clear();

            foreach (var itemUser in result)
            {
                dr = dt.NewRow();

                dr["UserCode"] = itemUser.ID;
                dr["UserName"] = itemUser.Name;
                dr["LoginCode"] = itemUser.Account;
                dr["PassWord"] = itemUser.Password;
                dr["Sex"] = itemUser.Sex;
                dr["MobileTel"] = itemUser.Mobile;
                dr["Email"] = itemUser.Email;
                dr["IsFirstLogin"] = 1;
                dr["IsUse"] = 1;

                dt.Rows.Add(dr);

                sql.AppendLine($@"DELETE Tb_Sys_User WHERE UserCode='{itemUser.ID}';");
            }

            _logger.LogInformation($"生成用户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var trans = sqlServerConn.OpenTransaction();
            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                _logger.LogInformation($"删除用户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                stopwatch.Restart();

                await DbBatch.InsertSingleTable(sqlServerConn, dt, "Tb_Sys_User", stoppingToken,trans);

                stopwatch.Stop();

                _logger.LogInformation($"插入用户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();

                _logger.LogInformation($"插入用户数据失败:{ex.Message}{ex.StackTrace}");

            }
            _logger.LogInformation($"------同步用户数据结束------");

        }
    }
}
