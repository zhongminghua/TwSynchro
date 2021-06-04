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
        /// 服务开始
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        public override Task StartAsync(CancellationToken
         cancellationToken)
        {
            _logger.LogInformation("服务开始");
            //_logger.LogTrace($"跟踪信息");
            //_logger.LogDebug($"调试信息");
            //_logger.LogInformation($"普通信息");
            //_logger.LogWarning($"警告信息");
            //_logger.LogError($"错误信息");
            //_logger.LogCritical($"致命错误信息");
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

        /// <summary>
        /// 服务启动
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
                      _logger.LogError($"用户:\r\n{ex.Message}{ex.StackTrace}");
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
                       _logger.LogError($"机构:\r\n{ex.Message}{ex.StackTrace}");
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
                       _logger.LogError($"客户:\r\n{ex.Message}{ex.StackTrace}");
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
                        _logger.LogError($"菜单:\r\n{ex.Message}{ex.StackTrace}");
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
                        _logger.LogError($"人员绑定岗位:\r\n{ex.Message}{ex.StackTrace}");
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
                        _logger.LogError($"岗位授权菜单:\r\n{ex.Message}{ex.StackTrace}");
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
                        _logger.LogError($"岗位授权机构授权项目:\r\n{ex.Message}{ex.StackTrace}");
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
                       _logger.LogError($"收费科目、标准:\r\n{ex.Message}{ex.StackTrace}");
                   }

                   Thread.Sleep(_appSettings.CostItemStopMsec);
               }
           }, stoppingToken);
        }

        /// <summary>
        /// 税率
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
                       _logger.LogError($"税率:\r\n{ex.Message}{ex.StackTrace}");
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
                       _logger.LogError($"资源:\r\n{ex.Message}{ex.StackTrace}");
                   }

                   Thread.Sleep(_appSettings.ResourceStopMsec);
               }
           }, stoppingToken);
        }

    }
}
