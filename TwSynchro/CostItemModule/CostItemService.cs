using Dapper;
using DapperFactory;
using DapperFactory.Enum;
using Entity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TwSynchro.CostItemModule
{
    public class CostItemService
    {
        public static void Synchro(ILogger<Worker> _logger)
        {
            SynchroCorpCostItem(_logger);
        }

        /// <summary>
        /// 同步公司科目
        /// </summary>
        /// <param name="_logger"></param>
        public async static void SynchroCorpCostItem(ILogger<Worker> _logger)
        {
            _logger.LogInformation($"------同步公司科目数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            StringBuilder sql = new($@"SELECT id AS CorpCostID,parent_id,sort AS CostSNum,cost_name AS CostName,cost_type AS CostType,min_unit AS RoundingNum,
            business_type AS SysCostSign, is_use AS IsSealed, product_name AS BillType, product_code AS BillCode FROM tb_base_charge_cost");

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, "Erp_Base");

            _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            var result = await DbConnectionExtensions.QueryPagerAsync<List<CorpCostItem>>(mySqlConn,sql.ToString(),"id",1000,1);

            _logger.LogInformation($"读取公司科目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, "PMS_Base");

            sql.Clear();

            sql.AppendLine("SELECT UserCode,UserName,LoginCode,PassWord,Sex,MobileTel,Email,IsFirstLogin,IsUse FROM Tb_Sys_User WHERE 1<>1;");

            var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

            DataTable dt = new DataTable("Tb_Sys_User");

            dt.Load(reader);

            DataRow dr;

            sql.Clear();

            foreach (var item in result)
            {
                dr = dt.NewRow();



                dt.Rows.Add(dr);

                sql.AppendLine($@"DELETE Tb_Sys_User WHERE UserCode='{itemUser.ID.ToString()}';");
            }



            stopwatch.Restart();


        }

    }
}
