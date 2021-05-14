using Dapper;
using DapperFactory;
using DapperFactory.Enum;
using Entity;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace TwSynchro.OrganizeModule
{
    public class OrganizeService
    {
        public async static void Synchro(ILogger<Worker> _logger)
        {

            _logger.LogInformation($"------同步项目机构岗位数据开始------");

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            int pageIndex = 1;

            bool isHasNext = true;

            while (isHasNext)
            {
                _logger.LogInformation($"第 {pageIndex} 页 数据");

                StringBuilder sql = new("SELECT * FROM rf_organize;");

                using var mySqlConn = DbService.GetDbConnection(DBType.MySql, "erp_base");

                _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                var result = await mySqlConn.QueryPagerAsync<Organize>(DBType.MySql, sql.ToString(), "ID", 10, pageIndex);

                isHasNext = result.HasNext;

                pageIndex++;

                _logger.LogInformation($"读取数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, "PMS_Base");

                _logger.LogInformation($"创建SqlServer连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                sql.Clear();

                sql.AppendLine("SELECT OrganCode,OrganName,IsComp FROM Tb_Sys_Organ WHERE 1<>1;");

                sql.AppendLine("SELECT * FROM Tb_Sys_OrganPartial WHERE 1<>1;");

                sql.AppendLine("SELECT * FROM Tb_HSPR_Community WHERE 1<>1;");

                sql.AppendLine("SELECT * FROM Tb_HSPR_CommunityChargesMode WHERE 1<>1;");

                sql.AppendLine("SELECT * FROM Tb_Sys_Department WHERE 1<>1;");

                sql.AppendLine("SELECT * FROM Tb_Sys_Role WHERE 1<>1;");

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

                foreach (var itemOrganize in result.Data)
                {
                    if (itemOrganize.Type == 1)
                    {
                        dr = dtTb_Sys_Organ.NewRow();

                        dr["OrganCode"] = itemOrganize.Id;

                        dr["OrganName"] = itemOrganize.Name;

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

                            dtTb_HSPR_Community.Rows.Add(dr);

                            sql.AppendLine($"DELETE Tb_HSPR_Community WHERE CommID='{itemOrganize.Id}';");

                            if (itemOrganize.ChargingModel is not null)
                            {

                                dr = dtTb_HSPR_CommunityChargesMode.NewRow();

                                dr["IID"] = Guid.NewGuid().ToString();

                                dr["CommID"] = itemOrganize.Id ;
                                
                                dr["ChargesMode"] = itemOrganize.ChargingModel;

                                dtTb_HSPR_CommunityChargesMode.Rows.Add(dr);

                                sql.AppendLine($"DELETE Tb_HSPR_CommunityChargesMode WHERE CommID='{itemOrganize.Id}';");
                            }
                        }
                    }

                    if (itemOrganize.Type == 2)
                    {


                    }

                    if (itemOrganize.Type == 3)
                    {

                        dr = dtTb_Sys_Role.NewRow();

                        dr["RoleCode"] = Guid.NewGuid().ToString();

                        dr["RoleName"] = itemOrganize.Name;

                        dtTb_Sys_Role.Rows.Add(dr);

                        sql.AppendLine($"DELETE Tb_Sys_Role WHERE CommID='{itemOrganize.Id}';");

                    }

                }

                _logger.LogInformation($"生成数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                if (string.IsNullOrEmpty(sql.ToString()))
                {
                    _logger.LogInformation($"数据为空,退出同步!");

                    break;
                }


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

                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
            _logger.LogInformation($"------同步项目机构岗位结束------");
        }
    }
}
