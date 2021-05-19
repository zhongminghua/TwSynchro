using Dapper;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Utils;

namespace TwSynchro.Utils
{
    public static class TimestampHelp
    {
        /// <summary>
        /// 读取时间戳
        /// </summary>
        /// <param name="sqlServerConn"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<string> GetTimestampAsync(IDbConnection sqlServerConn, string key)
        {
            object timestamp = CacheHelper.CacheValue(key);

            if (timestamp is null)
            {

                string sql = $@"SELECT TOP 1 TimestampValue FROM Tb_Sys_SynchroTimestamp_MySql WHERE TimestampKey='{key}'";

                var data = (await sqlServerConn.QueryAsync<string>(sql)).ToList();

                if (data.Count > 0)
                    timestamp = DateTime.Parse(data[0]).ToString("yyyy-MM-dd HH:mm:ss");
                else
                    timestamp = DateTime.Now.AddYears(-10).ToString("yyyy-MM-dd HH:mm:ss");

            }
            return timestamp.ToString();
        }

        /// <summary>
        /// 设置时间戳
        /// </summary>
        /// <param name="sqlServerConn">数据库连接</param>
        /// <param name="key"></param>
        /// <param name="value">时间戳值</param>
        /// <param name="minute">绝对过期时间（分钟）</param>
        public static async void SetTimestampAsync(IDbConnection sqlServerConn, string key, string value, int minute)
        {

            CacheHelper.CacheInsertAddMinutes(key, value, minute);

            string sql = $@"SELECT COUNT(1) FROM Tb_Sys_SynchroTimestamp_MySql WHERE TimestampKey='{key}'";

            int count = sqlServerConn.QuerySingle<int>(sql.ToString());

            if (count > 1)
                sql = $@"UPDATE Tb_Sys_SynchroTimestamp_MySql SET TimestampValue='{value}' WHERE TimestampKey='{key}'";
            else
                sql = $@"INSERT INTO Tb_Sys_SynchroTimestamp_MySql(TimestampKey,TimestampValue) VALUES ('{key}','{value}')";

            await sqlServerConn.ExecuteAsync(sql.ToString());

        }


    }
}
