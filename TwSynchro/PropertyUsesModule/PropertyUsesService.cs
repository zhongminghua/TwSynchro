using Dapper;
using DapperFactory;
using DapperFactory.Enum;
using Entity;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwSynchro.Utils;
using Utils;

namespace TwSynchro.PropertyUsesModule
{
    public class PropertyUsesService
    {
        static readonly string TS_KEY = "Key_PropertyUsesService";

        public static async Task Synchro(ILogger<Worker> _logger, CancellationToken stoppingToken)
        {


            StringBuilder log = new("\r\n------同步房屋使用性质开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var timestamp = await UtilsSynchroTimestamp.GetTimestampAsync(TS_KEY);

            StringBuilder sql = new(
                $@" select d.* from  rf_dictionary d 
                            where d.ParentId in (
                                    select ID from rf_dictionary t
                                    where t.Title='使用性质')
                                  AND d.time_stamp > '{timestamp}'");

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            log.Append($"\r\n创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            var data = (await mySqlConn.QueryAsync<Dictionary>(sql.ToString())).ToList();

            if (!data.Any())
            {
                log.Append($"\r\n数据为空SQL语句:\r\n{sql}");

                _logger.LogInformation(log.ToString());

                return;
            }

            log.Append($"\r\n读取房屋使用性质数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            sql.Clear();

            sql.AppendLine("SELECT DictionaryCode,DictionaryName,DictionaryOrderId,DictionaryMemo,DictionarySign FROM Tb_Dictionary_PropertyUses WITH(NOLOCK) WHERE 1<>1;");

            var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

            DataTable dt = new DataTable("Tb_Dictionary_PropertyUses");

            dt.Load(reader);

            DataRow dr;

            sql.Clear();

            foreach (var itemDic in data)
            {
                dr = dt.NewRow();

                dr["DictionaryCode"] = itemDic.Id;
                dr["DictionaryName"] = itemDic.Title;
                dr["DictionaryOrderId"] = itemDic.Sort;
                dr["DictionaryMemo"] = itemDic.Note;
                dr["DictionarySign"] = itemDic.Title_en;

                dt.Rows.Add(dr);

                sql.AppendLine($@"DELETE Tb_Dictionary_PropertyUses WHERE DictionaryCode='{itemDic.Id}';");
            }

            log.Append($"\r\n生成房屋使用性质数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var trans = sqlServerConn.OpenTransaction();
            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                log.Append($"\r\n删除房屋使用性质数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                stopwatch.Restart();

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dt, "Tb_Dictionary_PropertyUses", stoppingToken, trans);

                stopwatch.Stop();

                log.Append($"\r\n插入房屋使用性质数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                trans.Commit();

                _ = UtilsSynchroTimestamp.SetTimestampAsync(TS_KEY, data.Max(c => c.time_stamp));
            }
            catch (Exception ex)
            {
                trans.Rollback();

                log.Append($"\r\n插入房屋使用性质数据失败:{ex.Message}{ex.StackTrace}");

            }

            log.Append($"\r\n------同步房屋使用性质结束------");

            _logger.LogInformation(log.ToString());
        }
    }
}
