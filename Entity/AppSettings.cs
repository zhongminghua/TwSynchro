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
        public int UserStopMsec { set; get; } = 60000;

        /// <summary>
        /// 同步项目机构岗位停留时间。
        /// </summary>
        public int OrganizeStopMsec { set; get; } = 60000;

        /// <summary>
        /// 同步客户停留时间。
        /// </summary>
        public int CustomerStopMsec { set; get; } = 60000;

        /// <summary>
        /// 同步菜单停留时间。
        /// </summary>
        public int MenuStopMsec { set; get; } = 60000;

        /// <summary>
        /// 同步人员绑定岗位停留时间。
        /// </summary>
        public int OrganizeUserStopMsec { set; get; } = 60000;

        /// <summary>
        /// 同步岗位授权菜单停留时间。
        /// </summary>
        public int MenuUserStopMsec { set; get; } = 60000;

        /// <summary>
        /// 同步岗位授权机构授权项目停留时间。
        /// </summary>
        public int PermissionStopMsec { set; get; } = 60000;


        /// <summary>
        /// 收费科目，标准停留时间。
        /// </summary>
        public int CostItemStopMsec { set; get; } = 60000;

        /// <summary>
        /// 税率停留时间。
        /// </summary>
        public int TaxRateSettingStopMsec { set; get; } = 60000;

        /// <summary>
        /// 资源停留时间。
        /// </summary>
        public int ResourceStopMsec { set; get; } = 60000;

        /// <summary>
        /// 同步房屋使用性质停留时间。
        /// </summary>
        public int PropertyUsesStopMsec { set; get; } = 60000;

    }
}
