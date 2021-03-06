using System;

namespace Entity
{

    /// <summary>
    /// 同步到SqlServer同步岗位授权机构授权项目DTO
    /// </summary>
    public record Permission
    {
        public object Id { get; set; }

        /// <summary>
        /// 岗位ID
        /// </summary>
        public object roleid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public object ParentId { get; set; }

        /// <summary>
        /// 单位ID
        /// </summary>
        public object unitid { get; set; }


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
