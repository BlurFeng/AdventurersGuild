using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FsStoryIncident
{
    public static class StoryIncidentEditorStaticData
    {
        /// <summary>
        /// 自动保存
        /// </summary>
        public static bool AutoSave
        {
            get { return EditorPrefs.GetBool("StoryIncidentStaticDataEditor.AutoSave", false); }
            set
            {
                if(AutoSave != value)
                {
                    EditorPrefs.SetBool("StoryIncidentStaticDataEditor.AutoSave", value);
                    OnAutoSaveChange?.Invoke(AutoSave);
                }
            }
        }
        public static event Action<bool> OnAutoSaveChange;

        /// <summary>
        /// 当前编辑的故事集合配置
        /// </summary>
        public static StoryGatherConfig StoryGatherConfig
        {
            get
            {
                if(storyGatherConfig == null)
                {
                    string path = EditorPrefs.GetString("StoryIncidentStaticDataEditor.StoryGatherConfig");
                    if(!string.IsNullOrEmpty(path))
                        storyGatherConfig = AssetDatabase.LoadAssetAtPath<StoryGatherConfig>(path);
                }

                return storyGatherConfig;
            }

            set
            {
                storyGatherConfig = value;
                string path = storyGatherConfig != null ? AssetDatabase.GetAssetPath(storyGatherConfig) : String.Empty;
                EditorPrefs.SetString("StoryIncidentStaticDataEditor.StoryGatherConfig", path);
            }
        }
        private static StoryGatherConfig storyGatherConfig;

        /// <summary>
        /// 当前编辑的事件包集合配置
        /// </summary>
        public static IncidentPackGatherConfig IncidentPackGatherConfig
        {
            get
            {
                if (incidentPackGatherConfig == null)
                {
                    string path = EditorPrefs.GetString("StoryIncidentStaticDataEditor.IncidentPackGatherConfig");
                    if (!string.IsNullOrEmpty(path))
                        incidentPackGatherConfig = AssetDatabase.LoadAssetAtPath<IncidentPackGatherConfig>(path);
                }

                return incidentPackGatherConfig;
            }

            set
            {
                incidentPackGatherConfig = value;
                string path = incidentPackGatherConfig != null ? AssetDatabase.GetAssetPath(incidentPackGatherConfig) : String.Empty;
                EditorPrefs.SetString("StoryIncidentStaticDataEditor.IncidentPackGatherConfig", path);
            }
        }
        private static IncidentPackGatherConfig incidentPackGatherConfig;
    }
}