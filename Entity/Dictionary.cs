using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class Dictionary
    {
        public Guid Id { get; set; }

        public Guid ParentId { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 唯一代码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// 其它信息
        /// </summary>
        public string Other { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 0 正常 1 删除
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 标题_英语
        /// </summary>
        public string Title_en { get; set; }

        /// <summary>
        /// 标题_繁体中文
        /// </summary>
        public string Title_zh { get; set; }

    }
}
