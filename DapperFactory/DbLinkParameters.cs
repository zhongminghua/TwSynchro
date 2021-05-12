using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DapperFactory
{
    /// <summary>
    /// 数据库连接参数对象工厂。
    /// </summary>
    public abstract record DbLinkParameters
    {
        /// <summary>
        /// 数据库服务器地址。
        /// </summary>
        public string Host { get; init; } = default!;

        /// <summary>
        /// 数据库服务器端口。
        /// </summary>
        public int? Port { get; init; } = default!;

        /// <summary>
        /// 数据库名称。
        /// </summary>
        public string Database { get; init; } = default!;

        /// <summary>
        /// 数据库登录账号。
        /// </summary>
        public string User { get; init; } = default!;

        /// <summary>
        /// 数据库登录密码。
        /// </summary>
        public string Password { get; init; } = default!;

        /// <summary>
        /// 数据库其它属性信息。
        /// </summary>
        public Dictionary<string, string>? Properties { get; init; }

        /// <summary>
        /// 初始化数据库连接参数对象。
        /// </summary>
        protected DbLinkParameters(){}

        /// <summary>
        /// 初始化数据库连接参数对象。
        /// </summary>
        /// <param name="server">数据库服务器地址。</param>
        /// <param name="name">数据库名称。</param>
        /// <param name="user">数据库登录账号。</param>
        /// <param name="password">数据库登录密码。</param>
        /// <param name="properties">数据库其它属性信息。</param>
        protected DbLinkParameters([DisallowNull] string host, int? port, [DisallowNull] string database, [DisallowNull] string user, string password, Dictionary<string, string>? properties)
            => (Host, Port, Database, User, Password, Properties) = (host, port, database, user, password, properties);

        /// <summary>
        /// 获取数据库连接字符串。
        /// </summary>
        /// <returns></returns>
        public abstract string GetConnectionString();
    }
}
