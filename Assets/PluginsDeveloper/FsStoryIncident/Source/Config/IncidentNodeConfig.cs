using System;
using System.Collections.Generic;
using UnityEngine;

namespace FsStoryIncident
{
    [Serializable]
    public class IncidentNodeConfig : StorySubConfigBase
    {
        public IncidentNodeConfig()
        {
            chooses = new IncidentChooseConfig[0];
        }

        [Tooltip(ConfigConstData.incidentNodeConfig_nodeType_Tooltip)]
        public NodeType nodeType;

        [Tooltip(ConfigConstData.scoreLimit_Tooltip)]
        public ComparisonOperators scoreLimit;

        [Tooltip(ConfigConstData.scoreLimitNum_Tooltip)]
        public int scoreLimitNum;

        [Tooltip(ConfigConstData.incidentNodeConfig_paragraphs_Tooltip)]
        public Paragraph[] paragraphs;

        [Tooltip(ConfigConstData.taskConfig_Tooltip)]
        public TaskConfig taskConfig;

        [Tooltip(ConfigConstData.incidentNodeConfig_chooses_Tooltip)]
        public IncidentChooseConfig[] chooses;

        public bool HaveChooses => chooses != null && chooses.Length > 0;

        #region Runtime
        /// <summary>
        /// 获取一个选择配置
        /// </summary>
        /// <param name="index"></param>
        /// <param name="chooseConfig"></param>
        /// <returns></returns>
        public bool GetChoose(int index, out IncidentChooseConfig chooseConfig)
        {
            chooseConfig = null;
            if (index < 0 || index >= chooses.Length) return false;

            chooseConfig = chooses[index];

            return true;
        }
        #endregion

#if UNITY_EDITOR
        [DisplayOnly, NonSerialized, Tooltip(ConfigConstData.incidentItemConfig_Tooltip)]
        public IncidentItemConfig incidentItemConfig;

        [HideInInspector]
        public bool fadeAreaOpenCached = true;

        [HideInInspector]
        public bool foldoutParagraphs = true;

        #region Chooses Editor Data
        public event Action OnRefreshChoosesEditorData;

        [HideInInspector, NonSerialized] public List<string> chooseHeaders = new List<string>();
        public void RefreshChooseEditorData()
        {
            if (chooseHeaders == null) chooseHeaders = new List<string>();

            chooseHeaders.Clear();
            if (HaveChooses)
            {
                for (int i = 0; i < chooses.Length; i++)
                {
                    var choose = chooses[i];
                    chooseHeaders.Add($"{(string.IsNullOrEmpty(choose.configCommonData.NameEditorShow) ? $"Choose {choose.Guid()}" : choose.configCommonData.NameEditorShow)}");
                }
            }
            else
            {
                chooseHeaders.Add("None");
            }

            OnRefreshChoosesEditorData?.Invoke();
        }

        public bool GetChooseIndexByGuid(Guid guid, out int index)
        {
            if (!HaveChooses) { index = 0; return false; }

            for (int i = 0; i < chooses.Length; i++)
            {
                if (chooses[i].Guid() == guid)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        public Guid GetChooseGuidByIndex(int index)
        {
            if (index >= 0 && index < chooses.Length)
                return chooses[index].Guid();
            return System.Guid.Empty;
        }
        #endregion
#endif
    }

    /// <summary>
    /// 节点类型
    /// </summary>
    public enum NodeType
    {
        None,

        /// <summary>
        /// 情节
        /// </summary>
        Scenarios,

        /// <summary>
        /// 抉择
        /// </summary>
        Choice,

        /// <summary>
        /// 战斗
        /// </summary>
        Battle,
    }

    /// <summary>
    /// 段落
    /// </summary>
    [Serializable]
    public struct Paragraph
    {
        [Tooltip(ConfigConstData.incidentNodeConfig_paragraphs_actor_Tooltip)]
        public string actor;

        [Tooltip(ConfigConstData.describe_Tooltip)]
        public string describe;

#if UNITY_EDITOR
        [HideInInspector]
        public bool foldoutSelf;
#endif
    }
}