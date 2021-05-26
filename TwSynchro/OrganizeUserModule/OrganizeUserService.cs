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
using TwSynchro.Utils;
using Utils;

namespace TwSynchro.OrganizeUserModule
{
    public class OrganizeUserService
    {
        static readonly string TS_KEY = "Key_OrganizeUserService";

        public static async Task Synchro(ILogger<Worker> _logger, CancellationToken stoppingToken)
        {
            StringBuilder log = new("\r\n------同步人员绑定岗位数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var timestamp = UtilsSynchroTimestamp.GetTimestampAsync(TS_KEY);

            StringBuilder sql = new($@"SELECT Id,OrganizeId,UserId,time_stamp FROM rf_organizeuser 
                                       WHERE time_stamp > '{timestamp}'");

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            log.Append($"\r\n创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            var data = (await mySqlConn.QueryAsync<OrganizeUser>(sql.ToString())).ToList();

            if (data.Count == 0)
            {
                log.Append($"\r\n数据为空SQL语句:\r\n{sql}");

                _logger.LogInformation(log.ToString());

                return;
            }

            log.Append($"\r\n读取人员绑定岗位数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            sql.Clear();

            sql.AppendLine("SELECT UserRoleCode,UserCode,RoleCode FROM Tb_Sys_UserRole WHERE 1<>1;");

            var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

            DataTable dt = new DataTable("Tb_Sys_UserRole");

            dt.Load(reader);

            DataRow dr;

            sql.Clear();

            foreach (var itemMenu in data)
            {
                sql.AppendLine($@"DELETE Tb_Sys_UserRole WHERE UserRoleCode='{itemMenu.Id}';");
                if (itemMenu.Is_Delete == 1) continue;
                dr = dt.NewRow();
                dr["UserRoleCode"] = itemMenu.Id;
                dr["UserCode"] = itemMenu.UserId;
                dr["RoleCode"] = itemMenu.OrganizeId;
                dt.Rows.Add(dr);

            }

            log.Append($"\r\n生成人员绑定岗位数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var trans = sqlServerConn.OpenTransaction();
            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                log.Append($"\r\n删除人员绑定岗位数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                stopwatch.Restart();

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dt, "Tb_Sys_UserRole",stoppingToken, trans);

                stopwatch.Stop();

                log.Append($"\r\n插入人员绑定岗位数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                trans.Commit();

                await UtilsSynchroTimestamp.SetTimestampAsync(TS_KEY, data.Max(c => c.time_stamp));

            }
            catch (Exception ex)
            {
                trans.Rollback();

                log.Append($"\r\n插入人员绑定岗位数据失败:{ex.Message}{ex.StackTrace}");

            }

            log.Append($"\r\n------同步人员绑定岗位数据结束------");

            _logger.LogInformation(log.ToString());



        }
    }
}
