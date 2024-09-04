using UnityEngine;
using System;
using System.Collections.Generic;

namespace FsStoryIncident
{
    [Serializable]
    public struct ConditionConfig
    {
        [Tooltip(ConfigConstData.conditionConfig_conditionItems_Tooltip)]
        public List<ConditionItemConfig> conditionItems;

        [Tooltip(ConfigConstData.conditionConfig_logicExpression_Tooltip)]
        public string logicExpression;

        [Tooltip(ConfigConstData.conditionConfig_OwnerType_Tooltip)]
        public StoryIncidentConfigType ownerType;

        public bool HaveCondition => !string.IsNullOrEmpty(logicExpression) && HaveConditionItems;
        public bool HaveConditionItems => conditionItems != null && conditionItems.Count > 0;

#if UNITY_EDITOR
        /// <summary>
        /// 编辑窗口显示条件配置
        /// </summary>
        [HideInInspector]
        public bool foldout;

        /// <summary>
        /// 逻辑表达式缓存，修改过的逻辑表达式会缓存在此。合法的表达式才会赋值到logicExpression
        /// 不合法时logicExpression实际上为空
        /// </summary>
        [HideInInspector]
        public string logicExpressionCached;

        [HideInInspector]
        public bool logicExpressionNotValid;

        /// <summary>
        /// 拥有实现了IStoryCondition接口的条件类
        /// </summary>
        public static bool HaveConditionTypes => conditionTypes != null && conditionTypes.Length > 0;

        [HideInInspector, NonSerialized]
        public static string[] conditionTypes;//所有实现了IStoryCondition接口的条件类

        [HideInInspector, NonSerialized]
        public static string[] conditionParamsComments;

        /// <summary>
        /// 获取某个条件类在数组中的Index
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetIndexByType(string type)
        {
            if (!HaveConditionTypes || string.IsNullOrEmpty(type)) return 0;

            for (int i = 0; i < conditionTypes.Length; i++)
            {
                if (conditionTypes[i].Equals(type)) return i;
            }

            return 0;
        }
#endif
    }

    [Serializable]
    public struct ConditionItemConfig
    {
        [Tooltip(ConfigConstData.conditionItemConfig_type_Tooltip)]
        public string typeStr;

        [Tooltip(ConfigConstData.param_Tooltip)]
        public string param;

        /// <summary>
        /// 类型
        /// </summary>
        public Type Type { get { if (type == null) type = Type.GetType(typeStr); return type; } }
        [NonSerialized] private Type type;

#if UNITY_EDITOR
        public bool foldout;

        [SerializeField] private int conditionTypePopupIndex;
        public int ConditionTypePopupIndex
        {
            get { return conditionTypePopupIndex; }
            set
            {
                if(conditionTypePopupIndex != value)
                {
                    conditionTypePopupIndex = value;
                    if (conditionTypePopupIndex != 0) typeStr = ConditionConfig.conditionTypes[conditionTypePopupIndex];
                    else typeStr = string.Empty;
                }
            }
        }
#endif
    }
}