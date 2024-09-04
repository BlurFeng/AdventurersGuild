using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deploy;
using com.ootii.Messages;
using Google.Protobuf.Collections;
using System.Text;
using FsSearchPathSystem;

namespace WorldMap
{
    /// <summary>
    /// 世界地图模块
    /// </summary>
    public class WorldMapModel : Singleton<WorldMapModel>, IDestroy, ISaveData
    {
        Dictionary<int, Place> m_PlacesDic;//Key=配置表地点ID
        Dictionary<int, Road> m_RoadsDic;//Key=配置表道路ID

        public override void Init()
        {
            base.Init();

            m_PlacesDic = new Dictionary<int, Place>();
            m_RoadsDic = new Dictionary<int, Road>();

            InitDigraphAndDSP();
            InitTravel();

            MessageDispatcher.AddListener(TimeModelMsgType.TIMEMODEL_DAYCOUNT_CHANGE, OnExecuteFinishTurn);
        }

        public override void Destroy()
        {
            base.Destroy();

            MessageDispatcher.RemoveListener(TimeModelMsgType.TIMEMODEL_DAYCOUNT_CHANGE, OnExecuteFinishTurn);
        }

        private void OnExecuteFinishTurn(IMessage rMessage)
        {
            OnExecuteFinishTurnTravel();
        }

        #region ISaveData

        [ES3Serializable]
        public class WorldMapData
        {
            public TravelData travelData;

            public WorldMapData()
            {
                travelData = new TravelData();
            }
        }

        [ES3Serializable]
        public class TravelData
        {
            //旅行容器生成时获取ID 优先获取未使用的 销毁时返还ID记录在未使用ID列表

            public Dictionary<int,TravelContainer> travelContainerMap;//所有旅行容器
            public List<int> travelIdUnusedList;//未使用的旅行容器ID
            public int travelIdCounter;//旅行容器ID计数器

            public TravelData()
            {
                travelContainerMap = new Dictionary<int, TravelContainer>();
                travelIdUnusedList = new List<int>();
            }
        }

        const string m_SaveDataKey = "WorldMapModel_WorldMapData";

        public void SaveData(ES3File saveData)
        {
            WorldMapData worldMapData = new WorldMapData();

            SaveDataTravel(worldMapData);

            saveData.Save(m_SaveDataKey, worldMapData);
        }

        public void LoadData(ES3File saveData)
        {
            WorldMapData worldMapData = null;
            //20220528 Winhoo :TODO 世界地图模块暂时没用，有需要在处理存档数据
            if(false/*saveData.KeyExists(m_SaveDataKey)*/)
            {
                //saveData.LoadInto<WorldMapData>("WorldMapModel_WorldMapData", worldMapData);
            }
            else
            {
                worldMapData = new WorldMapData();
            }

            LoadDataTravel(worldMapData);
        }
        #endregion

        /// <summary>
        /// 获取地点
        /// </summary>
        /// <param name="placeId"></param>
        /// <param name="outPlace"></param>
        /// <returns></returns>
        public bool GetPlace(int placeId, out Place outPlace)
        {
            outPlace = null;
            if (!m_PlacesDic.ContainsKey(placeId)) return false;

            outPlace = m_PlacesDic[placeId];

            return true;
        }

        /// <summary>
        /// 获取路
        /// </summary>
        /// <param name="roadId"></param>
        /// <param name="outRoad"></param>
        /// <returns></returns>
        public bool GetRoad(int roadId, out Road outRoad)
        {
            outRoad = null;
            if (!m_RoadsDic.ContainsKey(roadId)) return false;

            outRoad = m_RoadsDic[roadId];

            return true;
        }

        public bool GetPathRoad(int startPlaceID, int endPlaceId, out Road[] outRoads)
        {
            outRoads = null;
            if (!GetPathRoadId(startPlaceID, endPlaceId, out int[] roadIds)) return false;

            outRoads = new Road[roadIds.Length];
            for (int i = 0; i < roadIds.Length; i++)
            {
                outRoads[i] = m_RoadsDic[roadIds[i]];
            }

            return true;
        }

        #region Digraph and DSP 有向图和寻路

        //基于加权边有向图和dijkstra的寻路

        //加权边有向图寻路系统
        EWDSearchPath m_EWDSearchPath;
        Dictionary<int, int> m_PlaceVertexIdMapDic;//Key=配置表地点ID Vaule=顶点
        Dictionary<int, int> m_VertexPlaceMapDic;//Key=顶点 Vaule=地点ID
        Dictionary<int, int> m_RoadEdgeIdMapDic;//Key=配置表道路ID Vaule=有向边ID
        Dictionary<int, int> m_EdgeRoadIdMapDic;//Key=有向边ID Vaule=配置表道路ID

        #region public
        /// <summary>
        /// 获取路径
        /// </summary>
        /// <param name="startPlaceID">起点地点Id</param>
        /// <param name="endPlaceId">终点地点Id</param>
        /// <param name="outPlaceIds">途经的地点Id（不包括起点 包括终点）</param>
        /// <returns>是否有可到达的路径</returns>
        public bool GetPathPlaceID(int startPlaceID, int endPlaceId, out int[] outPlaceIds)
        {
            outPlaceIds = null;

            //确认输入的地点ID有效性 (是否有对应的顶点)
            if (!m_PlaceVertexIdMapDic.ContainsKey(startPlaceID) || !m_PlaceVertexIdMapDic.ContainsKey(endPlaceId)) return false;

            int start = m_PlaceVertexIdMapDic[startPlaceID];//起点顶点
            int end = m_PlaceVertexIdMapDic[endPlaceId];//终点顶点

            if (!m_EWDSearchPath.GetPathVertexs(start, end, out int[] vertexs)) return false;//获取路径

            outPlaceIds = new int[vertexs.Length];
            //将顶点路径转换为 地点ID路径
            for (int i = 0; i < vertexs.Length; i++)
            {
                int vertex = vertexs[i];

                outPlaceIds[i] = m_PlaceVertexIdMapDic[vertex];
            }

            return true;
        }

        public bool GetPathRoadId(int startPlaceID, int endPlaceId, out int[] outRoadIds)
        {
            outRoadIds = null;

            //确认输入的地点ID有效性 (是否有对应的顶点)
            if (!m_PlaceVertexIdMapDic.ContainsKey(startPlaceID) || !m_PlaceVertexIdMapDic.ContainsKey(endPlaceId)) return false;

            int start = m_PlaceVertexIdMapDic[startPlaceID];//起点顶点
            int end = m_PlaceVertexIdMapDic[endPlaceId];//终点顶点

            if (!m_EWDSearchPath.GetPathEdges(start, end, out int[] edgeIds)) return false;//获取路径

            outRoadIds = new int[edgeIds.Length];
            //将顶点路径转换为 地点ID路径
            for (int i = 0; i < edgeIds.Length; i++)
            {
                int edgeId = edgeIds[i];

                outRoadIds[i] = m_EdgeRoadIdMapDic[edgeId];
            }

            return true;
        }
        #endregion

        /// <summary>
        /// 初始化寻路功能
        /// </summary>
        private void InitDigraphAndDSP()
        {
            //初始化映射Dic
            m_PlaceVertexIdMapDic = new Dictionary<int, int>();
            m_VertexPlaceMapDic = new Dictionary<int, int>();
            m_RoadEdgeIdMapDic = new Dictionary<int, int>();
            m_EdgeRoadIdMapDic = new Dictionary<int, int>();

            var places = ConfigSystem.Instance.GetConfigMap<WorldMap_Place>();
            m_EWDSearchPath = new EWDSearchPath();
            m_EWDSearchPath.Init(places.Count);

            //生成地点ID对寻路有向图VertexNum的映射Map
            int vertexNum = 0;
            foreach (var placeInfo in places.Values)
            {
                //添加ID和vertexNum的映射
                m_PlaceVertexIdMapDic.Add(placeInfo.Id, vertexNum);
                m_VertexPlaceMapDic.Add(vertexNum, placeInfo.Id);

                Place place = new Place(placeInfo.Id);//Place动态数据
                m_PlacesDic.Add(placeInfo.Id, place);

                vertexNum++;
            }

            //添加有向边 添加的边都是以自身为起点的
            foreach (var place in m_PlacesDic.Values)
            {
                GetVertexNumByPlaceID(place.ID, out vertexNum);
                var roadIds = place.GetRoadIds();//获取路径
                for (int i = 0; i < roadIds.Length; i++)
                {
                    int roadId = roadIds[i];

                    Road Road = new Road(roadId);//生成路径动态数据
                    m_RoadsDic.Add(roadId, Road);

                    GetVertexNumByPlaceID(Road.GetDestinationPlaceID(), out int toVertexNum);
                    AddEdge(roadId, vertexNum, toVertexNum, Road.GetWeight());//添加有向边

                    //设置其他信息
                    Road.SetInfo(m_VertexPlaceMapDic[vertexNum], m_VertexPlaceMapDic[toVertexNum]);
                }
            }
        }

        //添加一条边 并添加到映射字典
        private void AddEdge(int roadId,int from, int to, int weight)
        {
            int edgeId = m_EWDSearchPath.AddEdge(from, to, weight);
            m_RoadEdgeIdMapDic.Add(roadId, edgeId);
            m_EdgeRoadIdMapDic.Add(edgeId, roadId);
        }

        //获取地点ID映射的顶点
        private bool GetVertexNumByPlaceID(int Id, out int outVertexNum)
        {
            outVertexNum = 0;
            if (!m_PlaceVertexIdMapDic.ContainsKey(Id)) return false;

            outVertexNum = m_PlaceVertexIdMapDic[Id];

            return true;
        }

        /// <summary>
        /// 改变某条路对应的有向边权重
        /// </summary>
        /// <param name="roadId"></param>
        private void ChangeEdgeWeight(int roadId)
        {
            //获取edge对应的ID 和Road
            if (!m_RoadEdgeIdMapDic.ContainsKey(roadId) || !GetRoad(roadId, out Road road)) return;

            m_EWDSearchPath.ChangeEdgeWeight(m_RoadEdgeIdMapDic[roadId], road.GetWeight());
        }

#if DEBUG
        /// <summary>
        /// 打印从某个地点出发到所有可到达地点的路径
        /// </summary>
        /// <param name="placeId">地点ID</param>
        /// <returns></returns>
        public void PrintPlaceRoads(int placeId)
        {
            if (!GetVertexNumByPlaceID(placeId, out int start)) return;

            m_EWDSearchPath.PrintPlacePath(start, m_VertexPlaceMapDic);

            //var edgeId = m_EWDSearchPath.AddEdge(0, 4, 10);

            //m_EWDSearchPath.PrintPlacePath(start, m_VertexPlaceMapDic);

            //m_EWDSearchPath.ChangeEdgeWeight(m_RoadEdgeIdMapDic[100102], 1);
            //m_EWDSearchPath.ChangeEdgeWeight(m_RoadEdgeIdMapDic[100303], 1);

            //m_EWDSearchPath.PrintPlacePath(start, m_VertexPlaceMapDic);
        }
#endif
        #endregion

        #region Travel 旅行移动

        /// <summary>
        /// 交通工具
        /// </summary>
        public enum VehicleType
        {
            None,

            /// <summary>
            /// 步行
            /// </summary>
            Walk,

            /// <summary>
            /// 马匹
            /// </summary>
            Horse,

            /// <summary>
            /// 马车
            /// </summary>
            Carriage,
        }

        [ES3Serializable]
        public sealed partial class TravelContainer
        {
            public int id;//自身对应ID
            public int vehicleId;//交通工具ID 影响交通工具类型 移动能力基数等
            public int placeIdCur;//当前所在地点ID
            public int roadIdCur;//点前所在道路ID
            public int roadSpotCur;//当前所在道路点位 0-道路长度（权重值）
            public int targetPlaceId;//目标地点ID

            private VehicleType m_Vehicle;
            /// <summary>
            /// 交通工具
            /// </summary>
            public VehicleType Vehicle { get { return m_Vehicle; } }
            private int m_MovePointBase;
            /// <summary>
            /// 移动点数基数
            /// </summary>
            public int MovePointBase { get { return m_MovePointBase; } }

            //到达目的地的路线
            private Road[] m_Roads;
            private int m_RoadsIndexCur;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="Id">自身Id</param>
            /// <param name="vehicleId">交通工具配置Id</param>
            /// <param name="placeId">所在地点Id</param>
            /// <param name="roadId">所在路线Id</param>
            /// <param name="roadSpotCur">所在路线位置点</param>
            public TravelContainer(int Id, int vehicleId, int placeId, int roadId, int roadSpotCur)
            {
                this.id = Id;
                this.vehicleId = vehicleId;
                this.placeIdCur = placeId;
                this.roadIdCur = roadId;
                this.roadSpotCur = roadSpotCur;

                Init(vehicleId);
            }

            /// <summary>
            /// 初始化
            /// </summary>
            /// <param name="vehicleId">交通工具ID</param>
            public void Init(int vehicleId)
            {
                //根据交通工具ID获取更多信息
                WorldMap_Vehicle vehicleConfig = ConfigSystem.Instance.GetConfig<WorldMap_Vehicle>(vehicleId);
                if (vehicleConfig != null)
                {
                    m_Vehicle = (VehicleType)vehicleConfig.Type;
                    m_MovePointBase = vehicleConfig.MovePointBase;
                }

                //目的地Id不为空 获取更多移动需要信息
                if (targetPlaceId != 0)
                {
                    SetTargetPlace(targetPlaceId);
                }
            }

            public void Destroy()
            {

            }

            /// <summary>
            /// 设置目的地
            /// </summary>
            /// <param name="placeId">地点Id</param>
            /// <returns></returns>
            public bool SetTargetPlace(int placeId)
            {
                if (!WorldMapModel.Instance.GetPathRoad(placeIdCur, placeId, out Road[] roads)) return false;

                targetPlaceId = placeId;
                m_Roads = roads;
                m_RoadsIndexCur = 0;
                roadIdCur = m_Roads[m_RoadsIndexCur].ID;

                //根据持久化的RoadSpotCur数据 设置具体位置

                return true;
            }

            /// <summary>
            /// 前进一次
            /// </summary>
            public bool MoveForward()
            {
                if (targetPlaceId == 0) return false;

                Road road = m_Roads[m_RoadsIndexCur];
                int movePoint = WorldMapModel.Instance.CalculateMovePoint(this, road);
                if (movePoint == 0) return false;

                roadSpotCur += movePoint;

                //到达下一个地点
                if (roadSpotCur >= road.Length)
                {
                    placeIdCur = road.ToPlaceId;//更新当前所在地点
                    m_RoadsIndexCur++;

                    if (m_RoadsIndexCur < m_Roads.Length)
                    {
                        roadIdCur = m_Roads[m_RoadsIndexCur].ID;
                        roadSpotCur -= road.Length;
                    }
                    //到达最终目的地
                    else
                    {
                        targetPlaceId = 0;
                        m_Roads = null;
                        m_RoadsIndexCur = 0;
                        roadSpotCur = 0;
                        roadIdCur = 0;

                        Debug.Log("到达目的地！！");
                    }
                }

                Debug.Log(string.Format("旅行容器前进！ 旅行容器ID:{0}; 当前所在地点：{1}; 当前所在道路：{2};当前道路长度：{3}; 当前所在道路地点：{4}; 目的地：{5}; ", id, placeIdCur, roadIdCur, road.Length, roadSpotCur, targetPlaceId));

                return true;
            }

            /// <summary>
            /// 获取当前位置
            /// </summary>
            /// <param name="PlaceOrRoad">在地点或者道路上</param>
            /// <param name="id">地点或道路的ID</param>
            /// <returns></returns>
            public bool GetPositionCurrent(out TravelStayInfo outTravelPositionInfo)
            {
                outTravelPositionInfo = new TravelStayInfo();
                if (placeIdCur == 0 && roadIdCur == 0) return false;

                //在某个道路
                if (roadIdCur != 0 && roadSpotCur != 0)
                {
                    outTravelPositionInfo = new TravelStayInfo(
                        id,
                        false,
                        roadIdCur,
                        roadSpotCur
                        );
                }
                //在某个地点
                else
                {
                    outTravelPositionInfo = new TravelStayInfo(
                        id,
                        true,
                        placeIdCur,
                        0
                        );
                }

                return true;
            }

            /// <summary>
            /// 旅行位置信息
            /// </summary>
            public struct TravelStayInfo
            {
                /// <summary>
                /// 旅行容器Id
                /// </summary>
                public int travelId;

                /// <summary>
                /// 在地点或道路上
                /// </summary>
                public bool inPlaceOrRoad;

                /// <summary>
                /// 地点或道路的ID
                /// </summary>
                public int placeOrRoadId;

                /// <summary>
                /// 道路点位
                /// </summary>
                public int roadSpotCur;

                public TravelStayInfo(int travelId, bool inPlaceOrRoad, int placeOrRoadId, int roadSpotCur)
                {
                    this.travelId = travelId;
                    this.inPlaceOrRoad = inPlaceOrRoad;
                    this.placeOrRoadId = placeOrRoadId;
                    this.roadSpotCur = roadSpotCur;
                }
            }
        }


        private TravelData m_TravelData;

        public void SaveDataTravel(WorldMapData worldMapData)
        {
            worldMapData.travelData = m_TravelData;
        }

        public void LoadDataTravel(WorldMapData worldMapData)
        {
            if (worldMapData != null && worldMapData.travelData != null)
            {
                m_TravelData = worldMapData.travelData;
            }
            else
            {
                m_TravelData = new TravelData();
            }
            if (m_TravelData == null) m_TravelData = new TravelData();

            //初始化旅行容器 (只存储了必要的信息 之后通过必要信息初始化）
            foreach (var item in m_TravelData.travelContainerMap.Values)
            {
                item.Init(item.vehicleId);
            }
        }

        /// <summary>
        /// 初始化旅行功能
        /// </summary>
        private void InitTravel()
        {
            //数据由LoadData初始化

            //Test
            //CreateTravelContainerSimple();
        }

        private void OnExecuteFinishTurnTravel()
        {
            foreach (var item in m_TravelData.travelContainerMap.Values)
            {
                //Test
                //if (item.TargetPlaceId == 0)
                //    item.SetTargetPlace(100500);

                item.MoveForward();//进行移动

                //当前所在位置
                if(item.GetPositionCurrent(out TravelContainer.TravelStayInfo outTravelPositionInfo))
                {
                    MessageDispatcher.SendMessageData(WorldMapModelMsgType.WORLDMAP_TRAVEL_STAY, outTravelPositionInfo);
                }
            }
        }

        public void CreateTravelContainerSimple()
        {
            CreateTravelContainer(101, 100100);
        }

        /// <summary>
        /// 创建一个新的旅行容器
        /// </summary>
        /// <param name="vehicleId">交通工具ID</param>
        /// <param name="placeId">地点Id</param>
        /// <param name="roadId">道路Id</param>
        /// <param name="roadSpotCur">道路点位</param>
        /// <returns></returns>
        public int CreateTravelContainer(int vehicleId, int placeId, int roadId = -1, int roadSpotCur = -1)
        {
            int Id;
            //获取未使用的Id
            if(m_TravelData.travelIdUnusedList.Count > 0)
            {
                int getIndex = m_TravelData.travelIdUnusedList.Count - 1;
                Id = m_TravelData.travelIdUnusedList[getIndex];
                m_TravelData.travelIdUnusedList.RemoveAt(getIndex);
            }
            //新增Id
            else
            {
                m_TravelData.travelIdCounter++;
                Id = m_TravelData.travelIdCounter;
            }

            //生成新的旅行容器
            TravelContainer tc = new TravelContainer(Id, vehicleId, placeId, roadId, roadSpotCur);
            m_TravelData.travelContainerMap.Add(Id, tc);

            return Id;
        }

        /// <summary>
        /// 销毁某个旅行容器
        /// </summary>
        /// <param name="travelId"></param>
        public void DestroyTravelContainer(int travelId)
        {
            if (m_TravelData.travelContainerMap.ContainsKey(travelId)) return;

            //调用销毁并从Dic移除
            m_TravelData.travelContainerMap[travelId].Destroy();
            m_TravelData.travelContainerMap.Remove(travelId);

            //添加Id到未使用Id列表
            m_TravelData.travelIdUnusedList.Add(travelId);
        }

        /// <summary>
        /// 计算某个交通工具在道路上的行驶点数
        /// </summary>
        /// <param name="tc"></param>
        /// <param name="road"></param>
        public int CalculateMovePoint(TravelContainer tc, Road road)
        {
            if (tc == null || road == null) return 0;

            //Tips:不根据交通工具类型影响移动点数的计算 否则需要为不同的交通工具类型准备不同的加权有向图和DSP(即对于不同的交通工具类型来说 每条路的权重不一样
            //之后可以考虑做
            
            int movePoint = Mathf.FloorToInt(tc.MovePointBase * road.GetRoadSurfaceCoefficient());

            return movePoint;
        }

        #endregion
    }
}