using System;
using System.Collections.Generic;
using UnityEngine;

namespace FsStoryIncident
{
    [Serializable]
    public class IncidentItemConfig : StorySubConfigBase, IRandomData
    {
        public IncidentItemConfig()
        {
            nodes = new IncidentNodeConfig[0];
            conditionConfig.ownerType = StoryIncidentConfigType.IncidentItem;

            randomData = new RandomData(10000, 0, 1);
        }

        [Tooltip(ConfigConstData.randomData_Tooltip)]
        public RandomData randomData;

        [Tooltip(ConfigConstData.incidentItemConfig_startNodeId_Tooltip)]
        public SGuid startNodeGuid;

        [Tooltip(ConfigConstData.conditionConfig_Tooltip)]
        public ConditionConfig conditionConfig;

        [Tooltip(ConfigConstData.taskConfig_Tooltip)]
        public TaskConfig taskConfig;

        [Tooltip(ConfigConstData.incidentItemConfig_nodes_Tooltip)]
        public IncidentNodeConfig[] nodes;

        public bool HaveNodes => nodes != null && nodes.Length > 0;

        public bool HaveStartNodePId => startNodeGuid.Guid != System.Guid.Empty;

        #region IRandomData

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
            return conditionConfig.CheckCondition(CustomData);
        }
        #endregion

#if UNITY_EDITOR
        [DisplayOnly, NonSerialized, Tooltip(ConfigConstData.incidentConfig_Tooltip)]
        public IncidentConfig incidentConfig;

        [HideInInspector]
        public bool fadeAreaOpenCached = true;

        [SerializeField] private int startNodeIndex;
        public int StartNodeIndex
        {
            get { return startNodeIndex; }
            set
            {
                if(startNodeIndex != value)
                {
                    startNodeIndex = value;
                    startNodeGuid.GuidStr = HaveNodes && startNodeIndex < nodes.Length ? nodes[startNodeIndex].configCommonData.GuidStr : string.Empty;
                }
            }
        }

        #region Nodes Editor Data
        [HideInInspector, NonSerialized] public List<string> nodeHeaders = new List<string>();
        public void RefreshNodeEditorData()
        {
            if (nodeHeaders == null) nodeHeaders = new List<string>();

            nodeHeaders.Clear();

            bool startNodePIdValid = false;
            if (HaveNodes)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    var node = nodes[i];
                    nodeHeaders.Add($"{(string.IsNullOrEmpty(node.configCommonData.NameEditorShow) ? $"Node {node.Guid()}" : node.configCommonData.NameEditorShow)}");

                    //确认之前设置的StartNode是否有效，并更新StartNodeIndex
                    if (!startNodePIdValid && startNodeGuid.Guid == nodes[i].configCommonData.Guid)
                    {
                        StartNodeIndex = i;
                        startNodePIdValid = true;
                    }
                }
            }
            else
            {
                nodeHeaders.Add("None");
            }

            if (!startNodePIdValid)
            {
                StartNodeIndex = 0;
            }

            incidentConfig.RefreshAllNodesEditorData();
        }

        public bool GetNodeIndexByGuid(Guid guid, out int index)
        {
            if (!HaveNodes) { index = 0; return false; }

            for (int i = 0; i < nodes.Length; i++)
            {
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
            if (index >= 0 && index < nodes.Length && nodes[index].Guid() != Guid())
                return nodes[index].Guid();
            return System.Guid.Empty;
        }
        #endregion
#endif
    }
}