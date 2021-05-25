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
    public class MenuService
    {
        public async static Task Synchro(ILogger<Worker> _logger, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"------同步菜单数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            StringBuilder sql = new($@"SELECT b.Address,a.Id,a.Title,a.ParentId FROM rf_menu a
                                           LEFT JOIN rf_applibrary b ON a.AppLibraryId = b.Id");

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            var result = (await mySqlConn.QueryAsync<Menu>(sql.ToString())).ToList();

            _logger.LogInformation($"读取菜单数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            sql.Clear();

            sql.AppendLine("SELECT PNodeCode,PNodeName,ParentId,URLPage FROM Tb_Sys_PowerNode WHERE 1<>1;");

            var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

            DataTable dt = new DataTable("Tb_Sys_PowerNode");

            dt.Load(reader);

            DataRow dr;

            sql.Clear();

            foreach (var itemMenu in result)
            {
                dr = dt.NewRow();

                dr["PNodeCode"] = itemMenu.Id;
                dr["PNodeName"] = itemMenu.Title;
                dr["ParentId"] = itemMenu.ParentId;
                dr["URLPage"] = itemMenu.Address;

                dt.Rows.Add(dr);

                sql.AppendLine($@"DELETE Tb_Sys_PowerNode WHERE UserCode='{itemMenu.Id}';");
            }

            _logger.LogInformation($"生成菜单数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var trans = sqlServerConn.OpenTransaction();
            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                _logger.LogInformation($"删除菜单数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                stopwatch.Restart();

                await DbBatch.InsertSingleTable(sqlServerConn, dt, "Tb_Sys_User", stoppingToken, trans);

                stopwatch.Stop();

                _logger.LogInformation($"插入菜单数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();

                _logger.LogInformation($"插入菜单数据失败:{ex.Message}{ex.StackTrace}");

            }
            _logger.LogInformation($"------同步菜单数据结束------");

        }
    }
}
