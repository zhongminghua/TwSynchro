using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperFactory.Enum
{
   public class EnumHelper
    {
        public static string GetDBLibraryName(DBLibraryName dbLibraryName)
        {
            var name = dbLibraryName switch
            {
                DBLibraryName.PMS_Bs => "PMS_Bs",
                DBLibraryName.PMS_Base => "PMS_Base",
                DBLibraryName.Erp_Base => "Erp_Base",
                _ => throw new ArgumentException("未知的动作", nameof(dbLibraryName))
            };
            return $"{name}";
        }
    }
}
