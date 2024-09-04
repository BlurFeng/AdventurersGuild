using System;
using System.Collections.Generic;
using UnityEngine;

namespace FsStoryIncident
{
    [Serializable]
    public class ChapterConfig : StorySubConfigBase
    {
        public ChapterConfig()
        {
            incidents = new IncidentConfig[0];
            linkOtherConfig = new LinkOtherConfig(); 
        }

        [Tooltip(ConfigConstData.taskConfig_Tooltip)]
        public TaskConfig taskConfig;

        [Tooltip(ConfigConstData.linkOtherConfig_Tooltip)]
        public LinkOtherConfig linkOtherConfig;

        [Tooltip(ConfigConstData.chapterConfig_type_Tooltip)]
        public ChapterType type = ChapterType.Prose;

        [Header(ConfigConstData.chapterConfig_Line_Header)]
        [Tooltip(ConfigConstData.chapterConfig_StartIncidentPId_Tooltip)]
        public SGuid startIncidentGuid;

        [Header(ConfigConstData.chapterConfig_Tier_Header)]
        [Tooltip(ConfigConstData.chapterConfig_ConditionNum_Tooltip)]
        public int conditionNum;

        [Tooltip(ConfigConstData.chapterConfig_incidents_Tooltip)]
        public IncidentConfig[] incidents;

        public bool HaveIncidents => incidents != null && incidents.Length > 0;

#if UNITY_EDITOR

        [DisplayOnly, NonSerialized, Tooltip(ConfigConstData.storyConfig_Tooltip)]
        public StoryConfig storyConfig;

        [HideInInspector]
        public bool fadeAreaOpenCached = true;

        [SerializeField] private int startIncidentIndex;
        public int StartIncidentIndex
        {
            get { return startIncidentIndex; }
            set
            {
                if (startIncidentIndex != value)
                {
                    startIncidentIndex = value;

                    int getIndex = startIncidentIndex - 1;

                    if (getIndex >= 0 && getIndex < incidents.Length)
                        startIncidentGuid.GuidStr = incidents[getIndex].configCommonData.GuidStr;
                    else
                    {
                        startIncidentIndex = 0;
                        startIncidentGuid.GuidStr = String.Empty;
                    }
                }
            }
        }

        public void OnRefreshChaptersEditorData()
        {
            if (linkOtherConfig != null && linkOtherConfig.linkOtherItems != null)
            {
                for (int i = 0; i < linkOtherConfig.linkOtherItems.Count; i++)
                {
                    var item = linkOtherConfig.linkOtherItems[i];
                    item.TargetIndex = storyConfig.GetChaptersIndexByGuid(item.TargetGuid, out int index) ? index + 1 : 0;
                }
            }
        }

        public void OnRefreshAllNodesEditorDataCall()
        {
            if (linkOtherConfig != null && linkOtherConfig.linkOtherItems != null)
            {
                for (int i = 0; i < linkOtherConfig.linkOtherItems.Count; i++)
                {
                    var item = linkOtherConfig.linkOtherItems[i];
                    for (int j = 0; j < item.nodeChooseLimit.Count; j++)
                    {
                        item.nodeChooseLimit[j].NodeIndex = GetNodeIndexByGuid(item.nodeChooseLimit[j].nodeGuid, out int index) ? index + 1 : 0;
                    }
                }
            }
        }

        #region Incidents Editor Data
        public event Action OnRefreshIncidentsEditorData;

        [HideInInspector]
        public List<string> incidentHeaders = new List<string>();

        public void RefreshIncidentsEditorData()
        {
            if (incidentHeaders == null) incidentHeaders = new List<string>();
            incidentHeaders.Clear();

            if (HaveIncidents)
            {
                for (int i = 0; i < incidents.Length; i++)
                {
                    var config = incidents[i];
                    incidentHeaders.Add($"{(string.IsNullOrEmpty(config.configCommonData.NameEditorShow) ? $"Incident {config.Guid()}" : config.configCommonData.NameEditorShow)}");
                }
            }
            else
            {
                incidentHeaders.Add("None");
            }

            //确认开始事件
            StartIncidentIndex = GetIncidentIndexByGuid(startIncidentGuid.Guid, out int index) ? index + 1 : 0;

            OnRefreshIncidentsEditorData?.Invoke();
        }

        public bool GetIncidentIndexByGuid(Guid guid, out int index)
        {
            if (!HaveIncidents) { index = 0; return false; }

            for (int i = 0; i < incidents.Length; i++)
            {
                if (incidents[i].Guid() == guid)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        public Guid GetIncidentGuidByIndex(int index)
        {
            if (index >= 0 && index < incidents.Length && incidents[index].Guid() != Guid())
                return incidents[index].Guid();
            return System.Guid.Empty;
        }
        #endregion

        #region All Nodes Editor Data

        [HideInInspector, NonSerialized] public List<string> nodeHeaders = new List<string>();
        [HideInInspector, NonSerialized] public List<IncidentNodeConfig> nodes = new List<IncidentNodeConfig>();

        public void RefreshAllNodesEditorData()
        {
            if (nodeHeaders == null) nodeHeaders = new List<string>();
            nodes.Clear();
            nodeHeaders.Clear();

            if (HaveIncidents)
            {
                for (int i = 0; i < incidents.Length; i++)
                {
                    if (!incidents[i].HaveNodes) continue;
                    nodeHeaders.AddRange(incidents[i].nodeHeaders);
                    nodes.AddRange(incidents[i].nodes);
                }
            }
            else
            {
                nodeHeaders.Remove("None");
                nodeHeaders.Insert(0, "None");
            }

            OnRefreshAllNodesEditorDataCall();
        }

        public bool GetNodeIndexByGuid(Guid guid, out int index)
        {
            if (!HaveIncidents) { index = 0; return false; }

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null) continue;
                if (nodes[i].Guid() == guid)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        public Guid GetNodeGuidByIndex(int index)
        {
            if (index >= 0 && index < nodes.Count)
                return nodes[index].Guid();
            return System.Guid.Empty;
        }
        #endregion
#endif
    }
}