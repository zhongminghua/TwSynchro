using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.IO;

namespace DapperFactory
{

    /// <summary>
    /// 配置文件读取帮助类
    /// </summary>
    public class UtilsConfig
    {
        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="configFileName">配置文件名称</param>
        /// <returns></returns>
        public static IConfiguration ReadConfig(string configFileName = "dbsettings.json")
        {
            var path = $"{AppContext.BaseDirectory}{configFileName}";
            if (!File.Exists(path))
                throw new Exception($"未读取到{configFileName} 数据库配置文件");
            //var source = new JsonConfigurationSource { Path = "dbsettings.json", ReloadOnChange = true };
            return new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile(configFileName).Build();
        }

    }
}
