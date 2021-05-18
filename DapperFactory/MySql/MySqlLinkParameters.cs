using System.Collections.Generic;
using System.Linq;

namespace DapperFactory.MySql
{
    /// <summary>
    /// 指定MySQL数据库连接参数信息。
    /// </summary>
    public record MySqlLinkParameters : DbLinkParameters
    {


        string _connectionString => $"Server={Host};Database={DatabaseName};User ID={User};Password={Password};";

        /// <summary>
        /// 数据库相关属性默认值。
        /// </summary>
        private static readonly Dictionary<string, string> DefaultProperties = new()
        {
            //["Pooling"] = "true",
            //["AutoEnlist"] = "true",
            ["Min Pool Size"] = "10",
            ["Max Pool Size"] = "128",
            ["Connect Timeout"] = "15",
            ["Command Timeout"] = "5"
        };

        /// <summary>
        /// 初始化SqlServer数据库连接参数信息。
        /// </summary>
        public MySqlLinkParameters() => (Properties) = DefaultProperties;


        /// <summary>
        /// 初始化SqlServer数据库连接参数。
        /// </summary>
        /// <param name="host">数据库服务器地址。</param>
        /// <param name="port">数据库服务器端口。</param>
        /// <param name="databaseName">数据库名称。</param>
        /// <param name="user">数据库登录账号。</param>
        /// <param name="password">数据库登录密码。</param>
        public MySqlLinkParameters(string host, int port, string databaseName, string user, string password)
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
        public MySqlLinkParameters(string host, int port, string databaseName, string user, string password, Dictionary<string, string>? properties)
            : base(host, port, databaseName, user, password, properties)
        {

        }

        /// <summary>
        /// 获取MySql数据库连接字符串。
        /// </summary>
        /// <returns></returns>
        public override string GetConnectionString()
        {
            var connectionString = string.Empty;
            if (Port is null)
                connectionString = _connectionString;
            else
                connectionString = $"{_connectionString}Port = {Port};";

            if (Properties is null) return connectionString;

            var ebProperties = Properties.Keys.Select(key => $"{key}={Properties[key]}");

            var linkProperties = string.Join(";", ebProperties);

            return $"{connectionString}{linkProperties}";
        }
    }
}
