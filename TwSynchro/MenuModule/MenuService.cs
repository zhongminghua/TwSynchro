using Dapper;
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
using System.Threading;
using System.Threading.Tasks;
using TwSynchro.Utils;
using Utils;

namespace TwSynchro.MenuModule
{


    public class MenuService
    {
        static readonly string TS_KEY = "Key_MenuService";

        public static async Task Synchro(ILogger<Worker> _logger, CancellationToken stoppingToken)
        {
            StringBuilder log = new("\r\n------同步菜单数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var timestamp = await UtilsSynchroTimestamp.GetTimestampAsync(TS_KEY);

            StringBuilder sql = new($@"
                SELECT b.Address,a.Id,a.Ico,a.Sort,a.Title,a.ParentId,a.Is_Delete,a.time_stamp FROM rf_menu a
                    LEFT JOIN rf_applibrary b ON a.AppLibraryId = b.Id
                WHERE a.time_stamp > '{timestamp}';

                SELECT b.Address,a.Id,a.Ico,a.Sort,a.Title,a.ParentId,a.Is_Delete,b.time_stamp FROM rf_menu a
                    LEFT JOIN rf_applibrary b ON a.AppLibraryId = b.Id
                WHERE b.time_stamp > '{timestamp}';
            ");

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            log.Append($"\r\n创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            var readerMultiple = (await mySqlConn.QueryMultipleAsync(sql.ToString()));

            var data1 = (await readerMultiple.ReadAsync<Menu>()).ToList();

            var data2 = (await readerMultiple.ReadAsync<Menu>()).ToList();

            List<Menu> data = new();

            Menu modelMenuSearch;

            foreach (var item in data1)
            {
                modelMenuSearch = data2.Where(c => c.Id.ToString() == item.Id.ToString()).FirstOrDefault();

                if (modelMenuSearch is not null) { item.Address = modelMenuSearch.Address; }

                data.Add(item);
            }

            if (!data.Any())
            {
                log.Append($"\r\n数据为空SQL语句:\r\n{sql}");

                _logger.LogInformation(log.ToString());

                return;
            }

            log.Append($"\r\n读取菜单数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            sql.Clear();

            sql.AppendLine("SELECT PNodeCode,PNodeName,ParentId,URLPage,IsDelete,BackTitleImg,PNodeSort FROM Tb_Sys_PowerNode WITH(NOLOCK) WHERE 1<>1;");

            var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

            DataTable dt = new DataTable("Tb_Sys_PowerNode");

            dt.Load(reader);

            DataRow dr;

            sql.Clear();

            foreach (var itemMenu in data)
            {
                dr = dt.NewRow();

                dr["PNodeCode"] = itemMenu.Id;
                dr["PNodeName"] = itemMenu.Title;
                dr["ParentId"] = itemMenu.ParentId;
                dr["URLPage"] = itemMenu.Address;
                dr["IsDelete"] = itemMenu.Is_Delete;
                dr["BackTitleImg"] = itemMenu.Ico;
                dr["PNodeSort"] = itemMenu.Sort;

                dt.Rows.Add(dr);

                sql.AppendLine($@"DELETE Tb_Sys_PowerNode WHERE PNodeCode='{itemMenu.Id}';");
            }

            log.Append($"\r\n生成菜单数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var trans = sqlServerConn.OpenTransaction();

            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                log.Append($"\r\n删除菜单数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                stopwatch.Restart();

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dt, "Tb_Sys_PowerNode", stoppingToken, trans);

                stopwatch.Stop();

                log.Append($"\r\n插入菜单数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                trans.Commit();

                _ = UtilsSynchroTimestamp.SetTimestampAsync(TS_KEY, data.Max(c => c.time_stamp));
            }
            catch (Exception ex)
            {
                trans.Rollback();

                log.Append($"\r\n插入菜单数据失败:{ex.Message}{ex.StackTrace}");

            }

            log.Append($"\r\n------同步菜单数据结束------");

            _logger.LogInformation(log.ToString());

        }
    }
}
