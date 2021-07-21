using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class Resource
    {
        /// <summary>
        /// 资源信息id
        /// </summary>
        public object id { get; set; }

        /// <summary>
        /// 项目编码
        /// </summary>
        public object comm_id { get; set; }

        /// <summary>
        /// 资源属性 1：房屋，2：车位
        /// </summary>
        public string resource_attr { get; set; }

        /// <summary>
        /// 资源分组
        /// </summary>
        public string resource_group { get; set; }

        /// <summary>
        /// 父级id
        /// </summary>
        public object parent_id { get; set; }

        /// <summary>
        /// 资源类别， 0：房屋区域，1：房屋楼栋，2：房屋单元，3：房屋，4：车位区域，5：车位
        /// </summary>
        public string resource_type { get; set; }

        /// <summary>
        /// 楼层序号(只能输入正负整数)
        /// </summary>
        public string floor_sort { get; set; }

        /// <summary>
        /// 楼层名称(默认楼层序号，可改)
        /// </summary>
        public string floor_name { get; set; }

        /// <summary>
        /// 资源类型，房屋时为：1：自持房屋，2：销售房屋，3：物业用房，4：公共区域，5：虚拟房屋，车位时为：
        /// 6：自持车位，7：销售车位，8：长期使用权车位，9：人防车位，10：自划车位，11：地面车位，12：虚拟车位
        /// </summary>
        public string resource_class { get; set; }

        /// <summary>
        /// 资源编号
        /// </summary>
        public string resource_code { get; set; }

        /// <summary>
        /// 资源名称
        /// </summary>
        public string resource_name { get; set; }

        /// <summary>
        /// 建筑面积（平米）
        /// </summary>
        public string build_area { get; set; }

        /// <summary>
        /// 计费面积（平米）
        /// </summary>
        public string calc_area { get; set; }

        /// <summary>
        /// 交付状态，固定选项：1.自持、2.待售、3.已售未收、4.已收、5.已收未装、6.已收装修、7.已装未住、8.入住，
        /// </summary>
        public string resource_status { get; set; }

        /// <summary>
        /// 入住状态： 常驻， 非常驻
        /// </summary>
        public string live_status { get; set; }

        /// <summary>
        /// 产权证号
        /// </summary>
        public string property_card_no { get; set; }

        /// <summary>
        /// 合同编号
        /// </summary>
        public string contract_no { get; set; }

        /// <summary>
        /// 合同交付时间
        /// </summary>
        public string contract_date { get; set; }

        /// <summary>
        /// 集中交付开始时间
        /// </summary>
        public string get_house_start_date { get; set; }

        /// <summary>
        /// 集中交付结束时间
        /// </summary>
        public string get_house_end_date { get; set; }

        /// <summary>
        /// 实际交付时间
        /// </summary>
        public string actual_sub_date { get; set; }

        /// <summary>
        /// 装修属性，毛坯/简装/精装/豪装
        /// </summary>
        public string fitting_reno { get; set; }

        /// <summary>
        /// 装修时间
        /// </summary>
        public string fitting_time { get; set; }

        /// <summary>
        /// 入住时间
        /// </summary>
        public string stay_time { get; set; }

        /// <summary>
        /// 开始计费时间
        /// </summary>
        public string pay_begin_date { get; set; }

        /// <summary>
        /// 管家人员id
        /// </summary>
        public object house_keeper { get; set; }

        /// <summary>
        /// 房屋套内面积（平米）
        /// </summary>
        public string interior_area { get; set; }

        /// <summary>
        /// 房屋花园面积（平米）
        /// </summary>
        public string garden_area { get; set; }

        /// <summary>
        /// 房屋地下室面积（平米）
        /// </summary>
        public string yard_area { get; set; }

        /// <summary>
        /// 房屋公摊面积（平米）
        /// </summary>
        public string common_area { get; set; }

        /// <summary>
        /// 房屋大堂公摊面积（平米）
        /// </summary>
        public string common_lobby_area { get; set; }

        /// <summary>
        /// 房屋层间公摊面积（平米）
        /// </summary>
        public string common_layer_area { get; set; }

        /// <summary>
        /// 房屋公摊比率（%）
        /// </summary>
        public string pool_ratio { get; set; }

        /// <summary>
        /// 房屋户型
        /// </summary>
        public string room_model { get; set; }

        /// <summary>
        /// 房屋层高（米）
        /// </summary>
        public string floor_height { get; set; }

        /// <summary>
        /// 房屋使用性质
        /// </summary>
        public string property_rights { get; set; }

        /// <summary>
        /// 车位形态	固定选项：标准车位/子母车位/豪华车位/机械车位/充电车位/无障碍车位/不合规车位，
        /// </summary>
        public string parking_space_form { get; set; }

        /// <summary>
        /// 车位停车数量
        /// </summary>
        public string number_parking_spaces { get; set; }

        /// <summary>
        /// 车位用途	固定选项：自用/月租或临停，
        /// </summary>
        public string use_parking_space { get; set; }

        /// <summary>
        /// 拆分合并状态 (0未处理，1被拆分，2已拆分，3被合并，4已合并)
        /// </summary>
        public string is_split_merge { get; set; }

        /// <summary>
        /// 车位绑定标准id（房屋不允许填充次字段）
        /// </summary>
        public string binding_stan_id { get; set; }

        /// <summary>
        /// 绑定人
        /// </summary>
        public string binding_user { get; set; }

        /// <summary>
        /// 绑定时间
        /// </summary>
        public string binding_date { get; set; }

        /// <summary>
        /// 资源自定义字段，根据房屋/车位类型自动判别
        /// </summary>
        public string custom_field { get; set; }

        /// <summary>
        /// 发起人
        /// </summary>
        public string create_user { get; set; }

        /// <summary>
        /// 发起时间
        /// </summary>
        public string create_date { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string modify_user { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public string modify_date { get; set; }

        /// <summary>
        /// 记录是否删除状态
        /// </summary>
        public string is_delete { get; set; }

        /// <summary>
        /// 删除人
        /// </summary>
        public string delete_user { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        public string delete_date { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public string deal_type { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public string deal_date { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string deal_user { get; set; }

        /// <summary>
        /// 管家移动电话
        /// </summary>
        public string mobile { get; set; }

    }
}
