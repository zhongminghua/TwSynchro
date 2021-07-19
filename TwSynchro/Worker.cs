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
using TwSynchro.MenuUserModule;
using TwSynchro.OrganizeModule;
using TwSynchro.OrganizeUserModule;
using TwSynchro.PermissionModule;
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
            _logger.LogInformation("����ʼ");
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

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.WhenAll(new[] { RunTaskMenuUser(stoppingToken) });

                //await Task.WhenAll(new[] { RunTaskUser(stoppingToken), RunTaskOrganize(stoppingToken), RunTaskCustomer(stoppingToken),
                //RunTaskMenu(stoppingToken), RunTaskOrganizeUser(stoppingToken), RunTaskMenuUser(stoppingToken), RunTaskPermission(stoppingToken),
                //RunTaskCostItem(stoppingToken), RunTaskTaxRateSetting(stoppingToken), RunTaskResource(stoppingToken)});
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
            return Task.Run(async () =>
           {

               while (!stoppingToken.IsCancellationRequested)
               {
                   try
                   {
                       await OrganizeService.Synchro(_logger, stoppingToken);
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
            return Task.Run(async () =>
           {

               while (!stoppingToken.IsCancellationRequested)
               {
                   try
                   {
                       await CustomerService.Synchro(_logger, stoppingToken);
                   }
                   catch (Exception ex)
                   {
                       _logger.LogError($"�ͻ�:\r\n{ex.Message}{ex.StackTrace}");
                   }

                   Thread.Sleep(_appSettings.UserStopMsec);
               }
           }, stoppingToken);
        }

        protected Task RunTaskMenu(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await MenuService.Synchro(_logger, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"�˵�:\r\n{ex.Message}{ex.StackTrace}");
                    }

                    Thread.Sleep(_appSettings.MenuStopMsec);
                }
            }, stoppingToken);
        }

        protected Task RunTaskOrganizeUser(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await OrganizeUserService.Synchro(_logger, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"��Ա�󶨸�λ:\r\n{ex.Message}{ex.StackTrace}");
                    }

                    Thread.Sleep(_appSettings.OrganizeUserStopMsec);
                }
            }, stoppingToken);
        }

        protected Task RunTaskMenuUser(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await MenuUserService.Synchro(_logger, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"��λ��Ȩ�˵�:\r\n{ex.Message}{ex.StackTrace}");
                    }

                    Thread.Sleep(_appSettings.MenuUserStopMsec);
                }
            }, stoppingToken);
        }

        protected Task RunTaskPermission(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await PermissionService.Synchro(_logger, stoppingToken);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"��λ��Ȩ������Ȩ��Ŀ:\r\n{ex.Message}{ex.StackTrace}");
                    }

                    Thread.Sleep(_appSettings.PermissionStopMsec);
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
                   }
                   catch (Exception ex)
                   {
                       _logger.LogError($"�շѿ�Ŀ����׼:\r\n{ex.Message}{ex.StackTrace}");
                   }

                   Thread.Sleep(_appSettings.CostItemStopMsec);
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
            return Task.Run(async () =>
           {

               while (!stoppingToken.IsCancellationRequested)
               {
                   try
                   {
                       await TaxRateSettingService.Synchro(_logger, stoppingToken);
                   }
                   catch (Exception ex)
                   {
                       _logger.LogError($"˰��:\r\n{ex.Message}{ex.StackTrace}");
                   }

                   Thread.Sleep(_appSettings.TaxRateSettingStopMsec);
               }
           }, stoppingToken);
        }

        protected Task RunTaskResource(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
           {
               while (!stoppingToken.IsCancellationRequested)
               {
                   try
                   {
                       await ResourceService.Synchro(_logger, stoppingToken);
                   }
                   catch (Exception ex)
                   {
                       _logger.LogError($"��Դ:\r\n{ex.Message}{ex.StackTrace}");
                   }

                   Thread.Sleep(_appSettings.ResourceStopMsec);
               }
           }, stoppingToken);
        }

    }
}
