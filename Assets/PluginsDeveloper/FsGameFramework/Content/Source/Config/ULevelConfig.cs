using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FsGameFramework
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "NewLevelConfig", menuName = "FsGameFramework/LevelConfig", order = 22)]
    public class ULevelConfig : ScriptableObject
    {
        [Header("关卡名称")]
        public string m_LevelName;

        [Header("地形预制体")]
        [Tooltip("关卡地形环境预制体")]
        public GameObject m_TerrainPrefab;

        public bool CheckDataValid()
        {
            if (string.IsNullOrEmpty(m_LevelName)) return false;
            if (m_TerrainPrefab == null) return false;

            return true;
        }
    }
}
