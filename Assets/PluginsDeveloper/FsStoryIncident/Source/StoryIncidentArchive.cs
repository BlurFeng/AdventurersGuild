
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static FsStoryIncident.NodeChooseLimitConfig;

namespace FsStoryIncident
{
    /// <summary>
    /// 故事存档
    /// 故事存档包括所有玩家已经进行过的故事的存档数据
    /// 存档数据一般需要持久化，可以在游戏开始时读取持久化数据
    /// </summary>
    [Serializable]
    public class StoryIncidentArchive
    {
        #region Serialize 序列化存档数据
        /// <summary>
        /// 读取存档数据
        /// 数据可以通过StoryIncidentArchive.GetSaveData获取
        /// </summary>
        public static StoryIncidentArchive Deserialize(byte[] data)
        {
            if (data == null || data.Length == 0) return new StoryIncidentArchive();

            MemoryStream ms = new MemoryStream(data);
            BinaryFormatter bf = new BinaryFormatter();
            StoryIncidentArchive storyIncidentArchive = bf.Deserialize(ms) as StoryIncidentArchive;
            ms.Close();

            return storyIncidentArchive != null ? storyIncidentArchive : new StoryIncidentArchive();
        }

        /// <summary>
        /// 获取用于存档的序列化数据
        /// </summary>
        public static byte[] Serialize(StoryIncidentArchive storyIncidentArchive)
        {
            if (storyIncidentArchive == null) return null;

            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, storyIncidentArchive);
            byte[] data = ms.ToArray();
            ms.Close();

            return data;
        }
        #endregion

        //存档数据，以故事集为单位
        private readonly  Dictionary<Guid, StoryGatherArchive> m_StoryGatherArchiveDic = new Dictionary<Guid, StoryGatherArchive>();

        /// <summary>
        /// 获取事件存档
        /// </summary>
        /// <param name="iGuid"></param>
        /// <param name="incidentArchive"></param>
        public static bool GetIncidentArchive(Guid iGuid, out IncidentArchive incidentArchive, out IncidentItemConfig incidentItemConfig, object customData = null)
        {
            //此方法获取incidentArchive，一个incidentArchive总是新的
            //完成一个事件后使用SaveIncidentArchive进行存储
            //可能有多个相同IPId的incidentArchive，这取决于一个事件配置是否允许重复触发

            incidentItemConfig = null;

            if (!StoryIncidentManager.GetConfig(iGuid, out IncidentConfig iConfig))
            {
                incidentArchive = new IncidentArchive();
                return false;
            }

            //获取发生的事件项目
            if (StoryIncidentLibrary.RandomItemGet(out incidentItemConfig, iConfig.HappendIncidentItems, customData, false) && incidentItemConfig.HaveStartNodePId)
            {
                //设置当前节点
                //生成事件项目存档
                IncidentItemArchive incidentItemArchive = new IncidentItemArchive(incidentItemConfig.Guid());
                incidentArchive = new IncidentArchive(iGuid, incidentItemArchive);
                return true;
            }

            incidentArchive = new IncidentArchive();
            return false;
        }

        /// <summary>
        /// 存储事件存档
        /// </summary>
        /// <param name="iGuid"></param>
        /// <param name="incidentArchive"></param>
        /// <param name="customData"></param>
        /// <returns>true=故事未结束 false=故事结束</returns>
        public static bool SaveIncidentArchive(Guid iGuid, IncidentArchive incidentArchive, object customData = null)
        {
            //获取需要的配置
            if (!StoryIncidentManager.GetConfig(iGuid, out IncidentConfig iConfig, out ChapterConfig cConfig, out StoryConfig sConfig, out StoryGatherConfig sgConfig)) return false;

            //获取故事存档
            StoryGatherArchive sgArc = GetStoryGatherArchive(sgConfig.Guid());
            StoryArchive sArc = sgArc.GetStoryArchive(sConfig.Guid());
            ChapterArchive cArc = sArc.GetChapterArchive(cConfig.Guid());

            //存储事件数据
            if (!cArc.AddIncidentArchive(incidentArchive, customData))
            {
                //章结束，处理对应章存档
                cArc.End();
                cConfig.taskConfig.ExecuteTask(customData);

                //获取链接到其他章节信息
                if (StoryIncidentLibrary.GetLinkOtherItemTargetPId(
                    cConfig.linkOtherConfig.linkOtherItems,
                    cArc.Score,
                    (NodeChooseLimitConfig config) =>
                    {
                        return cArc.CheckNodeChoose(config.type, config.nodeGuid, config.chooseGuid);
                    },
                    out Guid guid, customData))
                {
                    sArc.SetTargetChapterGuid(guid);
                }
                else
                {
                    //故事结束，处理对应故事存档
                    sArc.End();
                    sConfig.taskConfig.ExecuteTask(customData);

                    if (sgArc.CompleteStory(sArc.Guid))
                    {
                        sgConfig.taskConfig.ExecuteTask(customData);
                    }

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取故事集存档
        /// </summary>
        /// <param name="sgGuid">故事集PId</param>
        /// <param name="autoCreate">当没有此存档时，自动创建一个新的故事集存档数据</param>
        /// <returns></returns>
        private static StoryGatherArchive GetStoryGatherArchive(Guid sgGuid, bool autoCreate = true)
        {
            var sia = StoryIncidentManager.Instance.StoryIncidentArchive;

            if (!sia.m_StoryGatherArchiveDic.TryGetValue(sgGuid, out StoryGatherArchive sgArc) && autoCreate)
            {
                sgArc = new StoryGatherArchive(sgGuid);
                sia.m_StoryGatherArchiveDic.Add(sgGuid, sgArc);
            }

            return sgArc;
        }

        /// <summary>
        /// 获取故事集存档
        /// 没有故事存档时不会自动创建，返回false
        /// </summary>
        /// <param name="sgGuid"></param>
        /// <param name="storyGatherArchive"></param>
        /// <returns></returns>
        private static bool GetStoryGatherArchive(Guid sgGuid, out StoryGatherArchive storyGatherArchive)
        {
            storyGatherArchive = GetStoryGatherArchive(sgGuid, false);
            return storyGatherArchive != null;
        }

        #region Check Data

        /// <summary>
        /// 确认故事集是否发生
        /// </summary>
        /// <param name="sgGuid"></param>
        /// <returns></returns>
        public static bool CheckStoryGatherHappened(Guid sgGuid)
        {
            return StoryIncidentManager.Instance.StoryIncidentArchive.m_StoryGatherArchiveDic.ContainsKey(sgGuid);
        }

        /// <summary>
        /// 确认故事是否发生
        /// </summary>
        /// <param name="sGuid"></param>
        /// <returns></returns>
        public static bool CheckStoryHappened(Guid sGuid)
        {
            if (!StoryIncidentManager.GetConfig(sGuid, out StoryConfig sConfig)) return false;
            if (GetStoryGatherArchive(sConfig.configCommonData.ownerGuid, out StoryGatherArchive sgArc))
            {
                return sgArc.ContainsStoryArchive(sGuid);
            }
            
            return false;
        }

        /// <summary>
        /// 确认章是否发生
        /// </summary>
        /// <param name="cGuid"></param>
        /// <returns></returns>
        public static bool CheckChapterHappened(Guid cGuid)
        {
            if (!StoryIncidentManager.GetConfig(cGuid, out ChapterConfig cConfig)) return false;
            if (!StoryIncidentManager.GetConfig(cConfig.configCommonData.ownerGuid, out StoryConfig sConfig)) return false;
            if (GetStoryGatherArchive(sConfig.configCommonData.ownerGuid, out StoryGatherArchive sgArc))
            {
                var sArc = sgArc.GetStoryArchive(sConfig.Guid());
                return sArc.ContainsChapterArchive(cGuid);
            }

            return false;
        }

        /// <summary>
        /// 确认事件是否发生
        /// </summary>
        /// <param name="iGuid"></param>
        /// <returns></returns>
        public static bool CheckIncidentHappened(Guid iGuid)
        {
            //获取需要的配置
            if (!StoryIncidentManager.GetConfig(iGuid, out IncidentConfig iConfig, out ChapterConfig cConfig, out StoryConfig sConfig, out StoryGatherConfig sgConfig)) return false;

            var sia = StoryIncidentManager.Instance.StoryIncidentArchive;
            if (GetStoryGatherArchive(sgConfig.Guid(), out StoryGatherArchive sgArc))
            {
                var sArc = sgArc.GetStoryArchive(sConfig.Guid());
                var cArc = sArc.GetChapterArchive(cConfig.Guid());
                return cArc.ContainsIncidentArchive(iGuid);
            }

            return false;
        }

        public static bool CheckStoryEnd(Guid sGuid)
        {
            if (!StoryIncidentManager.GetConfig(sGuid, out StoryConfig sConfig, out StoryGatherConfig sgConfig)) return false;
            StoryGatherArchive sgArc = GetStoryGatherArchive(sgConfig.Guid());
            StoryArchive sArc = sgArc.GetStoryArchive(sConfig.Guid());
            return sArc.IsEnd;
        }

        /// <summary>
        /// 确认章节是否是故事当前的目标章节（正在发生的）
        /// </summary>
        /// <param name="cGuid"></param>
        /// <returns></returns>
        public static bool CheckChapterIsTarget(Guid cGuid)
        {
            if (!StoryIncidentManager.GetConfig(cGuid, out ChapterConfig cConfig, out StoryConfig sConfig, out StoryGatherConfig sgConfig)) return false;
            StoryGatherArchive sgArc = GetStoryGatherArchive(sgConfig.Guid());
            StoryArchive sArc = sgArc.GetStoryArchive(sConfig.Guid());
            return sArc.TargetChapterGuid.Equals(cGuid);
        }

        /// <summary>
        /// 确认事件是否是章的下一个目标事件
        /// 当章节类型不是Line线型时，无限制直接返回true
        /// </summary>
        /// <param name="iGuid"></param>
        /// <returns></returns>
        public static bool CheckIncidentIsTarget(Guid iGuid)
        {
            if (!StoryIncidentManager.GetConfig(iGuid, out IncidentConfig iConfig, out ChapterConfig cConfig, out StoryConfig sConfig, out StoryGatherConfig sgConfig)) return false;
            StoryGatherArchive sgArc = GetStoryGatherArchive(sgConfig.Guid());
            StoryArchive sArc = sgArc.GetStoryArchive(sConfig.Guid());
            ChapterArchive cArc = sArc.GetChapterArchive(cConfig.Guid());

            if (cArc.IsEnd) return false;//章已经结束

            if (cConfig.type == ChapterType.Line)
            {
                return cArc.TargetIncidentGuid.Equals(iGuid);
            }

            return true;
        }
        #endregion
    }

    #region Archive Data
    [Serializable]
    public class StoryGatherArchive
    {
        public SGuid Guid => guid;
        [SerializeField] private SGuid guid;

        /// <summary>
        /// 发生过的故事
        /// </summary>
        [SerializeField] private readonly List<StoryArchive> m_StoryArchives = new List<StoryArchive>();

        /// <summary>
        /// 完成过的故事
        /// </summary>
        [SerializeField] private readonly List<SGuid> m_CompleteStory = new List<SGuid>();

        /// <summary>
        /// 完成了所有的故事
        /// </summary>
        public bool IsComplete => isComplete;
        [SerializeField] private bool isComplete;

        public StoryGatherArchive(Guid guid)
        {
            this.guid = new SGuid(guid);
        }

        /// <summary>
        /// 获取故事存档
        /// </summary>
        /// <param name="guid">故事PId</param>
        /// <returns></returns>
        public StoryArchive GetStoryArchive(Guid guid)
        {
            for (int i = 0; i < m_StoryArchives.Count; i++)
            {
                if (m_StoryArchives[i].Guid.Equals(guid))
                    return m_StoryArchives[i];
            }

            StoryArchive sArc = new StoryArchive(guid);
            m_StoryArchives.Add(sArc);
            return sArc;
        }

        public bool ContainsStoryArchive(Guid guid)
        {
            for (int i = 0; i < m_StoryArchives.Count; i++)
            {
                if (m_StoryArchives[i].Guid.Equals(guid))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 完成一个故事（故事结束）时
        /// </summary>
        /// <param name="sGuid"></param>
        public bool CompleteStory(SGuid sGuid)
        {
            if (IsComplete) return false;
            if (m_CompleteStory.Contains(sGuid)) return false;

            m_CompleteStory.Add(sGuid);
            if(StoryIncidentManager.GetConfig(guid.Guid, out StoryGatherConfig sgConfig))
            {
                isComplete = m_CompleteStory.Count >= sgConfig.storyConfigs.Length;
                return true;
            }

            return false;
        }
    }

    [Serializable]
    public class StoryArchive
    {
        public SGuid Guid => guid;
        [SerializeField] private SGuid guid;

        /// <summary>
        /// 目标章Guid，当前发生的章
        /// </summary>
        public SGuid TargetChapterGuid => targetChapterGuid;
        [SerializeField] private SGuid targetChapterGuid;

        /// <summary>
        /// 发生过的章
        /// </summary>
        [SerializeField] private readonly List<ChapterArchive> m_ChapterArchives = new List<ChapterArchive>();

        /// <summary>
        /// 故事是否结束
        /// </summary>
        public bool IsEnd => targetChapterGuid.Guid == System.Guid.Empty;

        public StoryArchive(Guid guid)
        {
            this.guid = new SGuid(guid);

            if(StoryIncidentManager.GetConfig(guid, out StoryConfig sConfig))
            {
                SetTargetChapterGuid(sConfig.startChapterGuid.Guid);
            }
        }

        /// <summary>
        /// 获取章存档
        /// </summary>
        /// <param name="guid">章Id</param>
        /// <returns></returns>
        public ChapterArchive GetChapterArchive(Guid guid)
        {
            for (int i = 0; i < m_ChapterArchives.Count; i++)
            {
                if (m_ChapterArchives[i].Guid.Equals(guid))
                    return m_ChapterArchives[i];
            }

            ChapterArchive cArc = new ChapterArchive(guid);
            m_ChapterArchives.Add(cArc);
            return cArc;
        }

        /// <summary>
        /// 是否包含章存档
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool ContainsChapterArchive(Guid guid)
        {
            for (int i = 0; i < m_ChapterArchives.Count; i++)
            {
                if (m_ChapterArchives[i].Guid.Equals(guid))
                    return true;
            }

            return false;
        }

        public void SetTargetChapterGuid(Guid guid)
        {
            targetChapterGuid.Guid = guid;
        }

        public void End()
        {
            targetChapterGuid.Guid = System.Guid.Empty;
        }
    }

    [Serializable]
    public class ChapterArchive
    {
        public SGuid Guid => guid;
        [SerializeField] private SGuid guid;

        /// <summary>
        /// 目标事件Guid，下一个需要发生的事件
        /// 当章的类型为Line线性时，会记录下一个目标事件
        /// </summary>
        public SGuid TargetIncidentGuid => targetIncidentGuid;
        [SerializeField] private SGuid targetIncidentGuid;

        /// <summary>
        /// 在章获得的分数
        /// </summary>
        public int Score => score;
        [SerializeField] private int score;

        /// <summary>
        /// 完成的事件计数器
        /// </summary>
        [SerializeField] private uint completeIncidentCounter;

        public bool IsEnd => isEnd;
        [SerializeField] private bool isEnd;

        /// <summary>
        /// 发生过的事件
        /// 如果一个事件允许多次发生，那么列表中就会有多个PId相同的事件存档
        /// </summary>
        private readonly List<IncidentArchive> incidentArchives = new List<IncidentArchive>();

        private static readonly List<IncidentArchive> incidentArchiveGetCached = new List<IncidentArchive>();

        public ChapterArchive(Guid guid)
        {
            this.guid = new SGuid(guid);
            score = 0;

            if (StoryIncidentManager.GetConfig(guid, out ChapterConfig cConfig))
            {
                SetTargetIncidentGuid(cConfig.startIncidentGuid.Guid);
            }
        }

        /// <summary>
        /// 添加一个事件存档
        /// </summary>
        /// <param name="incidentArchive"></param>
        /// <param name="customData"></param>
        /// <returns>true=章未结束 false=章结束</returns>
        public bool AddIncidentArchive(IncidentArchive incidentArchive, object customData = null)
        {
            if (!StoryIncidentManager.GetConfig(Guid.Guid, out ChapterConfig cConfig)) return false;

            completeIncidentCounter++;
            incidentArchives.Add(incidentArchive);

            IncidentItemArchive iiArc = incidentArchive.ItemArchive;
            score += iiArc.Score;

            switch (cConfig.type)
            {
                case ChapterType.Prose:
                    //零散型，不会自动结束
                    break;
                case ChapterType.Line:
                    //线型号，在一个事件结束后，确认链接到的下一个事件
                    //没有链接的事件时视为章结束
                    if (StoryIncidentManager.GetConfig(incidentArchive.Guid.Guid, out IncidentConfig iConfig))
                    {
                        //获取链接到其他事件信息
                        StoryIncidentLibrary.GetLinkOtherItemTargetPId(
                            iConfig.linkOtherConfig.linkOtherItems,
                            iiArc.Score,
                            (NodeChooseLimitConfig config) =>
                            {
                                return iiArc.CheckNodeChoose(config.type, config.nodeGuid, config.chooseGuid);
                            },
                            out Guid guid , customData);

                        SetTargetIncidentGuid(guid);
                    }
                    return TargetIncidentGuid.GuidStr != string.Empty;
                case ChapterType.Tier:
                    //层型，完成数量达标后结束章
                    return completeIncidentCounter < cConfig.conditionNum;
            }

            return true;
        }

        /// <summary>
        /// 获取事件存档
        /// </summary>
        /// <param name="guid">事件PId</param>
        /// <param name="incidentArchive"></param>
        /// <returns></returns>
        public bool GetIncidentArchive(Guid guid, out IncidentArchive incidentArchive)
        {
            for (int i = 0; i < incidentArchives.Count; i++)
            {
                if (incidentArchives[i].Guid.Equals(guid))
                {
                    incidentArchive = incidentArchives[i];
                    return true;
                }
            }

            incidentArchive = new IncidentArchive();
            return false;
        }

        /// <summary>
        /// 获取某个PId的所有事件存档
        /// </summary>
        /// <param name="guid">事件PId</param>
        /// <param name="outIncidentArchives"></param>
        /// <returns></returns>
        public bool GetIncidentArchives(Guid guid, out IncidentArchive[] outIncidentArchives)
        {
            incidentArchiveGetCached.Clear();

            for (int i = 0; i < incidentArchives.Count; i++)
            {
                if (incidentArchives[i].Guid.Equals(guid))
                    incidentArchiveGetCached.Add(incidentArchives[i]);
            }

            outIncidentArchives = incidentArchiveGetCached.ToArray();
            return outIncidentArchives != null && outIncidentArchives.Length > 0;
        }

        public bool ContainsIncidentArchive(Guid guid)
        {
            for (int i = 0; i < incidentArchives.Count; i++)
            {
                if (incidentArchives[i].Guid.Equals(guid))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 确认节点选择是否符合要求
        /// </summary>
        /// <param name="type">确认方式</param>
        /// <param name="nodeGuid">节点PId</param>
        /// <param name="chooseGuid">选择PId</param>
        /// <returns></returns>
        public bool CheckNodeChoose(NodeChooseLimitType type, Guid nodeGuid, Guid chooseGuid)
        {
            //确认内部所有事件项目的节点选择是否符合要求
            //一否皆否原则

            for (int i = 0; i < incidentArchives.Count; i++)
            {
                if (!incidentArchives[i].ItemArchive.CheckNodeChoose(type, nodeGuid, chooseGuid))
                    return false;
            }

            return true;
        }

        public void SetTargetIncidentGuid(Guid guid)
        {
            targetIncidentGuid.Guid = guid;
        }

        public void End()
        {
            isEnd = true;
        }
    }

    /// <summary>
    /// 事件存档数据
    /// </summary>
    [Serializable]
    public struct IncidentArchive
    {
        public SGuid Guid => guid;
        [SerializeField] private SGuid guid;

        /// <summary>
        /// 发生过的事件项目
        /// </summary>
        public IncidentItemArchive ItemArchive => itemArchive;
        [SerializeField] private IncidentItemArchive itemArchive;

        public IncidentArchive(Guid guid, IncidentItemArchive incidentItemArchive)
        {
            this.guid = new SGuid(guid);
            itemArchive = incidentItemArchive;
        }
    }

    [Serializable]
    public struct IncidentItemArchive
    {
        public SGuid Guid => guid;
        [SerializeField] private SGuid guid;

        /// <summary>
        /// 在事件项目最终获得的分数
        /// </summary>
        public int Score => score;
        [SerializeField] private int score;

        /// <summary>
        /// 节点存档数据列表
        /// </summary>
        [SerializeField] private readonly List<IncidentNodeArchive> nodeArchives;

        public IncidentItemArchive(Guid guid)
        {
            this.guid = new SGuid(guid);
            score = 0;
            nodeArchives = new List<IncidentNodeArchive>();
        }

        public IncidentNodeArchive GetIncidentNodeArchive(Guid guid)
        {
            if (nodeArchives != null)
            {
                for (int i = 0; i < nodeArchives.Count; i++)
                {
                    if (nodeArchives[i].Guid.Equals(guid))
                        return nodeArchives[i];
                }
            }

            return new IncidentNodeArchive(guid);
        }

        public void AddNodeArchive(IncidentNodeArchive incidentNodeArchive)
        {
            score += incidentNodeArchive.Score;

            nodeArchives.Add(incidentNodeArchive);
        }

        /// <summary>
        /// 确认节点选择是否符合要求
        /// </summary>
        /// <param name="type">确认方式</param>
        /// <param name="nodeGuid">节点PId</param>
        /// <param name="chooseGuid">选择PId</param>
        /// <returns></returns>
        public bool CheckNodeChoose(NodeChooseLimitType type, Guid nodeGuid, Guid chooseGuid)
        {
            for (int i = 0; i < nodeArchives.Count; i++)
            {
                var nArc = nodeArchives[i];
                if(nArc.Guid.Equals(nodeGuid))
                {
                    nArc.CheckNodeChoose(type, chooseGuid);
                }
            }

            return false;
        }
    }

    [Serializable]
    public struct IncidentNodeArchive
    {
        public SGuid Guid => guid;
        [SerializeField] private SGuid guid;

        /// <summary>
        /// 在这个节点获得的分数
        /// </summary>
        public int Score => score;
        [SerializeField] private int score;

        /// <summary>
        /// 按顺序记录在这个节点做过的选择Id
        /// </summary>
        [SerializeField] private List<SGuid> choosePIds;

        public IncidentNodeArchive(Guid guid)
        {
            this.guid = new SGuid(guid);
            this.score = 0;
            this.choosePIds = new List<SGuid>();
        }

        public void AddChoose(SGuid sGuid)
        {
            choosePIds.Add(sGuid);
        }

        public bool ContainsChoose(Guid chooseGuid)
        {
            for (int i = 0; i < choosePIds.Count; i++)
            {
                if (choosePIds[i].Equals(chooseGuid))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 确认节点选择是否符合要求
        /// </summary>
        /// <param name="type">确认方式</param>
        /// <param name="nodeGuid">节点PId</param>
        /// <param name="chooseGuid">选择PId</param>
        /// <returns></returns>
        public bool CheckNodeChoose(NodeChooseLimitType type, Guid chooseGuid)
        {
            switch (type)
            {
                case NodeChooseLimitType.choose:
                    return ContainsChoose(chooseGuid);
                case NodeChooseLimitType.unchoose:
                    return !ContainsChoose(chooseGuid);
            }

            return false;
        }

        public void SetScore(int score)
        {
            this.score = score;
        }
    }

    #endregion
}