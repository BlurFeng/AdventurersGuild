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
    /// 路线
    /// </summary>
    public class Road
    {
        public Road(int Id)
        {
            this.ID = Id;
        }

        public void SetInfo(int fromPlaceId, int toPlaceId)
        {
            FromPlaceId = fromPlaceId;
            ToPlaceId = toPlaceId;
        }

        public int ID { get; private set; }

        /// <summary>
        /// 起点地点Id
        /// </summary>
        public int FromPlaceId { get; private set; }

        /// <summary>
        /// 终点地点Id
        /// </summary>
        public int ToPlaceId { get; private set; }

        private WorldMap_Road m_RoadConfig;
        /// <summary>
        /// 路线配置信息
        /// </summary>
        public WorldMap_Road RoadConfig
        {
            get
            {
                if(m_RoadConfig == null)
                {
                    m_RoadConfig = ConfigSystem.Instance.GetConfig<WorldMap_Road>(ID);
                }

                return m_RoadConfig;
            }
        }

        private RoadSurfaceType m_RoadSurfaceType;
        /// <summary>
        /// 路面类型
        /// </summary>
        public RoadSurfaceType RoadSurfaceType
        {
            get
            {
                if (m_RoadSurfaceType == RoadSurfaceType.None)
                {
                    if(RoadConfig != null)
                    {
                        m_RoadSurfaceType = (RoadSurfaceType)RoadConfig.RoadSurfaceType;
                    }
                }

                return m_RoadSurfaceType;
            }
        }

        /// <summary>
        /// 获取当前路径长度 (缓存信息 缓存的刷新在影响长度)
        /// </summary>
        public int Length { get { return m_RoadConfig.LengthBase; } }

        /// <summary>
        /// 获取目的地地点ID
        /// </summary>
        /// <returns></returns>
        public int GetDestinationPlaceID()
        {
            if (RoadConfig == null) return -1;

            return RoadConfig.Destination;
        }

        /// <summary>
        /// 获得道路权重
        /// </summary>
        /// <returns></returns>
        public int GetWeight()
        {
            //在影响因素变化后才有必要重新获取一次长度

            if (RoadConfig == null) return -1;

            int outLength = m_RoadConfig.LengthBase;

            //20210710 Winhoo TODO：路径长度受到天气影响
            return outLength;
        }

        /// <summary>
        /// 获得路面影响系数 0-1f
        /// </summary>
        /// <returns></returns>
        public float GetRoadSurfaceCoefficient()
        {
            switch (RoadSurfaceType)
            {
                case RoadSurfaceType.None:
                    return 0f;
                case RoadSurfaceType.Ice:
                    return 0.1f;
                case RoadSurfaceType.Mire:
                    return 0.2f;
                case RoadSurfaceType.Snowfield:
                    return 0.3f;
                case RoadSurfaceType.Sandy:
                    return 0.4f;
                case RoadSurfaceType.Rock:
                    return 0.5f;
                case RoadSurfaceType.Macadam:
                    return 0.6f;
                case RoadSurfaceType.Mud:
                    return 0.7f;
                case RoadSurfaceType.StrengthenMudRoad:
                    return 0.8f;
                case RoadSurfaceType.CobbledRoad:
                    return 0.9f;
                case RoadSurfaceType.StoneBrickRoad:
                    return 1f;
            }

            return 0f;
        }
    }

    /// <summary>
    /// 路面类型
    /// </summary>
    public enum RoadSurfaceType
    {
        None,

        /// <summary>
        /// 冰面
        /// </summary>
        Ice,

        /// <summary>
        /// 泥沼
        /// </summary>
        Mire,

        /// <summary>
        /// 雪地
        /// </summary>
        Snowfield,

        /// <summary>
        /// 沙土
        /// </summary>
        Sandy,

        /// <summary>
        /// 岩石
        /// </summary>
        Rock,

        /// <summary>
        /// 碎石
        /// </summary>
        Macadam,

        /// <summary>
        /// 泥土
        /// </summary>
        Mud,

        /// <summary>
        /// 土路
        /// </summary>
        StrengthenMudRoad,

        /// <summary>
        /// 石子路
        /// </summary>
        CobbledRoad,

        /// <summary>
        /// 石砖路
        /// </summary>
        StoneBrickRoad,
    }
}