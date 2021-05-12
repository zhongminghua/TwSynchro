using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using NLog;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using DapperFactory;

namespace TwSynchro
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            //是否是windows平台
            bool isWinPlantform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            Console.WriteLine($"是否是windows平台:{isWinPlantform}");

            var iHostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, loggerFactory) =>
                {
                    loggerFactory.AddFilter("System", Microsoft.Extensions.Logging.LogLevel.Warning);
                    loggerFactory.AddFilter("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning);
                    loggerFactory.AddNLog(@"NLog\NLog.config");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMySqlFactory();
                    services.AddSqlServerFactory();
                    services.AddHostedService<Worker>();
                });

            if (!isWinPlantform)
                return iHostBuilder.UseSystemd();
            return iHostBuilder.UseWindowsService();

        }
    }
}
