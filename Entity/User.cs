using System;

namespace Entity
{

    /// <summary>
    /// 同步到SqlServer用户表DTO
    /// </summary>
    public record User
    {
        public object ID { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public string SexName => Sex switch
        {
            0 => "女",
            1 => "男",
            _ => ""
        };


    /// <summary>
    /// 邮箱
    /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

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
