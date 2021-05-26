﻿using Dapper;
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

namespace TwSynchro.CostItemModule
{
    public class TaxRateSettingService
    {
        /// <summary>
        /// 增值税率同步
        /// </summary>
        /// <param name="_logger"></param>
        public  static void Synchro(ILogger<Worker> _logger)
        {
            _logger.LogInformation($"------同步增值税数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string timesTamp =   UtilsSynchroTimestamp.GetTimestamp("TaxRateSetting");

            StringBuilder Strsql = new($@"SELECT id AS TaxRateSettingID,comm_id AS CommID,cost_id AS CorpCostID,tax_rate AS TaxRate,
           tax_latefee_rate AS ContractPenaltyRate,begin_time AS StartDate,end_time AS EndDate,
           tax_latefee_rate AS IsContractPenalty,modify_user AS OperationUserCode,modify_date AS OperationDate,
           is_delete AS IsDelete FROM tb_base_charge_set_tax WHERE times_tamp>'{timesTamp}'");

            StringBuilder sql = new();

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            int PageIndex = 1;

            #region 读取当前最大时间戳

            sql.Clear();

            sql.Append("SELECT MAX(times_tamp) times_tamp  FROM tb_base_charge_set_tax");

            var times_tamp = ( mySqlConn.Query<string>(sql.ToString())).ToList();

            #endregion

            while (true)
            {
                var result =  mySqlConn.QueryPager<TaxRateSetting>(DBType.MySql, Strsql.ToString(), "sort", 10, PageIndex);

                _logger.LogInformation($"读取增值税数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

                sql.Clear();

                sql.AppendLine($@"select [TaxRateSettingID], [CommID], [CorpCostID], [TaxRate],
                                [ContractPenaltyRate], [StartDate], [EndDate], [AuditStatus], 
                                [IsContractPenalty], [OperationUserCode], [OperationDate], 
                                [IsDelete] from Tb_HSPR_TaxRateSetting WHERE 1=0");

                var reader =  sqlServerConn.ExecuteReader(sql.ToString());

                DataTable dt = new DataTable("Tb_HSPR_TaxRateSetting");

                dt.Load(reader);

                DataRow dr;

                sql.Clear();

                foreach (var item in result.Data)
                {
                    dr = dt.NewRow();

                    dr["TaxRateSettingID"] = item.TaxRateSettingID;//主键

                    dr["CommID"] = item.CommID;//项目ID

                    dr["CorpCostID"] = item.CorpCostID;//项目收费科目id

                    UtilsDataTable.DataRowIsNull(dr, "TaxRate", item.TaxRate);//收费率

                    UtilsDataTable.DataRowIsNull(dr, "ContractPenaltyRate", item.ContractPenaltyRate);//合同违约金税率

                    UtilsDataTable.DataRowIsNull(dr, "StartDate", item.StartDate);//开始时间

                    UtilsDataTable.DataRowIsNull(dr, "EndDate", item.EndDate);//结束时间

                    dr["AuditStatus"] = 1;//审核状态

                    UtilsDataTable.DataRowIsNull(dr, "IsContractPenalty", item.IsContractPenalty);//合同违约金税率

                    UtilsDataTable.DataRowIsNull(dr, "OperationUserCode", item.OperationUserCode);//操作人

                    UtilsDataTable.DataRowIsNull(dr, "OperationDate", item.OperationDate);//操作时间

                    UtilsDataTable.DataRowIsNull(dr, "IsDelete", item.IsDelete);//是否删除

                    dt.Rows.Add(dr);

                    sql.AppendLine($@"DELETE Tb_HSPR_TaxRateSetting WHERE TaxRateSettingID='{item.TaxRateSettingID}';");
                }
                _logger.LogInformation($"生成增值税数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                using var trans = sqlServerConn.OpenTransaction();


                try
                {
                    int rowsAffected = 0;

                    if (!string.IsNullOrEmpty(sql.ToString()))
                        rowsAffected =  sqlServerConn.Execute(sql.ToString(), transaction: trans);

                    _logger.LogInformation($"删除增值税数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                    stopwatch.Restart();

                     DbBatch.InsertSingleTable(sqlServerConn, dt, "Tb_HSPR_TaxRateSetting",  trans);

                    _logger.LogInformation($"插入增值税数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                    stopwatch.Restart();

                    trans.Commit();

                    _logger.LogInformation($"第{PageIndex}次提交增值税数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                    stopwatch.Restart();
                }
                catch (Exception e)
                {
                    trans.Rollback();

                    _logger.LogInformation($"第{PageIndex}次提交增值税发生错误；错误信息：{e.Message}");

                    return;
                }

                PageIndex++;//下一页

                if (!result.HasNext)
                    break;

            }

            //保存时间戳
            UtilsSynchroTimestamp.SetTimestamp("TaxRateSetting", times_tamp[0], 180);

            _logger.LogInformation($"------同步增值税数据结束------");
        }
    }
}
