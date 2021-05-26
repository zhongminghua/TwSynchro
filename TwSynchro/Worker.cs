using Entity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using TwSynchro.CostItemModule;
using TwSynchro.CustomerModule;
using TwSynchro.MenuModule;
using TwSynchro.OrganizeModule;
using TwSynchro.ResourceModule;
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
                await Task.WhenAll(new[] { RunTaskMenu(stoppingToken) });

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
            return Task.Run(() =>
          {

              while (!stoppingToken.IsCancellationRequested)
              {
                  try
                  {

                      UserService.Synchro(_logger);

                  }
                  catch (Exception ex)
                  {
                      _logger.LogError($"�û�:\r\n{ex.Message}{ex.StackTrace}");
                  }

                  Thread.Sleep(_appSettings.UserStopMsec);
              }
          }, stoppingToken);
        }

        protected Task RunTaskOrganize(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
           {

               while (!stoppingToken.IsCancellationRequested)
               {
                   try
                   {
                       OrganizeService.Synchro(_logger);


                   }
                   catch (Exception ex)
                   {
                       _logger.LogError($"����:\r\n{ex.Message}{ex.StackTrace}");
                   }

                   Thread.Sleep(_appSettings.UserStopMsec);
               }
           }, stoppingToken);
        }

        protected Task RunTaskCustomer(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
           {

               while (!stoppingToken.IsCancellationRequested)
               {
                   try
                   {

                       CustomerService.Synchro(_logger);

                       Thread.Sleep(_appSettings.UserStopMsec);
                   }
                   catch (Exception ex)
                   {
                       _logger.LogError($"�ͻ�:\r\n{ex.Message}{ex.StackTrace}");
                   }
               }
           }, stoppingToken);
        }

        protected Task RunTaskMenu(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {

                        MenuService.Synchro(_logger);

                        Thread.Sleep(_appSettings.UserStopMsec);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"�˵�:\r\n{ex.Message}{ex.StackTrace}");
                    }
                }
            }, stoppingToken);
        }
        protected Task RunTaskCostItem(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
           {

               while (!stoppingToken.IsCancellationRequested)
               {
                   try
                   {
                       CostItemService.Synchro(_logger);

                       Thread.Sleep(_appSettings.CostItemStopMsec);
                   }
                   catch (Exception ex)
                   {
                       _logger.LogError($"�շѿ�Ŀ����׼:\r\n{ex.Message}{ex.StackTrace}");
                   }
               }
           }, stoppingToken);
        }

        /// <summary>
        /// ˰��
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected Task RunTaskTaxRateSetting(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
           {

               while (!stoppingToken.IsCancellationRequested)
               {
                   try
                   {
                       TaxRateSettingService.Synchro(_logger);

                       Thread.Sleep(_appSettings.TaxRateSettingStopMsec);
                   }
                   catch (Exception ex)
                   {
                       _logger.LogError($"˰��:\r\n{ex.Message}{ex.StackTrace}");
                   }
               }
           }, stoppingToken);
        }


        protected Task RunTaskResource(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
           {
                //await ResourceService.Synchro(_logger, stoppingToken);
                while (!stoppingToken.IsCancellationRequested)
               {
                   try
                   {
                       ResourceService.Synchro(_logger);

                       Thread.Sleep(_appSettings.ResourceStopMsec);
                   }
                   catch (Exception ex)
                   {
                       _logger.LogError($"��Դ:\r\n{ex.Message}{ex.StackTrace}");
                   }
               }
           }, stoppingToken);
        }

    }
}
