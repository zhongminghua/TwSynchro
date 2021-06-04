using System;

namespace Entity
{
    /// <summary>
    /// MySql 表名:rf_organize 项目机构岗位表
    /// </summary>
    public record Organize
    {
        public object Id { get; set; }

        public object ParentId { get; set; }

        /// <summary>
        /// 层级名称
        /// </summary>
        public string LevelName { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类型:1 单位 2 部门 3 岗位
        /// </summary>
        public int Type { get; set; }
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
        public int Status { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 组织机构唯一数字ID
        /// </summary>
        public string IntId { get; set; }

        /// <summary>
        /// 工作角色
        /// </summary>
        public string WorkRole { get; set; }

        /// <summary>
        /// 单位类型:1-总部，2-大区,3-公司，4-区域，5-片区，6-项目
        /// </summary>
        public int OrganType { get; set; }
        /// <summary>
        /// 项目性质:1-商住,2-公建
        /// </summary>
        public int CommKind { get; set; }

        /// <summary>
        /// 项目来源：1-集团项目，2-外拓项目
        /// </summary>
        public int? CommSource { get; set; } = 1;

        /// <summary>
        /// 管理面积
        /// </summary>
        public decimal TakeoverArea { get; set; }

        /// <summary>
        /// 接管时间
        /// </summary>
        public DateTime? TakeoverDate { get; set; }

        /// <summary>
        /// 管理性质:1-全委,2-半委,3-顾问
        /// </summary>
        public int TakeoverKind { get; set; }
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
        public int CoopModel { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string LinkPhone { get; set; }

        /// <summary>
        /// 收费方式:1-本月收本月,2-本月收上月
        /// </summary>
        public int? ChargingModel { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public int? SortNum { get; set; }


        /// <summary>
        /// 通用岗位ID
        /// </summary>
        public object UniversalRoleId { get; set; }


        /// <summary>
        /// 是否删除
        /// </summary>
        public int Is_Delete { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime time_stamp { get; set; }
    }
}
