using System;

namespace Entity
{
    public class CustomerLive : BaseModel
    {

        /// <summary>
        /// 公司客户编码
        /// </summary>
        public object customer_corp_id { get; set; }

        /// <summary>
        /// 项目编码
        /// </summary>
        public object comm_id { get; set; }

        /// <summary>
        /// 项目客户ID
        /// </summary>
        public object customer_id { get; set; }

        /// <summary>
        /// 资源编码
        /// </summary>
        public object resource_id { get; set; }

        /// <summary>
        /// 居住关系
        /// 1-业主
        /// 2-租户
        /// 3-业主成员
        /// 4-租户成员
        /// 5-临时客户
        ///（2）资源类别为房屋时可以选择业主、租户、业主成员、租户成员，资源类别为车位时只能选择业主；
        ///（3）资源编号当前无业主时不允许登记业主成员、租户、租户成员，当前无租户时不允许登记租户成员，否则提示“请先迁入业主、租户后再迁入成员”；当前有租户时不允许登记第二个租户，否则提示“当前已存在租户，不能重复迁入”；',
        /// </summary>
        public int relation { get; set; }

        /// <summary>
        /// 是否第一联系人  1-不是，2-是	
        /// （1）居住关系为业主时才显示，必填
        /// （2）一个资源编号处于迁入状态的业主中必须且最多有一个第一联系人，默认将迁入的第一个业主设为是；
        /// （3）修改时只能将当前为迁入状态、不是第一联系人的业主改为是，同时自动将当前为迁入状态、是第一联系人的业主改为否；',
        /// </summary>
        public int first_contact { get; set; }

        /// <summary>
        /// 与户主关系
        ///（1）固定选项：本人/配偶/子女/孙子女/重孙子女/父母/祖父母/曾祖父母/兄弟/姊妹/叔父母/侄子女/亲属/员工/同事/朋友/其他；必填；
        ///（2）居住关系为业主、租户时自动默认为本人，不允许修改；
        /// </summary>
        public string owner_relation { get; set; }

        /// <summary>
        /// 成员对应户主
        /// 居住关系为业主成员、租户成员时才显示，居住关系为业主成员时默认当前第一联系人，居住关系为租户默认当前租户；必填；
        /// </summary>
        public string householder_id { get; set; }

        /// <summary>
        /// 居住状态
        /// 只有：迁入/迁出 1-迁入，2-迁出
        /// </summary>
        public string active_status { get; set; }


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
        /// 时间戳
        /// </summary>
        public DateTime time_stamp { get; set; }

    }
}
