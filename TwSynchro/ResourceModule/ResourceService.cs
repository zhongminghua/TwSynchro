using Dapper;
using DapperFactory;
using DapperFactory.Enum;
using Entity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwSynchro.Utils;
using Utils;

namespace TwSynchro.ResourceModule
{
    public class ResourceService
    {
        /// <summary>
        /// 资源同步
        /// </summary>
        /// <param name="_logger"></param>
        public async static void Synchro(ILogger<Worker> _logger)
        {

            _logger.LogInformation($"------同步资源数据开始------");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            string timesTamp = await TimestampHelp.GetTimestampAsync("Resource");

            StringBuilder sql = new($"SELECT * FROM tb_base_masterdata_resource WHERE times_tamp>'{timesTamp}'");

            using var mySqlConn = DbService.GetDbConnection(DBType.MySql, DBLibraryName.Erp_Base);

            _logger.LogInformation($"创建MySql连接 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            //获取要同步的数据
            var readerMultiple = await mySqlConn.QueryMultipleAsync(sql.ToString());

            var ResourceData = readerMultiple.Read<Resource>().ToList();

            sql.Clear();

            //获取customer_live表数据
            sql.Append(@$"SELECT comm_id,customer_id,resource_id FROM tb_base_masterdata_customer_live 
                        WHERE resource_id in (SELECT id FROM tb_base_masterdata_resource WHERE times_tamp>'{timesTamp}') AND first_contact=2");

            var customerLiveMultiple = await mySqlConn.QueryMultipleAsync(sql.ToString());

            var customerLiveData = customerLiveMultiple.Read<(Guid comm_id, Guid customer_id, Guid resource_id)>().ToList();

            _logger.LogInformation($"读取数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            sql.Clear();

            #region 拼装（区域，楼栋，房屋，车位区域，车位）查询语句

            sql.AppendLine("SELECT RegionID,CommID,RegionName,RegionSNum,IsDelete,SynchFlag FROM Tb_HSPR_Region WHERE 1<>1;");

            sql.AppendLine(@"SELECT BuildID,CommID,BuildSign,BuildName,BuildType,BuildUses,PropertyRights        
                , PropertyUses, BuildHeight, FloorsNum, UnitNum, HouseholdsNum, UnderFloorsNum
                , NamingPatterns, BuildSNum, PerFloorNum, RegionSNum, IsDelete, SynchFlag, ConstUnitName FROM Tb_HSPR_Building WHERE 1<>1;");

            sql.AppendLine(@"SELECT RoomID,CommID,RoomSign,RoomName,RegionSNum,BuildSNum,UnitSNum,FloorSNum,RoomSNum,UnitName,FloorName    
                             ,RoomModel,RoomType,PropertyRights,RoomTowards,BuildArea,CalcArea,InteriorArea,CommonArea,CommonLobbyArea,CommonLayerArea    
                             ,RightsSign,PropertyUses,RoomState,ChargeTypeID,UsesState,FloorHeight,BuildStructure,PoolRatio      
                             ,BearParameters,Renovation,Configuration,Advertising,IsDelete,IsSplitUnite      
                             ,GardenArea,PropertyArea,AreaType,YardArea,BedTypeID,UseType,SynchFlag,getHouseStartDate,getHouseEndDate    
                             ,SaleState,PayState,ContSubDate,TakeOverDate,ActualSubDate,FittingTime,StayTime,ContractSign,PayBeginDate
                             ,PayDateChangeMemo,ConstUnitName,LiveStates FROM Tb_HSPR_Room WHERE 1<>1;");

            sql.AppendLine("SELECT [CommID], [CustID], [RoomID], [NewRoomState], [RoomState], [UserCode], [ChangeDate]  FROM Tb_HSPR_RoomStateHis WHERE 1<>1;");

            sql.AppendLine("SELECT CarparkID,CommID,CarparkName,CarparkPosition,IsDelete,SynchFlag FROM Tb_HSPR_Carpark WHERE 1<>1;");

            sql.AppendLine(@"SELECT ParkID,CommID,CustID,RoomID,ParkType,ParkArea,CarparkID,ParkName,ParkingNum,
		                    PropertyRight,StanID,PropertyUses,UseState,IsDelete,ParkCategory,ParkMemo,SynchFlag,
		                    IsPropertyService,DevelopmentSubject,ReserveSale,ParkWriteDate,TakeOverDate FROM Tb_HSPR_Parking WHERE 1<>1;");

            #endregion

            var reader = await sqlServerConn.ExecuteReaderAsync(sql.ToString());

            #region 定义（区域，楼栋，房屋，车位区域，车位）DataTable变量
            //区域
            DataTable dtTb_HSPR_Region = new DataTable("Tb_HSPR_Region");

            dtTb_HSPR_Region.Load(reader);

            //楼栋
            DataTable dtTb_HSPR_Building = new DataTable("Tb_HSPR_Building");

            dtTb_HSPR_Building.Load(reader);

            //房屋
            DataTable dtTb_HSPR_Room = new DataTable("Tb_HSPR_Room");

            dtTb_HSPR_Room.Load(reader);

            //房屋状态变更
            DataTable dtTb_HSPR_RoomStateHis = new DataTable("Tb_HSPR_RoomStateHis");

            dtTb_HSPR_RoomStateHis.Load(reader);

            //车位区域
            DataTable dtTb_HSPR_Carpark = new DataTable("Tb_HSPR_Carpark");

            dtTb_HSPR_Carpark.Load(reader);

            //车位
            DataTable dtTb_HSPR_Parking = new DataTable("Tb_HSPR_Parking");

            dtTb_HSPR_Parking.Load(reader);
            #endregion

            #region 获取本次同步房屋在sqlserver的数据

            string rommIDs = "'00000000-0000-0000-0000-000000000000'";
            foreach (var item in ResourceData.Where(c => c.resource_attr == "1" && c.resource_type == "3"))
            {
                rommIDs += $",'{item.id}'";
            }

            sql.Clear();

            sql.Append($"SELECT RoomID,RoomState FROM Tb_HSPR_Room  WHERE 1=1 AND RoomID IN ({rommIDs})");

            var roomStateData = await sqlServerConn.QueryAsync<(object roomID, string roomState)>(sql.ToString());

            #endregion

            _logger.LogInformation($"读取sqlserver房屋数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            StringBuilder sqlRegionDel = new(), sqlBuildingDel = new(), sqlRoomDel = new(), sqlCarparkDel = new(), sqlParkingDel = new();

            DataRow dr;

            Resource model = new();

            #region 读取当前最大时间戳

            sql.Clear();

            sql.Append("SELECT MAX(times_tamp) times_tamp  FROM tb_base_masterdata_resource");

            var times_tamp = (await mySqlConn.QueryAsync<string>(sql.ToString())).ToList();

            #endregion

            foreach (var Resource in ResourceData)
            {
                //资源属性:房屋
                if (Resource.resource_attr == "1")
                {
                    //资源类别:房屋区域
                    if (Resource.resource_type == "0")
                    {
                        dr = dtTb_HSPR_Region.NewRow();

                        dr["RegionID"] = Resource.id;//组图ID

                        dr["CommID"] = Resource.comm_id;//项目编码

                        dr["RegionName"] = Resource.resource_name;//组图名称

                        //dr["RegionSNum"] = null;//

                        dr["IsDelete"] = Resource.is_delete;//是否删除

                        dr["SynchFlag"] = 0;//

                        dtTb_HSPR_Region.Rows.Add(dr);

                        sqlRegionDel.AppendLine($"DELETE FROM Tb_HSPR_Region WHERE RegionID='{Resource.id}'");

                    }
                    //资源类别:房屋楼栋
                    else if (Resource.resource_type == "1")
                    {
                        dr = dtTb_HSPR_Building.NewRow();

                        dr["BuildID"] = Resource.id;//楼栋ID

                        dr["CommID"] = Resource.comm_id;//项目编码

                        dr["BuildSign"] = Resource.resource_code;//项目编码

                        dr["BuildName"] = Resource.resource_name;//楼栋名称

                        //dr["BuildType"] = null;//楼栋类型

                        //dr["BuildUses"] = null;//

                        //dr["PropertyRights"] = null;//产权性质

                        //dr["PropertyUses"] = null;//

                        //dr["BuildHeight"] = null;//

                        //dr["FloorsNum"] = null;//

                        //dr["UnitNum"] = null;//    

                        //dr["HouseholdsNum"] = null;//   

                        //dr["UnderFloorsNum"] = null;//

                        //dr["NamingPatterns"] = null;//   

                        //dr["BuildSNum"] = null;//   

                        //dr["PerFloorNum"] = null;//

                        dr["RegionSNum"] = Resource.parent_id;//组团ID

                        dr["IsDelete"] = Resource.is_delete;//是否删除

                        dr["SynchFlag"] = 0;//

                        //dr["ConstUnitName"] = null;//

                        dtTb_HSPR_Building.Rows.Add(dr);

                        sqlBuildingDel.AppendLine($"DELETE FROM Tb_HSPR_Building WHERE BuildID='{Resource.id}'");
                    }
                    //资源类别:房屋
                    else if (Resource.resource_type == "3")
                    {
                        dr = dtTb_HSPR_Room.NewRow();

                        //获取所有父级节点
                        var FatherList = GetFatherList(ResourceData, Resource.parent_id.ToString()).ToList();

                        #region 添加行数据
                        dr["RoomID"] = Resource.id;//房屋ID

                        dr["CommID"] = Resource.comm_id;//项目ID

                        dr["RoomSign"] = Resource.resource_code;//房屋编号

                        dr["RoomName"] = Resource.resource_name;//房屋名称

                        //获取区域
                        model = GetResource(FatherList, "1", "0");

                        UtilsDataTable.DataRowIsNull(dr, "RegionSNum", model is null ? "" : model.id);//区域ID

                        //获取楼栋
                        model = GetResource(FatherList, "1", "1");

                        UtilsDataTable.DataRowIsNull(dr, "BuildSNum", model is null ? "" : model.id);//楼栋ID

                        //获取单元
                        model = GetResource(FatherList, "1", "2");

                        dr["UnitSNum"] = model is null ? "1" : model.resource_code;//单元，如果单元信息为空，默认为1

                        UtilsDataTable.DataRowIsNull(dr, "FloorSNum", Resource.floor_sort);//楼层

                        //dr["RoomSNum"] = null;//同单元同楼层流水号

                        dr["UnitName"] = model is null ? "1" : model.resource_name;//单元名称，如果单元信息为空，默认为1

                        dr["FloorName"] = Resource.floor_name;//楼层名称

                        dr["RoomModel"] = Resource.room_model;//房屋户型

                        dr["RoomType"] = Resource.room_model switch
                        {
                            "2" => "1",
                            "1" => "2",
                            "3" => "3",
                            "4" => "4",
                            "5" => "5",
                            _ => ""
                        };//房屋类型

                        //dr["PropertyRights"] = null;//产权性质

                        UtilsDataTable.DataRowIsNull(dr, "RoomTowards", Resource.garden_area);//花园面积

                        UtilsDataTable.DataRowIsNull(dr, "BuildArea", Resource.build_area);//建筑面积

                        UtilsDataTable.DataRowIsNull(dr, "CalcArea", Resource.calc_area);//计费面积

                        UtilsDataTable.DataRowIsNull(dr, "InteriorArea", Resource.interior_area);//套内面积

                        UtilsDataTable.DataRowIsNull(dr, "CommonArea", Resource.common_area);//公摊面积

                        UtilsDataTable.DataRowIsNull(dr, "CommonLobbyArea", Resource.common_lobby_area);//大堂分摊面积

                        UtilsDataTable.DataRowIsNull(dr, "CommonLayerArea", Resource.common_layer_area);//层间分摊面积

                        UtilsDataTable.DataRowIsNull(dr, "RightsSign", Resource.property_card_no);//房屋产权号

                        UtilsDataTable.DataRowIsNull(dr, "PropertyUses", Resource.property_rights);//使用性质

                        UtilsDataTable.DataRowIsNull(dr, "RoomState", Resource.resource_status);//交房状态   ?????

                        //dr["ChargeTypeID"] = "0";//

                        dr["UsesState"] = "0";//使用状态

                        UtilsDataTable.DataRowIsNull(dr, "FloorHeight", Resource.floor_height);//层高

                        //dr["BuildStructure"] = null;//建筑结构

                        UtilsDataTable.DataRowIsNull(dr, "PoolRatio", Resource.pool_ratio);//公摊比率

                        //dr["BearParameters"] = null;//单位面积承重参数

                        //dr["Renovation"] = null;//装修情况

                        //dr["Configuration"] = null;//

                        //dr["Advertising"] = null;//

                        dr["IsDelete"] = Resource.is_delete;//是否删除

                        dr["IsSplitUnite"] = 0;//

                        UtilsDataTable.DataRowIsNull(dr, "GardenArea", Resource.garden_area);//花园面积

                        //dr["PropertyArea"] = null;//

                        //dr["AreaType"] = null;//

                        UtilsDataTable.DataRowIsNull(dr, "YardArea", Resource.yard_area);//地下室面积

                        //dr["BedTypeID"] = null;//

                        //dr["UseType"] = null;//

                        //dr["SynchFlag"] = null;//

                        UtilsDataTable.DataRowIsNull(dr, "getHouseStartDate", Resource.get_house_start_date);//集中交房时间从

                        UtilsDataTable.DataRowIsNull(dr, "getHouseEndDate", Resource.get_house_end_date);//集中交房时间到

                        //dr["SaleState"] = null;//销售状态

                        //dr["PayState"] = null;//交付状态

                        UtilsDataTable.DataRowIsNull(dr, "ContSubDate", Resource.contract_date);//合同交房时间

                        //UtilsDataTable.DataRowIsNull(dr, "TakeOverDate", Resource);//物业接管时间  

                        UtilsDataTable.DataRowIsNull(dr, "ActualSubDate", Resource.actual_sub_date);//实际交房时间

                        UtilsDataTable.DataRowIsNull(dr, "FittingTime", Resource.fitting_time);//装修时间

                        UtilsDataTable.DataRowIsNull(dr, "StayTime", Resource.stay_time);//入住时间

                        UtilsDataTable.DataRowIsNull(dr, "ContractSign", Resource.contract_no);//购房合同号

                        UtilsDataTable.DataRowIsNull(dr, "PayBeginDate", Resource.pay_begin_date);//开始缴费时间

                        //dr["PayDateChangeMemo"] = null;//更改原因

                        //dr["ConstUnitName"] = null;//开发主体

                        //dr["LiveStates"] = null;//入住状态

                        dtTb_HSPR_Room.Rows.Add(dr);

                        #endregion

                        #region 房屋状态发生变更

                        var roomStateModel = roomStateData.Where(c => c.roomID.ToString() == Resource.id.ToString());

                        if (roomStateModel.Count() == 0 || (roomStateModel.Count() > 0 && roomStateModel.FirstOrDefault().roomState != Resource.resource_status))
                        {
                            var customerLiveModel = customerLiveData.Where(c => c.comm_id.ToString() == Resource.comm_id.ToString()
                                                 && c.resource_id.ToString() == Resource.id.ToString());

                            dr = dtTb_HSPR_RoomStateHis.NewRow();

                            dr["CommID"] = Resource.comm_id;//项目编码

                            UtilsDataTable.DataRowIsNull(dr, "CustID", customerLiveModel.Count() > 0 ? customerLiveModel.FirstOrDefault().customer_id : ""); //客户ID(只取第一条)

                            dr["RoomID"] = Resource.id;//房屋ID

                            dr["NewRoomState"] = Resource.resource_status;//新状态

                            UtilsDataTable.DataRowIsNull(dr, "RoomState", roomStateModel.Count() > 0 ? roomStateModel.FirstOrDefault().roomState : "");//旧状态

                            dr["UserCode"] = Resource.modify_user;//修改人

                            UtilsDataTable.DataRowIsNull(dr, "ChangeDate", Resource.modify_date);//修改时间

                            dtTb_HSPR_RoomStateHis.Rows.Add(dr);
                        }

                        #endregion

                        sqlRoomDel.AppendLine($"DELETE FROM Tb_HSPR_Room WHERE RoomID='{Resource.id}'");
                    }
                }
                //资源属性:车位
                else if (Resource.resource_attr == "2")
                {
                    //资源类别:车位区域
                    if (Resource.resource_type == "4")
                    {
                        dr = dtTb_HSPR_Carpark.NewRow();

                        dr["CarparkID"] = Resource.id;//车位区域ID

                        dr["CommID"] = Resource.comm_id;//项目编码

                        dr["CarparkName"] = Resource.resource_name;//车位区域名称

                        //dr["CarparkPosition"] = null;//车位区域名称

                        dr["IsDelete"] = Resource.is_delete;//是否删除

                        dr["SynchFlag"] = 0;//

                        dtTb_HSPR_Carpark.Rows.Add(dr);

                        sqlCarparkDel.AppendLine($"DELETE FROM Tb_HSPR_Carpark WHERE CarparkID='{Resource.id}'");
                    }
                    //资源类别:车位
                    else if (Resource.resource_type == "5")
                    {
                        //获取所有父级节点
                        var FatherList = GetFatherList(ResourceData, Resource.parent_id.ToString()).ToList();

                        dr = dtTb_HSPR_Parking.NewRow();

                        dr["ParkID"] = Resource.id;//车位ID

                        dr["CommID"] = Resource.comm_id;//项目编码

                        dr["CustID"] = Resource.comm_id;//用户ID        ?????

                        //获取房屋
                        model = GetResource(FatherList, "1", "3");

                        UtilsDataTable.DataRowIsNull(dr, "RoomID", model is null ? "" : model.id);//房屋ID

                        UtilsDataTable.DataRowIsNull(dr, "ParkType", Resource.parking_space_form);//车位类型

                        //dr["ParkArea"] = null;//车位面积

                        UtilsDataTable.DataRowIsNull(dr, "CarparkID", Resource.parent_id);//车位区域

                        UtilsDataTable.DataRowIsNull(dr, "ParkName", Resource.resource_code);//车位编号

                        UtilsDataTable.DataRowIsNull(dr, "ParkingNum", Resource.number_parking_spaces);//泊车数量

                        //dr["PropertyRight"] = null;//产权性质

                        //dr["StanID"] = null;//收费标准

                        UtilsDataTable.DataRowIsNull(dr, "PropertyUses", Resource.use_parking_space);//使用状态

                        dr["UseState"] = "闲置";//使用状态

                        dr["IsDelete"] = Resource.is_delete;//是否删除

                        dr["ParkCategory"] = "0";//车位类别           ?????

                        //dr["ParkMemo"] = null;//备注

                        dr["SynchFlag"] = 0;//

                        dr["IsPropertyService"] = 0;//

                        //dr["DevelopmentSubject"] = null;//开发主体

                        //dr["ReserveSale"] = null;//地产预留出售

                        UtilsDataTable.DataRowIsNull(dr, "ParkWriteDate", Resource.create_date);//添加时间

                        //dr["TakeOverDate"] = null;//物业接管时间

                        dtTb_HSPR_Parking.Rows.Add(dr);

                        sqlParkingDel.AppendLine($"DELETE FROM Tb_HSPR_Parking WHERE ParkID='{Resource.id}'");
                    }
                }

            }

            _logger.LogInformation($"生成资源数据 耗时{stopwatch.ElapsedMilliseconds}毫秒!");

            stopwatch.Restart();

            ResultMessage resultMessage = new();

            #region 保存数据

            resultMessage = await SynchroRegion(sqlRegionDel.ToString(), dtTb_HSPR_Region);

            _logger.LogInformation($"插入区域数据 耗时{stopwatch.ElapsedMilliseconds}毫秒! {resultMessage.Message}");

            stopwatch.Restart();

            resultMessage = await SynchroBuilding(sqlBuildingDel.ToString(), dtTb_HSPR_Building);

            _logger.LogInformation($"插入楼栋数据 耗时{stopwatch.ElapsedMilliseconds}毫秒! {resultMessage.Message}");

            stopwatch.Restart();

            resultMessage = await SynchroRoom(sqlRoomDel.ToString(), dtTb_HSPR_Room, dtTb_HSPR_RoomStateHis);

            _logger.LogInformation($"插入房屋数据 耗时{stopwatch.ElapsedMilliseconds}毫秒! {resultMessage.Message}");

            stopwatch.Restart();

            resultMessage = await SynchroCarpark(sqlCarparkDel.ToString(), dtTb_HSPR_Carpark);

            _logger.LogInformation($"插入车位区域数据 耗时{stopwatch.ElapsedMilliseconds}毫秒! {resultMessage.Message}");

            stopwatch.Restart();

            resultMessage = await SynchroParking(sqlParkingDel.ToString(), dtTb_HSPR_Parking);

            _logger.LogInformation($"插入车位数据 耗时{stopwatch.ElapsedMilliseconds}毫秒! {resultMessage.Message}");

            stopwatch.Restart();

            #endregion

            //保存时间戳
            TimestampHelp.SetTimestampAsync("Resource", times_tamp[0], 180);

            _logger.LogInformation($"------同步资源数据结束------");

        }

        /// <summary>
        /// 保存区域
        /// </summary>
        /// <param name="sql">删除区域sql</param>
        /// <param name="dtTb_HSPR_Region">区域数据</param>
        /// <returns></returns>
        public static async Task<ResultMessage> SynchroRegion(string sql, DataTable dtTb_HSPR_Region)
        {

            ResultMessage resultMessage = new();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            using var trans = sqlServerConn.OpenTransaction();

            try
            {
                if (!string.IsNullOrEmpty(sql))
                    await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                //await DbBatch.InsertSingleTable(sqlServerConn, dtTb_HSPR_Region, "Tb_HSPR_Region", trans);

                trans.Commit();

                resultMessage.SetSuccessResultMessage();

            }
            catch (Exception ex)
            {
                resultMessage.Message = $"{ex.Message}{ex.StackTrace}";

                trans.Rollback();
            }

            return resultMessage;
        }

        /// <summary>
        /// 保存楼栋
        /// </summary>
        /// <param name="sql">删除楼栋sql</param>
        /// <param name="dtTb_HSPR_Building">楼栋数据</param>
        /// <returns></returns>
        public static async Task<ResultMessage> SynchroBuilding(string sql, DataTable dtTb_HSPR_Building)
        {

            ResultMessage resultMessage = new();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            using var trans = sqlServerConn.OpenTransaction();

            try
            {
                if (!string.IsNullOrEmpty(sql))
                    await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                //await DbBatch.InsertSingleTable(sqlServerConn, dtTb_HSPR_Building, "Tb_HSPR_Building", trans);

                trans.Commit();

                resultMessage.SetSuccessResultMessage();

            }
            catch (Exception ex)
            {
                resultMessage.Message = $"{ex.Message}{ex.StackTrace}";

                trans.Rollback();
            }

            return resultMessage;
        }

        /// <summary>
        /// 保存房屋
        /// </summary>
        /// <param name="sql">删除房屋sql</param>
        /// <param name="dtTb_HSPR_Room">房屋数据</param>
        /// <param name="dtTb_HSPR_RoomStateHis">房屋历史状态变更数据</param>
        /// <returns></returns>
        public static async Task<ResultMessage> SynchroRoom(string sql, DataTable dtTb_HSPR_Room, DataTable dtTb_HSPR_RoomStateHis)
        {

            ResultMessage resultMessage = new();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            using var trans = sqlServerConn.OpenTransaction();

            try
            {
                if (!string.IsNullOrEmpty(sql))
                    await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                //await DbBatch.InsertSingleTable(sqlServerConn, dtTb_HSPR_Room, "Tb_HSPR_Room", trans);

                //await DbBatch.InsertSingleTable(sqlServerConn, dtTb_HSPR_RoomStateHis, "Tb_HSPR_RoomStateHis", trans);

                trans.Commit();

                resultMessage.SetSuccessResultMessage();

            }
            catch (Exception ex)
            {
                resultMessage.Message = $"{ex.Message}{ex.StackTrace}";

                trans.Rollback();
            }

            return resultMessage;
        }

        /// <summary>
        /// 保存车位区域
        /// </summary>
        /// <param name="sql">删除车位区域sql</param>
        /// <param name="dtTb_HSPR_Carpark">车位区域数据</param>
        /// <returns></returns>
        public static async Task<ResultMessage> SynchroCarpark(string sql, DataTable dtTb_HSPR_Carpark)
        {

            ResultMessage resultMessage = new();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            using var trans = sqlServerConn.OpenTransaction();

            try
            {
                if (!string.IsNullOrEmpty(sql))
                    await sqlServerConn.ExecuteAsync(sql.ToString(), transaction: trans);

                //await DbBatch.InsertSingleTable(sqlServerConn, dtTb_HSPR_Carpark, "Tb_HSPR_Carpark", trans);

                trans.Commit();

                resultMessage.SetSuccessResultMessage();

            }
            catch (Exception ex)
            {
                resultMessage.Message = $"{ex.Message}{ex.StackTrace}";

                trans.Rollback();
            }

            return resultMessage;
        }

        /// <summary>
        /// 保存车位
        /// </summary>
        /// <param name="sql">删除车位sql</param>
        /// <param name="dtTb_HSPR_Parking">车位数据</param>
        /// <returns></returns>
        public static async Task<ResultMessage> SynchroParking(string sql, DataTable dtTb_HSPR_Parking)
        {

            ResultMessage resultMessage = new();

            using var sqlServerConn = DbService.GetDbConnection(DBType.SqlServer, DBLibraryName.PMS_Base);

            using var trans = sqlServerConn.OpenTransaction();

            try
            {
                if (!string.IsNullOrEmpty(sql))
                    await sqlServerConn.ExecuteAsync(sql, transaction: trans);

                //await DbBatch.InsertSingleTable(sqlServerConn, dtTb_HSPR_Parking, "Tb_HSPR_Parking", trans);

                trans.Commit();

                resultMessage.SetSuccessResultMessage();

            }
            catch (Exception ex)
            {
                resultMessage.Message = $"{ex.Message}{ex.StackTrace}";

                trans.Rollback();
            }

            return resultMessage;
        }

        /// <summary>
        /// 查询对应实体
        /// </summary>
        /// <param name="list">集合</param>
        /// <param name="esource_attr">资源属性 1：房屋，2：车位</param>
        /// <param name="resource_type">资源类别， 0：房屋区域，1：房屋楼栋，2：房屋单元，3：房屋，4：车位区域，5：车位</param>
        /// <returns></returns>
        public static Resource GetResource(IList<Resource> list, string esource_attr, string resource_type)
        {
            return list.Where(c => c.resource_attr.ToString() == esource_attr && c.resource_type.ToString() == resource_type).FirstOrDefault();
        }

        /// <summary>
        /// 查询所有父级节点
        /// </summary>
        /// <param name="list">集合</param>
        /// <param name="parent_id">父级ID</param>
        /// <returns></returns>
        public static IEnumerable<Resource> GetFatherList(IList<Resource> list, string parent_id)
        {
            var query = list.Where(p => p.id.ToString() == parent_id).ToList();

            return query.Concat(query.SelectMany(t => GetFatherList(list, t.parent_id.ToString())));
        }

    }
}
