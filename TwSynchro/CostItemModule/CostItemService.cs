using Dapper;
using DapperFactory;
using DapperFactory.Enum;
using Entity;
using Microsoft.Extensions.Logging;
using Swifter.Json;
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

namespace TwSynchro.CostItemModule
{
    public class CostItemService
    {
        public async static Task Synchro(ILogger<Worker> _logger, CancellationToken stoppingToken)
        {
            int pageSize = 10;
            await SynchroCorpCostItem(_logger, pageSize, stoppingToken);//公司科目
            //await SynchroCorpCostStandard(_logger, pageSize);//公司标准
            //await SynchroCostItem(_logger, pageSize);//项目科目
            //await SynchroCostStandard(_logger, pageSize);//项目标准
            //await SynchroCostStanSetting(_logger, pageSize);//客户标准绑定

        }

        #region 同步公司科目(费项)
        /// <summary>
        /// 同步公司科目
        /// </summary>
        /// <param name="_logger"></param>
        public async static Task<ResultMessage> SynchroCorpCostItem(ILogger<Worker> _logger, int pageSize, CancellationToken stoppingToken)
        {
            ResultMessage rm = new();

            rm.Result = true;

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            _logger.LogInformation($"------同步公司科目数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string timesTamp = await TimestampHelp.GetTimestampAsync("CorpCostItem");

            StringBuilder Strsql = new($@"SELECT id AS CorpCostID,parent_id AS Parent_Id,sort AS CostSNum,cost_name AS CostName,min_unit AS RoundingNum,
       is_use AS IsSealed,product_name AS BillType,product_code AS BillCode,is_delete AS IsDelete FROM tb_base_charge_cost WHERE times_tamp>'{timesTamp}'");

            StringBuilder sql = new();

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            int PageIndex = 1;

            #region 读取当前最大时间戳

            sql.Append("SELECT MAX(times_tamp) times_tamp  FROM tb_base_charge_cost");

            var times_tamp = (await mySqlConn.QueryAsync<string>(sql.ToString())).ToList();

            #endregion

            while (true)
            {
                var result = await mySqlConn.QueryPagerAsync<CorpCostItem>(DBType.MySql, Strsql.ToString(), "sort", pageSize, PageIndex);

                _logger.LogInformation($"读取公司科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

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
                    #region 读取行

                    dr = dt.NewRow();

                    dr["CorpCostID"] = item.CorpCostID;//主键

                    //dr["CostCode"] = "";//

                    dr["CostSNum"] = item.CostSNum;//序号

                    dr["CostName"] = item.CostName;//收费科目

                    dr["CostType"] = 0;//费用性质

                    dr["CostGeneType"] = 0;//是否允许输入费用

                    dr["CollUnitID"] = 0;

                    dr["DueDate"] = 1;

                    //dr["AccountsSign"] = null;

                    //dr["AccountsName"] = null;

                    dr["ChargeCycle"] = 0;//计费周期

                    dr["RoundingNum"] = item.RoundingNum;//计费取整位数：固定选项：元/角/分；必填

                    dr["IsBank"] = 0;//计费周期

                    dr["DelinDelay"] = 0;//合同违约金 按 天之后推迟

                    dr["DelinRates"] = 0;//合同违约金比率(天)

                    //dr["PreCostSign"] = null;//

                    //dr["Memo"] = null;//

                    dr["IsDelete"] = item.IsDelete;//是否删除

                    dr["IsTreeRoot"] = 0;//是否删除

                    //dr["SysCostSign"] = null;//业务类别

                    dr["DuePlotDate"] = 0;//

                    dr["CostBigType"] = 0;//是否包含费项

                    dr["DelinType"] = 0;//合同违约金

                    dr["DelinDay"] = 0;//

                    dr["IsSealed"] = item.IsSealed;//是否已封存（是否停用）

                    dr["BillType"] = item.BillType;//商品名称：必填(开票类别)

                    dr["BillCode"] = item.BillCode;//开票代码

                    dr["MaxDelinRate"] = 0;//合同违约金最大值

                    dr["Parent_Id"] = item.Parent_Id;//父级ID

                    dt.Rows.Add(dr);

                    #endregion

                    sql.AppendLine($@"DELETE Tb_HSPR_CorpCostItem WHERE CorpCostID='{item.CorpCostID}';");
                }
                _logger.LogInformation($"生成公司科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                using var trans = sqlServerConn.OpenTransaction();

                try
                {

                    int rowsAffected = 0;

                    if (result.Data.Count() > 0)
                        rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                    _logger.LogInformation($"删除公司科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                    stopwatch.Restart();

                    await DbBatch.InsertSingleTable(sqlServerConn, dt, "Tb_HSPR_CorpCostItem", stoppingToken,trans);

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

                PageIndex++;//下一页

                if (!result.HasNext)
                    break;

            }

            //保存时间戳
            TimestampHelp.SetTimestampAsync("CorpCostItem", times_tamp[0], 180);

            _logger.LogInformation($"------同步公司科目数据结束------");

            return rm;

        }
        #endregion

        #region 同步公司标准
        /// <summary>
        /// 同步公司标准
        /// </summary>
        /// <param name="_logger"></param>
        public async static Task<ResultMessage> SynchroCorpCostStandard(ILogger<Worker> _logger, int pageSize)
        {
            ResultMessage rm = new();

            rm.Result = true;

            _logger.LogInformation($"------同步公司标准数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string timesTamp = await TimestampHelp.GetTimestampAsync("CorpCostStandard");

            StringBuilder Strsql = new($@"SELECT id AS CorpStanID,cost_id AS CorpCostID, stan_code AS StanSign,stan_name AS StanName,stan_memo AS StanExplain,
       calc_type AS StanFormula,stan_price AS StanAmount,stop_use_date AS StanEndDate,is_condition_calc AS IsCondition,
       condition_content,calc_condition AS　ConditionField,latefee_rate AS DelinRates ,latefee_calc_date,is_delete AS IsDelete,
       calc_condition_type AS IsStanRange,min_unit AS AmountRounded,stan_ratio AS Modulus,allow_comm_modify AS IsCanUpdate FROM  tb_base_charge_stan 
            WHERE times_tamp>'{timesTamp}'");

            StringBuilder sql = new();
            StringBuilder sqltwo = new();

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            int PageIndex = 1;

            #region 读取当前最大时间戳

            sql.Clear();

            sql.Append("SELECT MAX(times_tamp) times_tamp  FROM tb_base_charge_stan");

            var times_tamp = (await mySqlConn.QueryAsync<string>(sql.ToString())).ToList();

            #endregion

            while (true)
            {
                var result = await mySqlConn.QueryPagerAsync<CorpCostStandard>(DBType.MySql, Strsql.ToString(), "sort", pageSize, PageIndex);

                _logger.LogInformation($"读取公司标准数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.Erp_Base);

                sql.Clear();
                sqltwo.Clear();

                sql.AppendLine($@"SELECT [CorpStanID], [CorpCostID], [StanSign], [StanName], [StanExplain], [StanFormula], [StanAmount], [StanStartDate], 
                            [StanEndDate], [IsCondition], [ConditionField], [DelinRates], [DelinDelay], [IsDelete], [IsStanRange], [ChargeCycle], 
                            [ManageFeesStyle], [ManageFeesAmount], [AmountRounded], [Modulus], [DelinType], [DelinDay], [IsLock], [IsCanUpdate], 
                            [EndRounded] FROM  Tb_HSPR_CorpCostStandard WHERE 1=0");

                var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

                DataTable dt = new DataTable("Tb_HSPR_CorpCostStandard");

                dt.Load(reader);

                DataRow dr;

                sqltwo.AppendLine($@"SELECT [CorpStanID], [StartCondition], [EndCondition], [CondStanAmount], [IsFix] 
                                    FROM  Tb_HSPR_CorpCostStanCondition WHERE 1=0");

                var readertwo = await sqlServerConn.ExecuteReaderAsync(sqltwo.ToString());

                DataTable dttwo = new DataTable("Tb_HSPR_CorpCostStanCondition");

                dttwo.Load(readertwo);

                DataRow drtwo;

                sql.Clear();
                sqltwo.Clear();

                foreach (var item in result.Data)
                {
                    #region 读取行数据

                    dr = dt.NewRow();

                    UtilsDataTable.DataRowIsNull(dr, "CorpStanID", item.CorpStanID);//主键

                    UtilsDataTable.DataRowIsNull(dr, "CorpCostID", item.CorpCostID);//公司科目ID

                    UtilsDataTable.DataRowIsNull(dr, "StanSign", item.StanSign);//标准编号

                    UtilsDataTable.DataRowIsNull(dr, "StanName", item.StanName);//标准名称

                    UtilsDataTable.DataRowIsNull(dr, "StanExplain", item.StanExplain);//标准说明

                    Dictionary<string, object> dicStanFormula = JsonFormatter.DeserializeObject<Dictionary<string, object>>(item.StanFormula);

                    UtilsDataTable.DataRowIsNull(dr, "StanFormula", GetStanFormula(dicStanFormula["label"].ToString()));//计算方式

                    UtilsDataTable.DataRowIsNull(dr, "StanAmount", item.StanAmount);//通用收费标准

                    //dr["StanStartDate"] = null;//启用日期

                    UtilsDataTable.DataRowIsNull(dr, "StanEndDate", item.StanEndDate);//停用日期

                    UtilsDataTable.DataRowIsNull(dr, "IsCondition", item.IsCondition);//是否按条件计算

                    if (!string.IsNullOrEmpty(item.ConditionField))
                    {
                        UtilsDataTable.DataRowIsNull(dr, "ConditionField", GetConditionField(item.ConditionField));//计算条件
                    }
                    //计算条件列表
                    if (!string.IsNullOrEmpty(item.condition_content))
                    {
                        List<Dictionary<string, object>> condition_content = JsonFormatter.DeserializeObject<List<Dictionary<string, object>>>(item.condition_content);

                        foreach (var temp in condition_content)
                        {
                            drtwo = dttwo.NewRow();

                            UtilsDataTable.DataRowIsNull(drtwo, "CorpStanID", item.CorpStanID);
                            UtilsDataTable.DataRowIsNull(drtwo, "StartCondition", temp["start"]);
                            UtilsDataTable.DataRowIsNull(drtwo, "EndCondition", temp["end"]);
                            UtilsDataTable.DataRowIsNull(drtwo, "CondStanAmount", temp["stanPrice"]);
                            UtilsDataTable.DataRowIsNull(drtwo, "IsFix", temp["stanType"]);
                            dttwo.Rows.Add(drtwo);
                        }
                        sqltwo.Append($"DELETE Tb_HSPR_CorpCostStanCondition WHERE CorpStanID='{item.CorpStanID}';");

                    }

                    UtilsDataTable.DataRowIsNull(dr, "DelinRates", item.DelinRates);//合同违约金比率

                    if (!string.IsNullOrEmpty(item.latefee_calc_date))
                    {
                        Dictionary<string, object> dicDelin = JsonFormatter.DeserializeObject<Dictionary<string, object>>(item.latefee_calc_date);

                        string day = dicDelin["day"].ToString(), type = dicDelin["type"].ToString();

                        if (type == "1")
                        {
                            UtilsDataTable.DataRowIsNull(dr, "DelinDelay", day);//合同违约金延期  按天延迟

                            dr["DelinType"] = 0;//合同违约金延期    类型（天、月）
                        }
                        else if (type == "2")
                        {
                            string month = dicDelin["month"].ToString() ?? "0";

                            dr["DelinType"] = 1;//合同违约金延期    类型（天、月）

                            dr["DelinDay"] = int.Parse(month) * 100 + int.Parse(day);//合同违约金延期   (按月几号开始)  DelinDay = iDelinMonth * 100 + iDelinDay;
                        }
                    }

                    UtilsDataTable.DataRowIsNull(dr, "IsDelete", item.IsDelete);//是否删除

                    UtilsDataTable.DataRowIsNull(dr, "IsStanRange", item.IsStanRange);//按条件计算方式

                    dr["ChargeCycle"] = 0;//计费周期

                    dr["ManageFeesStyle"] = 0;//

                    //dr["ManageFeesAmount"] = null;//

                    UtilsDataTable.DataRowIsNull(dr, "AmountRounded", item.AmountRounded);//数量取整方式

                    UtilsDataTable.DataRowIsNull(dr, "Modulus", item.Modulus);//标准系数

                    //dr["IsLock"] = null;//

                    UtilsDataTable.DataRowIsNull(dr, "IsCanUpdate", item.IsCanUpdate);//允许项目修改单价

                    //dr["EndRounded"] = null;//数量取整方式

                    dt.Rows.Add(dr);

                    #endregion

                    sql.AppendLine($@"DELETE Tb_HSPR_CorpCostStandard WHERE CorpStanID='{item.CorpStanID}';");
                }
                _logger.LogInformation($"生成公司标准数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                using var trans = sqlServerConn.OpenTransaction();
                try
                {
                    int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                    _logger.LogInformation($"删除公司标准数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                    stopwatch.Restart();

                    //await DbBatch.InsertSingleTable(sqlServerConn, dt, "Tb_HSPR_CorpCostStandard", trans);

                    _logger.LogInformation($"插入公司标准数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                    stopwatch.Restart();

                    int rowsAffectedtwo = await sqlServerConn.ExecuteAsync(sqltwo.ToString(), transaction: trans);

                    _logger.LogInformation($"删除公司标准计算条件列表数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffectedtwo}条");

                    stopwatch.Restart();

                    //await DbBatch.InsertSingleTable(sqlServerConn, dttwo, "Tb_HSPR_CorpCostStanCondition", trans);

                    _logger.LogInformation($"插入公司标准计算条件列表数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

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

            //保存时间戳
            TimestampHelp.SetTimestampAsync("CorpCostStandard", times_tamp[0], 180);

            _logger.LogInformation($"------同步公司标准数据结束------");

            return rm;

        }
        #endregion

        #region 同步项目科目(费项)（下发功能）
        /// <summary>
        /// 同步项目科目
        /// </summary>
        /// <param name="_logger"></param>
        public async static Task<ResultMessage> SynchroCostItem(ILogger<Worker> _logger, int pageSize)
        {

            ResultMessage rm = new();

            rm.Result = true;

            _logger.LogInformation($"------同步项目科目数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            StringBuilder sql = new();

            sql.AppendLine($@"SELECT CommID,IntId,CommName FROM Tb_HSPR_Community WHERE IsDelete=0 AND IntId IS NOT NULL");

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            var CommData = await sqlServerConn.QueryAsync<(string CommID, int IntID, string CommName)>(sql.ToString());

            _logger.LogInformation($"查询需要同步的项目 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            foreach (var Comm in CommData)
            {
                _logger.LogInformation($"({Comm.CommName})项目同步开始");

                string timesTamp = await TimestampHelp.GetTimestampAsync("CostItem-"+Comm.CommID);

                StringBuilder Strsql = new($@"select id AS CostID,comm_id AS CommID,parent_id AS Parent_Id,sort AS CostSNum,cost_name AS CostName,
                    min_unit AS RoundingNum,corp_cost_id AS CorpCostID,is_delete AS IsDelete from tb_charge_cost  
                        WHERE comm_id='{Comm.CommID}' AND times_tamp>'{timesTamp}'");//WHERE comm_id='{Comm.CommID}'

                //using var mySqlConn = DbService.GetDbConnection(DBType.MySql, "Erp_Develop");
                using var mySqlConn = DbService.GetSqlBurstDbConnection(DBType.MySql, DbBurstType.Charge, Comm.IntID);

                if (mySqlConn == null)
                {
                    _logger.LogInformation($"({Comm.CommName})项目在MySql未找到Charge库连接");
                    break;
                }

                _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                int PageIndex = 1;

                #region 读取当前最大时间戳

                sql.Clear();

                sql.Append("SELECT MAX(times_tamp) times_tamp  FROM tb_charge_cost WHERE comm_id='{Comm.CommID}'");

                var times_tamp = (await mySqlConn.QueryAsync<string>(sql.ToString())).ToList();

                #endregion

                while (true)
                {
                    var result = await mySqlConn.QueryPagerAsync<CostItem>(DBType.MySql, Strsql.ToString(), "sort", pageSize, PageIndex);

                    _logger.LogInformation($"读取项目科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                    stopwatch.Restart();

                    sql.Clear();

                    sql.AppendLine($@"select [CostID], [CommID], [CostSNum], [CostName], [CostType], [CostGeneType], [CollUnitID], [DueDate], 
                                [AccountsSign], [AccountsName], [ChargeCycle], [RoundingNum], [IsBank], [DelinDelay], [DelinRates], 
                                [PreCostSign], [Memo], [IsDelete], [CorpCostID], [CostCode], [SysCostSign], [DuePlotDate], 
                                [HighCorpCostID], [CostBigType], [DelinType], [DelinDay], [Parent_Id]  from Tb_HSPR_CostItem WHERE 1=0");

                    var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

                    DataTable dt = new DataTable("Tb_HSPR_CostItem");

                    dt.Load(reader);

                    DataRow dr;

                    sql.Clear();

                    foreach (var item in result.Data)
                    {
                        #region 读取行数据

                        dr = dt.NewRow();

                        UtilsDataTable.DataRowIsNull(dr, "CostID", item.CostID);//主键

                        UtilsDataTable.DataRowIsNull(dr, "CommID", item.CommID);//项目ID

                        UtilsDataTable.DataRowIsNull(dr, "CostSNum", item.CostSNum);//序号

                        UtilsDataTable.DataRowIsNull(dr, "CostName", item.CostName);//收费科目

                        dr["CostType"] = 0;//费用性质
                                           //
                        dr["CostGeneType"] = 0;//是否允许输入费用

                        dr["CollUnitID"] = 0;

                        dr["DueDate"] = 1;

                        //dr["AccountsSign"] = null;
                        //dr["AccountsName"] = null;

                        dr["ChargeCycle"] = 0;//计费周期

                        UtilsDataTable.DataRowIsNull(dr, "RoundingNum", item.RoundingNum);//计费取整位数：固定选项：元/角/分；必填

                        dr["IsBank"] = 0;//计费周期

                        dr["DelinDelay"] = 0;//合同违约金 按 天之后推迟

                        dr["DelinRates"] = 0;//合同违约金比率(天)

                        //dr["PreCostSign"] = null;//
                        //dr["Memo"] = null;//

                        UtilsDataTable.DataRowIsNull(dr, "IsDelete", item.IsDelete);//是否删除

                        UtilsDataTable.DataRowIsNull(dr, "CorpCostID", item.CorpCostID);//公司收费科目id

                        //dr["CostCode"] = "";//

                        //dr["SysCostSign"] = null;//业务类别

                        //dr["DuePlotDate"] = 0;//

                        //dr["HighCorpCostID"] = 0;//

                        dr["CostBigType"] = 0;//是否包含费项

                        dr["DelinType"] = 0;//合同违约金

                        dr["DelinDay"] = 0;//

                        UtilsDataTable.DataRowIsNull(dr, "Parent_Id", item.Parent_Id);//父级ID

                        dt.Rows.Add(dr);

                        #endregion

                        sql.AppendLine($@"DELETE Tb_HSPR_CostItem WHERE CostID='{item.CostID}';");
                    }
                    _logger.LogInformation($"生成项目科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                    stopwatch.Restart();

                    using var trans = sqlServerConn.OpenTransaction();

                    try
                    {
                        int rowsAffected = 0;

                        if (result.Data.Count() > 0)
                            rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                        _logger.LogInformation($"删除项目科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                        stopwatch.Restart();

                            //await DbBatch.InsertSingleTable(sqlServerConn, dt, "Tb_HSPR_CostItem", trans);

                        _logger.LogInformation($"插入项目科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                        stopwatch.Restart();

                        trans.Commit();

                        _logger.LogInformation($"第{PageIndex}次提交项目科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                        stopwatch.Restart();
                    }
                    catch (Exception e)
                    {
                        trans.Rollback();

                        rm.Result = false;

                        rm.Message = e.Message;

                        _logger.LogInformation($"第{PageIndex}次提交项目科目发生错误；错误信息：{e.Message}");

                        return rm;
                    }

                    PageIndex++;//下一页

                    if (!result.HasNext)
                        break;

                }

                //保存时间戳
                TimestampHelp.SetTimestampAsync("CostItem-" + Comm.CommID, times_tamp[0], 180);

                _logger.LogInformation($"({Comm.CommName})项目同步结束");

            }

            _logger.LogInformation($"------同步项目科目数据结束------");

            return rm;

        }
        #endregion

        #region 同步项目标准
        /// <summary>
        /// 同步项目标准
        /// </summary>
        /// <param name="_logger"></param>
        public async static Task<ResultMessage> SynchroCostStandard(ILogger<Worker> _logger, int pageSize)
        {
            ResultMessage rm = new();

            rm.Result = true;

            _logger.LogInformation($"------同步项目标准数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string timesTamp = await TimestampHelp.GetTimestampAsync("CostStandard");

            StringBuilder Strsql = new($@"SELECT id AS StanID,comm_id AS CommID,cost_id AS CostID,stan_code AS StanSign,stan_name AS StanName,
                       stan_memo AS StanExplain,calc_type AS StanFormula,stan_price AS StanAmount,is_condition_calc AS IsCondition,
                       condition_content,calc_condition AS ConditionField,latefee_rate AS DelinRates,latefee_calc_date ,is_delete AS IsDelete,
                       calc_condition_type AS IsStanRange,corp_stan_id AS CorpStanID,corp_cost_id AS CorpCostID,
                       min_unit AS AmountRounded,stan_ratio AS Modulus,allow_comm_modify AS IsCanUpdate FROM  tb_base_charge_comm_stan 
                        WHERE times_tamp>'{timesTamp}'");

            StringBuilder sql = new();

            StringBuilder sqltwo = new();

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            int PageIndex = 1;

            #region 读取当前最大时间戳

            sql.Clear();

            sql.Append("SELECT MAX(times_tamp) times_tamp  FROM tb_base_charge_comm_stan");

            var times_tamp = (await mySqlConn.QueryAsync<string>(sql.ToString())).ToList();

            #endregion

            while (true)
            {
                var result = await mySqlConn.QueryPagerAsync<CostStandard>(DBType.MySql, Strsql.ToString(), "sort", pageSize, PageIndex);

                _logger.LogInformation($"读取项目标准数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

                sql.Clear();
                sqltwo.Clear();

                sql.AppendLine($@"select [StanID], [CommID], [CostID], [StanSign], [StanName], [StanExplain], [StanFormula], 
                            [StanAmount], [StanStartDate], [StanEndDate], [IsCondition], [ConditionField], 
                            [DelinRates], [DelinDelay], [IsDelete], [IsStanRange], [ChargeCycle], [ManageFeesStyle],
                            [ManageFeesAmount], [CorpStanID], [CorpCostID], [AmountRounded], [Modulus], [DelinType], 
                            [DelinDay], [IsLock], [IsCanUpdate], [DelinRatesOne], [EndRounded]  from Tb_HSPR_CostStandard WHERE 1=0");

                var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

                DataTable dt = new DataTable("Tb_HSPR_CostStandard");

                dt.Load(reader);

                DataRow dr;

                sqltwo.AppendLine($@"SELECT [StanID], [StartCondition], [EndCondition], [CondStanAmount], [IsFix] 
                                    FROM  Tb_HSPR_CostStanCondition WHERE 1=0");

                var readertwo = await sqlServerConn.ExecuteReaderAsync(sqltwo.ToString());

                DataTable dttwo = new DataTable("Tb_HSPR_CostStanCondition");

                dttwo.Load(readertwo);

                DataRow drtwo;

                sql.Clear();
                sqltwo.Clear();

                sql.Clear();

                foreach (var item in result.Data)
                {
                    #region 读取行数据

                    dr = dt.NewRow();

                    UtilsDataTable.DataRowIsNull(dr, "StanID", item.StanID);//主键

                    UtilsDataTable.DataRowIsNull(dr, "CommID", item.CommID);//项目ID

                    UtilsDataTable.DataRowIsNull(dr, "CostID", item.CostID);//科目编码

                    UtilsDataTable.DataRowIsNull(dr, "StanSign", item.StanSign);//标准编号

                    UtilsDataTable.DataRowIsNull(dr, "StanName", item.StanName);//标准名称

                    UtilsDataTable.DataRowIsNull(dr, "StanExplain", item.StanExplain);//标准说明

                    Dictionary<string, object> dicStanFormula = JsonFormatter.DeserializeObject<Dictionary<string, object>>(item.StanFormula);
                    UtilsDataTable.DataRowIsNull(dr, "StanFormula", GetStanFormula(dicStanFormula["label"].ToString()));//计算方式

                    UtilsDataTable.DataRowIsNull(dr, "StanAmount", item.StanAmount);//通用收费标准

                    //dr["StanStartDate"] = null;//启用日期

                    //dr["StanEndDate"] = null;//停用日期
                    //UtilsDataTable.DataRowIsNull(dr, "StanEndDate", item.StanEndDate);//停用日期

                    UtilsDataTable.DataRowIsNull(dr, "IsCondition", item.IsCondition);//是否按条件计算

                    if (!string.IsNullOrEmpty(item.ConditionField))
                    {
                        UtilsDataTable.DataRowIsNull(dr, "ConditionField", GetConditionField(item.ConditionField));//计算条件
                    }
                    //计算条件列表
                    if (!string.IsNullOrEmpty(item.condition_content))
                    {
                        List<Dictionary<string, object>> condition_content = JsonFormatter.DeserializeObject<List<Dictionary<string, object>>>(item.condition_content);

                        foreach (var temp in condition_content)
                        {
                            drtwo = dttwo.NewRow();

                            UtilsDataTable.DataRowIsNull(drtwo, "StanID", item.StanID);
                            UtilsDataTable.DataRowIsNull(drtwo, "StartCondition", temp["start"]);
                            UtilsDataTable.DataRowIsNull(drtwo, "EndCondition", temp["end"]);
                            UtilsDataTable.DataRowIsNull(drtwo, "CondStanAmount", temp["stanPrice"]);
                            UtilsDataTable.DataRowIsNull(drtwo, "IsFix", temp["stanType"]);
                            dttwo.Rows.Add(drtwo);
                        }
                        sqltwo.Append($"DELETE Tb_HSPR_CostStanCondition WHERE StanID='{item.StanID}';");

                    }

                    UtilsDataTable.DataRowIsNull(dr, "DelinRates", item.DelinRates);//合同违约金比率

                    if (!string.IsNullOrEmpty(item.latefee_calc_date))
                    {
                        Dictionary<string, object> dicDelin = JsonFormatter.DeserializeObject<Dictionary<string, object>>(item.latefee_calc_date);

                        string day = dicDelin["day"].ToString(), type = dicDelin["type"].ToString();

                        if (type == "1")
                        {
                            UtilsDataTable.DataRowIsNull(dr, "DelinDelay", day);//合同违约金延期  按天延迟

                            dr["DelinType"] = 0;//合同违约金延期    类型（天、月）
                        }
                        else if (type == "2")
                        {
                            string month = dicDelin["month"].ToString() ?? "0";

                            dr["DelinType"] = 1;//合同违约金延期    类型（天、月）

                            dr["DelinDay"] = int.Parse(month) * 100 + int.Parse(day);//合同违约金延期   (按月几号开始)  DelinDay = iDelinMonth * 100 + iDelinDay;
                        }
                    }

                    UtilsDataTable.DataRowIsNull(dr, "IsDelete", item.IsDelete);//是否删除

                    UtilsDataTable.DataRowIsNull(dr, "IsStanRange", item.IsStanRange);//按条件计算方式 

                    dr["ChargeCycle"] = 0;//计费周期

                    dr["ManageFeesStyle"] = 0;//

                    //dr["ManageFeesAmount"] = null;//

                    UtilsDataTable.DataRowIsNull(dr, "CorpStanID", item.CorpStanID);//公司标准ID

                    UtilsDataTable.DataRowIsNull(dr, "CorpCostID", item.CorpCostID);//公司科目ID

                    UtilsDataTable.DataRowIsNull(dr, "AmountRounded", item.AmountRounded);//数量取整方式

                    UtilsDataTable.DataRowIsNull(dr, "Modulus", item.Modulus);//标准系数

                    UtilsDataTable.DataRowIsNull(dr, "IsCanUpdate", item.IsCanUpdate);//允许项目修改单价 

                    //dr["IsLock"] = null;//

                    //dr["DelinRatesOne"] = null;//

                    //dr["EndRounded"] = null;//数量取整方式

                    dt.Rows.Add(dr);

                    #endregion

                    sql.AppendLine($@"DELETE Tb_HSPR_CostStandard WHERE StanID='{item.StanID}';");
                }
                _logger.LogInformation($"生成项目标准数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                using var trans = sqlServerConn.OpenTransaction();
                try
                {
                    int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                    _logger.LogInformation($"删除项目标准数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                    stopwatch.Restart();

                    //await DbBatch.InsertSingleTable(sqlServerConn, dt, "Tb_HSPR_CostStandard", trans);

                    _logger.LogInformation($"插入项目标准数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                    stopwatch.Restart();

                    int rowsAffectedtwo = await sqlServerConn.ExecuteAsync(sqltwo.ToString(), transaction: trans);

                    _logger.LogInformation($"删除项目标准计算条件列表数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffectedtwo}条");

                    stopwatch.Restart();

                    //await DbBatch.InsertSingleTable(sqlServerConn, dttwo, "Tb_HSPR_CostStanCondition", trans);

                    _logger.LogInformation($"插入项目标准计算条件列表数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                    stopwatch.Restart();

                    trans.Commit();

                    _logger.LogInformation($"第{PageIndex}次提交项目标准数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                    stopwatch.Restart();
                }
                catch (Exception e)
                {
                    trans.Rollback();

                    rm.Result = false;

                    rm.Message = e.Message;

                    _logger.LogInformation($"第{PageIndex}次提交项目标准发生错误；错误信息：{e.Message}");

                    return rm;
                }

                PageIndex++;//下一页

                if (!result.HasNext)
                    break;

            }

            //保存时间戳
            TimestampHelp.SetTimestampAsync("CostStandard", times_tamp[0], 180);

            _logger.LogInformation($"------同步项目标准数据结束------");

            return rm;

        }
        #endregion

        #region 客户标准绑定
        /// <summary>
        /// 客户标准绑定
        /// </summary>
        /// <param name="_logger"></param>
        public async static Task<ResultMessage> SynchroCostStanSetting(ILogger<Worker> _logger, int pageSize)
        {
            ResultMessage rm = new();

            rm.Result = true;

            _logger.LogInformation($"------同步客户标准绑定数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            StringBuilder sql = new();

            sql.AppendLine($@"SELECT CommID,IntId,CommName FROM Tb_HSPR_Community WHERE IsDelete=0 AND IntId IS NOT NULL");

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            var CommData = await sqlServerConn.QueryAsync<(string CommID, int IntID, string CommName)>(sql.ToString());

            _logger.LogInformation($"查询需要同步的项目 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            foreach (var Comm in CommData)
            {
                _logger.LogInformation($"({Comm.CommName})项目同步开始");

                //using var mySqlConn = DbService.GetDbConnection(DBType.MySql, "Erp_Develop");
                using var mySqlConn = DbService.GetSqlBurstDbConnection(DBType.MySql, DbBurstType.Charge, Comm.IntID);

                if (mySqlConn == null)
                {
                    _logger.LogInformation($"({Comm.CommName})项目在MySql未找到Charge库连接");
                    break;
                }

                string timesTamp = await TimestampHelp.GetTimestampAsync("CostStanSetting-"+Comm.CommID);

                StringBuilder Strsql = new($@"select id AS IID,comm_id AS CommID,customer_id AS CustID,resource_id AS RoomID,cost_id AS CostID,
       meter_name AS MeterSign,calc_area AS CalcArea,calc_cycle AS ChargeCycle,is_delete AS IsDelete,
       calc_method AS PayType,calc_amount AS StanSingleAmount,delete_user AS DelUserName,calc_number AS RoomBuildArea from tb_charge_fee_stan_setting
                WHERE comm_id='{Comm.CommID}' AND times_tamp>'{timesTamp}'");

                _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                int PageIndex = 1;

                #region 读取当前最大时间戳

                sql.Clear();

                sql.Append("SELECT MAX(times_tamp) times_tamp  FROM tb_charge_fee_stan_setting");

                var times_tamp = (await mySqlConn.QueryAsync<string>(sql.ToString())).ToList();

                #endregion

                while (true)
                {
                    var result = await mySqlConn.QueryPagerAsync<CostStanSetting>(DBType.MySql, Strsql.ToString(), "sort", pageSize, PageIndex);

                    _logger.LogInformation($"读取客户标准绑定数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                    stopwatch.Restart();

                    sql.Clear();

                    sql.AppendLine($@"select [IID], [CommID], [CustID], [RoomID], [StanID], [IsInherit], [CostID], [HandID], [MeterSign], 
                            [FeesEndDate], [SetBeginDate], [SetEndDate], [CalcArea], [ChargeCycle], [CalcBeginDate], [IsDelete], 
                            [ChangeDate], [PayType], [AssumeCustID], [OldRoomID], [StanSingleAmount], [RoomBuildArea], [AuditState], 
                            [DelUserName] from Tb_HSPR_CostStanSetting WHERE 1=0");

                    var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

                    DataTable dt = new DataTable("Tb_HSPR_CostStanSetting");

                    dt.Load(reader);

                    DataRow dr;

                    sql.Clear();

                    foreach (var item in result.Data)
                    {
                        #region 读取行数据

                        dr = dt.NewRow();

                        UtilsDataTable.DataRowIsNull(dr, "IID", item.IID);//主键

                        UtilsDataTable.DataRowIsNull(dr, "CommID", item.CommID);//项目ID

                        UtilsDataTable.DataRowIsNull(dr, "CustID", item.CustID);//客户ID

                        UtilsDataTable.DataRowIsNull(dr, "RoomID", item.RoomID);//RoomID

                        UtilsDataTable.DataRowIsNull(dr, "StanID", item.StanID);//标准编码

                        dr["IsInherit"] = 0; //出租时自动转给租户

                        UtilsDataTable.DataRowIsNull(dr, "CostID", item.CostID);//项目科目编码

                        dr["HandID"] = 0; //

                        UtilsDataTable.DataRowIsNull(dr, "MeterSign", item.MeterSign);//表记

                        UtilsDataTable.DataRowIsNull(dr, "FeesEndDate", item.FeesEndDate);//计费截止时间

                        //dr["SetBeginDate"] = 0; //
                        //dr["SetEndDate"] = 0; //

                        UtilsDataTable.DataRowIsNull(dr, "CalcArea", item.CalcArea);//计算面积

                        UtilsDataTable.DataRowIsNull(dr, "ChargeCycle", item.ChargeCycle);//计费周期

                        //dr["CalcBeginDate"] = 0; //计费开始日期

                        UtilsDataTable.DataRowIsNull(dr, "IsDelete", item.IsDelete);//是否删除

                        UtilsDataTable.DataRowIsNull(dr, "ChangeDate", DateTime.Now);//修改时间

                        UtilsDataTable.DataRowIsNull(dr, "PayType", item.PayType);//计费方式

                        //dr["AssumeCustID"] = null; //
                        //dr["OldRoomID"] = null; //

                        UtilsDataTable.DataRowIsNull(dr, "StanSingleAmount", item.StanSingleAmount);//计费单价

                        UtilsDataTable.DataRowIsNull(dr, "RoomBuildArea", item.RoomBuildArea);//计费数量

                        //dr["AuditState"] = null; //

                        UtilsDataTable.DataRowIsNull(dr, "DelUserName", item.DelUserName);//删除人名称

                        dt.Rows.Add(dr);

                        #endregion

                        sql.AppendLine($@"DELETE Tb_HSPR_CostStanSetting WHERE IID='{item.IID}';");
                    }
                    _logger.LogInformation($"生成客户标准绑定数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                    stopwatch.Restart();

                    using var trans = sqlServerConn.OpenTransaction();
                    try
                    {
                        if (!string.IsNullOrEmpty(sql.ToString()))
                        {
                            int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                            _logger.LogInformation($"删除客户标准绑定数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                            stopwatch.Restart();

                            //await DbBatch.InsertSingleTable(sqlServerConn, dt, "Tb_HSPR_CostStanSetting", trans);

                            _logger.LogInformation($"插入客户标准绑定数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                            stopwatch.Restart();

                            trans.Commit();

                            _logger.LogInformation($"第{PageIndex}次提交客户标准绑定数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                            stopwatch.Restart();
                        }
                    }
                    catch (Exception e)
                    {
                        trans.Rollback();

                        rm.Result = false;

                        rm.Message = e.Message;

                        _logger.LogInformation($"第{PageIndex}次提交客户标准绑定发生错误；错误信息：{e.Message}");

                        return rm;
                    }

                    PageIndex++;//下一页

                    if (!result.HasNext)
                        break;

                }

                //保存时间戳
                TimestampHelp.SetTimestampAsync("CostStanSetting-" + Comm.CommID, times_tamp[0], 180);

                _logger.LogInformation($"({Comm.CommName})项目同步结束");
            }

            _logger.LogInformation($"------同步客户标准绑定数据结束------");

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

        public static string GetConditionField(string str)
        {
            string ConditionField = "";

            switch (str)
            {
                case "1":
                    ConditionField = "BuildArea";
                    break;
                case "2":
                    ConditionField = "Floor";
                    break;
                case "3":
                    ConditionField = "MeterDate";
                    break;
                case "4":
                    ConditionField = "Dosage";
                    break;
                case "5":
                    ConditionField = "YearTotalDosage";
                    break;
                default:
                    break;
            }

            return ConditionField;

        }
    }
}
