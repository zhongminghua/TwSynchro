using System;

namespace Entity
{

    /// <summary>
    /// 同步到SqlServer菜单表DTO
    /// </summary>
    public record Menu
    {
        public object Id { get; set; }

        public object ParentId { get; set; }

        /// <summary>
        /// 菜单名字
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 菜单URL
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Is_Delete { get; set; }
        


    }
}
