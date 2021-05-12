using DapperFactory.MySql;
using DapperFactory.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DapperFactory
{
    /// <summary>
    /// 数据库提供器。
    /// </summary>
    public static class DbProviderFactoriesServiceCollection
    {
        /// <summary>
        /// 添加SqlServer数据库提供器。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="description">SqlServer数据库工厂描述信息。</param>
        /// <returns></returns>
        public static IServiceCollection AddSqlServerFactory([NotNull] this IServiceCollection services, SqlServerFactoryDescription? description = null)
        {
            description ??= SqlServerFactoryDescription.Default;

            DbProviderFactories.RegisterFactory(description.InvariantName, description.FactoryTypeAssemblyQualifiedName);
            return services;
        }

        /// <summary>
        /// 添加MySql数据库提供器。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="description">MySql数据库工厂描述信息。</param>
        /// <returns></returns>
        public static IServiceCollection AddMySqlFactory([NotNull] this IServiceCollection services, MySqlFactoryDescription? description = null)
        {
            description ??= MySqlFactoryDescription.Default;

            DbProviderFactories.RegisterFactory(description.InvariantName, description.FactoryTypeAssemblyQualifiedName);
            return services;

        }
    }
}
