using System;

namespace FsStoryIncident
{
    /// <summary>
    /// 事件处理器
    /// 在游戏运行时，触发事件后生成一个事件处理器
    /// 用于获取一个事件的信息，并推进事件的进行
    /// 一个事件处理器结束后，将需要的数据记录在故事存档
    /// </summary>
    public class IncidentHandler
    {
        private Guid m_Guid;//事件Id
        private IncidentArchive m_IncidentArchive;
        private IncidentNodeArchive m_NodeArchive;
        private IncidentConfig m_IncidentConfig;//事件配置
        private IncidentItemConfig m_IncidentItemConfig;//发生的事件项目配置
        private IncidentNodeConfig m_NodeConfigCur;//当前节点配置
        private int m_Score;//分数统计，在节点更换时重置

        /// <summary>
        /// 事件是否结束
        /// </summary>
        public bool IsEnd { get; private set; }

        public IncidentHandler(Guid guid, IncidentConfig config)
        {
            m_Guid = guid;
            m_IncidentConfig = config;
        }

        /// <summary>
        /// 事件开始
        /// </summary>
        /// <param name="customData">自定义数据，由项目需求决定。用于条件确认。</param>
        public void Start(object customData = null)
        {
            //一个事件中可能有多个事件项目，触发事件后需要起码发生一个事件项目
            //根据事件项目的条件配置，用于实现“不同的种族触发一个事件时，实际上发生的事件内容不同”等需求
            //确认发生的事件项目

            bool end = !StoryIncidentArchive.GetIncidentArchive(m_Guid, out m_IncidentArchive, out m_IncidentItemConfig, customData);

            //没有发生事件项目，直接结束。
            if (end)
            {
                End(customData);
                return;
            }
            else
            {
                //设置当前节点
                SetNodeCur(m_IncidentItemConfig.startNodeGuid.Guid);
            }
        }

        /// <summary>
        /// 事件结束
        /// </summary>
        public void End( object customData = null)
        {
            m_IncidentItemConfig.taskConfig.ExecuteTask(customData);
            m_IncidentConfig.taskConfig.ExecuteTask(customData);

            //获取存档数据
            StoryIncidentArchive.SaveIncidentArchive(m_Guid, m_IncidentArchive, customData);

            IsEnd = true;
        }

        /// <summary>
        /// 设置当前节点
        /// </summary>
        /// <param name="nodeGuid"></param>
        /// <returns></returns>
        private bool SetNodeCur(Guid nodeGuid)
        {
            if (StoryIncidentManager.GetConfig(nodeGuid, out IncidentNodeConfig nodeC))
            {
                m_NodeConfigCur = nodeC;
                m_NodeArchive = new IncidentNodeArchive(m_NodeConfigCur.Guid());
                m_Score = 0;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取当前节点片段
        /// 用于展示信息给玩家
        /// </summary>
        /// <param name="paragraphs"></param>
        /// <returns></returns>
        public bool GetNodeCurParagraphs(out Paragraph[] paragraphs)
        {
            paragraphs = null;
            if (m_NodeConfigCur == null) return false;

            paragraphs = m_NodeConfigCur.paragraphs;
            return paragraphs != null && paragraphs.Length > 0;
        }

        /// <summary>
        /// 获取选择
        /// 用于展示信息给玩家
        /// 节点可能没有任何选择，但也需要调用MakeChoose来进行事件的流程
        /// </summary>
        /// <param name="chooses">选择数组</param>
        /// <param name="customData">自定义数据，用于条件确认</param>
        /// <param name="checkCondition">确认选择条件并缓存在IncidentChooseConfig.checkConditionCached</param>
        /// <returns></returns>
        public bool GetChooses(out IncidentChooseConfig[] chooses, object customData = null, bool checkCondition = true)
        {
            chooses = null;
            if (m_NodeConfigCur == null) return false;
            if (!m_NodeConfigCur.HaveChooses) return false;
            chooses = m_NodeConfigCur.chooses;

            if (checkCondition)
            {
                for (int i = 0; i < chooses.Length; i++)
                {
                    chooses[i].checkConditionCached = chooses[i].conditionConfig.CheckCondition(customData);
                }
            }

            return true;
        }

        /// <summary>
        /// 进行选择
        /// </summary>
        /// <param name="index">选择下标</param>
        /// <param name="customData">自定义数据，会用于条件确认</param>
        public void MakeChoose(int index, object customData = null)
        {
            if (m_NodeConfigCur == null) return;

            bool end = false;

            //是否允许离开节点，节点分数是否有要求
            bool allowLeave = m_NodeConfigCur.scoreLimit != ComparisonOperators.None;

            if (m_NodeConfigCur.GetChoose(index, out IncidentChooseConfig choConfig))
            {
                //记录选择
                m_NodeArchive.AddChoose(choConfig.configCommonData.sGuid);

                //计分，并确认是否符合要求
                if (!allowLeave)
                {
                    m_Score = StoryIncidentLibrary.ScoreHandler(choConfig.scoreUseType, choConfig.score, m_Score);
                    allowLeave = StoryIncidentLibrary.ScoreCompare(m_NodeConfigCur.scoreLimit, m_Score, m_NodeConfigCur.scoreLimitNum);
                }

                choConfig.taskConfig.ExecuteTask(customData);

                //离开节点
                if (allowLeave)
                {
                    //记录分数
                    m_NodeArchive.SetScore(m_Score);

                    //记录节点存档
                    m_IncidentArchive.ItemArchive.AddNodeArchive(m_NodeArchive);

                    m_NodeConfigCur.taskConfig.ExecuteTask(customData);

                    //进行到下一个节点
                    if (StoryIncidentLibrary.GetLinkOtherItemTargetPId(
                    choConfig.linkOtherConfig.linkOtherItems,
                    m_NodeArchive.Score,
                    (NodeChooseLimitConfig config) =>
                    {
                        return m_NodeArchive.CheckNodeChoose(config.type, config.chooseGuid);
                    },
                    out Guid targetGuid, customData))
                    {
                        //设置当前节点
                        if (!SetNodeCur(targetGuid)) end = true;
                    }
                    else end = true;
                }
            }
            else end = true;

            //结束事件
            if (end)
            {
                End(customData);
                return;
            }
        }
    }
}