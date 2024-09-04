using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deploy;
using com.ootii.Messages;
using Google.Protobuf.Collections;
using System.Text;

namespace WorldMap
{
    /// <summary>
    /// 地点
    /// </summary>
    public class Place
    {
        public int ID { get; private set; }

        private WorldMap_Place m_PlaceConfig;
        /// <summary>
        /// 地点配置信息
        /// </summary>
        public WorldMap_Place PlaceConfig
        {
            get
            {
                if(m_PlaceConfig == null)
                {
                    m_PlaceConfig = ConfigSystem.Instance.GetConfig<WorldMap_Place>(ID);
                }

                return m_PlaceConfig;
            }
        }

        /// <summary>
        /// 地点用途类型
        /// </summary>
        private PlaceUseType m_PlaceUseType;
        public PlaceUseType PlaceUseType
        {
            get
            {
                if (m_PlaceUseType == PlaceUseType.None)
                {
                    if(PlaceConfig != null)
                    {
                        m_PlaceUseType = (PlaceUseType)PlaceConfig.PlaceUseType;
                    }
                }

                return m_PlaceUseType;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id">地点ID</param>
        public Place(int Id)
        {
            this.ID = Id;
        }

        /// <summary>
        /// 获取所有自身为起点的路径ID
        /// </summary>
        /// <returns></returns>
        public int[] GetRoadIds()
        {
            if (PlaceConfig == null) return null;

            int[] outRoads = new int[PlaceConfig.Roads.Count];

            for (int i = 0; i < PlaceConfig.Roads.Count; i++)
            {
                outRoads[i] = PlaceConfig.Roads[i];
            }

            return outRoads;
        }
    }

    public enum PlaceUseType
    {
        None,

        /// <summary>
        /// 普通
        /// </summary>
        Normal,

        /// <summary>
        /// 分隔用地点 用于分隔路线 不可选中为目标
        /// </summary>
        Separation,
    }
}