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
        public  static void Synchro(ILogger<Worker> _logger)
        {
            _logger.LogInformation($"------同步人员绑定岗位数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string timestamp =  UtilsSynchroTimestamp.GetTimestamp("Key_MenuService");


            StringBuilder sql = new($@"SELECT b.Address,a.Id,a.Title,a.ParentId,a.Is_Delete FROM rf_menu a
                                           LEFT JOIN rf_applibrary b ON a.AppLibraryId = b.Id
                                       WHERE b.time_stamp > '{timestamp}'");

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            var result = ( mySqlConn.Query<Menu>(sql.ToString())).ToList();

            _logger.LogInformation($"读取人员绑定岗位数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            sql.Clear();

            sql.AppendLine("SELECT PNodeCode,PNodeName,ParentId,URLPage,IsDelete FROM Tb_Sys_PowerNode WHERE 1<>1;");

            var reader =  sqlServerConn.ExecuteReader(sql.ToString());

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
                dr["IsDelete"] = itemMenu.Is_Delete;

                dt.Rows.Add(dr);

                sql.AppendLine($@"DELETE Tb_Sys_PowerNode WHERE PNodeCode='{itemMenu.Id}';");
            }

            _logger.LogInformation($"生成人员绑定岗位数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var trans = sqlServerConn.OpenTransaction();
            try
            {
                int rowsAffected =  sqlServerConn.Execute(sql.ToString(), transaction: trans);

                _logger.LogInformation($"删除人员绑定岗位数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                stopwatch.Restart();

                 DbBatch.InsertSingleTable(sqlServerConn, dt, "Tb_Sys_PowerNode",  trans);

                stopwatch.Stop();

                _logger.LogInformation($"插入人员绑定岗位数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                trans.Commit();
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
