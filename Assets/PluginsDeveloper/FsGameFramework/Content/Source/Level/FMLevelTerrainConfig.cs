using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FsGameFramework
{
    [System.Serializable]
    /// <summary>
    /// 关卡通道 可以移动到其他关卡的某个通道位置
    /// </summary>
    public class LevelPassageway
    {
        public Vector3 position;
        public ULevelConfig targetLevel;
        public int targetLevelPassagewayIndex;

        public LevelPassageway()
        {
            position = Vector3.zero;
            targetLevel = null;
            targetLevelPassagewayIndex = -1;
        }
    }

    public class FMLevelTerrainConfig : FMonoConfig
    {
        [SerializeField]
        [Header("玩家角色出生点")]
        private FPlayerStart m_PlayerStart;
        public FPlayerStart PlayerStart { get { return m_PlayerStart; } }

        /// <summary>
        /// 关卡通道 记录前往其他Level的信息
        /// </summary>
        [Header("关卡通道")]
        public List<LevelPassageway> passageways;
    }
}