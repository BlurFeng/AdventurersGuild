using System;
using UnityEngine;

namespace FsStoryIncident
{
    /// <summary>
    /// 故事事件初始化配置
    /// 用于初始化StoryIncidentManager的数据
    /// </summary>
    [CreateAssetMenu(fileName = "NewStoryIncidentInitConfig", menuName = "FsStoryIncident/StoryIncidentInitConfig", order = 1)]
    public class StoryIncidentInitConfig : ScriptableObject, IConfigCommonData
    {
        [DisplayOnly, Tooltip(ConfigConstData.guid_Tooltip)]
        public Guid guid;

        [Tooltip(ConfigConstData.configCommonData_Tooltip)]
        public ConfigCommonData configCommonData;

        [Tooltip(ConfigConstData.storyIncidentInitConfig_storyGatherConfigs)]
        public StoryGatherConfig[] storyGatherConfigs;

        [Tooltip(ConfigConstData.storyIncidentInitConfig_incidentPackGatherConfigs)]
        public IncidentPackGatherConfig[] incidentPackGatherConfigs;

        public bool HaveStoryGatherConfigs => storyGatherConfigs != null && storyGatherConfigs.Length > 0;
        public bool HaveIncidentPackGatherConfigs => incidentPackGatherConfigs != null && incidentPackGatherConfigs.Length > 0;

        public StoryIncidentInitConfig()
        {
            configCommonData = new ConfigCommonData();
            storyGatherConfigs = new StoryGatherConfig[0];
            incidentPackGatherConfigs = new IncidentPackGatherConfig[0];
        }

        public Guid Guid() { return configCommonData.sGuid.Guid; }

        public bool ContainsTag(string tag) { return configCommonData.ContainsTag(tag); }

        public string Name() { return configCommonData.name; }

#if UNITY_EDITOR
        public string NameEditorShow() { return configCommonData.NameEditorShow; }
#endif
    }
}