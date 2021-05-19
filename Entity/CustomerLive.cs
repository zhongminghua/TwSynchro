namespace Entity
{
    public class CustomerLive
    {

        /// <summary>
        /// 客户居住关系表id
        /// </summary>
        public object id { get; set; }
        /// <summary>
        /// 公司客户编码
        /// </summary>
        public string customer_corp_id { get; set; }
        /// <summary>
        /// 项目编码
        /// </summary>
        public string comm_id { get; set; }
        /// <summary>
        /// 客户编码
        /// </summary>
        public string customer_id { get; set; }
        /// <summary>
        /// 资源编码
        /// </summary>
        public string resource_id { get; set; }
        /// <summary>
        /// 居住关系
        /// 1-业主
        /// 2-租户
        /// 3-业主成员
        /// 4-租户成员
        ///（2）资源类别为房屋时可以选择业主、租户、业主成员、租户成员，资源类别为车位时只能选择业主；
        ///（3）资源编号当前无业主时不允许登记业主成员、租户、租户成员，当前无租户时不允许登记租户成员，否则提示“请先迁入业主、租户后再迁入成员”；当前有租户时不允许登记第二个租户，否则提示“当前已存在租户，不能重复迁入”；',
        /// </summary>
        public string relation { get; set; }
        /// <summary>
        /// 是否第一联系人  1-不是，2-是	
        /// （1）居住关系为业主时才显示，必填
        /// （2）一个资源编号处于迁入状态的业主中必须且最多有一个第一联系人，默认将迁入的第一个业主设为是；
        /// （3）修改时只能将当前为迁入状态、不是第一联系人的业主改为是，同时自动将当前为迁入状态、是第一联系人的业主改为否；',
        /// </summary>
        public string first_contact { get; set; }
        /// <summary>
        /// 与户主关系
        ///（1）固定选项：本人/配偶/子女/孙子女/重孙子女/父母/祖父母/曾祖父母/兄弟/姊妹/叔父母/侄子女/亲属/员工/同事/朋友/其他；必填；
        ///（2）居住关系为业主、租户时自动默认为本人，不允许修改；
        /// </summary>
        public string owner_relation { get; set; }
        /// <summary>
        /// 成员对应户主
        /// 居住关系为业主成员、租户成员时才显示，居住关系为业主成员时默认当前第一联系人，居住关系为租户默认当前租户；必填；
        /// </summary>
        public string householder_id { get; set; }
        /// <summary>
        /// 居住状态
        /// 只有：迁入/迁出 1-迁入，2-迁出
        /// </summary>
        public string active_status { get; set; }
        /// <summary>
        /// 发起人
        /// </summary>
        public string create_user { get; set; }
        /// <summary>
        /// 发起时间
        /// </summary>
        public string create_date { get; set; }
        /// <summary>
        /// 修改人
        /// </summary>
        public string modify_user { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public string modify_date { get; set; }
        /// <summary>
        /// 记录是否删除状态,0-正常，1-已删除
        /// </summary>
        public string is_delete { get; set; }
        /// <summary>
        /// 删除人
        /// </summary>
        public string delete_user { get; set; }
        /// <summary>
        /// 删除时间
        /// </summary>
        public string delete_date { get; set; }

    }
}
