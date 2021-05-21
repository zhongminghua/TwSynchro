namespace Entity
{
    public class BaseModel
    {
        public object id { get; set; }

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
