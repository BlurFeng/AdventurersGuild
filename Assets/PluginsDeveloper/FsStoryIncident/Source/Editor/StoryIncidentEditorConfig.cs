using UnityEditor;
using UnityEngine;

namespace FsStoryIncident
{
    /// <summary>
    /// 故事事件编辑器配置文件
    /// </summary>
    public class StoryIncidentEditorConfig : ScriptableObject
    {
        private static StoryIncidentEditorConfig storyIncidentEditorConfig;
        /// <summary>
        /// 调试成员编辑窗口配置文件
        /// </summary>
        public static StoryIncidentEditorConfig Get
        {
            get
            {
                if (storyIncidentEditorConfig == null)
                    storyIncidentEditorConfig = StoryIncidentEditorLibrary.GetConfigDefault<StoryIncidentEditorConfig>(StoryIncidentEditorLibrary.EditorConfigPath + "/StoryIncidentEditorConfig.asset");

                return storyIncidentEditorConfig;
            }
        }

        public StoryIncidentEditorConfig()
        {

        }

        public static void InitializeOnLoadMethod()
        {
            var self = Get;//初始化
        }

        public static void Quit()
        {

        }
    }
}