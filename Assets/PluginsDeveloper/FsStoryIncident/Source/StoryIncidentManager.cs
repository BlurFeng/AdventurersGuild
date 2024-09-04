using System;
using System.Collections.Generic;
using UnityEngine;

namespace FsStoryIncident
{
    /// <summary>
    /// 故事事件管理器
    /// 用于管理故事集合配置的运行时加载，故事进度数据和推进故事进度
    /// 是项目业务的主要沟通对象
    /// </summary>
    public class StoryIncidentManager
    {
        private static StoryIncidentManager instance;
        public static StoryIncidentManager Instance
        {
            get { if (instance == null) instance = new StoryIncidentManager(); return instance; }
        }

        private bool isInit;
        private StoryIncidentInitConfig m_StoryIncidentInitConfig;//初始化数据

        /// <summary>
        /// 是否可以工作
        /// </summary>
        public bool CanWork => isInit && m_StoryIncidentInitConfig != null && m_StoryIncidentInitConfig.storyGatherConfigs != null;

        //故事配置
        private Dictionary<Guid, StoryGatherConfig> m_StoryGatherConfigDic;
        private Dictionary<Guid, StoryConfig> m_StoryConfigDic;
        private Dictionary<Guid, ChapterConfig> m_ChapterConfigDic;
        private Dictionary<Guid, IncidentConfig> m_IncidentConfigDic;
        private Dictionary<Guid, IncidentItemConfig> m_IncidentItemConfigDic;
        private Dictionary<Guid, IncidentNodeConfig> m_NodeConfigDic;
        private Dictionary<Guid, IncidentChooseConfig> m_ChooseConfigDic;

        //事件集合配置
        private Dictionary<Guid, IncidentPackGatherConfig> m_IncidentPackGatherConfigDic;
        private Dictionary<Guid, IncidentPackConfig> m_IncidentPackConfigDic;

        /// <summary>
        /// 初始化故事事件管理器
        /// </summary>
        /// <param name="storyIncidentInitConfig">故事事件初始化配置</param>
        /// <param name="storyIncidentArchiveData">存档数据，可以通过GetArchiveData()方法获取</param>
        public void Init(StoryIncidentInitConfig storyIncidentInitConfig, byte[] storyIncidentArchiveData = null)
        {
            if (isInit) return;
            isInit = true;

            //解析数据
            this.m_StoryIncidentInitConfig = storyIncidentInitConfig;

            m_StoryGatherConfigDic = new Dictionary<Guid, StoryGatherConfig>();
            m_StoryConfigDic = new Dictionary<Guid, StoryConfig>();
            m_ChapterConfigDic = new Dictionary<Guid, ChapterConfig>();
            m_IncidentConfigDic = new Dictionary<Guid, IncidentConfig>();
            m_IncidentItemConfigDic = new Dictionary<Guid, IncidentItemConfig>();
            m_NodeConfigDic = new Dictionary<Guid, IncidentNodeConfig>();
            m_ChooseConfigDic = new Dictionary<Guid, IncidentChooseConfig>();

            m_IncidentPackGatherConfigDic = new Dictionary<Guid, IncidentPackGatherConfig>();
            m_IncidentPackConfigDic = new Dictionary<Guid, IncidentPackConfig>();

            if (storyIncidentInitConfig == null) return;

            //故事事件配置数据
            if (storyIncidentInitConfig.HaveStoryGatherConfigs)
            {
                foreach (var sg in storyIncidentInitConfig.storyGatherConfigs)
                {
                    //故事集合
                    var sgCopy = UnityEngine.Object.Instantiate(sg);
                    m_StoryGatherConfigDic.Add(sgCopy.Guid(), sgCopy);
                    if (sgCopy.storyConfigs.Length == 0) continue;

                    foreach (var s in sgCopy.storyConfigs)
                    {
                        //故事
                        s.configCommonData.ownerGuid = sgCopy.Guid();
                        m_StoryConfigDic.Add(s.Guid(), s);
                        if (s.chapters.Length == 0) continue;

                        foreach (var cha in s.chapters)
                        {
                            //事件集合
                            cha.configCommonData.ownerGuid = s.Guid();
                            m_ChapterConfigDic.Add(cha.Guid(), cha);
                            if (cha.incidents.Length == 0) continue;

                            foreach (var i in cha.incidents)
                            {
                                //事件
                                i.configCommonData.ownerGuid = cha.Guid();
                                m_IncidentConfigDic.Add(i.Guid(), i);
                                if (i.incidentItems.Length == 0) continue;

                                foreach (var ii in i.incidentItems)
                                {
                                    //事件项目
                                    ii.configCommonData.ownerGuid = i.Guid();
                                    m_IncidentItemConfigDic.Add(ii.Guid(), ii);
                                    if (ii.nodes.Length == 0) continue;

                                    foreach (var n in ii.nodes)
                                    {
                                        //节点
                                        n.configCommonData.ownerGuid = ii.Guid();
                                        m_NodeConfigDic.Add(n.Guid(), n);
                                        if (n.chooses.Length == 0) continue;

                                        foreach (var cho in n.chooses)
                                        {
                                            //选择
                                            cho.configCommonData.ownerGuid = n.Guid();
                                            m_ChooseConfigDic.Add(cho.Guid(), cho);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //事件包数据
            if (storyIncidentInitConfig.HaveIncidentPackGatherConfigs)
            {
                foreach (var ipg in storyIncidentInitConfig.incidentPackGatherConfigs)
                {
                    m_IncidentPackGatherConfigDic.Add(ipg.Guid(), ipg);
                    if (!ipg.HaveIncidentPackConfigs) continue;

                    foreach (var ip in ipg.incidentPackConfigs)
                    {
                        m_IncidentPackConfigDic.Add(ip.Guid(), ip);
                    }
                }
            }

            //存档数据
            m_StoryIncidentArchive = StoryIncidentArchive.Deserialize(storyIncidentArchiveData);
        }

        public static bool GetStoryGatherConfigByIncidentGuid(Guid incidentGuid, out StoryGatherConfig storyGatherConfig)
        {
            storyGatherConfig = null;
            if (!GetConfig(incidentGuid, out IncidentConfig iConfig)) return false;
            if (!GetConfig(iConfig.configCommonData.ownerGuid, out ChapterConfig cConfig)) return false;
            if (!GetConfig(cConfig.configCommonData.ownerGuid, out StoryConfig sConfig)) return false;
            return GetConfig(sConfig.configCommonData.ownerGuid, out storyGatherConfig);
        }

        public static bool GetStoryGatherConfigByChapterGuid(Guid chapterGuid, out StoryGatherConfig storyGatherConfig)
        {
            storyGatherConfig = null;
            if (!GetConfig(chapterGuid, out ChapterConfig cConfig)) return false;
            if (!GetConfig(cConfig.configCommonData.ownerGuid, out StoryConfig sConfig)) return false;
            return GetConfig(sConfig.configCommonData.ownerGuid, out storyGatherConfig);
        }

        public static bool GetStoryGatherConfigByStoryGuid(Guid storyGuid, out StoryGatherConfig storyGatherConfig)
        {
            storyGatherConfig = null;
            if (!GetConfig(storyGuid, out StoryConfig sConfig)) return false;
            return GetConfig(sConfig.configCommonData.ownerGuid, out storyGatherConfig);
        }

        #region GetConfig

        public static bool GetConfig(Guid guid, out StoryGatherConfig storyGatherConfig)
        {
            storyGatherConfig = null;
            if (!Instance.CanWork || !Instance.m_StoryGatherConfigDic.ContainsKey(guid)) return false;

            storyGatherConfig = Instance.m_StoryGatherConfigDic[guid];
            return true;
        }

        public static bool GetConfig(Guid guid, out StoryConfig storyConfig)
        {
            storyConfig = null;
            if (!Instance.CanWork || !Instance.m_StoryConfigDic.ContainsKey(guid)) return false;

            storyConfig = Instance.m_StoryConfigDic[guid];
            return true;
        }

        public static bool GetConfig(Guid guid, out ChapterConfig chapterConfig)
        {
            chapterConfig = null;
            if (!Instance.CanWork || !Instance.m_ChapterConfigDic.ContainsKey(guid)) return false;

            chapterConfig = Instance.m_ChapterConfigDic[guid];
            return true;
        }

        /// <summary>
        /// 获取事件配置
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="incidentConfig"></param>
        /// <returns></returns>
        public static bool GetConfig(Guid guid, out IncidentConfig incidentConfig)
        {
            incidentConfig = null;
            if (!Instance.CanWork || !Instance.m_IncidentConfigDic.ContainsKey(guid)) return false;

            incidentConfig = Instance.m_IncidentConfigDic[guid];
            return true;
        }

        /// <summary>
        /// 获取节点配置
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="nodeConfig"></param>
        /// <returns></returns>
        public static bool GetConfig(Guid guid, out IncidentNodeConfig nodeConfig)
        {
            nodeConfig = null;
            if (!Instance.CanWork || !Instance.m_NodeConfigDic.ContainsKey(guid)) return false;

            nodeConfig = Instance.m_NodeConfigDic[guid];
            return true;
        }

        /// <summary>
        /// 获取事件包集合配置
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="incidentPackGatherConfig"></param>
        /// <returns></returns>
        public static bool GetConfig(Guid guid, out IncidentPackGatherConfig incidentPackGatherConfig)
        {
            incidentPackGatherConfig = null;
            if (!Instance.CanWork || !Instance.m_IncidentPackGatherConfigDic.ContainsKey(guid)) return false;

            incidentPackGatherConfig = Instance.m_IncidentPackGatherConfigDic[guid];
            return true;
        }

        /// <summary>
        /// 获取事件包配置
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="incidentPackConfig"></param>
        /// <returns></returns>
        public static bool GetConfig(Guid guid, out IncidentPackConfig incidentPackConfig)
        {
            incidentPackConfig = null;
            if (!Instance.CanWork || !Instance.m_IncidentPackConfigDic.ContainsKey(guid)) return false;

            incidentPackConfig = Instance.m_IncidentPackConfigDic[guid];
            return true;
        }

        public static bool GetConfig(Guid iGuid, out IncidentConfig iConfig, out ChapterConfig cConfig, out StoryConfig sConfig, out StoryGatherConfig sgConfig)
        {
            cConfig = null; sConfig = null; sgConfig = null;
            if (GetConfig(iGuid, out iConfig))
            {
                var cGuid = iConfig.configCommonData.ownerGuid;
                if (GetConfig(cGuid, out cConfig))
                {
                    var sGuid = cConfig.configCommonData.ownerGuid;
                    if (GetConfig(sGuid, out sConfig))
                    {
                        var sgGuid = sConfig.configCommonData.ownerGuid;
                        if (!GetConfig(sgGuid, out sgConfig))
                        {
                            Debug.LogWarning($"StoryIncidentArchive::GetConfig StoryGatherConfig cannot get by storyGatherGuid:{sgGuid}! 无法通过故事集Id:{sgGuid} 获取故事集配置！");
                            return false;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"StoryIncidentArchive::GetConfig StoryConfig cannot get by storyGuid:{sGuid}! 无法通过故事Id:{sGuid} 获取故事配置！");
                        return false;
                    }
                }
                else
                {
                    Debug.LogWarning($"StoryIncidentArchive::GetConfig ChapterConfig cannot get by chapterGuid:{cGuid}! 无法通过章Id:{cGuid} 获取章配置！");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning($"StoryIncidentArchive::GetConfig IncidentConfig cannot get by incidentGuid:{iGuid}! 无法通过事件Id:{iGuid} 获取事件配置！");
                return false;
            }

            return true;
        }

        public static bool GetConfig(Guid cGuid, out ChapterConfig cConfig, out StoryConfig sConfig, out StoryGatherConfig sgConfig)
        {
            sConfig = null; sgConfig = null;
            if (GetConfig(cGuid, out cConfig))
            {
                var sGuid = cConfig.configCommonData.ownerGuid;
                if (GetConfig(sGuid, out sConfig))
                {
                    var sgGuid = sConfig.configCommonData.ownerGuid;
                    if (!GetConfig(sgGuid, out sgConfig))
                    {
                        Debug.LogWarning($"StoryIncidentArchive::GetConfig StoryGatherConfig cannot get by storyGatherGuid:{sgGuid}! 无法通过故事集Id:{sgGuid} 获取故事集配置！");
                        return false;
                    }
                }
                else
                {
                    Debug.LogWarning($"StoryIncidentArchive::GetConfig StoryConfig cannot get by storyGuid:{sGuid}! 无法通过故事Id:{sGuid} 获取故事配置！");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning($"StoryIncidentArchive::GetConfig ChapterConfig cannot get by chapterGuid:{cGuid}! 无法通过章Id:{cGuid} 获取章配置！");
                return false;
            }

            return true;
        }

        public static bool GetConfig(Guid sGuid, out StoryConfig sConfig, out StoryGatherConfig sgConfig)
        {
            sgConfig = null;
            if (GetConfig(sGuid, out sConfig))
            {
                var sgGuid = sConfig.configCommonData.ownerGuid;
                if (!GetConfig(sgGuid, out sgConfig))
                {
                    Debug.LogWarning($"StoryIncidentArchive::GetConfig StoryGatherConfig cannot get by storyGatherGuid:{sgGuid}! 无法通过故事集Id:{sgGuid} 获取故事集配置！");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning($"StoryIncidentArchive::GetConfig StoryConfig cannot get by storyGuid:{sGuid}! 无法通过故事Id:{sGuid} 获取故事配置！");
                return false;
            }

            return true;
        }

        #endregion

        #region Incident Handler

        /// <summary>
        /// 创建一个事件处理器
        /// 当开始一个事件时，创建事件处理器，并对其进行操作
        /// </summary>
        public static bool CreateIncidentHandler(Guid incidentGuid, out IncidentHandler incidentHandler)
        {
            incidentHandler = null;
            if (!Instance.CanWork) return false;
            if (!GetConfig(incidentGuid, out IncidentConfig iConfig)) return false;

            return CreateIncidentHandler(iConfig, out incidentHandler);
        }

        /// <summary>
        /// 创建一个事件处理器
        /// 当开始一个事件时，创建事件处理器，并对其进行操作
        /// </summary>
        public static bool CreateIncidentHandler(IncidentConfig incidentConfig, out IncidentHandler incidentHandler)
        {
            incidentHandler = null;
            if (!Instance.CanWork) return false;
            if (incidentConfig == null) return false;

            incidentHandler = new IncidentHandler(incidentConfig.Guid(), incidentConfig);

            return true;
        }

        /// <summary>
        /// 随机发生事件，并创建对应的事件处理器
        /// </summary>
        /// <param name="incidentPackGuids"></param>
        /// <param name="incidentHandler"></param>
        /// <param name="customData"></param>
        /// <returns></returns>
        public static bool RandomIncidentHandler(Guid[] incidentPackGuids, out IncidentHandler incidentHandler, object customData = null)
        {
            incidentHandler = null;
            if (!Instance.CanWork) return false;
            if (!RandomIncidents(incidentPackGuids, out IncidentConfig happendIncidentConfig, customData)) return false;
            return CreateIncidentHandler(happendIncidentConfig, out incidentHandler);
        }

        /// <summary>
        /// 销毁一个事件处理器
        /// 当事件结束时，销毁一个事件处理器，销毁时将记录数据到故事存档
        /// </summary>
        public static void DestroyIncidentHandler(IncidentHandler incidentHandler, bool forceSave = false)
        {
            if (incidentHandler == null) return;
            if (!Instance.CanWork) return;

            //一般事件会在incidentHandler.MakeChoose()事件进行选择直到结束后自动End()并存档
            if (!incidentHandler.IsEnd && forceSave)
            {
                incidentHandler.End();
            }
        }

        #endregion

        #region RandomIncidents

        private readonly List<IncidentConfig> m_inputIncidentsCached = new List<IncidentConfig>();
        private readonly List<IncidentPackConfig> m_inputIncidentPackConfigsCached = new List<IncidentPackConfig>();

        /// <summary>
        /// 根据提供的事件包，按规则随机获取一个符合条件的事件
        /// </summary>
        /// <param name="incidentPackGuids"></param>
        /// <param name="happendIncidentConfig"></param>
        /// <param name="customData"></param>
        /// <returns></returns>
        public static bool RandomIncidents(Guid[] incidentPackGuids, out IncidentConfig happendIncidentConfig, object customData = null)
        {
            bool get = RandomIncidents(incidentPackGuids, out IncidentConfig[] happendIncidentConfigs, customData, 1);
            if (get)
            {
                happendIncidentConfig = happendIncidentConfigs[0];
                return true;
            }

            happendIncidentConfig = null;
            return false;
        }

        /// <summary>
        /// 根据提供的事件包，按规则随机获取符合条件的事件
        /// </summary>
        public static bool RandomIncidents(Guid[] incidentPackGuids, out IncidentConfig[] happendIncidentConfigs, object customData = null, int getNum = 1, int getPackNum = int.MaxValue)
        {
            happendIncidentConfigs = null;
            if (!Instance.CanWork) return false;

            happendIncidentConfigs = null;

            Instance.m_inputIncidentsCached.Clear();
            Instance.m_inputIncidentPackConfigsCached.Clear();

            //随机发生事件包
            for (int i = 0; i < incidentPackGuids.Length; i++)
            {
                if (GetConfig(incidentPackGuids[i], out IncidentPackConfig iPConfig))
                {
                    Instance.m_inputIncidentPackConfigsCached.Add(iPConfig);
                }
            }

            if (!StoryIncidentLibrary.RandomItemGet(out IncidentPackConfig[] happendIncidentPackConfigs, Instance.m_inputIncidentPackConfigsCached, customData, getPackNum)) return false;

            //获取满足条件的事件，随机发生事件
            for (int i = 0; i < happendIncidentPackConfigs.Length; i++)
            {
                var iPConfig = happendIncidentPackConfigs[i];

                for (int j = 0; j < iPConfig.IncidentGuids.Count; j++)
                {
                    //获取配置
                    if (GetConfig(iPConfig.IncidentGuids[j], out IncidentConfig iConfig, out ChapterConfig cConfig, out StoryConfig sConfig, out StoryGatherConfig sgConfig))
                    {
                        if (Instance.m_inputIncidentsCached.Contains(iConfig)) continue;//不能重复添加

                        if (!iConfig.repetition && StoryIncidentArchive.CheckIncidentHappened(iConfig.Guid())) continue;//确认事件是否允许重复发生
                        if (StoryIncidentArchive.CheckStoryEnd(sConfig.Guid())) continue;//确认所属故事是否结束
                        if (!StoryIncidentArchive.CheckChapterIsTarget(cConfig.Guid())) continue;//确认事件所属的章是否是故事当前目标的章
                        if (!StoryIncidentArchive.CheckIncidentIsTarget(iConfig.Guid())) continue;//确认事件是否是章当前的目标事件
                        if (!iConfig.CheckCondition(customData)) continue;//确认事件的条件是否满足
                        if (!sConfig.conditionConfig.CheckCondition(customData)) continue;//确认事件所属的故事要求的条件是否满足

                        Instance.m_inputIncidentsCached.Add(iConfig);
                    }
                }
            }

            return StoryIncidentLibrary.RandomItemGet(out happendIncidentConfigs, Instance.m_inputIncidentsCached, customData, getNum);
        }
        #endregion

        #region Archive
        /// <summary>
        /// 存档数据
        /// </summary>
        public StoryIncidentArchive StoryIncidentArchive
        {
            get
            {
                if(m_StoryIncidentArchive == null)
                {
                    if (!isInit)
                        Debug.LogWarning("StoryIncidentManager uninitialized when get StoryIncidentArchive! 在获取故事事件存档时，故事事件管理器还初始化！这可能导致存档的一些问题！");
                    m_StoryIncidentArchive = new StoryIncidentArchive();
                }

                return m_StoryIncidentArchive;
            }
        }
        private StoryIncidentArchive m_StoryIncidentArchive;

        /// <summary>
        /// 获取存档数据
        /// 然后按照项目自己的方式进行存档
        /// </summary>
        /// <returns></returns>
        public byte[] GetArchiveData()
        {
            return StoryIncidentArchive.Serialize(m_StoryIncidentArchive);
        }
        #endregion
    }
}