using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FsGameFramework
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "NewWorldConfig", menuName = "FsGameFramework/WorldConfig", order = 22)]
    public class UWorldConfig : ScriptableObject
    {
        [Header("世界名称")]
        [Tooltip("世界名称")]
        public string worldName;

        [Header("世界类型")]
        public WorldType type;

        [Header("主要关卡")]
        [Tooltip("切换到此世界时进入的主要关卡")]
        public ULevelConfig persistentLevel;

        [Header("关卡列表")]
        [Tooltip("世界中包含的所有的关卡信息，必须有起码一个关卡")]
        public List<ULevelConfig> levelConfigs;

        [Header("游戏模式")]
        [Tooltip("游戏模式，配置开始游戏时的一些信息，规定游戏玩法")]
        public UGameModeConfig gameModeConfig;

        public bool CheckDataValid()
        {
            if (string.IsNullOrEmpty(worldName)) return false;
            if (persistentLevel == null) return false;
            if (levelConfigs == null || levelConfigs.Count == 0) return false;
            if (gameModeConfig == null) return false;

            return true;
        }

#if UNITY_EDITOR

        ULevelConfig persistentLevelCached;
        List<ULevelConfig> levelConfigsCached;

        private void OnValidate()
        {
            //确认persistentLevel，persistentLevel和levelConfigs关系
            if(persistentLevelCached != persistentLevel)
            {
                if(persistentLevelCached == null)
                {
                    //新增

                    //添加到levelConfigs中
                    if (!levelConfigs.Contains(persistentLevel))
                        levelConfigs.Add(persistentLevel);
                }
                else if(!persistentLevel)
                {
                    //清空

                    //也从列表移除
                    levelConfigs.Remove(persistentLevelCached);
                }
                else
                {
                    //改变

                    //添加到levelConfigs中
                    if (!levelConfigs.Contains(persistentLevel))
                        levelConfigs.Add(persistentLevel);
                }

                persistentLevelCached = persistentLevel;
            }

            #region levelConfigs Check
            //确认levelConfigs中新增的LevelConfig的Name没有和已经在levelConfigs中的LevelConfig的Name重复
            //如果重复，将它移除并提示。每次变化时接受到回调，那么理论上只会有一个改变或新增的LevelConfig
            ULevelConfig newLevelConfig = null;
            int newLevelConfigIndex = -1;
            bool addOrChange = true;

            //获取新的LevelConfig
            if(levelConfigsCached != null)
            {
                if (levelConfigs.Count > levelConfigsCached.Count)
                {
                    //新增
                    newLevelConfigIndex = levelConfigs.Count - 1;
                    newLevelConfig = levelConfigs[newLevelConfigIndex];

                    addOrChange = true;
                }
                else if (levelConfigs.Count == levelConfigsCached.Count)
                {
                    //改变
                    for (int i = 0; i < levelConfigs.Count; i++)
                    {
                        if (levelConfigs[i] != levelConfigsCached[i])
                        {
                            //被改变的
                            newLevelConfigIndex = i;
                            newLevelConfig = levelConfigs[newLevelConfigIndex];

                            addOrChange = false;
                            break;
                        }
                    }

                    if (!levelConfigs.Contains(persistentLevel))
                        persistentLevel = null;
                }
                else
                {
                    //减少

                    if (!levelConfigs.Contains(persistentLevel))
                        persistentLevel = null;
                }
            }

            //确认是否时有效的
            if (newLevelConfig)
            {
                bool newLevelConfigDataValid = newLevelConfig.CheckDataValid();
                bool newLevelConfigValid = newLevelConfigDataValid;

                if(newLevelConfigDataValid)
                {
                    //确认是否有重名
                    for (int i = 0; i < levelConfigs.Count; i++)
                    {
                        if (i == newLevelConfigIndex) continue;

                        if (newLevelConfig.m_LevelName == levelConfigs[i].m_LevelName)
                        {
                            //新增的LevelConfig的名称和列表中的某个相同，不允许添加
                            newLevelConfigValid = false;

                            //改变时我们才提示，因为Unity点击加号新增List时会给最新的复默认值和上一个相同。我们需要直接制空而不是每次都提示
                            if (!addOrChange)
                                EditorUtility.DisplayDialog("WorldConfig Tips", "新的LevelConfig的名称配置不能和已经在列表中的其他LevelConfig的名称相同！", "确认");
                            break;
                        }
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("WorldConfig Tips", "新的LevelConfig不是有效数据，请确认其配置信息。", "确认");
                }
                

                if (!newLevelConfigValid)
                {
                    levelConfigs[newLevelConfigIndex] = null;
                }
            }
            else
            {
                //不应当没有获取到新的LevelConfig
            }

            //配置的levelConfigs可能会有空值，但在运行时，会把这些空值都移除掉
            #endregion

            //persistentLevel为空时，赋默认值
            if (persistentLevel == null)
            {
                if (levelConfigs != null && levelConfigs.Count > 0)
                    for (int i = 0; i < levelConfigs.Count; i++)
                    {
                        if (levelConfigs[i] != null)
                            persistentLevel = levelConfigs[i];
                    }
            }

            //更新缓存
            levelConfigsCached = new List<ULevelConfig>(levelConfigs);
            persistentLevelCached = persistentLevel;
        }
#endif

    }
}
