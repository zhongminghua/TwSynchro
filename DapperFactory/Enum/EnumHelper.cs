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
        public static string GetDbBurstTypeName(DbBurstType dbBurstType)
        {
            var name = dbBurstType switch
            {
                DbBurstType.CP => "CP",
                DbBurstType.Equipment => "EQ",
                DbBurstType.Safe => "SAFE",
                DbBurstType.Environment => "AMBIENT",
                DbBurstType.Patrol => "PATROL",
                DbBurstType.Supervision => "SUPERVISION",
                DbBurstType.HouseInspection => "HI_DC",
                DbBurstType.RiskManagement => "RM",
                DbBurstType.Incident => "INCIDENT",
                DbBurstType.Charge => "CHARGE",
                DbBurstType.Visit => "VISIT",
                DbBurstType.ReportStatistics => "RTS",
                _ => ""
            };
            return $"{name}";
        }
        
    }
}
