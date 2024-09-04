using System;
using System.Collections.Generic;
using UnityEngine;

namespace FsStoryIncident
{
    [Serializable]
    public class ConfigCommonData
    {
        public ConfigCommonData()
        {
            tags = new List<string>();
            sGuid.NewGuid();
        }

        [Tooltip(ConfigConstData.guid_Tooltip), DisplayOnly]
        public SGuid sGuid;

        public Guid Guid => sGuid.Guid;
        public string GuidStr => sGuid.GuidStr;

        [Tooltip(ConfigConstData.name_Tooltip)]
        public string name;

        [Tooltip(ConfigConstData.describe_Tooltip)]
        public string describe;

        [Tooltip(ConfigConstData.tags_Tooltip)]
        public List<string> tags;

        public bool ContainsTag(string tag)
        {
            return tags.Contains(tag);
        }

        #region Runtime
        /// <summary>
        /// 自身的拥有者配置（上一级配置）的项目Guid
        /// </summary>
        [NonSerialized] public Guid ownerGuid;
        #endregion

#if UNITY_EDITOR
        [HideInInspector] public bool foldout;

        [Tooltip(ConfigConstData.comment_Tooltip)]
        public string commentName;
        [Tooltip(ConfigConstData.comment_Tooltip)]
        public string comment;

        public string NameEditorShow
        {
            get
            {
                return string.IsNullOrEmpty(commentName) ? name : commentName;
            }
        }
#endif
    }

    /// <summary>
    /// 可序列化存储的Guid类
    /// 以string的方式存储，在运行时生成对应的Guid
    /// </summary>
    [Serializable]
    public struct SGuid
    {
        public string GuidStr
        {
            get
            {
                return guidStr;
            }
            set
            {
                if (!guidStr.Equals(guidStr))
                {
                    guidStr = value;
                    guidRefresh = false;
                }
            }
        }
        [SerializeField, HideInInspector] private string guidStr;

        public Guid Guid
        {
            get
            {
                if (!guidRefresh) 
                {
                    guidRefresh = true;
                    if (!string.IsNullOrEmpty(GuidStr)) guid = System.Guid.Parse(GuidStr);
                }

                return guid;
            }
            set
            {
                if (guid != value)
                {
                    guid = value;
                    guidStr = guid == System.Guid.Empty ? String.Empty : guid.ToString();
                }
            }
        }
        [NonSerialized] private Guid guid;
        [NonSerialized] private bool guidRefresh;

        public SGuid(Guid guid)
        {
            guidStr = guid.ToString();
            this.guid = System.Guid.Empty;
            guidRefresh = false;
        }

        public SGuid(string guidStr)
        {
            this.guidStr = guidStr;
            guid = System.Guid.Empty;
            guidRefresh = false;
        }

        public SGuid(bool autoNew)
        {
            guidStr = String.Empty;
            this.guid = System.Guid.Empty;
            guidRefresh = false;
        }

        public void NewGuid()
        {
            if (string.IsNullOrEmpty(guidStr))
                Guid = System.Guid.NewGuid();
        }

        public override bool Equals(object obj)
        {
            if (obj is Guid guid)
            {
                return Guid.Equals(guid);
            }
            else if (obj is SGuid sGuid)
            {
                return Guid.Equals(sGuid.Guid);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return guidStr.GetHashCode();
        }
    }

    [Serializable]
    public struct RandomData
    {
        [Tooltip(ConfigConstData.probability_Tooltip)]
        public ushort probability;

        [Tooltip(ConfigConstData.priority_Tooltip)]
        public short priority;

        [Tooltip(ConfigConstData.weight_Tooltip)]
        public ushort weight;

        public RandomData(ushort probability, short priority, ushort weight)
        {
            this.probability = probability;
            this.priority = priority;
            this.weight = weight;
        }
    }
}
