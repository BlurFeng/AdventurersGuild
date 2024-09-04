using System;
using System.Collections.Generic;
using UnityEngine;

namespace FsStoryIncident
{
    [Serializable]
    public class IncidentPackConfig : StorySubConfigBase, IRandomData
    {
        public IncidentPackConfig()
        {
            incidentGuidStrs = new List<string>();
            conditionConfig.ownerType = StoryIncidentConfigType.IncidentPack;

            randomData = new RandomData(10000, 0, 1);
        }

        [Tooltip(ConfigConstData.randomData_Tooltip)]
        public RandomData randomData;

        [Tooltip(ConfigConstData.conditionConfig_Tooltip)]
        public ConditionConfig conditionConfig;

        [Tooltip(ConfigConstData.incidentPackConfig_incidentIds_Tooltip)]
        public List<string> incidentGuidStrs;

        [HideInInspector, NonSerialized] public bool incidentGuidStrsIsDirty = true;
        public List<Guid> IncidentGuids
        {
            get
            {
                if (incidentGuidStrsIsDirty)
                {
                    incidentGuidStrsIsDirty = false;
                    incidentGuids.Clear();
                    for (int i = 0; i < incidentGuidStrs.Count; i++)
                    {
                        incidentGuids.Add(System.Guid.Parse(incidentGuidStrs[i]));
                    }
                }
                return incidentGuids;
            }
        }
        [NonSerialized] private readonly List<Guid> incidentGuids = new List<Guid>();

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
        [DisplayOnly, NonSerialized, Tooltip(ConfigConstData.incidentPackGatherConfig_Tooltip)]
        public IncidentPackGatherConfig incidentPackGatherConfig;

        [HideInInspector]
        public bool fadeAreaOpenCached = true;

        [HideInInspector]
        public List<int> incidentSelectIndexs = new List<int>();
#endif
    }
}