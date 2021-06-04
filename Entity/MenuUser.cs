using System;

namespace Entity
{

    /// <summary>
    /// 同步到SqlServer授权岗位菜单DTO
    /// </summary>
    public record MenuUser
    {
        public object Id { get; set; }

        /// <summary>
        /// 菜单ID
        /// </summary>
        public object MenuId { get; set; }

        /// <summary>
        /// 使用对象（组织机构ID）
        /// </summary>
        public object Organizes { get; set; }

        /// <summary>
        /// 授权按钮
        /// </summary>
        public string Buttons { get; set; }
        
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
