using Entity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using TwSynchro.CostItemModule;
using TwSynchro.CustomerModule;
using TwSynchro.OrganizeModule;
using TwSynchro.UserModule;

namespace TwSynchro
{
    public class Worker : BackgroundService
    {

        private readonly ILogger<Worker> _logger;
        private readonly AppSettings _appSettings;
        public Worker(ILogger<Worker> logger, IOptions<AppSettings> options)
        {
            _logger = logger;
            _appSettings = options.Value;
        }

        /// <summary>
        /// ����ʼ
        /// </summary>
        /// <param name="cancellationToken">ȡ������</param>
        /// <returns></returns>
        public override Task StartAsync(CancellationToken
         cancellationToken)
        {
            //_logger.LogInformation("����ʼ");
            //_logger.LogTrace($"������Ϣ");
            //_logger.LogDebug($"������Ϣ");
            //_logger.LogInformation($"��ͨ��Ϣ");
            //_logger.LogWarning($"������Ϣ");
            //_logger.LogError($"������Ϣ");
            //_logger.LogCritical($"����������Ϣ");
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
                await Task.WhenAll(new[] { RunTaskOrganize(stoppingToken)});

                //await Task.WhenAll(new[] { RunTaskUser(stoppingToken), RunTaskOrganize(stoppingToken), RunTaskCustomer(stoppingToken) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            finally
            {

            }
        }

        protected Task RunTaskUser(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
           {

               while (!stoppingToken.IsCancellationRequested)
               {
                   try
                   {

                       await UserService.Synchro(_logger, stoppingToken);

                       Thread.Sleep(_appSettings.UserStopMsec);
                   }
                   catch (Exception ex)
                   {
                       _logger.LogError($"�û�:\r\n{ex.Message}{ex.StackTrace}");
                   }
               }
           }, stoppingToken);
        }

        protected Task RunTaskOrganize(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await OrganizeService.Synchro(_logger, stoppingToken);

                        Thread.Sleep(_appSettings.UserStopMsec);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"�û�:\r\n{ex.Message}{ex.StackTrace}");
                    }
                }
            }, stoppingToken);
        }

        protected Task RunTaskCustomer(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {

                        await CustomerService.Synchro(_logger);

                        Thread.Sleep(_appSettings.UserStopMsec);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"�û�:\r\n{ex.Message}{ex.StackTrace}");
                    }
                }
            }, stoppingToken);
        }

        protected Task RunTaskCostItem(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await CostItemService.Synchro(_logger, stoppingToken);

                        Thread.Sleep(_appSettings.UserStopMsec);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"�û�:\r\n{ex.Message}{ex.StackTrace}");
                    }
                }
            }, stoppingToken);
        }

    }
}
