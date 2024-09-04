using System;
using UnityEngine;

namespace FsStoryIncident
{
    //故事配置集合，可使用故事编辑工具进行内容编辑。
    //StoryConfig及其子结构Config必须通过工具来创建，保证Id等分配数据的合法性。
    //其他可调整内容可以直接修改生成后的Config

    /// <summary>
    /// 故事集配置
    /// </summary>
    [CreateAssetMenu(fileName = "NewStoryGatherConfig", menuName = "FsStoryIncident/StoryGatherConfig", order = 1)]
    public class StoryGatherConfig : ScriptableObject, IConfigCommonData
    {
        public StoryGatherConfig()
        {
            configCommonData = new ConfigCommonData();
            storyConfigs = new StoryConfig[0];
        }

        [Tooltip(ConfigConstData.configCommonData_Tooltip)]
        public ConfigCommonData configCommonData;

        [Tooltip(ConfigConstData.taskConfig_Tooltip)]
        public TaskConfig taskConfig;

        [Tooltip(ConfigConstData.storyGatherConfig_storyConfigs_Tooltip)]
        public StoryConfig[] storyConfigs;

        public Guid Guid() { return configCommonData.Guid; }

        public bool ContainsTag(string tag) { return configCommonData.ContainsTag(tag); }

        public string Name() { return configCommonData.name; }

#if UNITY_EDITOR
        public string NameEditorShow() { return configCommonData.NameEditorShow; }
#endif

#if UNITY_EDITOR
        public void Copy(StoryGatherConfig config)
        {
            configCommonData = config.configCommonData;
            storyConfigs = config.storyConfigs;
        }
#endif
    }
}