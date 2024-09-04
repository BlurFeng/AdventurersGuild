using System;
using UnityEngine;

namespace FsStoryIncident
{
    [Serializable]
    public class IncidentChooseConfig : StorySubConfigBase
    {
        public IncidentChooseConfig()
        {
            linkOtherConfig = new LinkOtherConfig();
            conditionConfig.ownerType = StoryIncidentConfigType.Choose;
        }

        [Tooltip(ConfigConstData.incidentChooseConfig_scoreUseType_Tooltip)]
        public ScoreHandlerType scoreUseType;

        [Tooltip(ConfigConstData.incidentChooseConfig_score_Tooltip)]
        public int score;

        [Tooltip(ConfigConstData.conditionConfig_Tooltip)]
        public ConditionConfig conditionConfig;

        [Tooltip(ConfigConstData.taskConfig_Tooltip)]
        public TaskConfig taskConfig;

        [Tooltip(ConfigConstData.linkOtherConfig_Tooltip)]
        public LinkOtherConfig linkOtherConfig;

        #region Runtime

        /// <summary>
        /// 确认条件是否满足结果缓存
        /// </summary>
        [NonSerialized] public bool checkConditionCached;

        #endregion

#if UNITY_EDITOR
        [DisplayOnly, NonSerialized, Tooltip(ConfigConstData.incidentNodeConfig_Tooltip)]
        public IncidentNodeConfig incidentNodeConfig;

        [HideInInspector]
        public bool fadeAreaOpenCached = true;
#endif
    }
}