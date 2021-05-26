using Dapper;
using DapperFactory;
using DapperFactory.Enum;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Utils;

namespace TwSynchro.Utils
{
    public static class UtilsSynchroTimestamp
    {
        /// <summary>
        /// 读取时间戳
        /// </summary>
        /// <param name="sqlServerConn"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetTimestamp(string key)
        {
            object timestamp = CacheHelper.CacheValue(key);

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            if (timestamp is null)
            {

                var sql = $"SELECT TOP 1 Ts_Value FROM Tb_Synchro_TimeStamp WHERE Ts_Key='{key}'";

                var timestampValue = sqlServerConn.QueryFirstOrDefault<string>(sql);

                if (!string.IsNullOrEmpty(timestampValue))
                    timestamp = timestampValue;
                else
                    timestamp = DateTime.Now.AddYears(-10);
            }

            return DateTime.Parse(timestamp.ToString()).ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 设置时间戳
        /// </summary>
        /// <param name="sqlServerConn">数据库连接</param>
        /// <param name="key"></param>
        /// <param name="value">时间戳值</param>
        /// <param name="minute">绝对过期时间（分钟）</param>
        public static void SetTimestamp(string key, object value, int minute = 5)
        {
            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            CacheHelper.CacheInsertAddMinutes(key, value, minute);

            string sql = $"SELECT COUNT(1) FROM Tb_Synchro_TimeStamp WHERE Ts_Key='{key}'";

            int count = sqlServerConn.QuerySingle<int>(sql.ToString());

            if (count > 0)
                sql = $"UPDATE Tb_Synchro_TimeStamp SET Ts_Value='{value}' WHERE Ts_Key='{key}'";
            else
                sql = $"INSERT INTO Tb_Synchro_TimeStamp(Ts_Key,Ts_Value) VALUES ('{key}','{value}')";

            sqlServerConn.Execute(sql.ToString());

        }


    }
}
