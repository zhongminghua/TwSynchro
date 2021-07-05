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
using Utils;

namespace TwSynchro.OrganizeModule
{
    public class OrganizeService
    {

        public static async Task Synchro(ILogger<Worker> _logger, CancellationToken stoppingToken)
        {


            StringBuilder log = new("\r\n------同步项目机构岗位数据开始------");

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            //int pageIndex = 1;

            //bool isHasNext = true;

            //while (isHasNext)
            //{
            //log.Append($"\r\n第 {pageIndex} 页 数据");

            StringBuilder sql = new(@"
                                    SELECT * FROM rf_organize;
                                    SELECT Id,Title FROM rf_dictionary
                                    WHERE ParentId IN (
                                        SELECT id
                                        FROM rf_dictionary
                                        WHERE Title = '通用岗位'
                                    );
                                    ");

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            log.Append($"\r\n创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();


            //var result =  mySqlConn.QueryPagerAsync<Organize>(DBType.MySql, sql.ToString(), "ID", 10, pageIndex);

            var readerMultiple = await mySqlConn.QueryMultipleAsync(sql.ToString());

            sql.Clear();

            var organizeData = (await readerMultiple.ReadAsync<Organize>()).ToList();

            //通用岗位
            var dictionaryData = await readerMultiple.ReadAsync<Dictionary>();

            //isHasNext = result.HasNext;

            //pageIndex++;

            log.Append($"\r\n读取数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            log.Append($"\r\n创建SqlServer连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            sql.Clear();

            sql.AppendLine("SELECT OrganCode,OrganName,ParentId,IsComp,Sort,OrganType,SortCode,SortParentCode FROM Tb_Sys_Organ WITH(NOLOCK) WHERE 1<>1;");

            sql.AppendLine("SELECT OrganCode,IsDaQu,IsOrganComp,IsArea FROM Tb_Sys_OrganPartial WITH(NOLOCK) WHERE 1<>1;");

            sql.AppendLine(@"SELECT CommID,OrganCode,CommName,CommKind,ManageTime,ManageKind,CommAddress,Province,City,Borough,Street,CommunityName,
                                    GateSign,Num,Sort,IntId,CommSource,SortCode,SortParentCode FROM Tb_HSPR_Community WITH(NOLOCK) WHERE 1<>1;");

            sql.AppendLine("SELECT IID,CommID,ChargesMode FROM Tb_HSPR_CommunityChargesMode WITH(NOLOCK) WHERE 1<>1;");

            sql.AppendLine("SELECT DepCode,SortDepCode,DepName,ParentId,Sort,SortCode,SortParentCode FROM Tb_Sys_Department WITH(NOLOCK) WHERE 1<>1;");

            sql.AppendLine("SELECT RoleCode,RoleName,ParentId,UpLevelName,SysRoleCode,IsSysRole,Sort,DepCode,SortCode,SortParentCode FROM Tb_Sys_Role WITH(NOLOCK) WHERE 1<>1;");

            StringBuilder sqlOrgan = new(), sqlCommunity = new(), sqlDepartment = new(), sqlRole = new();

            var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

            DataTable dtTb_Sys_Organ = new DataTable("Tb_Sys_Organ");

            dtTb_Sys_Organ.Load(reader);

            DataTable dtTb_Sys_OrganPartial = new DataTable("Tb_Sys_OrganPartial");

            dtTb_Sys_OrganPartial.Load(reader);

            DataTable dtTb_HSPR_Community = new DataTable("Tb_HSPR_Community");

            dtTb_HSPR_Community.Load(reader);

            DataTable dtTb_HSPR_CommunityChargesMode = new DataTable("Tb_HSPR_CommunityChargesMode");

            dtTb_HSPR_CommunityChargesMode.Load(reader);

            DataTable dtTb_Sys_Department = new DataTable("Tb_Sys_Department");

            dtTb_Sys_Department.Load(reader);

            DataTable dtTb_Sys_Role = new DataTable("Tb_Sys_Role");

            dtTb_Sys_Role.Load(reader);

            DataRow dr;

            sql.Clear();

            List<Organize> listCleanData = new();

            void CleanData(string id)
            {

                id = id.ToUpper();

                var listChildNode = organizeData.Where(c => c.ParentId.ToString().ToUpper().Equals(id)).ToList();

                //清洗数据
                foreach (var modelOrganize in listChildNode)
                {
                    if (listCleanData.Where(c => c.Id.ToString().ToUpper() == modelOrganize.Id.ToString().ToUpper()).FirstOrDefault() is null)
                    {

                        var modelParentChildNode = listCleanData.Where(c => c.Id.ToString().ToUpper().Equals(modelOrganize.ParentId.ToString().ToUpper())).FirstOrDefault();

                        modelOrganize.LevelName = $"{modelParentChildNode.LevelName}_{modelOrganize.Name}";

                        int index = listChildNode.FindIndex(item => item.Id.ToString().ToUpper().Equals(modelOrganize.Id.ToString().ToUpper()));

                        modelOrganize.SortCodeNum = index + 1;

                        modelOrganize.SortCode = modelParentChildNode.SortCode + modelOrganize.SortCodeNum.ToString().PadLeft(4, '0');

                        modelOrganize.SortParentCode = modelParentChildNode.SortCode;

                        listCleanData.Add(modelOrganize with { });

                        CleanData(modelOrganize.Id.ToString());

                    }

                }
            }


            var firstModel = organizeData.Where(c => c.ParentId.ToString() == "00000000-0000-0000-0000-000000000000").FirstOrDefault();

            firstModel.LevelName = firstModel.Name;

            firstModel.SortCode = "0001";

            firstModel.SortCodeNum = 1;

            listCleanData.Add(firstModel with { });

            //清洗数据
            CleanData(firstModel.Id.ToString());


            sqlOrgan.AppendLine($"TRUNCATE TABLE Tb_Sys_Organ;");
            sqlOrgan.AppendLine($"TRUNCATE TABLE Tb_Sys_OrganPartial ;");
            sqlCommunity.AppendLine($"TRUNCATE TABLE Tb_HSPR_Community ;");
            sqlCommunity.AppendLine($"TRUNCATE TABLE Tb_HSPR_CommunityChargesMode ;");
            sqlDepartment.AppendLine($"TRUNCATE TABLE Tb_Sys_Department;");
            sqlRole.AppendLine($"TRUNCATE TABLE Tb_Sys_Role;");
            foreach (var itemOrganize in listCleanData)
            {

                if (itemOrganize.Type == 1)
                {

                    //sqlOrgan.AppendLine($"DELETE Tb_Sys_Organ WHERE OrganCode='{itemOrganize.Id}';");

                    if (itemOrganize.OrganType < 6)
                    {
                        dr = dtTb_Sys_Organ.NewRow();

                        dr["OrganCode"] = itemOrganize.Id;
                        dr["OrganName"] = itemOrganize.Name;
                        dr["ParentId"] = itemOrganize.ParentId;
                        dr["Sort"] = itemOrganize.Sort;
                        dr["OrganType"] = itemOrganize.OrganType;
                        dr["SortCode"] = itemOrganize.SortCode;
                        dr["SortParentCode"] = itemOrganize.SortParentCode;
                        


                        if (itemOrganize.OrganType == 3) dr["IsComp"] = 1;

                        dtTb_Sys_Organ.Rows.Add(dr);

                        if (itemOrganize.OrganType == 3 || itemOrganize.OrganType == 4 || itemOrganize.OrganType == 5)
                        {
                            dr = dtTb_Sys_OrganPartial.NewRow();

                            dr["OrganCode"] = itemOrganize.Id;

                            switch (itemOrganize.OrganType)
                            {
                                case 3:
                                    dr["IsDaQu"] = 1;
                                    break;
                                case 4:
                                    dr["IsOrganComp"] = 1;
                                    break;
                                case 5:
                                    dr["IsArea"] = 1;
                                    break;
                            };

                            dtTb_Sys_OrganPartial.Rows.Add(dr);

                            //sqlOrgan.AppendLine($"DELETE Tb_Sys_OrganPartial WHERE OrganCode='{itemOrganize.Id}';");
                        }
                    }
                    if (itemOrganize.OrganType == 6)
                    {
                        dr = dtTb_HSPR_Community.NewRow();

                        dr["CommID"] = itemOrganize.Id;
                        dr["OrganCode"] = itemOrganize.ParentId;
                        dr["CommName"] = itemOrganize.Name;
                        dr["CommKind"] = itemOrganize.CommKind; //项目业态
                        UtilsDataTable.DataRowIsNull(dr, "ManageTime", itemOrganize.TakeoverDate);
                        dr["ManageKind"] = itemOrganize.TakeoverKind;
                        dr["CommAddress"] = itemOrganize.Address;
                        dr["Province"] = itemOrganize.Province;
                        dr["City"] = itemOrganize.City;
                        dr["Borough"] = itemOrganize.Area;
                        dr["Street"] = itemOrganize.Street;
                        dr["CommunityName"] = itemOrganize.Community;
                        dr["GateSign"] = itemOrganize.GateSign;
                        dr["Num"] = itemOrganize.SortNum;
                        dr["Sort"] = itemOrganize.Sort;
                        dr["IntId"] = itemOrganize.IntId;
                        dr["CommSource"] = itemOrganize.CommSource;
                        dr["SortCode"] = itemOrganize.SortCode;
                        dr["SortParentCode"] = itemOrganize.SortParentCode;

                        dtTb_HSPR_Community.Rows.Add(dr);

                        //sqlCommunity.AppendLine($"DELETE Tb_HSPR_Community WHERE CommID='{itemOrganize.Id}';");

                        if (itemOrganize.ChargingModel is not null)
                        {

                            dr = dtTb_HSPR_CommunityChargesMode.NewRow();

                            dr["IID"] = Guid.NewGuid().ToString();
                            dr["CommID"] = itemOrganize.Id;
                            dr["ChargesMode"] = itemOrganize.ChargingModel;

                            dtTb_HSPR_CommunityChargesMode.Rows.Add(dr);

                            //sqlCommunity.AppendLine($"DELETE Tb_HSPR_CommunityChargesMode WHERE CommID='{itemOrganize.Id}';");
                        }
                    }
                }

                if (itemOrganize.Type == 2)
                {

                    dr = dtTb_Sys_Department.NewRow();

                    dr["DepCode"] = itemOrganize.Id;
                    //dr["SortDepCode"] = itemOrganize.Id;
                    dr["DepName"] = itemOrganize.Name;
                    dr["ParentId"] = itemOrganize.ParentId;
                    dr["Sort"] = itemOrganize.Sort;
                    dr["SortCode"] = itemOrganize.SortCode;
                    dr["SortParentCode"] = itemOrganize.SortParentCode;

                    dtTb_Sys_Department.Rows.Add(dr);

                    //sqlDepartment.AppendLine($"DELETE Tb_Sys_Department WHERE DepCode='{itemOrganize.Id}';");

                    AddDepartmentOrgan(itemOrganize.ParentId);

                }

                if (itemOrganize.Type == 3)
                {

                    dr = dtTb_Sys_Role.NewRow();

                    dr["RoleCode"] = itemOrganize.Id;
                    dr["RoleName"] = itemOrganize.Name;
                    dr["ParentId"] = itemOrganize.ParentId;
                    dr["DepCode"] = itemOrganize.ParentId;
                    dr["Sort"] = itemOrganize.Sort;
                    dr["SortCode"] = itemOrganize.SortCode;
                    dr["SortParentCode"] = itemOrganize.SortParentCode;

                    UtilsDataTable.DataRowIsNull(dr, "UpLevelName", itemOrganize.LevelName);

                    UtilsDataTable.DataRowIsNull(dr, "SysRoleCode", itemOrganize.UniversalRoleId);

                    dtTb_Sys_Role.Rows.Add(dr);

                    //sqlRole.AppendLine($"DELETE Tb_Sys_Role WHERE RoleCode='{itemOrganize.Id}';");

                }

            }

            void AddDepartmentOrgan(object id)
            {
                Organize modelOrganize = listCleanData.Where(c => c.Id.ToString() == id.ToString()).FirstOrDefault();

                if (modelOrganize is not null)
                {
                    if (dtTb_Sys_Department.Select($"DepCode='{id}'").Length == 0)
                    {
                        dr = dtTb_Sys_Department.NewRow();

                        dr["DepCode"] = modelOrganize.Id;
                        //dr["SortDepCode"] = modelOrganize.Id;
                        dr["DepName"] = modelOrganize.Name;
                        dr["ParentId"] = modelOrganize.ParentId;
                        dr["Sort"] = modelOrganize.Sort;
                        dr["SortCode"] = modelOrganize.SortCode;
                        dr["SortParentCode"] = modelOrganize.SortParentCode;

                        dtTb_Sys_Department.Rows.Add(dr);

                        //sqlDepartment.AppendLine($"DELETE Tb_Sys_Department WHERE DepCode='{modelOrganize.Id}';");

                        AddDepartmentOrgan(modelOrganize.ParentId);
                    }
                }
            }


            foreach (var itemDictionary in dictionaryData)
            {

                dr = dtTb_Sys_Role.NewRow();

                dr["RoleCode"] = itemDictionary.Id;
                dr["RoleName"] = itemDictionary.Title;
                dr["Sort"] = itemDictionary.Sort;
                dr["IsSysRole"] = 1;
                dr["SortCode"] = "";
                dr["SortParentCode"] = "";

                dtTb_Sys_Role.Rows.Add(dr);

                //sqlRole.AppendLine($"DELETE Tb_Sys_Role WHERE RoleCode='{itemDictionary.Id}';");

            }

            log.Append($"\r\n生成数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            ResultMessage resultMessage = new();

            resultMessage = await SynchroOrgan(sqlOrgan.ToString(), dtTb_Sys_Organ, dtTb_Sys_OrganPartial, stoppingToken);

            log.Append($"\r\n插入区域数据 耗时{stopwatch.ElapsedMilliseconds}毫秒! {resultMessage.Message}");

            stopwatch.Restart();

            resultMessage = await SynchroCommunity(sqlCommunity.ToString(), dtTb_HSPR_Community, dtTb_HSPR_CommunityChargesMode, stoppingToken);

            log.Append($"\r\n插入项目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒! {resultMessage.Message}");

            stopwatch.Restart();

            resultMessage = await SynchroDepartment(sqlDepartment.ToString(), dtTb_Sys_Department, stoppingToken);

            log.Append($"\r\n插入机构数据 耗时{stopwatch.ElapsedMilliseconds}毫秒! {resultMessage.Message}");

            stopwatch.Restart();

            resultMessage = await SynchroRole(sqlRole.ToString(), dtTb_Sys_Role, stoppingToken);

            stopwatch.Stop();

            log.Append($"\r\n插入岗位数据 耗时{stopwatch.ElapsedMilliseconds}毫秒! {resultMessage.Message}");

            log.Append($"\r\n------同步项目机构岗位结束------");

            _logger.LogInformation(log.ToString());
        }

        public static async Task<ResultMessage> SynchroOrgan(string sql, DataTable dtTb_Sys_Organ, DataTable dtTb_Sys_OrganPartial, CancellationToken stoppingToken)
        {

            ResultMessage resultMessage = new();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            using var trans = sqlServerConn.OpenTransaction();

            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dtTb_Sys_Organ, "Tb_Sys_Organ", stoppingToken, trans);

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dtTb_Sys_OrganPartial, "Tb_Sys_OrganPartial", stoppingToken, trans);

                trans.Commit();

                resultMessage.SetSuccessResultMessage();

            }
            catch (Exception ex)
            {
                resultMessage.Message = $"{ex.Message}{ex.StackTrace}";

                trans.Rollback();
            }

            return resultMessage;
        }

        public static async Task<ResultMessage> SynchroCommunity(string sql, DataTable dtTb_HSPR_Community, DataTable dtTb_HSPR_CommunityChargesMode, CancellationToken stoppingToken)
        {

            ResultMessage resultMessage = new();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            using var trans = sqlServerConn.OpenTransaction();

            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dtTb_HSPR_Community, "Tb_HSPR_Community", stoppingToken, trans);

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dtTb_HSPR_CommunityChargesMode, "Tb_HSPR_CommunityChargesMode", stoppingToken, trans);

                trans.Commit();

                resultMessage.SetSuccessResultMessage();

            }
            catch (Exception ex)
            {
                resultMessage.Message = $"{ex.Message}{ex.StackTrace}";

                trans.Rollback();
            }

            return resultMessage;
        }

        public static async Task<ResultMessage> SynchroDepartment(string sql, DataTable dtTb_Sys_Department, CancellationToken stoppingToken)
        {

            ResultMessage resultMessage = new();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            using var trans = sqlServerConn.OpenTransaction();

            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dtTb_Sys_Department, "Tb_Sys_Department", stoppingToken, trans);

                trans.Commit();

                resultMessage.SetSuccessResultMessage();
            }
            catch (Exception ex)
            {
                resultMessage.Message = $"{ex.Message}{ex.StackTrace}";

                trans.Rollback();
            }

            return resultMessage;
        }

        public static async Task<ResultMessage> SynchroRole(string sql, DataTable dtTb_Sys_Role, CancellationToken stoppingToken)
        {

            ResultMessage resultMessage = new();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            using var trans = sqlServerConn.OpenTransaction();

            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dtTb_Sys_Role, "Tb_Sys_Role", stoppingToken, trans);

                trans.Commit();

                resultMessage.SetSuccessResultMessage();
            }
            catch (Exception ex)
            {
                resultMessage.Message = $"{ex.Message}{ex.StackTrace}";

                trans.Rollback();
            }

            return resultMessage;
        }
    }
}
