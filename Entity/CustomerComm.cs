using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class CustomerComm : BaseModel
    {

        /// <summary>
        /// 项目编码
        /// </summary>
        public object comm_id { get; set; }
        /// <summary>
        /// 公司客户id
        /// </summary>
        public object customer_id { get; set; }
        /// <summary>
        /// 客户备注：存json
        /// </summary>
        public string remarks { get; set; }

        /// <summary>
        /// 客户姓名
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 外文名称
        /// </summary>
        public string name_en { get; set; }

        /// <summary>
        /// 证件名称
        /// 0：居民身份证
        /// 1：临时居民身份证
        /// 2：户口簿
        /// 3：军人身份证件
        /// 4：武装警察身份证件
        /// 5：港澳居民往来内地通行证
        /// 6：港澳居民居住证
        /// 7：台湾居民来往大陆通行证
        /// 8：台湾居民居住证
        /// 9：护照
        /// 10：社会保障卡
        /// 11：驾驶证
        /// 12：统一社会信用代码
        /// </summary>
        public int idcard_type { get; set; }

        /// <summary>
        /// 证件名称中文
        /// </summary>
        public string idcard_type_name => idcard_type switch
        {
            0 => "居民身份证",
            1 => "临时居民身份证",
            2 => "户口簿",
            3 => "军人身份证件",
            4 => "武装警察身份证件",
            5 => "港澳居民往来内地通行证",
            6 => "港澳居民居住证",
            7 => "台湾居民来往大陆通行证",
            8 => "台湾居民居住证",
            9 => "护照",
            10 => "社会保障卡",
            11 => "驾驶证",
            12 => "统一社会信用代码",
            _ => ""
        };


        /// <summary>
        /// 证件号码
        /// </summary>
        public string idcard_num { get; set; }
        /// <summary>
        /// 移动电话（暂只支持国内11位手机号码）
        /// </summary>
        public string mobile { get; set; }
        /// <summary>
        /// 其它移动电话（暂只支持国内11位手机号码）
        /// </summary>
        public string other_mobile { get; set; }
        /// <summary>
        /// 固定电话
        /// </summary>
        public string tel { get; set; }
        /// <summary>
        /// 传真电话
        /// </summary>
        public string fax { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string link_man { get; set; }
        /// <summary>
        /// 联系地址
        /// </summary>
        public string link_address { get; set; }
        /// <summary>
        /// 邮政编码
        /// </summary>
        public string post_code { get; set; }
        /// <summary>
        /// 电子邮箱地址
        /// </summary>
        public string e_mail { get; set; }
        /// <summary>
        /// 客户类别，0=个人，1=单位
        /// </summary>
        public int category { get; set; }
        /// <summary>
        /// 客户分类，0=业主，1=租户，2=成员，3=客商，4=其他
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 是否客商
        /// </summary>
        public int is_trade { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public int sex { get; set; }

        /// <summary>
        /// 证件名称中文
        /// </summary>
        public string sex_name => sex switch
        {
            1 => "女",
            0 => "男",
            _ => ""
        };

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? birthday { get; set; }
        /// <summary>
        /// 国籍
        /// </summary>
        public string nationality { get; set; }
        /// <summary>
        /// 民族
        /// </summary>
        public string nation { get; set; }
        /// <summary>
        /// 政治面貌
        /// </summary>
        public string political_outlook { get; set; }
        /// <summary>
        /// 婚姻状况
        /// </summary>
        public string marital_status { get; set; }
        /// <summary>
        /// 是否兵役
        /// </summary>
        public int is_military_service { get; set; }
        /// <summary>
        /// 文化程度
        /// </summary>
        public string degree_education { get; set; }
        /// <summary>
        /// 工作单位
        /// </summary>
        public string work_unit { get; set; }
        /// <summary>
        /// 所属行业
        /// </summary>
        public string industry { get; set; }
        /// <summary>
        /// 兴趣爱好如：钓鱼，打牌
        /// </summary>
        public string hobby { get; set; }
        /// <summary>
        /// 法定代表人
        /// </summary>
        public string legal_representative { get; set; }
        /// <summary>
        /// 法定代表人电话
        /// </summary>
        public string legal_representative_tel { get; set; }
        /// <summary>
        /// 负责人
        /// </summary>
        public string person_liable { get; set; }
        /// <summary>
        /// 负责人电话
        /// </summary>
        public string person_liable_tel { get; set; }
        /// <summary>
        /// 经营业态
        /// </summary>
        public string business_type { get; set; }
        /// <summary>
        /// 经营范围
        /// </summary>
        public string business_scope { get; set; }
        /// <summary>
        /// 经营品牌
        /// </summary>
        public string brand_management { get; set; }
        /// <summary>
        /// 经营级别
        /// </summary>
        public object business_level { get; set; }
        /// <summary>
        /// 客户自定义字段，根据客户类型自动判别
        /// </summary>
        public string customer_field { get; set; }

        /// <summary>
        /// 客户资源编号
        /// </summary>
        public string comm_resource_code { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime time_stamp { get; set; }

      
        public int LiveOwnerTotal { get; set; } = 0;
        public int LiveTenantTotal { get; set; } = 0;
        public int LiveTempTotal { get; set; } = 0;


    }
}
