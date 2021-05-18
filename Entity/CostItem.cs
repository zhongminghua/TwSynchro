using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    /// <summary>
    /// 公司收费科目表
    /// </summary>
    public record CostItem
    {

        /// <summary>
        /// 项目收费科目id
        /// </summary>
        public Guid CostID { get; set; }

        /// <summary>
        /// 项目ID
        /// </summary>
        public Guid CommID { get; set; }

        /// <summary>
        /// 公司收费科目id
        /// </summary>
        public Guid CorpCostID { get; set; }

        /// <summary>
        /// 父级收费项目id
        /// </summary>
        public Guid? Parent_Id { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public string CostSNum { get; set; }

        /// <summary>
        /// 收费科目
        /// </summary>
        public string CostName { get; set; }

        /// <summary>
        /// 计费取整位数：固定选项：元/角/分
        /// </summary>
        public string RoundingNum { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public string IsDelete { get; set; }
    }
}
