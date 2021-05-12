using DapperFactory.Enum;
using DapperFactory.MySql;
using DapperFactory.SqlServer;
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DapperFactory
{
    /// <summary>
    /// 数据库连接工厂。
    /// </summary>
    public static class DbConnectionFactory
    {
        /// <summary>
        /// 注册数据库提供器工厂。
        /// </summary>
        /// <param name="description"></param>
        public static void RegisterProviderFactory(IDbFactoryDescription description)
        {
            DbProviderFactories.RegisterFactory(description.InvariantName, description.FactoryTypeAssemblyQualifiedName);
        }

        /// <summary>
        /// 获取数据库连接对象。
        /// </summary>
        /// <param name="linkParameters">数据库链接参数信息。</param>
        /// <returns></returns>
        public static IDbConnection GetConnection([DisallowNull] DBType dbType,[DisallowNull] string connectionString)
        {
            var factory = dbType switch
            {
                DBType.SqlServer => DbProviderFactories.GetFactory(SqlServerFactoryDescription.Default.InvariantName),
                DBType.MySql => DbProviderFactories.GetFactory(MySqlFactoryDescription.Default.InvariantName),
                _ => null
            };

            if (factory is null)
                throw new Exception("数据库获取失败");

            var connection = factory.CreateConnection();
            if (connection is null)
                throw new Exception("数据库获取失败");

            connection.ConnectionString = connectionString;
            return connection;
        }
    }
}
