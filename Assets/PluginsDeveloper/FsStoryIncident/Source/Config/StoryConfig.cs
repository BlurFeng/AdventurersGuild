using System;
using System.Collections.Generic;
using UnityEngine;

namespace FsStoryIncident
{
    //故事的配置是一个故事最外层的配置结构，然后一次向下往事件，事件项目，事件节点，事件选择嵌套。
    //如果嵌套数据数组使用Id配置，可以提高其他配置资源的复用性。但可读性会下降，且我们实际上不希望故事中出现重复的内容。所以我们直接使用相应的Config类

    [Serializable]
    public class StoryConfig : StorySubConfigBase
    {
        public StoryConfig()
        {
            configCommonData = new ConfigCommonData();
            chapters = new ChapterConfig[0];
            conditionConfig.ownerType = StoryIncidentConfigType.Story;
        }

        [Tooltip(ConfigConstData.storyConfig_startChapterPId_Tooltip)]
        public SGuid startChapterGuid;

        [Tooltip(ConfigConstData.conditionConfig_Tooltip)]
        public ConditionConfig conditionConfig;

        [Tooltip(ConfigConstData.taskConfig_Tooltip)]
        public TaskConfig taskConfig;

        [Tooltip(ConfigConstData.storyConfig_chapters_Tooltip)]
        public ChapterConfig[] chapters;

        public bool HaveChapters => chapters != null && chapters.Length > 0;

#if UNITY_EDITOR

        [DisplayOnly, NonSerialized, Tooltip(ConfigConstData.storyGatherConfig_storyConfigs_Tooltip)]
        public StoryGatherConfig storyGatherConfig;

        [HideInInspector]
        public bool fadeAreaOpenCached = true;

        [SerializeField] private int startChapterIndex;
        public int StartChapterIndex
        {
            get { return startChapterIndex; }
            set
            {
                if (startChapterIndex != value)
                {
                    startChapterIndex = value;
                    startChapterGuid.Guid = HaveChapters && startChapterIndex < chapters.Length ? chapters[startChapterIndex].configCommonData.Guid : System.Guid.Empty;
                }
            }
        }

        #region Sub datas
        public event Action OnRefreshChaptersEditorData;

        [HideInInspector]
        public List<string> chapterHeaders = new List<string>();

        public void RefreshChaptersEditorData()
        {
            if (chapterHeaders == null) chapterHeaders = new List<string>();
            chapterHeaders.Clear();

            bool startChapterPIdValid = false;
            if (HaveChapters)
            {
                for (int i = 0; i < chapters.Length; i++)
                {
                    var config = chapters[i];
                    chapterHeaders.Add($"{(string.IsNullOrEmpty(config.configCommonData.NameEditorShow) ? $"Chapter {config.configCommonData.GuidStr}" : config.configCommonData.NameEditorShow)}");

                    //确认之前设置的StartNode是否有效，并更新StartNodeIndex
                    if (!startChapterPIdValid && startChapterGuid.Guid == chapters[i].Guid())
                    {
                        StartChapterIndex = i;
                        startChapterPIdValid = true;
                    }
                }
            }
            else
            {
                chapterHeaders.Add("None");
            }

            if (!startChapterPIdValid)
            {
                StartChapterIndex = 0;
            }

            OnRefreshChaptersEditorData?.Invoke();
        }

        public bool GetChaptersIndexByGuid(Guid guid, out int index)
        {
            if (!HaveChapters) { index = 0; return false; }

            for (int i = 0; i < chapters.Length; i++)
            {
                if (chapters[i].Guid() == guid)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        public Guid GetChaptersGuidByIndex(int index)
        {
            if (index >= 0 && index < chapters.Length && chapters[index].Guid() != Guid())
                return chapters[index].Guid();
            return System.Guid.Empty;
        }
        #endregion
#endif
    }
}