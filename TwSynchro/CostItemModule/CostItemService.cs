using Dapper;
using DapperFactory;
using DapperFactory.Enum;
using Entity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
            await SynchroCorpCostStandard(_logger);

        }

        #region 同步公司科目(费项)
        /// <summary>
        /// 同步公司科目
        /// </summary>
        /// <param name="_logger"></param>
        public async static Task<ResultMessage> SynchroCorpCostItem(ILogger<Worker> _logger)
        {

            ResultMessage rm = new();

            rm.Result = true;

            _logger.LogInformation($"------同步公司科目数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            StringBuilder Strsql = new($@"SELECT id AS CorpCostID,parent_id AS Parent_Id,sort AS CostSNum,cost_name AS CostName,min_unit AS RoundingNum,
       is_use AS IsSealed,product_name AS BillType,product_code AS BillCode,is_delete AS IsDelete FROM tb_base_charge_cost");

            StringBuilder sql = new();

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql,  DBLibraryName.Erp_Base);

            _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            int PageIndex = 1;

            while (true)
            {
                var result = await mySqlConn.QueryPagerAsync<CorpCostItem>(DBType.MySql, Strsql.ToString(), "sort", 10, PageIndex);

                _logger.LogInformation($"读取公司科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

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

                foreach (var item in result.Data)
                {
                    dr = dt.NewRow();

                    dr["CorpCostID"] = item.CorpCostID;//主键
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

                    sql.AppendLine($@"DELETE Tb_HSPR_CorpCostItem WHERE CorpCostID='{item.CorpCostID}';");
                }
                _logger.LogInformation($"生成公司科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                using var trans = sqlServerConn.OpenTransaction();


                if (result.Data.Count() > 0)
                {
                    try
                    {
                        int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                        _logger.LogInformation($"删除公司科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                        stopwatch.Restart();

                        DbBatch.InsertSingleTable(sqlServerConn, dt, "Tb_HSPR_CorpCostItem", trans);

                        _logger.LogInformation($"插入公司科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                        stopwatch.Restart();

                        trans.Commit();

                        _logger.LogInformation($"第{PageIndex}次提交公司科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                        stopwatch.Restart();
                    }
                    catch (Exception e)
                    {
                        trans.Rollback();

                        rm.Result = false;

                        rm.Message = e.Message;

                        _logger.LogInformation($"第{PageIndex}次提交公司科目发生错误；错误信息：{e.Message}");

                        return rm;
                    }

                }

                PageIndex++;//下一页

                if (!result.HasNext)
                    break;

            }

            _logger.LogInformation($"------同步公司科目数据结束------");

            return rm;

        }
        #endregion

        #region 同步公司标准
        /// <summary>
        /// 同步公司标准
        /// </summary>
        /// <param name="_logger"></param>
        public async static Task<ResultMessage> SynchroCorpCostStandard(ILogger<Worker> _logger)
        {
            ResultMessage rm = new();

            rm.Result = true;

            _logger.LogInformation($"------同步公司标准数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            StringBuilder Strsql = new($@"SELECT id AS CorpStanID,cost_id AS CorpCostID, stan_code AS StanSign,stan_name AS StanName,stan_memo AS StanExplain,
       calc_type AS StanFormula,stan_price AS StanAmount,stop_use_date AS StanEndDate,is_condition_calc AS IsCondition,
       condition_content AS　ConditionField,latefee_rate AS DelinRates ,latefee_calc_date,is_delete AS IsDelete,
       calc_condition_type AS IsStanRange,min_unit AS AmountRounded,stan_ratio AS Modulus,allow_comm_modify AS IsCanUpdate FROM  tb_base_charge_stan");

            StringBuilder sql = new();

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            int PageIndex = 1;

            while (true)
            {
                var result = await mySqlConn.QueryPagerAsync<CorpCostStandard>(DBType.MySql, Strsql.ToString(), "sort", 10, PageIndex);

                _logger.LogInformation($"读取公司标准数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.Erp_Base);

                sql.Clear();

                sql.AppendLine($@"SELECT [CorpStanID], [CorpCostID], [StanSign], [StanName], [StanExplain], [StanFormula], [StanAmount], [StanStartDate], 
                            [StanEndDate], [IsCondition], [ConditionField], [DelinRates], [DelinDelay], [IsDelete], [IsStanRange], [ChargeCycle], 
                            [ManageFeesStyle], [ManageFeesAmount], [AmountRounded], [Modulus], [DelinType], [DelinDay], [IsLock], [IsCanUpdate], 
                            [EndRounded] FROM  Tb_HSPR_CorpCostStandard WHERE 1=0");

                var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

                DataTable dt = new DataTable("Tb_HSPR_CorpCostItem");

                dt.Load(reader);

                DataRow dr;

                sql.Clear();

                foreach (var item in result.Data)
                {
                    dr = dt.NewRow();

                    dr["CorpStanID"] = item.CorpStanID;//主键

                    dr["CorpCostID"] = item.CorpCostID;//公司科目ID

                    dr["StanSign"] = item.StanSign;//标准编号

                    dr["StanName"] = item.StanName;//标准名称

                    dr["StanExplain"] = item.StanExplain;//标准说明

                    Dictionary<string, object> dicStanFormula = JsonConvert.DeserializeObject<Dictionary<string, object>>(item.StanFormula);

                    dr["StanFormula"] = GetStanFormula(dicStanFormula["label"].ToString());//计算方式

                    if (!string.IsNullOrEmpty(item.StanAmount))
                        dr["StanAmount"] = item.StanAmount;//通用收费标准

                    //dr["StanStartDate"] = null;//启用日期

                    if (!string.IsNullOrEmpty(item.StanEndDate))
                        dr["StanEndDate"] = item.StanEndDate;//停用日期

                    if (!string.IsNullOrEmpty(item.IsCondition))
                        dr["IsCondition"] = item.IsCondition;//是否按条件计算

                    if (!string.IsNullOrEmpty(item.ConditionField))
                    {
                        List<Dictionary<string, object>> ConditionFieldList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(item.ConditionField);
                        //dr["ConditionField"] = item.ConditionField;//计算条件
                    }

                    if (!string.IsNullOrEmpty(item.DelinRates))
                        dr["DelinRates"] = item.DelinRates;//合同违约金比率

                    if (!string.IsNullOrEmpty(item.latefee_calc_date))
                    {
                        Dictionary<string, object> dicDelin = JsonConvert.DeserializeObject<Dictionary<string, object>>(item.latefee_calc_date);

                        string day = dicDelin["day"].ToString(), type = dicDelin["type"].ToString();

                        if (type == "1")
                        {
                            if (!string.IsNullOrEmpty(day))
                                dr["DelinDelay"] = day;//合同违约金延期  按天延迟

                            dr["DelinType"] = 0;//合同违约金延期    类型（天、月）
                        }
                        else if (type == "2")
                        {
                            string month = dicDelin["month"].ToString() ?? "0";

                            dr["DelinType"] = 1;//合同违约金延期    类型（天、月）

                            dr["DelinDay"] = int.Parse(month) * 100 + int.Parse(day);//合同违约金延期   (按月几号开始)  DelinDay = iDelinMonth * 100 + iDelinDay;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.IsDelete))
                        dr["IsDelete"] = item.IsDelete;//是否删除

                    if (!string.IsNullOrEmpty(item.IsStanRange))
                        dr["IsStanRange"] = item.IsStanRange;//按条件计算方式

                    dr["ChargeCycle"] = 0;//计费周期

                    dr["ManageFeesStyle"] = 0;//

                    //dr["ManageFeesAmount"] = null;//

                    if (!string.IsNullOrEmpty(item.AmountRounded))
                        dr["AmountRounded"] = item.AmountRounded;//数量取整方式

                    if (!string.IsNullOrEmpty(item.Modulus))
                        dr["Modulus"] = item.Modulus;//标准系数

                    //dr["IsLock"] = null;//

                    if (!string.IsNullOrEmpty(item.IsCanUpdate))
                        dr["IsCanUpdate"] = item.IsCanUpdate;//允许项目修改单价

                    //dr["EndRounded"] = null;//数量取整方式

                    dt.Rows.Add(dr);

                    sql.AppendLine($@"DELETE Tb_HSPR_CorpCostItem WHERE CorpCostID='{item.CorpCostID}';");
                }
                _logger.LogInformation($"生成公司标准数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                using var trans = sqlServerConn.OpenTransaction();
                try
                {
                    int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                    _logger.LogInformation($"删除公司标准数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                    stopwatch.Restart();

                    DbBatch.InsertSingleTable(sqlServerConn, dt, "Tb_HSPR_CorpCostStandard", trans);

                    _logger.LogInformation($"插入公司标准数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                    stopwatch.Restart();

                    trans.Commit();

                    _logger.LogInformation($"第{PageIndex}次提交公司标准数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                    stopwatch.Restart();
                }
                catch (Exception e)
                {
                    trans.Rollback();

                    rm.Result = false;

                    rm.Message = e.Message;

                    _logger.LogInformation($"第{PageIndex}次提交公司标准发生错误；错误信息：{e.Message}");

                    return rm;
                }

                PageIndex++;//下一页

                if (!result.HasNext)
                    break;

            }

            _logger.LogInformation($"------同步公司标准数据结束------");

            return rm;

        }
        #endregion


        public static string GetStanFormula(string str)
        {
            string StanFormula = "";

            switch (str)
            {
                case "按定额每月计费":
                    StanFormula = "1";
                    break;
                case "按计费面积*单价每月计费":
                    StanFormula = "2";
                    break;
                case "按套内面积*单价每月计费":
                    StanFormula = "5";
                    break;
                case "按花园面积*单价每月计费":
                    StanFormula = "9";
                    break;
                case "按地下面积*单价每月计费":
                    StanFormula = "14";
                    break;
                case "按定额每季计费":
                    StanFormula = "20";
                    break;
                case "按计费面积*单价每季计费":
                    StanFormula = "21";
                    break;
                case "按套内面积*单价每季计费":
                    StanFormula = "22";
                    break;
                case "按花园面积*单价每季计费":
                    StanFormula = "23";
                    break;
                case "按地下面积*单价每季计费":
                    StanFormula = "25";
                    break;
                case "按定额每年计费":
                    StanFormula = "30";
                    break;
                case "按用量*单价计费":
                    StanFormula = "4";
                    break;
                case "按天数*单价计费":
                    StanFormula = "7";
                    break;
                case "按数量*单价计费":
                    StanFormula = "6";
                    break;
                case "按计费面积*天数*单价计费":
                    StanFormula = "12";
                    break;
                case "按计费面积*数量*单价计费":
                    StanFormula = "11";
                    break;
                case "按花园面积*天数*单价计费":
                    StanFormula = "15";
                    break;
                case "按实际发生额计费":
                    StanFormula = "8";
                    break;
                default:
                    break;
            }

            return StanFormula;
        }
    }
}
