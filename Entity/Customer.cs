using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class Customer
    {
        public object id { get; set; }
        public string name { get; set; }
        public string name_en { get; set; }
        public int idcard_type { get; set; }
        public string idcard_num { get; set; }
        public string mobile { get; set; }
        public string other_mobile { get; set; }
        public string tel { get; set; }
        public string fax { get; set; }
        public string link_man { get; set; }
        public string link_address { get; set; }
        public string post_code { get; set; }
        public string e_mail { get; set; }
        public int category { get; set; }
        public int type { get; set; }
        public int is_trade { get; set; }
        public int sex { get; set; }
        public DateTime? birthday { get; set; }
        public string nationality { get; set; }
        public string nation { get; set; }
        public string political_outlook { get; set; }
        public string marital_status { get; set; }
        public int is_military_service { get; set; }
        public string degree_education { get; set; }
        public string work_unit { get; set; }
        public string industry { get; set; }
        public string hobby { get; set; }
        public string legal_representative { get; set; }
        public string legal_representative_tel { get; set; }
        public string person_liable { get; set; }
        public string person_liable_tel { get; set; }
        public string business_type { get; set; }
        public string business_scope { get; set; }
        public string brand_management { get; set; }
        public object business_level { get; set; }
        public string customer_field { get; set; }
        public object create_user { get; set; }
        public DateTime? create_date { get; set; }
        public object modify_user { get; set; }
        public DateTime? modify_date { get; set; }
        public int is_delete { get; set; }
        public string delete_user { get; set; }
        public DateTime? delete_date { get; set; }


        public object Id { get; set; }

        public object ParentId { get; set; }

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
