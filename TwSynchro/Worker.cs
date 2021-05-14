using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TwSynchro.OrganizeModule;

namespace TwSynchro
{
    public class Worker : BackgroundService
    {

        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 服务开始
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        public override Task StartAsync(CancellationToken
         cancellationToken)
        {
            _logger.LogInformation("服务开始");
            _logger.LogTrace($"跟踪信息");
            _logger.LogDebug($"调试信息");
            _logger.LogInformation($"普通信息");
            _logger.LogWarning($"警告信息");
            _logger.LogError($"错误信息");
            _logger.LogCritical($"致命错误信息");
            return base.StartAsync(cancellationToken);
        }

        /// <summary>
        /// 服务停止
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("服务停止");
            return base.StopAsync(cancellationToken);
        }
        /// <summary>
        /// 服务销毁
        /// </summary>
        public override void Dispose()
        {
            _logger.LogInformation("服务销毁");
            base.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
               await Task.WhenAll(new[] { RunTaskOne(stoppingToken), RunTaskTwo(stoppingToken), RunTaskThree(stoppingToken) });
                //await Task.WhenAll(new[] { RunTaskOne(stoppingToken)});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            finally
            {

            }
        }

        protected Task RunTaskOne(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                OrganizeService.Synchro(_logger);
                //while (!stoppingToken.IsCancellationRequested)
                //{

                //    _logger.LogInformation("第一个程序 running at: {time}", DateTimeOffset.Now);
                //     Thread.Sleep(1000);
                //}
            }, stoppingToken);
        }

        protected Task RunTaskTwo(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                OrganizeService.Synchro(_logger);
                //while (!stoppingToken.IsCancellationRequested)
                //{
                //    _logger.LogInformation("第二个程序 running at: {time}", DateTimeOffset.Now);
                //    Thread.Sleep(1000);
                //}
            }, stoppingToken);
        }

        protected Task RunTaskThree(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("第三个程序 running at: {time}", DateTimeOffset.Now);
                    Thread.Sleep(1000);
                }
            }, stoppingToken);
        }

    }
}
