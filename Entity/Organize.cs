namespace Entity
{
    /// <summary>
    /// MySql 表名:rf_organize 项目机构岗位表
    /// </summary>
    public class Organize
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类型:1 单位 2 部门 3 岗位
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 部门或岗位领导
        /// </summary>
        public string Leader { get; set; }
        /// <summary>
        /// 分管领导
        /// </summary>
        public string ChargeLeader { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// 状态  0 正常 1 冻结
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public string Sort { get; set; }
        /// <summary>
        /// 组织机构唯一数字ID
        /// </summary>
        public string IntId { get; set; }

        /// <summary>
        /// 工作角色
        /// </summary>
        public string WorkRole { get; set; }

        /// <summary>
        /// 单位类型:1-总部，2-公司，3-区域，4-片区，5-项目
        /// </summary>
        public string OrganType { get; set; }
        /// <summary>
        /// 项目性质:1-商住,2-公建
        /// </summary>
        public string CommKind { get; set; }

        /// <summary>
        /// 管理面积
        /// </summary>
        public string TakeoverArea { get; set; }

        /// <summary>
        /// 接管时间
        /// </summary>
        public string TakeoverDate { get; set; }

        /// <summary>
        /// 管理性质:1-全委,2-半委,3-顾问
        /// </summary>
        public string TakeoverKind { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 区
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// 街道
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// 社区
        /// </summary>
        public string Community { get; set; }
        /// <summary>
        /// 门牌号
        /// </summary>
        public string GateSign { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 合作方式:1-开发商合作,2-业委会合作
        /// </summary>
        public string CoopModel { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string LinkPhone { get; set; }

        /// <summary>
        /// 收费方式:1-本月收本月,2-本月收上月
        /// </summary>
        public string ChargingModel { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public string SortNum { get; set; }
        /// <summary>
        /// 通用岗位ID
        /// </summary>
        public string UniversalRoleId { get; set; }
    }
}
