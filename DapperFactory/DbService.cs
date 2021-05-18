using Dapper;
using DapperFactory.Enum;
using DapperFactory.MySql;
using DapperFactory.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Linq;

namespace DapperFactory
{
    /// <summary>
    /// 提供数据库相关服务。
    /// </summary>
    public class DbService
    {
        /// <summary>
        /// 数据库配置信息。
        /// </summary>
        private static readonly IConfiguration Configuration;


        /// <summary>
        /// 数据库连接参数信息缓存。
        /// </summary>
        private static readonly ConcurrentDictionary<string, DbLinkParameters> DbLinkParametersPool = new();

        /// <summary>
        /// 初始化数据库服务。
        /// </summary>
        static DbService()
        {
            Configuration = UtilsConfig.ReadConfig();
        }

        /// <summary>
        /// 清除缓存的连接参数信息。
        /// </summary>
        public static void ClearCache()
        {
            DbLinkParametersPool.Clear();
        }

        /// <summary>
        /// 获取数据库链接。
        /// </summary>
        /// <param name="dbLibraryName">数据库配置节点名称。</param>
        /// <returns></returns>
        private static IDbConnection GetConnectionFromDbSettings(DBType dbType, DBLibraryName dbLibraryName)
        {
            var linkParameters = GetDbLinkParametersFromDbSettings(dbType, dbLibraryName);

            if (linkParameters is null)
                throw new Exception($"未找到指定数据库链接配置节点，节点名：{EnumHelper.GetDBLibraryName(dbLibraryName)}");

            return DbConnectionFactory.GetConnection(dbType, linkParameters.GetConnectionString());
        }

        /// <summary>
        /// 获取指定数据库链接参数。
        /// </summary>
        /// <param name="sectionName">数据库配置节点名称。</param>
        /// <returns></returns>
        public static DbLinkParameters GetDbLinkParametersFromDbSettings(DBType dbType, DBLibraryName dbLibraryName)
        {
            var libraryName = EnumHelper.GetDBLibraryName(dbLibraryName);

            var section = Configuration.GetSection(libraryName);
            if (section is null)
                throw new Exception($"未配置数据库链接，节点名：{libraryName}");
            if (DbLinkParametersPool.TryGetValue(libraryName, out var modelLinkParameters))
                return modelLinkParameters;

            DbLinkParameters linkParameters = dbType switch
            {
                DBType.SqlServer => section.Get<SqlServerLinkParameters>(),
                DBType.MySql => section.Get<MySqlLinkParameters>(),
                _ => throw new NotImplementedException($"暂不支持{dbType}数据库")
            };

            return linkParameters;
        }


        /// <summary>
        /// 获取数据库连接
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        /// <param name="dbLibraryName">数据库名称</param>
        /// <returns></returns>
        public static IDbConnection GetDbConnection(DBType dbType, DBLibraryName dbLibraryName)
        {
            var libraryName = EnumHelper.GetDBLibraryName(dbLibraryName);

            if (DbLinkParametersPool.TryGetValue(libraryName, out var linkParameters))
                return DbConnectionFactory.GetConnection(dbType, linkParameters.GetConnectionString());

            linkParameters = GetDbLinkParametersFromDbSettings(dbType, dbLibraryName);

            DbLinkParametersPool.AddOrUpdate(libraryName, _ => linkParameters, (_, _) => linkParameters);

            var conn = DbConnectionFactory.GetConnection(dbType, linkParameters.GetConnectionString());
            conn.Open();
            return conn;
        }

        /// <summary>
        /// 获取数据库连接。
        /// </summary>
        /// <param name="burstType">业务库类型。</param>
        /// <param name="subTreasuryId">分库id。</param>
        /// <param name="readonly">是否取只读数据库。</param>
        /// <returns></returns>
        public static IDbConnection GetSqlServerBurstDbConnection(DBType dbType, DbBurstType burstType, int subTreasuryId, bool @readonly = false)
        {
            var burstTypeName = burstType switch
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

            IDbConnection conn;

            var key = $"{subTreasuryId}_{burstTypeName}_{dbType}";

            if (DbLinkParametersPool.TryGetValue(key, out var linkParameters))
            {
                conn = DbConnectionFactory.GetConnection(dbType, linkParameters.GetConnectionString());
                conn.Open();
                return conn;
            }

            var sql = string.Empty;

            if (DBType.SqlServer == dbType)
            {
                sql = @"SELECT DataBaseIp AS Host,DataBaseName AS Database,DataBaseUser AS [User],DataBasePwd AS Password
                        FROM Tb_System_Burst 
                        WHERE CommID=@CommID AND BurstType=@BurstType;";

                if (@readonly)
                {
                    sql = @"SELECT DataBaseIp_Read AS Host,DataBaseName_Read AS Database,DataBaseUser_Read AS [User],DataBasePwd_Read AS Password 
                        FROM Tb_System_Burst 
                        WHERE  CommID=@CommID AND BurstType=@BurstType;";
                }

                using var bsConn = GetConnectionFromDbSettings(dbType, DBLibraryName.PMS_Bs);

                linkParameters = bsConn.QueryFirstOrDefault<SqlServerLinkParameters>(sql, new { CommID = subTreasuryId, BurstType = burstTypeName });

            }
            else if (DBType.MySql == dbType)
            {
                sql = @"SELECT database_ip_sqlserver AS Host,database_name AS DatabaseName,database_user AS User,database_pwd AS Password,database_port AS Port
                        FROM tb_base_burst where is_delete=0 AND burst_type=@BurstType and @SubTreasuryId >= start_value and @SubTreasuryId <= end_value";

                using var bsConn = GetConnectionFromDbSettings(dbType, DBLibraryName.Erp_Base);

                linkParameters = bsConn.QueryFirstOrDefault<SqlServerLinkParameters>(sql, new { SubTreasuryId = subTreasuryId, BurstType = burstTypeName });


            }

            if (linkParameters is null)
                throw new Exception($"未找到分库{burstType}连接配置");


            if (subTreasuryId != 0)
                DbLinkParametersPool.AddOrUpdate(key, _ => linkParameters, (_, _) => linkParameters);

            conn = DbConnectionFactory.GetConnection(dbType, linkParameters.GetConnectionString());
            conn.Open();
            return conn;
        }





    }
}
