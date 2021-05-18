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
    public record CorpCostItem
    {
        /// <summary>
        /// 公司收费科目id
        /// </summary>
        public object CorpCostID { get; set; }

        /// <summary>
        /// 父级收费项目id
        /// </summary>
        public object Parent_Id { get; set; }

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
        /// 是否停用：固定选项：是/否
        /// </summary>
        public string IsSealed { get; set; }

        /// <summary>
        /// 商品名称：必填(开票类别)
        /// </summary>
        public string BillType { get; set; }

        /// <summary>
        /// 开票代码
        /// </summary>
        public string BillCode { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public string IsDelete { get; set; }
    }
}
