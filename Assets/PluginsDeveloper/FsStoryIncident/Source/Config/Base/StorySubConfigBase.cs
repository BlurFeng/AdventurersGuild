using System;
using UnityEngine;

namespace FsStoryIncident
{
    [Serializable]
    public class StorySubConfigBase : IConfigCommonData
    {
        public StorySubConfigBase()
        {
            configCommonData = new ConfigCommonData();
        }

        [Tooltip(ConfigConstData.configCommonData_Tooltip)]
        public ConfigCommonData configCommonData;

        public Guid Guid() { return configCommonData.Guid; }

        public bool ContainsTag(string tag) { return configCommonData.ContainsTag(tag); }

        public string Name() { return configCommonData.name; }

#if UNITY_EDITOR
        public string NameEditorShow() { return configCommonData.NameEditorShow; }
#endif
    }
}