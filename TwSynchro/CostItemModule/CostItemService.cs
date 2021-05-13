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
using System.Threading.Tasks;

namespace TwSynchro.CostItemModule
{
    public class CostItemService
    {
        public async static void Synchro(ILogger<Worker> _logger)
        {
            await SynchroCorpCostItem(_logger);

        }

        /// <summary>
        /// 同步公司科目
        /// </summary>
        /// <param name="_logger"></param>
        public async static Task<bool> SynchroCorpCostItem(ILogger<Worker> _logger)
        {
            _logger.LogInformation($"------同步公司科目数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            StringBuilder sql = new($@"SELECT id AS CorpCostID,parent_id AS Parent_Id,sort AS CostSNum,cost_name AS CostName,cost_type AS CostType,min_unit AS RoundingNum,
       business_type AS SysCostSign,is_use AS IsSealed,product_name AS BillType,product_code AS BillCode,is_delete AS IsDelete FROM tb_base_charge_cost");

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, "Erp_Base");

            _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            var result = (await mySqlConn.QueryAsync<CorpCostItem>(sql.ToString())).ToList();

            _logger.LogInformation($"读取公司科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, "PMS_Base");

            sql.Clear();

            sql.AppendLine($@"SELECT [CorpCostID], [CostCode], [CostSNum], [CostName], [CostType], [CostGeneType], [CollUnitID], 
                    [DueDate], [AccountsSign], [AccountsName], [ChargeCycle], [RoundingNum], [IsBank], [DelinDelay], [DelinRates], 
                    [PreCostSign], [Memo], [IsDelete], [IsTreeRoot], [SysCostSign], [DuePlotDate], [CostBigType], [DelinType], [DelinDay], 
                    [IsSealed], [BillType], [BillCode], [MaxDelinRate], [Parent_Id] FROM Tb_HSPR_CorpCostItem WHERE 1=0");

            var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

            DataTable dt = new DataTable("Tb_HSPR_CorpCostItem");

            dt.Load(reader);

            DataRow dr;

            sql.Clear();

            foreach (var item in result)
            {
                dr = dt.NewRow();

                dr["CorpCostID"] = item.ID;//主键
                dr["CostCode"] = "";//
                dr["CostSNum"] = item.CostSNum;//序号
                dr["CostName"] = item.CostName;//收费科目
                dr["CostType"] = 0;//费用性质                    
                dr["CostGeneType"] = 0;//是否允许输入费用
                dr["CollUnitID"] = 0;
                dr["DueDate"] = 1;
                dr["AccountsSign"] = null;
                dr["AccountsName"] = null;
                dr["ChargeCycle"] = 0;//计费周期
                dr["RoundingNum"] = item.RoundingNum;//计费周期
                dr["IsBank"] = 0;//计费周期
                dr["DelinDelay"] = 0;//合同违约金 按 天之后推迟
                dr["DelinRates"] = 0;//合同违约金比率(天)
                dr["PreCostSign"] = null;//
                dr["Memo"] = null;//
                dr["IsDelete"] = item.IsDelete;//是否删除
                dr["IsTreeRoot"] = 0;//是否删除
                dr["SysCostSign"] = null;//业务类别             
                dr["DuePlotDate"] = 0;//
                dr["CostBigType"] = 0;//是否包含费项
                dr["DelinType"] = 0;//合同违约金
                dr["DelinDay"] = 0;//
                dr["IsSealed"] = item.IsSealed;//是否已封存（是否停用）
                dr["BillType"] = item.BillType;//商品名称：必填(开票类别)
                dr["BillCode"] = item.BillCode;//开票代码
                dr["MaxDelinRate"] = 0;//合同违约金最大值
                dr["Parent_Id"] = item.Parent_Id;//合同违约金最大值

                dt.Rows.Add(dr);

                sql.AppendLine($@"DELETE Tb_HSPR_CorpCostItem WHERE CorpCostID='{item.ID}';");
            }
            _logger.LogInformation($"生成公司科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var trans = sqlServerConn.OpenTransaction();
            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                _logger.LogInformation($"删除公司科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                stopwatch.Restart();

                DbBatch.InsertSingleTable(sqlServerConn, dt, "Tb_Sys_User", trans);

                stopwatch.Stop();

                _logger.LogInformation($"插入公司科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");
                trans.Commit();
            }
            catch (Exception)
            {
                trans.Rollback();
                return false;
            }
            _logger.LogInformation($"------同步用户数据结束------");

            return true;
        }

    }
}
