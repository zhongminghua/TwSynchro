using System;
using System.Diagnostics.CodeAnalysis;

namespace DapperFactory.MySql
{
    /// <summary>
    /// 指定MySql数据库工厂描述信息。
    /// </summary>
    public class MySqlFactoryDescription : IDbFactoryDescription
    {
        /// <summary>
        /// 默认SqlServer数据库提供器名称标识符。
        /// </summary>
        private const string DefaultInvariantName = "MY_SQL_DATABASE_PROVIDER";

        /// <summary>
        /// 默认SqlServer数据库工厂类型程序集限定名。
        /// </summary>
        private const string DefaultFactoryTypeAssemblyQualifiedName = "MySql.Data.MySqlClient.MySqlClientFactory,MySql.Data";

        /// <summary>
        /// 用于注册提供程序的固定提供程序名称。
        /// </summary>s
        public string InvariantName { get; init; }

        /// <summary>
        /// 指定SqlServer客户端工厂类所在程序集限定名称。
        /// </summary>
        public string FactoryTypeAssemblyQualifiedName { get; init; }

        /// <summary>
        /// SqlServer数据库工厂描述信息默认实例。
        /// </summary>
        public static MySqlFactoryDescription Default = new();

        /// <summary>
        /// 初始化SqlServer数据库工厂描述信息。
        /// </summary>
        public MySqlFactoryDescription(): this(DefaultInvariantName) { }

        /// <summary>
        /// 初始化SqlServer数据库工厂描述信息。
        /// </summary>
        /// <param name="invariantName">用于注册提供程序的固定提供程序名称。</param>
        public MySqlFactoryDescription([DisallowNull] string invariantName)
            : this(invariantName, DefaultFactoryTypeAssemblyQualifiedName)
        {

        }

        /// <summary>
        /// 初始化SqlServer数据库工厂描述信息。
        /// </summary>
        /// <param name="invariantName">用于注册提供程序的固定提供程序名称。</param>
        /// <param name="factoryTypeAssemblyQualifiedName"></param>
        public MySqlFactoryDescription([DisallowNull] string invariantName, [DisallowNull] string factoryTypeAssemblyQualifiedName)
        {
            InvariantName = invariantName ?? throw new ArgumentNullException(nameof(invariantName));
            FactoryTypeAssemblyQualifiedName = factoryTypeAssemblyQualifiedName ?? throw new ArgumentNullException(nameof(factoryTypeAssemblyQualifiedName));
        }
    }
}
