using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class TaxRateSetting
    {
        /// <summary>
        /// 主键
        /// </summary>
        public Guid TaxRateSettingID { get; set; }

        /// <summary>
        /// 项目ID
        /// </summary>
        public Guid CommID { get; set; }

        /// <summary>
        /// 项目收费科目id
        /// </summary>
        public Guid CorpCostID { get; set; }

        /// <summary>
        /// 收费率
        /// </summary>
        public string TaxRate { get; set; }

        /// <summary>
        /// 合同违约金税率
        /// </summary>
        public string ContractPenaltyRate { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 合同违约金税率
        /// </summary>
        public string IsContractPenalty { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string OperationUserCode { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime? OperationDate { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public string IsDelete { get; set; }

    }
}
