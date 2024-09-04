using System;
using System.Collections.Generic;
using UnityEngine;

namespace FsStoryIncident
{
    [Serializable]
    public class IncidentConfig : StorySubConfigBase, IRandomData
    {
        public IncidentConfig()
        {
            incidentItems = new IncidentItemConfig[0];
            linkOtherConfig = new LinkOtherConfig();
            conditionConfig.ownerType = StoryIncidentConfigType.Incident;
            randomData = new RandomData(10000, 0, 1); 
        }

        [Tooltip(ConfigConstData.randomData_Tooltip)]
        public RandomData randomData;

        [Tooltip(ConfigConstData.incidentConfig_repetition_Tooltip)]
        public bool repetition;

        [Tooltip(ConfigConstData.conditionConfig_Tooltip)]
        public ConditionConfig conditionConfig;

        [Tooltip(ConfigConstData.taskConfig_Tooltip)]
        public TaskConfig taskConfig;

        [Header(ConfigConstData.chapterConfig_Line_Header), Tooltip(ConfigConstData.linkOtherConfig_Tooltip)]
        public LinkOtherConfig linkOtherConfig;


        [Tooltip(ConfigConstData.incidentConfig_incidentItems_Tooltip)]
        public IncidentItemConfig[] incidentItems;

        /// <summary>
        /// 是否拥有事件项目
        /// </summary>
        public bool HaveIncidentItems => incidentItems != null && incidentItems.Length > 0;

        #region IRandomData
        /// <summary>
        /// 发生的事件项目缓存
        /// </summary>
        public List<IncidentItemConfig> HappendIncidentItems { get => m_HappendIncidentItems; }
        [NonSerialized] private readonly List<IncidentItemConfig> m_HappendIncidentItems = new List<IncidentItemConfig>();

        public bool HaveHappendIncidentItems => HappendIncidentItems != null && HappendIncidentItems.Count > 0;

        public ushort GetProbability()
        {
            return randomData.probability;
        }

        public short GetPriority()
        {
            return randomData.priority;
        }

        public ushort GetWeight()
        {
            return randomData.weight;
        }

        public bool CheckCondition(object CustomData = null)
        {
            if (!HaveIncidentItems) return false;
            if (!conditionConfig.CheckCondition(CustomData)) return false;

            //确认事件项目起码有一个符合条件的
            HappendIncidentItems.Clear();

            for (int i = 0; i < incidentItems.Length; i++)
            {
                if (!incidentItems[i].HaveNodes) continue;//没有任何可用的节点
                if (!incidentItems[i].conditionConfig.CheckCondition(CustomData)) continue;
                HappendIncidentItems.Add(incidentItems[i]);
            }

            return HappendIncidentItems.Count > 0;
        }
        #endregion

#if UNITY_EDITOR
        [DisplayOnly, NonSerialized, Tooltip(ConfigConstData.chapterConfig_Tooltip)]
        public ChapterConfig chapterConfig;

        [HideInInspector]
        public bool fadeAreaOpenCached = true;

        public void OnRefreshIncidentsEditorData()
        {
            if (linkOtherConfig != null && linkOtherConfig.linkOtherItems != null)
            {
                for (int i = 0; i < linkOtherConfig.linkOtherItems.Count; i++)
                {
                    var item = linkOtherConfig.linkOtherItems[i];
                    item.TargetIndex = chapterConfig.GetIncidentIndexByGuid(item.TargetGuid, out int index) ? index + 1 : 0;
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

        #region All Nodes Editor Data

        public bool HaveNodes => nodes != null && nodes.Count > 0;
        [HideInInspector, NonSerialized] public List<string> nodeHeaders = new List<string>();
        [HideInInspector, NonSerialized] public List<IncidentNodeConfig> nodes = new List<IncidentNodeConfig>();

        public void RefreshAllNodesEditorData()
        {
            if (nodeHeaders == null) nodeHeaders = new List<string>();
            nodes.Clear();
            nodeHeaders.Clear();

            if (HaveIncidentItems)
            {
                for (int i = 0; i < incidentItems.Length; i++)
                {
                    if (!incidentItems[i].HaveNodes) continue;

                    nodeHeaders.AddRange(incidentItems[i].nodeHeaders);
                    nodes.AddRange(incidentItems[i].nodes);
                }
            }
            else
            {
                nodeHeaders.Remove("None");
                nodeHeaders.Insert(0, "None");
            }

            OnRefreshAllNodesEditorDataCall();

            chapterConfig.RefreshAllNodesEditorData();
        }

        public bool GetNodeIndexByGuid(Guid guid, out int index)
        {
            if (!HaveIncidentItems) { index = 0; return false; }

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