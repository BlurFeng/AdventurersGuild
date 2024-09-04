using UnityEngine;
using System;
using System.Collections.Generic;

namespace FsStoryIncident
{
    [Serializable]
    public struct TaskConfig
    {
        [Tooltip(ConfigConstData.taskConfig_taskItems_Tooltip)]
        public List<TaskItemConfig> taskItems;

        [Tooltip(ConfigConstData.taskConfig_OwnerType_Tooltip)]
        public StoryIncidentConfigType ownerType;

        public bool HaveTaskItems => taskItems != null && taskItems.Count > 0;

#if UNITY_EDITOR
        /// <summary>
        /// 编辑窗口显示条件配置
        /// </summary>
        [HideInInspector]
        public bool foldout;

        /// <summary>
        /// 拥有实现了IStoryTask接口的条件类
        /// </summary>
        public static bool HaveTaskTypes => taskTypes != null && taskTypes.Length > 0;

        [HideInInspector, NonSerialized]
        public static string[] taskTypes;//所有实现了IStoryTask接口的条件类

        [HideInInspector, NonSerialized]
        public static string[] taskParamsComments;

        public static int GetIndexByType(string type)
        {
            if (!HaveTaskTypes || string.IsNullOrEmpty(type)) return 0;

            for (int i = 0; i < taskTypes.Length; i++)
            {
                if (taskTypes[i].Equals(type)) return i;
            }

            return 0;
        }
#endif
    }

    [Serializable]
    public struct TaskItemConfig
    {
        [Tooltip(ConfigConstData.taskItemConfig_type_Tooltip)]
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

        [SerializeField] private int taskTypePopupIndex;
        public int TaskTypePopupIndex
        {
            get { return taskTypePopupIndex; }
            set
            {
                if(taskTypePopupIndex != value)
                {
                    taskTypePopupIndex = value;
                    if (taskTypePopupIndex != 0) typeStr = TaskConfig.taskTypes[taskTypePopupIndex];
                    else typeStr = string.Empty;
                }
            }
        }
#endif
    }
}