using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class CostStanSetting
    {
        /// <summary>
        /// 主键
        /// </summary>
        public object IID { get; set; }

        /// <summary>
        /// 项目ID
        /// </summary>
        public object CommID { get; set; }

        /// <summary>
        /// 客户ID
        /// </summary>
        public object CustID { get; set; }

        /// <summary>
        /// RoomID
        /// </summary>
        public object RoomID { get; set; }

        /// <summary>
        /// 标准编码
        /// </summary>
        public object StanID { get; set; }

        /// <summary>
        /// 项目科目编码
        /// </summary>
        public object CostID { get; set; }

        /// <summary>
        /// 表记
        /// </summary>
        public string MeterSign { get; set; }

        /// <summary>
        /// 计费截止时间
        /// </summary>
        public DateTime? FeesEndDate { get; set; }

        /// <summary>
        /// 计算面积
        /// </summary>
        public string CalcArea { get; set; }

        /// <summary>
        /// 计费周期
        /// </summary>
        public int? ChargeCycle { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public string IsDelete { get; set; }

        /// <summary>
        /// 计费方式
        /// </summary>
        public int? PayType { get; set; }

        /// <summary>
        /// 计费单价
        /// </summary>
        public string StanSingleAmount { get; set; }

        /// <summary>
        /// 计费数量
        /// </summary>
        public string RoomBuildArea { get; set; }

        /// <summary>
        /// 删除人名称
        /// </summary>
        public string DelUserName { get; set; }
    }
}
