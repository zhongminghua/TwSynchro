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
        /// ����ʼ
        /// </summary>
        /// <param name="cancellationToken">ȡ������</param>
        /// <returns></returns>
        public override Task StartAsync(CancellationToken
         cancellationToken)
        {
            _logger.LogInformation("����ʼ");
            _logger.LogTrace($"������Ϣ");
            _logger.LogDebug($"������Ϣ");
            _logger.LogInformation($"��ͨ��Ϣ");
            _logger.LogWarning($"������Ϣ");
            _logger.LogError($"������Ϣ");
            _logger.LogCritical($"����������Ϣ");
            return base.StartAsync(cancellationToken);
        }

        /// <summary>
        /// ����ֹͣ
        /// </summary>
        /// <param name="cancellationToken">ȡ������</param>
        /// <returns></returns>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("����ֹͣ");
            return base.StopAsync(cancellationToken);
        }
        /// <summary>
        /// ��������
        /// </summary>
        public override void Dispose()
        {
            _logger.LogInformation("��������");
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

                //    _logger.LogInformation("��һ������ running at: {time}", DateTimeOffset.Now);
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
                //    _logger.LogInformation("�ڶ������� running at: {time}", DateTimeOffset.Now);
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
                    _logger.LogInformation("���������� running at: {time}", DateTimeOffset.Now);
                    Thread.Sleep(1000);
                }
            }, stoppingToken);
        }

    }
}
