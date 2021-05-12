using System;
using System.Data;
using System.Data.SqlClient;

namespace DapperFactory
{
    /// <summary>
    /// 数据库服务辅助方法。
    /// </summary>
    public static class DbServiceHelper
    {
        /// <summary>
        /// 获取一个有序的GUID。
        /// </summary>
        /// <param name="type">数据库类型</param>
        /// <returns></returns>
        public static string GetSequentialGuid(Type type)
        {
            if (!type.IsSubclassOf(typeof(IDbConnection)))
                throw new ArgumentException($"类型 {type} 未继承自 System.Data.IDbConnection");

            var scheme = type switch
            {
                { } when type == typeof(SqlConnection) => SequentialGuidScheme.SequentialAtEnd,
                _ => SequentialGuidScheme.SequentialAtEnd
            };

            return SequentialGuid.NewGuid(scheme).ToString();
        }

        /// <summary>
        /// 获取一个有序的GUID。
        /// </summary>
        /// <param name="conn">数据库连接。</param>
        /// <returns></returns>
        public static string GetSequentialGuid(this IDbConnection conn)
        {
            var scheme = conn switch
            {
                SqlConnection => SequentialGuidScheme.SequentialAtEnd,
                _ => SequentialGuidScheme.SequentialAtEnd
            };

            return SequentialGuid.NewGuid(scheme).ToString();
        }

        /// <summary>
        /// 源字符串转换为sql语句值。
        /// </summary>
        /// <param name="value">源字符串。</param>
        /// <returns></returns>
        public static string ValueHandle(string value)
        {
            return value is null ? "" : $"'{value.Replace("'", "''")}'";
        }

        public static string ValueLikeHandle(string value)
        {
            return value is null ? "''" : $"'%{value.Replace("'", "''")}%'";
        }

        /// <summary>
        /// 源字符串转换为sql语句值。
        /// </summary>
        /// <param name="value">源字符串。</param>
        /// <returns></returns>
        public static string NullValueHandle(string value)
        {
            return value is null ? "null" : $"'{value.Replace("'", "''")}'";
        }

    }
}
