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

namespace TwSynchro.MenuUserModule
{
    public class MenuUserService
    {
        static readonly string TS_KEY = "Key_MenuUserService";

        public static async Task Synchro(ILogger<Worker> _logger, CancellationToken stoppingToken)
        {
            StringBuilder log = new("\r\n------同步岗位授权菜单数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var timestamp = await UtilsSynchroTimestamp.GetTimestampAsync(TS_KEY);

            StringBuilder sql = new($@"SELECT Id,MenuId,Organizes,Is_Delete,Buttons,time_stamp FROM rf_menuuser 
                                       WHERE time_stamp > '{timestamp}';

                                       SELECT Id,menu_id MenuId,universal_role_id Organizes,Is_Delete,time_stamp FROM rf_universalrole_permission
                                       WHERE time_stamp > '{timestamp}';");

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            log.Append($"\r\n创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            var readerMultiple = await mySqlConn.QueryMultipleAsync(sql.ToString());

            var data = (await readerMultiple.ReadAsync<MenuUser>()).ToList();

            var data2 = (await readerMultiple.ReadAsync<MenuUser>()).ToList();

            data.AddRange(data2);

            if (!data.Any())
            {
                log.Append($"\r\n数据为空SQL语句:\r\n{sql}");

                _logger.LogInformation(log.ToString());

                return;
            }

            log.Append($"\r\n读取岗位授权菜单数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            sql.Clear();

            sql.AppendLine("SELECT IID,RoleCode,PNodeCode FROM Tb_Sys_RolePope WITH(NOLOCK) WHERE 1<>1;");

            sql.AppendLine("SELECT Id,RoleCode,FunCode,BtnName FROM Tb_Sys_FunctionPope WITH(NOLOCK) WHERE 1<>1;");

            var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

            DataTable dtTb_Sys_RolePope = new DataTable("Tb_Sys_RolePope");

            dtTb_Sys_RolePope.Load(reader);

            DataTable dtTb_Sys_FunctionPope = new DataTable("Tb_Sys_FunctionPope");

            dtTb_Sys_FunctionPope.Load(reader);

            DataRow dr;

            sql.Clear();

            string[] arrBtnID;
            var strBtnID = string.Empty;

            foreach (var itemMenuUser in data)
            {
                if (!string.IsNullOrEmpty(itemMenuUser.Buttons))
                {
                    arrBtnID = itemMenuUser.Buttons.Split(',');
                    foreach (var id in arrBtnID)
                    {
                        strBtnID += $"'{id}',";
                    }
                }
            }

            sql.Clear();

            sql.AppendLine($"SELECT ButtonId,Name FROM RF_AppLibraryButton where ButtonId IN({strBtnID.Trim(',')});");

            var dataAppLibraryButton = await mySqlConn.QueryAsync<(object ButtonId, string Name)>(sql.ToString());

            sql.Clear();

            foreach (var itemMenuUser in data)
            {
                sql.AppendLine($@"DELETE Tb_Sys_RolePope WHERE IID='{itemMenuUser.Id}';");
                if (itemMenuUser.Is_Delete == 1) continue;

                dr = dtTb_Sys_RolePope.NewRow();
                dr["IID"] = itemMenuUser.Id;
                dr["RoleCode"] = itemMenuUser.Organizes;
                dr["PNodeCode"] = itemMenuUser.MenuId;
                dtTb_Sys_RolePope.Rows.Add(dr);

                if (!string.IsNullOrEmpty(itemMenuUser.Buttons))
                {

                    arrBtnID = itemMenuUser.Buttons.Split(',');

                    foreach (var id in arrBtnID)
                    {
                        sql.AppendLine($@"DELETE Tb_Sys_FunctionPope WHERE RoleCode='{itemMenuUser.Organizes}' AND FunCode='{id}';");

                        (object ButtonId, string Name) = dataAppLibraryButton.Where(c => c.ButtonId.ToString() == id).FirstOrDefault();

                        dr = dtTb_Sys_FunctionPope.NewRow();
                        dr["ID"] = Guid.NewGuid().ToString();
                        dr["RoleCode"] = itemMenuUser.Organizes;
                        dr["FunCode"] = id;
                        dr["BtnName"] = Name;
                        dtTb_Sys_FunctionPope.Rows.Add(dr);
                    }


                }
            }

            log.Append($"\r\n生成岗位授权菜单数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var trans = sqlServerConn.OpenTransaction();
            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                log.Append($"\r\n删除岗位授权菜单数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                stopwatch.Restart();

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dtTb_Sys_RolePope, "Tb_Sys_RolePope", stoppingToken, trans);

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dtTb_Sys_FunctionPope, "Tb_Sys_FunctionPope", stoppingToken, trans);

                stopwatch.Stop();

                log.Append($"\r\n插入岗位授权菜单数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                trans.Commit();

               // _ = UtilsSynchroTimestamp.SetTimestampAsync(TS_KEY, data.Max(c => c.time_stamp));

            }
            catch (Exception ex)
            {
                trans.Rollback();

                log.Append($"\r\n插入岗位授权菜单数据失败:{ex.Message}{ex.StackTrace}");

            }

            log.Append($"\r\n------同步岗位授权菜单数据结束------");

            _logger.LogInformation(log.ToString());



        }
    }
}
