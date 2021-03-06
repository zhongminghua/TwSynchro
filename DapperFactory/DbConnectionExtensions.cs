using Dapper;
using DapperFactory.Enum;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DapperFactory
{
    /// <summary>
    /// <see cref="IDbConnection"/> 扩展方法。
    /// </summary>
    public static class DbConnectionExtensions
    {
        /// <summary>
        /// 打开数据库事务。
        /// </summary>
        /// <param name="conn">数据库连接。</param>
        /// <returns></returns>
        public static IDbTransaction OpenTransaction(this IDbConnection conn)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            return conn.BeginTransaction();
        }

        /// <summary>
        /// 分页查询。
        /// </summary>
        /// <typeparam name="T">查询结果集要映射得实体类型。</typeparam>
        /// <param name="conn">数据库链接。</param>
        /// <param name="sql">查询sql语句。</param>
        /// <param name="orderBy">排序。</param>
        /// <param name="pageSize">页长。</param>
        /// <param name="pageIndex">页码。</param>
        /// <param name="trans">数据库事务。</param>
        /// <returns></returns>
        public static async Task<ResultPager<T>> QueryPagerAsync<T>(this IDbConnection conn, DBType dbType, string sql, string orderBy,
            int pageSize, int pageIndex, IDbTransaction? trans = null) where T : notnull
        {
            sql = sql.Trim(';');

            var strSql = dbType switch
            {
                DBType.SqlServer => $@"
                                    SELECT count(1) FROM ({sql}) AS t;
                                    SELECT * FROM 
                                    (
                                    SELECT row_number() over (ORDER BY {orderBy}) AS RN,*
                                    FROM 
                                    ({sql}) AS t
                                    ) AS t
                                    WHERE t.RN BETWEEN ({pageIndex}-1)*{pageSize}+1 AND {pageIndex}*{pageSize};",

                DBType.MySql => $@" SELECT count(1) FROM ({sql}) AS t;
                                    {sql} limit {(pageIndex - 1) * pageSize},{pageSize}
                                    ",
                _ => throw new NotImplementedException($"暂不支持{dbType}数据库")
            };

            var reader = await conn.QueryMultipleAsync(strSql, null, trans);

            var total = await reader.ReadFirstOrDefaultAsync<Int64>();
            var data = await reader.ReadAsync<T>();

            var result = new ResultPager<T> { PageSize = pageSize, PageIndex = pageIndex, TotalCount = total, Data = data };

            return result;
        }

    }
}
