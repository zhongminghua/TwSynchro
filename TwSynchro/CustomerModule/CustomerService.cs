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
using System.Threading;
using System.Threading.Tasks;
using TwSynchro.Utils;
using Utils;

namespace TwSynchro.CustomerModule
{
    public class CustomerService
    {

        static readonly string TS_KEY_CUSTOMER = "Key_Customer";

        static readonly string TS_KEY_CUSTOMER_LIVE = "Key_Customer_Live";


        public static async Task Synchro(ILogger<Worker> _logger, CancellationToken stoppingToken)
        {

            StringBuilder log = new("\r\n------同步客户数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var timestampCustomer = await UtilsSynchroTimestamp.GetTimestampAsync(TS_KEY_CUSTOMER);

            var timestampCustomerLive = await UtilsSynchroTimestamp.GetTimestampAsync(TS_KEY_CUSTOMER_LIVE);

            StringBuilder sql = new($@"
                SELECT id,comm_id,name,idcard_type,idcard_num,post_code,mobile,e_mail,
                       fax,sex,birthday,link_man,nationality,work_unit,industry,category,
                       legal_representative,legal_representative_tel,is_trade,is_delete,time_stamp,
                       (SELECT COUNT(1) from tb_base_masterdata_customer_live b where b.is_delete=0 and b.customer_id=tb_base_masterdata_customer_comm.id) as 'LiveTotal'
                FROM tb_base_masterdata_customer_comm
                WHERE time_stamp > '{timestampCustomer}';

                SELECT a.id,a.customer_id,a.comm_id,a.resource_id,a.first_contact,a.is_delete,a.relation,a.owner_relation,
                    b.name,b.idcard_type,b.idcard_num,b.post_code,b.mobile,b.e_mail,b.fax,b.sex,b.birthday,
                    b.link_man,b.nationality,b.work_unit,b.industry,b.category,b.legal_representative,
                    b.legal_representative_tel,b.is_trade,a.active_status,a.time_stamp
                FROM tb_base_masterdata_customer_live a
                    LEFT JOIN tb_base_masterdata_customer_comm b ON a.customer_id = b.id
                WHERE a.time_stamp > '{timestampCustomerLive}';

            ");

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            log.Append($"\r\n创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            var readerMultiple = await mySqlConn.QueryMultipleAsync(sql.ToString());

            var dataCustomerComm = (await readerMultiple.ReadAsync<CustomerComm>()).ToList();

            var dataCustomerLive = (await readerMultiple.ReadAsync<CustomerLive>()).ToList();

            //if (!dataCustomerComm.Any() && !dataCustomerLive.Any())
            //{
            //    log.Append($"\r\n数据为空SQL语句:\r\n{sql}");

            //    _logger.LogInformation(log.ToString());

            //    return;
            //}

            log.Append($"\r\n读取客户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            sql.Clear();

            sql.AppendLine($@"
                SELECT CustID,CommID,CustName,PaperName,PaperCode,PostCode,MobilePhone,EMail,FaxTel,Sex,Birthday,Linkman,Nationality,
                        WorkUnit,Job,IsUnit,LegalRepr,LegalReprTel,IsSupplier,IsDelete,LiveType1,LiveType2,LiveType3
                FROM Tb_HSPR_Customer WITH(NOLOCK) WHERE 1<>1;

                SELECT LiveID,CommID,RoomID,CustID,IsActive,IsDelLive,LiveType FROM Tb_HSPR_CustomerLive WITH(NOLOCK) WHERE 1<>1;

                SELECT HoldID,CustID,CommID,RoomID,Name,PaperName,PaperCode,MobilePhone,Sex,Birthday,Linkman,Nationality,
				        WorkUnit,Job,IsDelete,Relationship
		        FROM Tb_HSPR_Household WITH(NOLOCK) WHERE 1<>1;");

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            using var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

            log.Append($"\r\n创建SqlServer连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

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
                //dr["LiveType1"] = 1;
                //dr["LiveType2"] = 0;
                //dr["LiveType3"] = 0;

                dtTb_HSPR_Customer.Rows.Add(dr);

                if (itemCustomerComm.LiveTotal == 0)
                {
                    sql.AppendLine($@"DELETE Tb_HSPR_CustomerLive WHERE LiveType ='3' AND CustID='{itemCustomerComm.id}';");
                    dr = dtTb_HSPR_CustomerLive.NewRow();
                    dr["LiveID"] = Guid.NewGuid().ToString();
                    dr["CommID"] = itemCustomerComm.comm_id;
                    dr["CustID"] = itemCustomerComm.id;
                    dr["IsActive"] = 0;
                    dr["IsDelLive"] = 0;
                    dr["LiveType"] = 3;
                    dtTb_HSPR_CustomerLive.Rows.Add(dr);
                }

                sql.AppendLine($@"DELETE Tb_HSPR_Customer WHERE CustID='{itemCustomerComm.id}';");

            }

            foreach (var itemCustomerLive in dataCustomerLive)
            {
                if (itemCustomerLive.active_status == "2")
                {
                    itemCustomerLive.first_contact = 0;
                    itemCustomerLive.is_delete = 1;
                }


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

                    continue;
                }


                dr = dtTb_HSPR_CustomerLive.NewRow();

                dr["LiveID"] = itemCustomerLive.id;
                dr["CommID"] = itemCustomerLive.comm_id;
                dr["RoomID"] = itemCustomerLive.resource_id;
                dr["CustID"] = itemCustomerLive.customer_id;

                dr["IsActive"] = itemCustomerLive.first_contact == 2 ? 1 : 0;
                dr["IsDelLive"] = itemCustomerLive?.is_delete;

                switch (itemCustomerLive.relation)
                {
                    case 1:
                        dr["LiveType"] = 1;
                        break;
                    case 2:
                        dr["LiveType"] = 2;
                        break;
                    //case 5: java没有客户关系就是临时客户
                    //    dr["LiveType"] = 3;
                    //break;
                    default:
                        dr["LiveType"] = itemCustomerLive.relation;
                        break;
                }

                dtTb_HSPR_CustomerLive.Rows.Add(dr);

                sql.AppendLine($@"DELETE Tb_HSPR_CustomerLive WHERE LiveType ='3' AND CustID='{itemCustomerLive.customer_id}';");
                sql.AppendLine($@"DELETE Tb_HSPR_CustomerLive WHERE LiveID='{itemCustomerLive.id}';");

            }

            log.Append($"\r\n生成客户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var trans = sqlServerConn.OpenTransaction();

            try
            {

                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                log.Append($"\r\n删除客户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                stopwatch.Restart();

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dtTb_HSPR_Customer, "Tb_HSPR_Customer", stoppingToken, trans);

                log.Append($"\r\n插入客户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dtTb_HSPR_CustomerLive, "Tb_HSPR_CustomerLive", stoppingToken, trans);

                log.Append($"\r\n插入Tb_HSPR_CustomerLive数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                stopwatch.Restart();

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dtTb_HSPR_Household, "Tb_HSPR_Household", stoppingToken, trans);

                stopwatch.Stop();

                log.Append($"\r\n插入家庭成员数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                trans.Commit();

                if (dataCustomerComm.Any())
                    _ = UtilsSynchroTimestamp.SetTimestampAsync(TS_KEY_CUSTOMER, dataCustomerComm.Max(c => c.time_stamp));

                if (dataCustomerLive.Any())
                    _ = UtilsSynchroTimestamp.SetTimestampAsync(TS_KEY_CUSTOMER_LIVE, dataCustomerLive.Max(c => c.time_stamp));

            }
            catch (Exception ex)
            {
                trans.Rollback();

                log.Append($"\r\n插入客户数据失败:{ex.Message}{ex.StackTrace}");

            }
            log.Append($"\r\n------同步客户数据结束------");

            _logger.LogInformation(log.ToString());
        }
    }
}
