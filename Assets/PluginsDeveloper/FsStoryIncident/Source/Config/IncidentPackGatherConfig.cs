using System;
using UnityEngine;

namespace FsStoryIncident
{
    /// <summary>
    /// 事件包集合配置
    /// </summary>
    [CreateAssetMenu(fileName = "NewIncidentPackGatherConfig", menuName = "FsStoryIncident/IncidentPackGatherConfig", order = 1)]
    public class IncidentPackGatherConfig : ScriptableObject, IConfigCommonData
    {
        public IncidentPackGatherConfig()
        {
            configCommonData = new ConfigCommonData();
            incidentPackConfigs = new IncidentPackConfig[0];
        }

        [Tooltip(ConfigConstData.configCommonData_Tooltip)]
        public ConfigCommonData configCommonData;

        [Tooltip(ConfigConstData.incidentPackGatherConfig_incidentPackConfigs_Tooltip)]
        public IncidentPackConfig[] incidentPackConfigs;

        public bool HaveIncidentPackConfigs => incidentPackConfigs != null && incidentPackConfigs.Length > 0;

        public Guid Guid() { return configCommonData.Guid; }

        public bool ContainsTag(string tag) { return configCommonData.ContainsTag(tag); }

        public string Name() { return configCommonData.name; }

#if UNITY_EDITOR
        public string NameEditorShow() { return configCommonData.NameEditorShow; }
#endif

#if UNITY_EDITOR
        [NonSerialized]
        public string guidStr;

        public void Copy(IncidentPackGatherConfig config)
        {
            configCommonData = config.configCommonData;
            incidentPackConfigs = config.incidentPackConfigs;
        }
#endif
    }
}