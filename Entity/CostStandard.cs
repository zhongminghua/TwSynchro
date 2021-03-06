using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    /// <summary>
    /// 公司收费标准表
    /// </summary>
    public record CostStandard
    {

        /// <summary>
        /// 主键
        /// </summary>
        public object StanID { get; set; }

        /// <summary>
        /// 项目ID
        /// </summary>
        public object CommID { get; set; }

        /// <summary>
        /// 项目科目ID
        /// </summary>
        public object CostID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public object CorpStanID { get; set; }

        /// <summary>
        /// 公司科目ID
        /// </summary>
        public object CorpCostID { get; set; }

        /// <summary>
        /// 标准编号
        /// </summary>
        public string StanSign { get; set; }

        /// <summary>
        /// 标准名称
        /// </summary>
        public string StanName { get; set; }

        /// <summary>
        /// 标准说明
        /// </summary>
        public string StanExplain { get; set; }

        /// <summary>
        /// 计算方式
        /// </summary>
        public string StanFormula { get; set; }

        /// <summary>
        /// 通用收费标准
        /// </summary>
        public string StanAmount { get; set; }

        /// <summary>
        /// 停用日期
        /// </summary>
        public string StanEndDate { get; set; }

        /// <summary>
        /// 是否按条件计算
        /// </summary>
        public string IsCondition { get; set; }

        /// <summary>
        /// 计算条件
        /// </summary>
        public string ConditionField { get; set; }

        /// <summary>
        /// 计算条件列表
        /// </summary>
        public string condition_content { get; set; }

        /// <summary>
        /// 合同违约金比率
        /// </summary>
        public string DelinRates { get; set; }

        /// <summary>
        /// 合同违约金延期
        /// </summary>
        public string latefee_calc_date { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public string IsDelete { get; set; }

        /// <summary>
        /// 按条件计算方式
        /// </summary>
        public string IsStanRange { get; set; }

        /// <summary>
        /// 数量取整方式
        /// </summary>
        public string AmountRounded { get; set; }

        /// <summary>
        /// 标准系数
        /// </summary>
        public string Modulus { get; set; }

        /// <summary>
        /// 允许项目修改单价
        /// </summary>
        public string IsCanUpdate { get; set; }

        /// <summary>
        /// 是否停用/回收 固定选项：是/否，默认否，可修改；1-停用/回收，0-使用
        /// </summary>
        public int is_use { get; set; }
    }
}
