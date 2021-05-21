
using Entity;
using Microsoft.Extensions.Configuration;
using Utils;

namespace TwSynchro
{
    public class UtilsAppSettings
    {
        public static AppSettings GetAppSettings()
        {

            IConfiguration configuration = UtilsConfig.ReadConfig("appsettings.json");

            var s = configuration.GetSection("UserStopMsec");

            return configuration.GetSection("StopMsec").Get<AppSettings>();

        }
    }
}
