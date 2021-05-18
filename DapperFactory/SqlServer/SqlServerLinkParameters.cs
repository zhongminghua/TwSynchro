using System.Collections.Generic;
using System.Linq;

namespace DapperFactory.SqlServer
{
    /// <summary>
    /// 指定SqlServer数据库连接参数信息。
    /// </summary>
    public record SqlServerLinkParameters : DbLinkParameters
    {
        string _connectionString => $"Data Source={Host};Initial Catalog={DatabaseName};User ID={User};Password={Password};";
        /// <summary>
        /// 数据库相关属性默认值。
        /// </summary>
        private static readonly Dictionary<string, string> DefaultProperties = new()
        {
            //["Pooling"] = "true",
            ["Min Pool Size"] = "10",
            ["Max Pool Size"] = "128",
            ["Connect Timeout"] = "5"
        };

        /// <summary>
        /// 初始化SqlServer数据库连接参数信息。
        /// </summary>
        public SqlServerLinkParameters() => (Properties) = DefaultProperties;

        /// <summary>
        /// 初始化SqlServer数据库连接参数。
        /// </summary>
        /// <param name="host">数据库服务器地址。</param>
        /// <param name="port">数据库服务器端口。</param>
        /// <param name="databaseName">数据库名称。</param>
        /// <param name="user">数据库登录账号。</param>
        /// <param name="password">数据库登录密码。</param>
        public SqlServerLinkParameters(string host, int port, string databaseName, string user, string password)
            : this(host, port, databaseName, user, password, DefaultProperties)
        {

        }

        /// <summary>
        /// 初始化SqlServer数据库连接参数。
        /// </summary>
        /// <param name="host">数据库服务器地址。</param>
        /// <param name="port">数据库服务器端口。</param>
        /// <param name="databaseName">数据库名称。</param>
        /// <param name="user">数据库登录账号。</param>
        /// <param name="password">数据库登录密码。</param>
        /// <param name="properties">数据库其它属性信息。</param>
        public SqlServerLinkParameters(string host, int port, string databaseName, string user, string password, Dictionary<string, string>? properties)
            : base(host, port, databaseName, user, password, properties)
        {

        }

        /// <summary>
        /// 获取SqlServer数据库连接字符串。
        /// </summary>
        /// <returns></returns>
        public override string GetConnectionString()
        {
            if (Properties is null) return _connectionString;

            var ebProperties = Properties.Keys.Select(key => $"{key}={Properties[key]}");

            var linkProperties = string.Join(";", ebProperties);

            return $"{_connectionString}{linkProperties}";
        }
    }
}
