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
using Utils;

namespace TwSynchro.CustomerModule
{
    public class CustomerService
    {
        public async static void Synchro(ILogger<Worker> _logger)
        {
            _logger.LogInformation($"------同步客户数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //StringBuilder sql = new($@"
            //SELECT * FROM tb_base_masterdata_customer;
            //SELECT * FROM tb_base_masterdata_customer_comm;
            //SELECT * FROM tb_base_masterdata_customer_live; ");

            StringBuilder sql = new($@"
            SELECT id,comm_id,name,idcard_type,idcard_num,post_code,mobile,e_mail,
                   fax,sex,birthday,link_man,nationality,work_unit,industry,category,
                   legal_representative,legal_representative_tel,is_trade,is_delete
            FROM tb_base_masterdata_customer_comm;

            SELECT a.id,a.customer_id,a.comm_id,a.resource_id,a.first_contact,a.is_delete,a.relation,a.owner_relation,
                b.name,b.idcard_type,b.idcard_num,b.post_code,b.mobile,b.e_mail,b.fax,b.sex,b.birthday,
                b.link_man,b.nationality,b.work_unit,b.industry,b.category,b.legal_representative,
                b.legal_representative_tel,b.is_trade
            FROM tb_base_masterdata_customer_live a
                LEFT JOIN tb_base_masterdata_customer_comm b ON a.customer_id = b.id;

            ");
            List<string> errorlist = new List<string>() { "123123" };
            var successJobNo = $"'{string.Join("','", errorlist)}'";


            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            var readerMultiple = await mySqlConn.QueryMultipleAsync(sql.ToString());

            var dataCustomerComm = readerMultiple.Read<CustomerComm>();

            var dataCustomerLive = readerMultiple.Read<CustomerLive>();

            _logger.LogInformation($"读取客户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            sql.Clear();

            sql.AppendLine($@"
            SELECT CustID,CommID,CustName,PaperName,PaperCode,PostCode,MobilePhone,EMail,FaxTel,Sex,Birthday,Linkman,Nationality,
                   WorkUnit,Job,IsUnit,LegalRepr,LegalReprTel,IsSupplier,IsDelete
            FROM Tb_HSPR_Customer WHERE 1<>1;

            SELECT LiveID,CommID,RoomID,CustID,IsActive,IsDelLive,LiveType FROM Tb_HSPR_CustomerLive WHERE 1<>1;

            SELECT HoldID,CustID,CommID,RoomID,Name,PaperName,PaperCode,MobilePhone,Sex,Birthday,Linkman,Nationality,
				   WorkUnit,Job,IsDelete,Relationship
			FROM Tb_HSPR_Household WHERE 1<>1;");

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

            _logger.LogInformation($"创建SqlServer连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            DataTable dtTb_HSPR_Customer = new DataTable("Tb_HSPR_Customer");

            dtTb_HSPR_Customer.Load(reader);

            DataTable dtTb_HSPR_CustomerLive = new DataTable("Tb_HSPR_CustomerLive");

            dtTb_HSPR_CustomerLive.Load(reader);

            DataTable dtTb_HSPR_Household = new DataTable("Tb_HSPR_Household");

            dtTb_HSPR_Household.Load(reader);

            stopwatch.Restart();

            DataRow dr;

            sql.Clear();

            foreach (var itemCustomerComm in dataCustomerComm)
            {
                dr = dtTb_HSPR_Customer.NewRow();

                dr["CustID"] = itemCustomerComm.id;
                dr["CommID"] = itemCustomerComm.comm_id;
                dr["CustName"] = itemCustomerComm.name;
                dr["PaperName"] = itemCustomerComm.idcard_type_name;
                dr["PaperCode"] = itemCustomerComm.idcard_num;
                dr["PostCode"] = itemCustomerComm.post_code;
                dr["MobilePhone"] = itemCustomerComm.mobile;
                dr["EMail"] = itemCustomerComm.e_mail;
                dr["FaxTel"] = itemCustomerComm.fax;
                dr["Sex"] = itemCustomerComm.sex_name;
                UtilsDataTable.DataRowIsNull(dr, "Birthday", itemCustomerComm.birthday);
                dr["Linkman"] = itemCustomerComm.link_man;
                dr["Nationality"] = itemCustomerComm.nationality;
                dr["WorkUnit"] = itemCustomerComm.work_unit;
                dr["Job"] = itemCustomerComm.industry;
                dr["IsUnit"] = itemCustomerComm.category;
                dr["LegalRepr"] = itemCustomerComm.legal_representative;
                dr["LegalReprTel"] = itemCustomerComm.legal_representative_tel;
                dr["IsSupplier"] = itemCustomerComm.is_trade;
                dr["IsDelete"] = itemCustomerComm.is_delete;

                dtTb_HSPR_Customer.Rows.Add(dr);

                sql.AppendLine($@"DELETE Tb_HSPR_Customer WHERE CustID='{itemCustomerComm.id}';");

            }

            foreach (var itemCustomerLive in dataCustomerLive)
            {
                dr = dtTb_HSPR_CustomerLive.NewRow();

                dr["LiveID"] = itemCustomerLive.id;
                dr["CommID"] = itemCustomerLive.comm_id;
                dr["RoomID"] = itemCustomerLive.resource_id;
                dr["CustID"] = itemCustomerLive.customer_id;
                dr["IsActive"] = itemCustomerLive.first_contact == 2 ? 1 : 0;
                dr["IsDelLive"] = itemCustomerLive.is_delete;

                switch (itemCustomerLive.relation)
                {
                    case 1:
                        dr["LiveType"] = 1;
                        break;
                    case 2:
                        dr["LiveType"] = 2;
                        break;
                    case 5:
                        dr["LiveType"] = 3;
                        break;
                    default:
                        dr["LiveType"] = itemCustomerLive.relation;
                        break;
                }

                dtTb_HSPR_CustomerLive.Rows.Add(dr);

                sql.AppendLine($@"DELETE Tb_HSPR_CustomerLive WHERE LiveID='{itemCustomerLive.id}';");

                //业主成员
                if (itemCustomerLive.relation == 3)
                {
                    dr = dtTb_HSPR_Household.NewRow();
                    dr["HoldID"] = itemCustomerLive.id;
                    dr["CustID"] = itemCustomerLive.customer_id;
                    dr["CommID"] = itemCustomerLive.comm_id;
                    dr["RoomID"] = itemCustomerLive.resource_id;
                    dr["Name"] = itemCustomerLive.name;
                    dr["PaperName"] = itemCustomerLive.idcard_type_name;
                    dr["PaperCode"] = itemCustomerLive.idcard_num;
                    dr["MobilePhone"] = itemCustomerLive.mobile;
                    dr["Sex"] = itemCustomerLive.sex_name;
                    dr["Relationship"] = itemCustomerLive.owner_relation;
                    UtilsDataTable.DataRowIsNull(dr, "Birthday", itemCustomerLive.birthday);
                    dr["Linkman"] = itemCustomerLive.link_man;
                    dr["Nationality"] = itemCustomerLive.nationality;
                    dr["WorkUnit"] = itemCustomerLive.work_unit;
                    dr["Job"] = itemCustomerLive.industry;
                    dr["IsDelete"] = itemCustomerLive.is_delete;

                    dtTb_HSPR_Household.Rows.Add(dr);

                    sql.AppendLine($@"DELETE Tb_HSPR_Household WHERE HoldID='{itemCustomerLive.id}';");
                }

            }

            _logger.LogInformation($"生成客户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var trans = sqlServerConn.OpenTransaction();
            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                _logger.LogInformation($"删除客户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                stopwatch.Restart();

                await DbBatch.InsertSingleTable(sqlServerConn, dtTb_HSPR_Customer, "Tb_HSPR_Customer", trans);

                _logger.LogInformation($"插入客户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                await DbBatch.InsertSingleTable(sqlServerConn, dtTb_HSPR_CustomerLive, "Tb_HSPR_CustomerLive", trans);

                _logger.LogInformation($"插入Tb_HSPR_CustomerLive数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                await DbBatch.InsertSingleTable(sqlServerConn, dtTb_HSPR_Household, "Tb_HSPR_Household", trans);

                stopwatch.Stop();

                _logger.LogInformation($"插入家庭成员数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();

                _logger.LogInformation($"插入客户数据失败:{ex.Message}{ex.StackTrace}");

            }
            _logger.LogInformation($"------同步客户数据结束------");
        }
    }
}
