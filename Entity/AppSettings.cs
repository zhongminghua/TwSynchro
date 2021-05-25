using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Entity
{
    /// <summary>
    /// 配置文件模型。
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 同步用户停留时间。
        /// </summary>
        public int UserStopMsec { set; get; }

        /// <summary>
        /// 同步项目机构岗位停留时间。
        /// </summary>
        public int OrganizeStopMsec { set; get; }

        /// <summary>
        /// 同步客户停留时间。
        /// </summary>
        public int CustomerStopMsec { set; get; }

        /// <summary>
        /// 收费科目，标准停留时间。
        /// </summary>
        public int CostItemStopMsec { set; get; }

        /// <summary>
        /// 税率停留时间。
        /// </summary>
        public int TaxRateSettingStopMsec { set; get; }

        /// <summary>
        /// 资源停留时间。
        /// </summary>
        public int ResourceStopMsec { set; get; }

    }
}
