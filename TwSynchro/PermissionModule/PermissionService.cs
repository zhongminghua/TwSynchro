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
using TwSynchro.Utils;
using Utils;

namespace TwSynchro.PermissionModule
{


    public class PermissionService
    {
        static readonly string TS_KEY = "Key_PermissionService";

        public static async Task Synchro(ILogger<Worker> _logger, CancellationToken stoppingToken)
        {
            StringBuilder log = new("\r\n------同步岗位授权机构授权项目开始------");

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            var timestamp = await UtilsSynchroTimestamp.GetTimestampAsync(TS_KEY);

            StringBuilder sql = new($@"
                SELECT a.id,a.roleid,a.unitid,a.Is_Delete,a.time_stamp,b.ParentId FROM rf_organize_permission a
                    LEFT JOIN rf_organize b ON a.unitid = b.Id
                WHERE a.time_stamp > '{timestamp}';"
            );

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            log.Append($"\r\n创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            var data = (await mySqlConn.QueryAsync<Permission>(sql.ToString())).ToList();

            if (!data.Any())
            {
                log.Append($"\r\n数据为空SQL语句:\r\n{sql}");
                _logger.LogInformation(log.ToString());
                return;
            }

            log.Append($"\r\n读取岗位授权机构授权项目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            sql.Clear();

            sql.AppendLine(@"SELECT IID,RoleCode,OrganCode,CommID FROM Tb_Sys_RoleData WITH(NOLOCK) WHERE 1<>1;
                             SELECT Id,RoleCode,DepCode FROM Tb_Sys_DepartmentRolePermissions WITH(NOLOCK)  WHERE 1<>1;");

            var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

            DataTable dtTb_Sys_RoleData = new DataTable("Tb_Sys_RoleData");

            dtTb_Sys_RoleData.Load(reader);

            DataTable dtTb_Sys_DepartmentRolePermissions = new DataTable("Tb_Sys_DepartmentRolePermissions");

            dtTb_Sys_DepartmentRolePermissions.Load(reader);

            DataRow dr;

            sql.Clear();

            foreach (var itemPermission in data)
            {
                sql.AppendLine($@"DELETE Tb_Sys_RoleData WHERE IID='{itemPermission.Id}';");

                sql.AppendLine($@"DELETE Tb_Sys_DepartmentRolePermissions WHERE Id='{itemPermission.Id}';");

                if (itemPermission.Is_Delete == 1) continue;

                dr = dtTb_Sys_RoleData.NewRow();
                dr["IID"] = itemPermission.Id;
                dr["RoleCode"] = itemPermission.roleid;
                dr["OrganCode"] = itemPermission.ParentId;
                dr["CommID"] = itemPermission.unitid;
                dtTb_Sys_RoleData.Rows.Add(dr);

                dr = dtTb_Sys_DepartmentRolePermissions.NewRow();
                dr["Id"] = itemPermission.Id;
                dr["RoleCode"] = itemPermission.roleid;
                dr["DepCode"] = itemPermission.unitid;
                dtTb_Sys_DepartmentRolePermissions.Rows.Add(dr);

            }

            log.Append($"\r\n生成岗位授权机构授权项目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var trans = sqlServerConn.OpenTransaction();

            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                log.Append($"\r\n删除岗位授权机构授权项目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                stopwatch.Restart();

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dtTb_Sys_RoleData, "Tb_Sys_RoleData", stoppingToken, trans);

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dtTb_Sys_DepartmentRolePermissions, "Tb_Sys_DepartmentRolePermissions", stoppingToken, trans);

                stopwatch.Stop();

                log.Append($"\r\n插入岗位授权机构授权项目数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                trans.Commit();

                _ = UtilsSynchroTimestamp.SetTimestampAsync(TS_KEY, data.Max(c => c.time_stamp));
            }
            catch (Exception ex)
            {
                trans.Rollback();

                log.Append($"\r\n插入岗位授权机构授权项目数据失败:{ex.Message}{ex.StackTrace}");

            }

            log.Append($"\r\n------同步岗位授权机构授权项目数据结束------");

            _logger.LogInformation(log.ToString());

        }
    }
}
