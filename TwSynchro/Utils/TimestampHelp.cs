using Dapper;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace TwSynchro.Utils
{
    public static class TimestampHelp
    {
        /// <summary>
        /// 时间戳缓存
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> _timestampList = new();

        /// <summary>
        /// 读取时间戳
        /// </summary>
        /// <param name="sqlServerConn"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<string> GetTimestampAsync(IDbConnection sqlServerConn, string key)
        {
            string timestamp=string.Empty;

            if (CacheHelper.CacheValue(key)!=null)
            {
                timestamp = Convert.ToDateTime(CacheHelper.CacheValue(key)).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                string sql= $@"SELECT TOP 1 TimestampKey, TimestampValue FROM Tb_Sys_SynchroTimestamp_MySql WHERE TimestampKey='{key}'";

                var timestampdata = await sqlServerConn.QueryAsync<(string TimestampKey, DateTime TimestampValue)>(sql);

                if (timestampdata.AsList().Count>0)
                    timestamp = timestampdata.AsList()[0].TimestampValue.ToString("yyyy-MM-dd HH:mm:ss");
                else
                    timestamp = DateTime.Now.AddYears(-10).ToString("yyyy-MM-dd HH:mm:ss");

            }

            return timestamp;
        }

        /// <summary>
        /// 设置时间戳
        /// </summary>
        /// <param name="sqlServerConn">数据库连接</param>
        /// <param name="key"></param>
        /// <param name="value">时间戳值</param>
        /// <param name="minute">绝对过期时间（分钟）</param>
        public static async void SetTimestamp(IDbConnection sqlServerConn, string key, string value, int minute)
        {

            // _timestampList.AddOrUpdate(key, _ => value, (_, _) => value);

            CacheHelper.CacheInsertAddMinutes(key, value, minute);

            StringBuilder sql = new($@"SELECT COUNT(1) FROM Tb_Sys_SynchroTimestamp_MySql WHERE TimestampKey='{key}'");

            int i = sqlServerConn.QuerySingle<int>(sql.ToString());

            sql.Clear();

            if (i > 1)
            {
                sql.Append($@"UPDATE Tb_Sys_SynchroTimestamp_MySql SET TimestampValue='{value}' WHERE TimestampKey='{key}'");

                await sqlServerConn.ExecuteAsync(sql.ToString());
            }
            else
            {
                sql.Append($@"INSERT INTO Tb_Sys_SynchroTimestamp_MySql(TimestampKey,TimestampValue) VALUES ('{key}','{value}')");

                await sqlServerConn.ExecuteAsync(sql.ToString());
            }

        }


    }
}
