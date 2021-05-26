using System;

namespace Entity
{

    /// <summary>
    /// 同步到SqlServer人授权岗位DTO
    /// </summary>
    public record OrganizeUser
    {
        public object Id { get; set; }

        /// <summary>
        /// 岗位ID
        /// </summary>
        public object OrganizeId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public object UserId { get; set; }


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
