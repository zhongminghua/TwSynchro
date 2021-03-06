using Dapper;
using DapperFactory;
using DapperFactory.Enum;
using Entity;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwSynchro.Utils;
using Utils;

namespace TwSynchro.UserModule
{
    public class UserService
    {
        static readonly string TS_KEY = "Key_UserService";

        public static async Task Synchro(ILogger<Worker> _logger, CancellationToken stoppingToken)
        {


            StringBuilder log = new("\r\n------同步用户数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var timestamp = await UtilsSynchroTimestamp.GetTimestampAsync(TS_KEY);

            StringBuilder sql = new(
                $@"SELECT a.ID,a.Name,a.Account,a.Password,a.Sex,a.Email,a.Mobile,a.time_stamp,d.id OrganizeId FROM rf_user a
                        left join rf_organizeuser b on a.Id=b.UserId and b.IsMain=1 and  b.Is_Delete=0
                        left join rf_organize c on b.OrganizeId=c.Id and c.Is_Delete=0
                        left join rf_organize d on c.ParentId=d.Id and d.Is_Delete=0
                   WHERE a.time_stamp > '{timestamp}'");

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            log.Append($"\r\n创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            var data = (await mySqlConn.QueryAsync<User>(sql.ToString())).ToList();

            if (!data.Any())
            {
                log.Append($"\r\n数据为空SQL语句:\r\n{sql}");

                _logger.LogInformation(log.ToString());

                return;
            }

            log.Append($"\r\n读取用户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            sql.Clear();

            sql.AppendLine("SELECT UserCode,UserName,LoginCode,PassWord,DepCode,Sex,MobileTel,Email,IsFirstLogin,IsUse,IsDelete FROM Tb_Sys_User WITH(NOLOCK) WHERE 1<>1;");

            var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

            DataTable dt = new DataTable("Tb_Sys_User");

            dt.Load(reader);

            DataRow dr;

            sql.Clear();

            foreach (var itemUser in data)
            {
                dr = dt.NewRow();

                dr["UserCode"] = itemUser.ID;
                dr["UserName"] = itemUser.Name;
                dr["LoginCode"] = itemUser.Account;
                dr["PassWord"] = itemUser.Password;
                dr["DepCode"] = itemUser.OrganizeId;
                dr["Sex"] = itemUser.SexName;
                dr["MobileTel"] = itemUser.Mobile;
                dr["Email"] = itemUser.Email;
                dr["IsFirstLogin"] = 1;
                dr["IsUse"] = 1;
                dr["IsDelete"] = itemUser.Is_Delete;

                dt.Rows.Add(dr);

                sql.AppendLine($@"DELETE Tb_Sys_User WHERE UserCode='{itemUser.ID}';");
            }

            log.Append($"\r\n生成用户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            using var trans = sqlServerConn.OpenTransaction();
            try
            {
                int rowsAffected = await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                log.Append($"\r\n删除用户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!删除数据总数: {rowsAffected}条");

                stopwatch.Restart();

                await DbBatch.InsertSingleTableAsync(sqlServerConn, dt, "Tb_Sys_User", stoppingToken, trans);

                stopwatch.Stop();

                log.Append($"\r\n插入用户数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

                trans.Commit();

                _ = UtilsSynchroTimestamp.SetTimestampAsync(TS_KEY, data.Max(c => c.time_stamp));
            }
            catch (Exception ex)
            {
                trans.Rollback();

                log.Append($"\r\n插入用户数据失败:{ex.Message}{ex.StackTrace}");

            }

            log.Append($"\r\n------同步用户数据结束------");

            _logger.LogInformation(log.ToString());
        }
    }
}
