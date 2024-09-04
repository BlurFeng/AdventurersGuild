using System;
using System.Collections.Generic;
using UnityEngine;

namespace FsStoryIncident
{
    /// <summary>
    /// 链接其他配置
    /// 用于表示自身链接到的其他配置
    /// </summary>
    [Serializable]
    public class LinkOtherConfig
    {
        public List<LinkOtherItem> linkOtherItems;

        public bool HaveItems => linkOtherItems != null && linkOtherItems.Count > 0;

        public LinkOtherConfig()
        {
            linkOtherItems = new List<LinkOtherItem>();
        }

#if UNITY_EDITOR
        [HideInInspector] public bool foldoutLinkItems = false;
#endif
    }

    [Serializable]
    public class LinkOtherItem : IRandomData
    {
        [Tooltip(ConfigConstData.linkOtherConfig_targetPId_Tooltip)]
        public SGuid targetSGuid;
        public Guid TargetGuid => targetSGuid.Guid;

        [Tooltip(ConfigConstData.randomData_Tooltip)]
        public RandomData randomData;

        [Tooltip(ConfigConstData.scoreLimit_Tooltip)]
        public ComparisonOperators scoreLimit;

        [Tooltip(ConfigConstData.scoreLimitNum_Tooltip)]
        public int scoreLimitNum;

        [Tooltip(ConfigConstData.linkOtherConfig_nodeChooseLimit_Tooltip)]
        public List<NodeChooseLimitConfig> nodeChooseLimit; 

        [Tooltip(ConfigConstData.conditionConfig_Tooltip)]
        public ConditionConfig conditionConfig;

        public bool HaveNodeChooseLimit => nodeChooseLimit != null && nodeChooseLimit.Count > 0;

        public LinkOtherItem()
        {
            scoreLimit = ComparisonOperators.None;
            scoreLimitNum = 0;
            nodeChooseLimit = new List<NodeChooseLimitConfig>();
            conditionConfig = new ConditionConfig();

            randomData = new RandomData(10000, 0, 1);

#if UNITY_EDITOR
            foldoutNodeChooseLimit = false;
            targetIndex = 0;
#endif
        }

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
        [HideInInspector] public bool foldoutSelf;
        [HideInInspector] public bool foldoutNodeChooseLimit;

        /// <summary>
        /// 目标事件下标
        /// 下标0为None
        /// </summary>
        public int TargetIndex
        {
            get { return targetIndex; }
            set
            {
                if (TargetIndex != value)
                {
                    targetIndex = value;
                }
            }
        }
        [SerializeField] private int targetIndex;
#endif
    }

    /// <summary>
    /// 链接事件节点选择限制
    /// 要求在某个节点选择了某个选择
    /// </summary>
    [Serializable]
    public class NodeChooseLimitConfig
    {
        public enum NodeChooseLimitType
        {
            choose,
            unchoose,
        }
        [Tooltip(ConfigConstData.nodeChooseLimitConfig_type_Tooltip)]
        public NodeChooseLimitType type;

        [Tooltip(ConfigConstData.nodeChooseLimitConfig_nodeId_Tooltip)]
        public Guid nodeGuid;

        [Tooltip(ConfigConstData.nodeChooseLimitConfig_chooseId_Tooltip)]
        public Guid chooseGuid;

        public NodeChooseLimitConfig()
        {
        }

#if UNITY_EDITOR
        [HideInInspector] public bool foldoutSelf;
        [NonSerialized, HideInInspector] public IncidentNodeConfig nodeConfig; 

        /// <summary>
        /// 目标事件下标
        /// 下标0为None
        /// </summary>
        public int NodeIndex
        {
            get { return nodeIndex; }
            set
            {
                if (nodeIndex != value)
                {
                    nodeIndex = value;
                }
            }
        }
        private int nodeIndex;

        /// <summary>
        /// 目标事件下标
        /// 下标0为None
        /// </summary>
        public int ChooseIndex
        {
            get { return chooseIndex; }
            set
            {
                if (chooseIndex != value)
                {
                    chooseIndex = value;

                    Guid pid = System.Guid.Empty;
                    if (nodeConfig != null) pid = nodeConfig.GetChooseGuidByIndex(chooseIndex);
                    if (pid != System.Guid.Empty) chooseGuid = pid;
                    else chooseIndex = 0;
                }
            }
        }
        private int chooseIndex;

        public void SetNodeConfig(IncidentNodeConfig config)
        {
            if (nodeConfig != null)
            {
                nodeConfig.OnRefreshChoosesEditorData -= OnRefreshChoosesEditorData;
            }

            nodeConfig = config;
            if(nodeConfig != null)
            {
                nodeConfig.OnRefreshChoosesEditorData += OnRefreshChoosesEditorData;
            }

            if (nodeConfig == null || ChooseIndex >= nodeConfig.chooseHeaders.Count)
                ChooseIndex = 0;
        }

        public void OnRefreshChoosesEditorData()
        {
            ChooseIndex = nodeConfig.GetChooseIndexByGuid(chooseGuid, out int index) ? index : 0;
        }
#endif
    }
}