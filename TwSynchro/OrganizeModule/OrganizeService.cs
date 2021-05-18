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
using System.Threading.Tasks;
using Utils;

namespace TwSynchro.OrganizeModule
{
    public class OrganizeService
    {

        public async static void Synchro(ILogger<Worker> _logger)
        {

            _logger.LogInformation($"------同步项目机构岗位数据开始------");

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            //int pageIndex = 1;

            //bool isHasNext = true;

            //while (isHasNext)
            //{
            //_logger.LogInformation($"第 {pageIndex} 页 数据");

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

            _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            //var result = await mySqlConn.QueryPagerAsync<Organize>(DBType.MySql, sql.ToString(), "ID", 10, pageIndex);

            var readerMultiple = await mySqlConn.QueryMultipleAsync(sql.ToString());

            var organizeData = readerMultiple.Read<Organize>().ToList();

            var dictionaryData = readerMultiple.Read<Dictionary>();

            //isHasNext = result.HasNext;

            //pageIndex++;

            _logger.LogInformation($"读取数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            _logger.LogInformation($"创建SqlServer连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            sql.Clear();

            sql.AppendLine("SELECT * FROM Tb_Sys_Organ WHERE 1<>1;");

            sql.AppendLine("SELECT * FROM Tb_Sys_OrganPartial WHERE 1<>1;");

            sql.AppendLine("SELECT CommID,CommName,CommKind,ManageTime,ManageKind,CommAddress,Province,City,Borough,Street,CommunityName,GateSign,Num,ParentId,Sort,IntId FROM Tb_HSPR_Community WHERE 1<>1;");

            sql.AppendLine("SELECT * FROM Tb_HSPR_CommunityChargesMode WHERE 1<>1;");

            sql.AppendLine("SELECT * FROM Tb_Sys_Department WHERE 1<>1;");

            sql.AppendLine("SELECT RoleCode,RoleName,ParentId,UpLevelName,SysRoleCode,IsSysRole,Sort FROM Tb_Sys_Role WHERE 1<>1;");

            sql.AppendLine("SELECT * FROM Tb_Sys_Department");


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

            DataTable dtTb_Sys_DepartmentData = new DataTable("Tb_Sys_Department");

            dtTb_Sys_DepartmentData.Load(reader);

            DataRow dr;

            sql.Clear();

            List<Organize> listCleanData = new();

            void CleanData(string id)
            {

                var listChildNode = organizeData.Where(c => c.ParentId.ToString() == id.ToString()).ToList();

                //清洗数据
                foreach (var modelOrganize in listChildNode)
                {
                    //var index = organizeData.IndexOf(modelOrganize);

                    //if (string.IsNullOrEmpty(modelOrganize.LevelName))
                    //{
                    //    organizeData[index].LevelName = levelName is null ? modelOrganize.Name : $"{modelOrganize.Name}_{levelName}";

                    //    CleanData(modelOrganize.ParentId, modelOrganize.LevelName);
                    //}

                    if (listCleanData.Where(c => c.Id == modelOrganize.Id).FirstOrDefault() is null)
                    {
                        var modelChildNode = listCleanData.Where(c => c.Id == modelOrganize.ParentId).FirstOrDefault();

                        modelOrganize.LevelName = $"{modelChildNode.LevelName}_{modelOrganize.Name}";

                        listCleanData.Add(modelOrganize with { });

                        CleanData(modelOrganize.Id.ToString());

                    }

                }
            }


            var firstModel = organizeData.Where(c => c.ParentId.ToString() == "00000000-0000-0000-0000-000000000000").FirstOrDefault();

            firstModel.LevelName = firstModel.Name;

            listCleanData.Add(firstModel with { });

            //清洗数据
            CleanData(firstModel.Id.ToString());

            foreach (var itemOrganize in listCleanData)
            {

                if (itemOrganize.Type == 1)
                {
                    dr = dtTb_Sys_Organ.NewRow();

                    dr["OrganCode"] = itemOrganize.Id;

                    dr["OrganName"] = itemOrganize.Name;

                    dr["ParentId"] = itemOrganize.ParentId;

                    dr["Sort"] = itemOrganize.Sort;

                    if (itemOrganize.OrganType == 3) dr["IsComp"] = 1;

                    dtTb_Sys_Organ.Rows.Add(dr);

                    sql.AppendLine($"DELETE Tb_Sys_Organ WHERE OrganCode='{itemOrganize.Id}';");

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

                        sql.AppendLine($"DELETE Tb_Sys_OrganPartial WHERE OrganCode='{itemOrganize.Id}';");
                    }

                    if (itemOrganize.OrganType == 6)
                    {
                        dr = dtTb_HSPR_Community.NewRow();

                        dr["CommID"] = itemOrganize.Id;

                        dr["CommName"] = itemOrganize.Name;

                        dr["CommKind"] = itemOrganize.CommKind; //项目业态

                        dr["ManageTime"] = itemOrganize.TakeoverDate;

                        dr["ManageKind"] = itemOrganize.TakeoverKind;

                        dr["CommAddress"] = itemOrganize.Address;

                        dr["Province"] = itemOrganize.Province;

                        dr["City"] = itemOrganize.City;

                        dr["Borough"] = itemOrganize.Area;

                        dr["Street"] = itemOrganize.Street;

                        dr["CommunityName"] = itemOrganize.Community;

                        dr["GateSign"] = itemOrganize.GateSign;

                        dr["Num"] = itemOrganize.SortNum;

                        dr["ParentId"] = itemOrganize.ParentId;

                        dr["Sort"] = itemOrganize.Sort;

                        dr["IntId"] = itemOrganize.IntId;

                        dtTb_HSPR_Community.Rows.Add(dr);

                        sql.AppendLine($"DELETE Tb_HSPR_Community WHERE CommID='{itemOrganize.Id}';");

                        if (itemOrganize.ChargingModel is not null)
                        {

                            dr = dtTb_HSPR_CommunityChargesMode.NewRow();

                            dr["IID"] = Guid.NewGuid().ToString();

                            dr["CommID"] = itemOrganize.Id;

                            dr["ChargesMode"] = itemOrganize.ChargingModel;

                            dtTb_HSPR_CommunityChargesMode.Rows.Add(dr);

                            sql.AppendLine($"DELETE Tb_HSPR_CommunityChargesMode WHERE CommID='{itemOrganize.Id}';");
                        }
                    }
                }

                if (itemOrganize.Type == 2)
                {

                    dr = dtTb_Sys_Department.NewRow();

                    dr["DepCode"] = itemOrganize.Id;

                    dr["DepName"] = itemOrganize.Name;

                    dr["ParentId"] = itemOrganize.ParentId;

                    dr["Sort"] = itemOrganize.Sort;

                    dtTb_Sys_Department.Rows.Add(dr);

                    sql.AppendLine($"DELETE Tb_Sys_Department WHERE DepCode='{itemOrganize.Id}';");

                    AddDepartmentOrgan(itemOrganize.ParentId);

                }

                if (itemOrganize.Type == 3)
                {

                    dr = dtTb_Sys_Role.NewRow();

                    dr["RoleCode"] = itemOrganize.Id;

                    dr["RoleName"] = itemOrganize.Name;

                    dr["ParentId"] = itemOrganize.ParentId;

                    dr["Sort"] = itemOrganize.Sort;

                    UtilsDataTable.DataRowIsNull(dr, "UpLevelName", itemOrganize.LevelName);

                    UtilsDataTable.DataRowIsNull(dr, "SysRoleCode", itemOrganize.UniversalRoleId);

                    dtTb_Sys_Role.Rows.Add(dr);

                    sql.AppendLine($"DELETE Tb_Sys_Role WHERE RoleCode='{itemOrganize.Id}';");

                }

            }

            void AddDepartmentOrgan(object id)
            {
                Organize modelOrganize = listCleanData.Where(c => c.Id.ToString() == id.ToString()).FirstOrDefault();

                if (modelOrganize is not null)
                {
                    if (dtTb_Sys_DepartmentData.Select($"DepCode='{id}'").Length == 0 && dtTb_Sys_Department.Select($"DepCode='{id}'").Length == 0)
                    {
                        dr = dtTb_Sys_Department.NewRow();

                        dr["DepCode"] = modelOrganize.Id;

                        dr["DepName"] = modelOrganize.Name;

                        dr["ParentId"] = modelOrganize.ParentId;

                        dr["Sort"] = modelOrganize.Sort;

                        dtTb_Sys_Department.Rows.Add(dr);

                        sql.AppendLine($"DELETE Tb_Sys_Department WHERE DepCode='{modelOrganize.Id}';");

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

                dtTb_Sys_Role.Rows.Add(dr);

                sql.AppendLine($"DELETE Tb_Sys_Role WHERE RoleCode='{itemDictionary.Id}';");

            }


            _logger.LogInformation($"生成数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            if (string.IsNullOrEmpty(sql.ToString()))
            {
                _logger.LogInformation($"数据为空,退出同步!");

                return;
            }


            await SynchroOrgan(sql.ToString(), dtTb_Sys_Organ, dtTb_Sys_OrganPartial);

            await SynchroOrgan(sql.ToString(), dtTb_Sys_Organ, dtTb_Sys_OrganPartial);

            await SynchroOrgan(sql.ToString(), dtTb_Sys_Organ, dtTb_Sys_OrganPartial);

            await SynchroOrgan(sql.ToString(), dtTb_Sys_Organ, dtTb_Sys_OrganPartial);
            await SynchroOrgan(sql.ToString(), dtTb_Sys_Organ, dtTb_Sys_OrganPartial);


            using var trans = sqlServerConn.OpenTransaction();

            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                _logger.LogInformation($"删除数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                stopwatch.Restart();

                DbBatch.InsertSingleTable(sqlServerConn, dtTb_Sys_Organ, "Tb_Sys_Organ", trans);

                DbBatch.InsertSingleTable(sqlServerConn, dtTb_Sys_OrganPartial, "Tb_Sys_OrganPartial", trans);

                stopwatch.Restart();

                _logger.LogInformation($"插入区域数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                DbBatch.InsertSingleTable(sqlServerConn, dtTb_HSPR_Community, "Tb_HSPR_Community", trans);

                DbBatch.InsertSingleTable(sqlServerConn, dtTb_HSPR_CommunityChargesMode, "Tb_HSPR_CommunityChargesMode", trans);

                stopwatch.Stop();

                _logger.LogInformation($"插入项目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                DbBatch.InsertSingleTable(sqlServerConn, dtTb_Sys_Department, "Tb_Sys_Department", trans);

                stopwatch.Stop();

                _logger.LogInformation($"插入机构数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Stop();

                DbBatch.InsertSingleTable(sqlServerConn, dtTb_Sys_Role, "Tb_Sys_Role", trans);

                _logger.LogInformation($"插入角色数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                trans.Commit();
            }
            catch (Exception)
            {
                trans.Rollback();
                throw;
            }
            //}
            _logger.LogInformation($"------同步项目机构岗位结束------");
        }

        public static async Task<ResultMessage> SynchroOrgan(string sql, DataTable dtTb_Sys_Organ, DataTable dtTb_Sys_OrganPartial)
        {

            ResultMessage resultMessage = new();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            using var trans = sqlServerConn.OpenTransaction();

            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                await DbBatch.InsertSingleTable(sqlServerConn, dtTb_Sys_Organ, "Tb_Sys_Organ", trans);

                await DbBatch.InsertSingleTable(sqlServerConn, dtTb_Sys_OrganPartial, "Tb_Sys_OrganPartial", trans);

                resultMessage.Result = true;

                trans.Commit();
            }
            catch (Exception ex)
            {
                resultMessage.Message = ex.ToString();

                trans.Rollback();
            }

            return resultMessage;
        }


        public static async Task<ResultMessage> SynchroCommunity(string sql, DataTable dtTb_HSPR_Community, DataTable dtTb_HSPR_CommunityChargesMode)
        {

            ResultMessage resultMessage = new();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            using var trans = sqlServerConn.OpenTransaction();

            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                await DbBatch.InsertSingleTable(sqlServerConn, dtTb_HSPR_Community, "Tb_HSPR_Community", trans);

                await DbBatch.InsertSingleTable(sqlServerConn, dtTb_HSPR_CommunityChargesMode, "Tb_HSPR_CommunityChargesMode", trans);

                resultMessage.Result = true;

                trans.Commit();
            }
            catch (Exception ex)
            {
                resultMessage.Message = ex.ToString();

                trans.Rollback();
            }

            return resultMessage;
        }
    }
}
