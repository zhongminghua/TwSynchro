namespace DapperFactory
{
    /// <summary>
    /// 指定数据库连接工厂描述信息。
    /// </summary>
    public interface IDbFactoryDescription
    {
        /// <summary>
        /// 用于注册提供程序的固定提供程序名称。
        /// </summary>
        string InvariantName { get; init; }

        /// <summary>
        /// 指定继承自 <see cref="System.Data.Common.DbProviderFactory"/> 类的所在程序集限定名称。
        /// </summary>
        string FactoryTypeAssemblyQualifiedName { get; init; }
    }
}
