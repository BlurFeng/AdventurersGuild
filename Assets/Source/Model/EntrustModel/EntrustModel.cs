using com.ootii.Messages;
using Deploy;
using EntrustSystem;
using FsStoryIncident;
using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 委托模块
/// 此处委托指游戏中的冒险者公会发布的委托任务，由冒险者接取并执行
/// </summary>
public class EntrustModel : Singleton<EntrustModel>, IDestroy, ISaveData
{
    /// <summary>
    /// 委托系统管理器
    /// </summary>
    private EntrustSystemManager m_EntrustSystemManager; 

    /// <summary>
    /// 当前选中的委托的状态
    /// </summary>
    public EEntrustState EntrustCurState;

    public override void Init()
    {
        base.Init();

        //初始化委托系统管理器
        m_EntrustSystemManager = new EntrustSystemManager();
        m_EntrustSystemManager.Init();

        MessageDispatcher.AddListener(TimeModelMsgType.TIMEMODEL_DAYCOUNT_CHANGE, OnExecuteFinishTurn);

        EntrustSpawnInit();
        MsgInit();
        InformationStatsInit();
    }

    public override void Destroy()
    {
        base.Destroy();

        MessageDispatcher.RemoveListener(TimeModelMsgType.TIMEMODEL_DAYCOUNT_CHANGE, OnExecuteFinishTurn);
    }

    #region SaveData

    public class EntrustModelSaveData
    {
        public EntrustItem[] entrustItems;
        public int guildRank;

        public EntrustModelInformationStats informationStats;
    }

    const string m_SaveDataKey = "EntrustModel_EntrustModelSaveData";

    public void LoadData(ES3File saveData)
    {
        EntrustModelSaveData entrustModelSaveData = null;

        if (saveData.KeyExists(m_SaveDataKey))
        {
            entrustModelSaveData = saveData.Load<EntrustModelSaveData>(m_SaveDataKey);

            SetGuildRank(entrustModelSaveData.guildRank > 0 ? entrustModelSaveData.guildRank : 1);

            for (int i = 0; i < entrustModelSaveData.entrustItems.Length; i++)
            {
                BindEventWithEntrustItem(entrustModelSaveData.entrustItems[i]);
                m_EntrustSystemManager.CreateEntrustItem(entrustModelSaveData.entrustItems[i]);
            }
        }
        else
        {
            //初始化内容

            SetGuildRank(1);
        }

        InformationStatsLoadData(entrustModelSaveData);
    }

    public void SaveData(ES3File saveData)
    {
        EntrustModelSaveData entrustModelSaveData = new EntrustModelSaveData();

        entrustModelSaveData.guildRank = m_GuildRank;
        entrustModelSaveData.entrustItems = m_EntrustSystemManager.GetEntrustItems();

        InformationStatsSaveData(entrustModelSaveData);

        saveData.Save(m_SaveDataKey, entrustModelSaveData);
    }

    public void ReloadModelData()
    {

    }

    #endregion

    //当回合变化时
    private void OnExecuteFinishTurn(IMessage rMessage)
    {
        m_EntrustSystemManager.RoundUpdate();

        EntrustRandomSpawnCalculate();
    }

    #region Entrust Spawn and Destroy 委托的生成和销毁

    /// <summary>
    /// 获取委托等级
    /// 不同等级能接取到不同的委托
    /// </summary>
    private int m_GuildRank;

    /// <summary>
    /// 委托项目配置Dic 按等级区分
    /// Key=等级 Vaule=委托项目配置
    /// </summary>
    private Dictionary<int, Dictionary<int, Entrust_ItemConfig>> m_EntrustItemConfigDic_Rank;

    public void EntrustSpawnInit()
    {
        //获取所有委托配置数据
        m_EntrustItemConfigDic_Rank = new Dictionary<int, Dictionary<int, Entrust_ItemConfig>>();
        var cfgMap = ConfigSystem.Instance.GetConfigMap<Entrust_ItemConfig>();
        foreach (var item in cfgMap)
        {
            int rank = item.Value.Rank;

            //按等级划分委托项目配置
            if (!m_EntrustItemConfigDic_Rank.ContainsKey(rank))
                m_EntrustItemConfigDic_Rank.Add(rank, new Dictionary<int, Entrust_ItemConfig>());
            if(!m_EntrustItemConfigDic_Rank[rank].ContainsKey(item.Value.Id))
            {
                m_EntrustItemConfigDic_Rank[rank].Add(item.Value.Id, item.Value);

                //添加对应数据到管理器委托随机生成功能要求数据
                m_EntrustSystemManager.AddEntrustRandomInfo(
                    item.Value.Rank,
                    new EntrustSystemManager.EntrustRandomItemInfo()
                    {
                        id = item.Value.Id,
                        rankKey = item.Value.Rank,
                        probabilityBase = item.Value.ProbabilityBase / 10000f,//项目配置基础出现概率，精确到小数点后两位
                        weight = item.Value.Weight,
                    });
            }
            else
            {
                Debug.LogWarning($"Entrust_ItemConfig  ---  委托配置表数据错误，Id为{item.Value.Id}，抬头名称为{item.Value.Title}的委托，Id重复，请检查配置表！！！！");
            }
        }
    }

    private bool CreateEntrustItem(EntrustSystemManager.EntrustRandomItemDynamicInfo entrustRandomItemDynamicInfo, out EntrustItem entrustItem)
    {
        Entrust_ItemConfig config;
        if (!m_EntrustItemConfigDic_Rank.ContainsKey(entrustRandomItemDynamicInfo.RankKey) && m_EntrustItemConfigDic_Rank[entrustRandomItemDynamicInfo.RankKey].ContainsKey(entrustRandomItemDynamicInfo.id))
        {
            entrustItem = new EntrustItem();
            Debug.LogWarning($"CreateEntrustItem  ---  生成委托时失败，无法查询到RankKey为{entrustRandomItemDynamicInfo.RankKey}，Id为{entrustRandomItemDynamicInfo.id}的委托配置数据，请检查配置表和配置表初始化。");
            return false;
        }

        config = m_EntrustItemConfigDic_Rank[entrustRandomItemDynamicInfo.RankKey][entrustRandomItemDynamicInfo.id];

        entrustItem = new EntrustItem();
        entrustItem.Init(
            new EntrustSystem.EntrustItemData(
                config.Id,
                config.Title,
                config.DescribeSimple,
                config.Describe,
                config.Type,
                config.Rank,
                config.RoundsLimit,
                config.RoundsConsumeBase,
                config.CanTryMultipleTimes == 1,
                config.VenturerNumMust,
                config.VenturerNumOptional
                ));

        //初始化委托条件
        if(config.ConditionMap != null && config.ConditionMap.Count > 0)
        {
            EntrustConditionItem[] entrustConditionItems = new EntrustConditionItem[config.ConditionMap.Count];

            int index = 0;
            foreach (var item in config.ConditionMap)
            {
                var conditionConfig = ConfigSystem.Instance.GetConfig<Entrust_Condition>(item.Key);
                if (conditionConfig == null) continue;

                string[] conditionParams = item.Value.Split('|');
                string[] conditionItemParams = conditionParams[0].Split('-');

                entrustConditionItems[index] =
                    new EntrustConditionItem(
                        entrustItem,
                        conditionConfig.ClassName,
                        conditionParams[1],
                        item.Key,
                        int.Parse(conditionItemParams[0]),
                        int.Parse(conditionItemParams[1]) == 1 ? true : false,
                        int.Parse(conditionItemParams[2]) == 1 ? true : false);

                index++;
            }

            entrustItem.SetConditionData(entrustConditionItems);
        }

        BindEventWithEntrustItem(entrustItem);

        return true;
    }

    private void BindEventWithEntrustItem(EntrustItem entrustItem)
    {
        //EntrustItem销毁时无需解绑，因为对象会被销毁

        entrustItem.BindEventWithStateChange(OnStateChange);
        entrustItem.BindEventWithRoundsChange(OnRoundsChange);
        entrustItem.BindEventWithStart(OnStart);
        entrustItem.BindEventWithComplete(OnComplete);
        entrustItem.BindEventWithStatement(OnStatement);
        entrustItem.BindEventWithTimeout(OnTimeout);
        entrustItem.BindEventWithVenturerAddOrRemove(OnVenturerAddOrRemove);
    }

    /// <summary>
    /// 设置公会等级
    /// 根据公会等级，我们会获取需要的委托等级列表
    /// </summary>
    /// <param name="guildRank">公会等级</param>
    public void SetGuildRank(int guildRank)
    {
        if (m_GuildRank == guildRank) return;

        m_GuildRank = guildRank;

        var config = ConfigSystem.Instance.GetConfig<Guild_Rank>(m_GuildRank);

        int index = 0;
        List<EntrustSystemManager.EntrustRandomRankInfo> entrustRandomRankInfo = new List<EntrustSystemManager.EntrustRandomRankInfo>();
        foreach (var item in config.EntrustRankWeight)
        {
            if (item.Value == 0) continue;

            entrustRandomRankInfo.Add(new EntrustSystemManager.EntrustRandomRankInfo()
            {
                rankKey = item.Key,
                //TODO:声望加成影响数量权重
                numWeight = item.Value,
                probabilityAdjustIntensity = (7.1f - item.Key) / 7f //委托等级越高，出现概率修正强度越小//TODO更加细分化时可以在配置表中配置，现在统一根据委托等级进行设置
            });
            index++;
        }

        //设置需要获取的委托等级信息
        m_EntrustSystemManager.SetEntrustRandomSpawnGetEntrustRanks(entrustRandomRankInfo.ToArray());

        //设置生成委托期望总数
        //TODO:获取公会声望加成信息，加成委托期望总数
        m_EntrustSystemManager.SetEntrustRandomSpawnExpectsTotalNumber(config.EntrustCount);
    }

    /// <summary>
    /// 当回合更新时 生成新的委托
    /// </summary>
    public void EntrustRandomSpawnCalculate()
    {
        m_EntrustSystemManager.EntrustRandomSpawn(CreateEntrustItem);
    }

    #endregion

    #region Func 允许外部调用 执行的动作

    /// <summary>
    /// 获取世界池中的委托数据
    /// </summary>
    /// <param name="entrustGetType">获取类型 确认需要获取那些委托</param>
    /// <returns></returns>
    public EntrustItemHandler[] GetEntrustHandles(EEntrustPoolType entrustGetType)
    {
        return m_EntrustSystemManager.GetEntrustHandlers(entrustGetType, out bool isDirty);
    }

    /// <summary>
    /// 转换类型Type到项目定义的Enum
    /// </summary>
    /// <param name="entrustItem"></param>
    /// <returns></returns>
    public EEntrustType GetEntrustTypeTrans(EntrustItemHandler entrustItemHandler)
    {
        return (EEntrustType)entrustItemHandler.Type;
    }

    public bool GetEntrustItemHandle(int entrustId, out EntrustItemHandler entrustItemHandler)
    {
        if(m_EntrustSystemManager == null)
        {
            entrustItemHandler = null;
            return false;
        }
        return m_EntrustSystemManager.GetEntrustItemHandle(entrustId, out entrustItemHandler);
    }

    /// <summary>
    /// 受理委托
    /// </summary>
    /// <param name="entrustId"></param>
    public bool AcceptEntrust(int entrustId)
    {
        return m_EntrustSystemManager.AcceptedEntrust(entrustId);
    }

    /// <summary>
    /// 放弃委托
    /// </summary>
    /// <param name="entrustId"></param>
    /// <returns></returns>
    public bool AbortEntrust(int entrustId)
    {
        return m_EntrustSystemManager.AbortEntrust(entrustId);
    }

    /// <summary>
    /// 开始委托
    /// </summary>
    /// <param name="entrustId"></param>
    /// <returns></returns>
    public bool StartEntrust(int entrustId)
    {
        return m_EntrustSystemManager.StartEntrust(entrustId);
    }

    /// <summary>
    /// 取消一个正在执行中的委托
    /// </summary>
    /// <param name="entrustId"></param>
    /// <returns></returns>
    public bool CancelEntrust(int entrustId)
    {
        return m_EntrustSystemManager.CancelEntrust(entrustId);
    }

    /// <summary>
    /// 结算委托
    /// </summary>
    /// <param name="entrustId"></param>
    /// <returns></returns>
    public bool StatementEntrust(int entrustId)
    {
        return m_EntrustSystemManager.StatementEntrust(entrustId);
    }

    /// <summary>
    /// 添加一个冒险者到委托队伍中
    /// </summary>
    /// <param name="entrustId"></param>
    /// <param name="venturerId"></param>
    /// <returns></returns>
    public bool AddVenturerToEntrust(int entrustId, int venturerId)
    {
        //冒险正正在工作中
        if (m_EntrustSystemManager.CheckVenturerIsWorking(venturerId))
        {
            Debug.Log(string.Format("EntrustModel::AddVenturerToEntrust : 冒险者{0}不能加入委托{1}，因为他正在另一个正在进行中的委托队伍中。", venturerId, entrustId));
            return false;
        }

        return m_EntrustSystemManager.AddVenturerToEntrust(entrustId, venturerId);
    }

    /// <summary>
    /// 移除一个冒险者从委托队伍中
    /// </summary>
    /// <param name="entrustId"></param>
    /// <param name="venturerId"></param>
    /// <returns></returns>
    public bool RemoveVenturerFromEntrust(int entrustId, int venturerId)
    {
        return m_EntrustSystemManager.RemoveVenturerFromEntrust(entrustId, venturerId);
    }

    /// <summary>
    /// 确认某个冒险者Id是否在某个委托的队伍中
    /// </summary>
    /// <param name="entrustId"></param>
    /// <param name="venturerId"></param>
    /// <returns></returns>
    public bool ContainsVenturerInEntrust(int entrustId, int venturerId)
    {
        return m_EntrustSystemManager.ContainsVenturerInEntrust(entrustId, venturerId);
    }

    /// <summary>
    /// 确认冒险者是否正在工作中
    /// 冒险者处于Underway，Complete的委托时，认为冒险者正在工作中
    /// 此时冒险者一般被锁定在此委托
    /// </summary>
    /// <param name="venturerId"></param>
    /// <returns>是否处在工作中</returns>
    public bool CheckVenturerIsWorking(int venturerId)
    {
        return m_EntrustSystemManager.CheckVenturerIsWorking(venturerId);
    }
    #endregion

    #region EntrustItem Action 委托事件
    public Action<int, EEntrustState, EEntrustState> onEntrustStateChange;

    private void OnStateChange(int entrustId, EEntrustState oldState, EEntrustState newState)
    {
        onEntrustStateChange?.Invoke(entrustId, oldState, newState);
    }

    private void OnRoundsChange(int entrustId, EEntrustState state, int round, int roundUnderway, bool exceedLimitRounds)
    {
        if (!GetEntrustItemHandle(entrustId, out EntrustItemHandler entrustItemHandler)) return;
        
        Entrust_ItemConfig config = ConfigSystem.Instance.GetConfig<Entrust_ItemConfig>(entrustId);
        if (config == null) return;

        //TODO:将委托中发生的事件信息记录到展示档案

        //委托进行中
        if(state == EEntrustState.Underway)
        {
            //委托发生事件
            List<Guid> incidentPackGuids = new List<Guid>();
            for (int i = 0; i < config.IncidentPacks.Count; i++)
            {
                if (Guid.TryParse(config.IncidentPacks[i], out Guid guid))
                {
                    incidentPackGuids.Add(guid);
                }
            }

            CCD_Entrust ccd_entrustVenturerTeam = new CCD_Entrust(entrustItemHandler);

            //发生事件
            if (AdventureModel.Instance.IncidentHappen(incidentPackGuids.ToArray(), out IncidentHandler incidentHandler, ccd_entrustVenturerTeam))
            {
                //委托队伍自动处理事件
                incidentHandler.Start();
                while (!incidentHandler.IsEnd)
                {
                    //展示段落
                    if(incidentHandler.GetNodeCurParagraphs(out Paragraph[] paragraphs))
                    {
                        for (int i = 0; i < paragraphs.Length; i++)
                        {
                            Debugx.Log(1, $"{paragraphs[i].actor}: {paragraphs[i].describe}");
                        }
                    }

                    //获取符合条件的选择
                    incidentHandler.GetChooses(out IncidentChooseConfig[] chooses, ccd_entrustVenturerTeam);
                    m_RandomIndexCached.Clear();
                    for (int i = 0; i < chooses.Length; i++)
                    {
                        var choose = chooses[i];
                        if (!choose.checkConditionCached) continue;
                        m_RandomIndexCached.Add(i);
                    }

                    //没有可选择的选项，直接结束
                    if (m_RandomIndexCached.Count == 0)
                    {
                        incidentHandler.End();

                        Debugx.Log(1, "没有可用的选择");
                    }
                    //随机进行一个选择
                    else
                    {
                        //TODO:根据冒险者特性，有不同的选择倾向。可以在Choose上配置Tag来进行匹配，提高选择倾向
                        var index = StoryIncidentLibrary.Random(m_RandomIndexCached.ToArray());
                        incidentHandler.MakeChoose(index, ccd_entrustVenturerTeam);

                        Debugx.Log(1, $"选择了：{chooses[index].Name()}");
                    }
                }

                //事件结束
                AdventureModel.Instance.IncidentDone(incidentHandler);
            }
        }

        //TODO 委托进行时，一些事件可能改变不同结果的权重。可以在Incident上配置Tag来进行匹配，改变权重效果
        //m_EntrustSystemManager.SetEntrustResultItemWeight()
    }

    private List<int> m_RandomIndexCached = new List<int>();

    //委托开始时
    private void OnStart(int entrustId)
    {
        //获取成功率
        if (!GetEntrustSuccessRate(entrustId, out float successRate)) return;

        //结果权重计算
        //失败和成功结果共享0-1系数区间，大成功1-2系数区间，极大成功2-3系数区间
        EntrustResultItem[] entrustResultItems = new EntrustResultItem[] 
        {
            new EntrustResultItem((int)EEntrustResultType.Failed, GetResultWeight(EEntrustResultType.Failed, successRate)),
            new EntrustResultItem((int)EEntrustResultType.Succeed, GetResultWeight(EEntrustResultType.Succeed, successRate)),
            new EntrustResultItem((int)EEntrustResultType.GreateSucceed, GetResultWeight(EEntrustResultType.GreateSucceed, successRate)),
            new EntrustResultItem((int)EEntrustResultType.ExcellentSucceed, GetResultWeight(EEntrustResultType.ExcellentSucceed, successRate)),
        };

        //结果输入信息
        //委托底层只在乎结果输入信息，并在结束时返回指定数量的结果
        //不在乎业务层以“成功率”或者“隐藏结果”等方式定义
        EntrustResultInputData[] entrustResultInputDatas = new EntrustResultInputData[]
        {
            new EntrustResultInputData()
            {
                resultNum = 1,
                entrustResultItems = new List<EntrustResultItem>(entrustResultItems)
            }
        };

        m_EntrustSystemManager.SetResultData(entrustId, entrustResultInputDatas);

        //发送委托消息
        MessageDispatcher.SendMessageData(
            EntrustModelMsgType.ENTRUSTMODEL_ENTRUST_START,
            new EntrustCommonInfo()
            {
                entrustId = entrustId,
            });
    }

    //委托完成时
    private void OnComplete(int entrustId, int roundUnderway)
    {
        if (!GetEntrustItemHandle(entrustId, out EntrustItemHandler entrustItemHandler)) return;

        Debug.Log($"委托完成！委托Id：{entrustId}，委托名称：{entrustItemHandler.Title}");

        OnEntrustItemComplete_InformationStats(entrustItemHandler);
    }

    //委托结算时
    private void OnStatement(int entrustId)
    {
        if (!GetEntrustItemHandle(entrustId, out EntrustItemHandler entrustItemHandler)) return;

        Debug.Log($"委托结算！委托Id：{entrustId}，委托名称：{entrustItemHandler.Title}");

        //结算时分配奖励
        Entrust_Reward entrustReward = ConfigSystem.Instance.GetConfig<Entrust_Reward>(entrustId);
        int coinBase = entrustReward.Coin;
        int prestigBase = entrustReward.Prestige;
        int expBase = entrustReward.VentureExp;
        MapField<int, int> propMap = null;

        //根据任务结果，调整奖励
        if (entrustItemHandler.GetEntrustResult(out EntrustResultInfo outEntrustResultInfo))
        {
            var entrustResultInfoItem = outEntrustResultInfo.entrustResultInfoItems[0];
            var entrustResultItem = entrustResultInfoItem.entrustResultItems[0];
            EEntrustResultType resultType = (EEntrustResultType)entrustResultItem.resultId;

            switch (resultType)
            {
                case EEntrustResultType.Failed:
                    coinBase = Mathf.CeilToInt(coinBase * 0.1f);
                    prestigBase = -Mathf.CeilToInt(prestigBase * 0.2f);
                    expBase = Mathf.CeilToInt(expBase * 1.5f);
                    propMap = entrustReward.PropMapFailed;
                    break;
                case EEntrustResultType.Succeed:
                    //coin = Mathf.CeilToInt(coin * 1f);
                    //prestig = Mathf.CeilToInt(prestig * 1f);
                    //expBase = Mathf.CeilToInt(expBase * 1f);
                    propMap = entrustReward.PropMapSucceed;
                    break;
                case EEntrustResultType.GreateSucceed:
                    //coin = Mathf.CeilToInt(coin * 1.1f);
                    prestigBase = Mathf.CeilToInt(prestigBase * 1.1f);
                    expBase = Mathf.CeilToInt(expBase * 1.1f);
                    propMap = entrustReward.PropMapGreateSucceed;
                    break;
                case EEntrustResultType.ExcellentSucceed:
                    //coin = Mathf.CeilToInt(coin * 1.2f);
                    prestigBase = Mathf.CeilToInt(prestigBase * 1.2f);
                    expBase = Mathf.CeilToInt(expBase * 1.2f);
                    propMap = entrustReward.PropMapExcellentSucceed;
                    break;
            }
        }

        //获得货币
        PlayerModel.Instance.ChangeCoinCount(coinBase);

        //公会声望增加
        GuildModel.Instance.ChangePrestigeValue(prestigBase);

        //冒险者获得经验
        var venturerTeam = entrustItemHandler.GetVenturerTeam();
        if(venturerTeam.Length > 0)
        {
            for (int i = 0; i < venturerTeam.Length; i++)
            {
                int exp = expBase;
                //TODO :根据不同参与冒险者情况调整实际获得的经验
                VenturerModel.Instance.ChangeVenturerExp(venturerTeam[i], exp);
            }
        }

        //获得道具奖励
        if (propMap != null && propMap.Count > 0)
        {
            foreach (var item in propMap)
            {
                PlayerModel.Instance.AddPropInfo(item.Key, item.Value);
            }
        }

        //发送委托结算消息
        MessageDispatcher.SendMessageData(
            EntrustModelMsgType.ENTRUSTMODEL_ENTRUST_STATEMENT,
            new EntrustCommonInfo()
            {
                entrustId = entrustId,
            });

        //结算结束，销毁这个委托
        m_EntrustSystemManager.DestroyEntrust(entrustId);
    }

    //委托超时时
    private void OnTimeout(int entrustId, EEntrustState state)
    {
        m_EntrustSystemManager.DestroyEntrust(entrustId);
    }

    //当冒险者添加或移除时
    public void OnVenturerAddOrRemove(int entrustId, bool add, int venturerIndex, int venturerId)
    {
        if (!GetEntrustItemHandle(entrustId, out EntrustItemHandler entrustItemHandler)) return;

        //如果任务在进行中，更新基础权重
        if (entrustItemHandler.State == EEntrustState.Underway && GetEntrustSuccessRate(entrustId, out float successRate))//获取成功率
        {
            //委托结果Index0的内容，失败和成功结果共享0-1系数区间，大成功1-2系数区间，极大成功2-3系数区间
            m_EntrustSystemManager.SetEntrustResultItemWeightBase(entrustId, 0, (int)EEntrustResultType.Failed, GetResultWeight(EEntrustResultType.Failed, successRate));
            m_EntrustSystemManager.SetEntrustResultItemWeightBase(entrustId, 0, (int)EEntrustResultType.Succeed, GetResultWeight(EEntrustResultType.Succeed, successRate));
            m_EntrustSystemManager.SetEntrustResultItemWeightBase(entrustId, 0, (int)EEntrustResultType.GreateSucceed, GetResultWeight(EEntrustResultType.GreateSucceed, successRate));
            m_EntrustSystemManager.SetEntrustResultItemWeightBase(entrustId, 0, (int)EEntrustResultType.ExcellentSucceed, GetResultWeight(EEntrustResultType.ExcellentSucceed, successRate));
        }
    }

    #endregion

    #region SucceedRate 成功率
    /// <summary>
    /// 获取委托成功率
    /// </summary>
    /// <param name="entrustId"></param>
    /// <param name="successRate"></param>
    /// <returns></returns>
    public bool GetEntrustSuccessRate(int entrustId, out float successRate)
    {
        //成功率0-3，1为100%，3为300%。0-1区间处理失败和成功结果，大于1的系数处理其他结果
        successRate = 0;

        if (!GetEntrustItemHandle(entrustId, out EntrustItemHandler entrustItemHandler)) return false;

        //计算结果概率。初始化到委托。失败，成功，大成功，极大成功
        if (entrustItemHandler.IsHaveCondition)
        {
            //根据配置的委托条件进行结果权重计算
            //一些基本规则
            //1.委托成功率 = (条件1 * 条件1权重 + 条件2 * 条件2权重 + ...) / 总权重
            //解释：总条件完成系数0-3 * 调整系数（按照结果数量将完成系数映射到0-1或者0-2或者0-3）
            //2.每个条件返回的完成系数应当在0-3区间，这样才能保证最后计算得出的成功系数能在0-3区间。
            //3.委托可以参加的冒险者总数等于必要数量加额外可选数量，一般保证符合必要人数和基准要求后达到达成率1，额外可选冒险者用于提升达成率
            //4.可选冒险者的效果弱于必要冒险者

            if (entrustItemHandler.ConditionItemsAchievingRateCalculate(out float weightTotal, out float achievingRateWeightNumTotal))
            {
                if (achievingRateWeightNumTotal != 0f && weightTotal != 0f)
                    successRate = achievingRateWeightNumTotal / weightTotal;
                else
                    successRate = 0f;
            }
        }
        else
        {
            //根据委托等级和参与者等级进行结果权重计算
            int[] venturerTeam = entrustItemHandler.GetVenturerTeam();
            for (int i = 0; i < venturerTeam.Length; i++)
            {
                VenturerInfo venturerInfo = VenturerModel.Instance.GetVenturerInfo(venturerTeam[i]);
                if (venturerInfo == null) continue;

                float rankRate = venturerInfo.RankId * 1f / entrustItemHandler.Rank;//冒险者阶级和委托阶级比率
                                                                                   //委托成功率 = 必要冒险者阶级比率总和 / 必要冒险者数量 + 额外冒险者阶级比率总和 / 必要冒险者数量 * 修正系数
                if (i < entrustItemHandler.VenturerNumMust)
                {
                    successRate += rankRate / entrustItemHandler.VenturerNumMust;
                }
                else
                {
                    //额外槽位效果较弱
                    successRate += rankRate / entrustItemHandler.VenturerNumMust * 0.8f;
                }
            }
        }

//#if UNITY_EDITOR
//        Debug.Log($"GetEntrustSuccessRate  ---  SuccessRateClampBefore : {successRate}");
//#endif

        successRate = Mathf.Clamp(successRate, 0f, 3f);

//#if UNITY_EDITOR
//        Debug.Log($"GetEntrustSuccessRate  ---  SuccessRateClampAfter : {successRate}");
//#endif

        return true;
    }

    /// <summary>
    /// 获取不同结果权重
    /// </summary>
    /// <param name="resultType"></param>
    /// <param name="successRate"></param>
    /// <returns></returns>
    private int GetResultWeight(EEntrustResultType resultType, float successRate)
    {
        switch (resultType)
        {
            case EEntrustResultType.Failed:
                return successRate < 1 ? Mathf.CeilToInt(10000 * (1 - successRate)) : 0;
            case EEntrustResultType.Succeed:
                return successRate < 1 ? Mathf.CeilToInt(10000 * successRate) : 10000;
            case EEntrustResultType.GreateSucceed:
                return successRate > 1 ? 1000 * Mathf.CeilToInt(successRate - 1) : 0;
            case EEntrustResultType.ExcellentSucceed:
                return successRate > 2 ? 200 * Mathf.CeilToInt(successRate - 2) : 0;
        }

        return 0;
    }
    #endregion

    #region 与其他模块沟通的消息

    public void MsgInit()
    {
        MessageDispatcher.AddListener(VenturerModelMsgType.VENTURERMODEL_VENTURERPOOL_RED, MsgVentureReduce);
        MessageDispatcher.AddListener(TimeModelMsgType.TIMEMODEL_ERAYEAR_CHANGE, MsgEraYearChange);
    }

    //有冒险者离开时
    private void MsgVentureReduce(IMessage rMessage)
    {
        var vInfo = rMessage.Data as VenturerInfo;

        //冒险者离开时，从他参与的委托中移除
        if (m_EntrustSystemManager.GetEntrustItemIdsByVenturerId(vInfo.Id, out int[] entrustIds))
        {
            for (int i = 0; i < entrustIds.Length; i++)
            {
                RemoveVenturerFromEntrust(entrustIds[i], vInfo.Id);
            }
        }
    }

    private void MsgEraYearChange(IMessage rMessage)
    {
        TimeModel.TimeInfo info = (TimeModel.TimeInfo)rMessage.Data;

        //m_InformationStats.ResetYearStats();
    }
    #endregion

    #region Information Stats 信息统计

    /// <summary>
    /// 委托信息统计类
    /// </summary>
    public class EntrustModelInformationStats
    {
        public EntrustModelInformationStats()
        {
            entrustItemCountDic_Complete = new Dictionary<int, int>();
            entrustItemCountDic_Succeed = new Dictionary<int, int>();

            entrustItemCountDic_Complete_Year = new Dictionary<int, int>();
            entrustItemCountDic_Succeed_Year = new Dictionary<int, int>();
        }

        //Key=Rank
        public Dictionary<int, int> entrustItemCountDic_Complete;//完成的委托
        public Dictionary<int, int> entrustItemCountDic_Succeed;//成功的委托
        public int entrustItemTotal_Complete;
        public int entrustItemTotal_Succeed;

        //Key=Rank，每年重置
        public Dictionary<int, int> entrustItemCountDic_Complete_Year;//完成的委托
        public Dictionary<int, int> entrustItemCountDic_Succeed_Year;//成功的委托
        public int entrustItemTotal_Complete_Year;
        public int entrustItemTotal_Succeed_Year;

        /// <summary>
        /// 重置年份统计数据
        /// </summary>
        public void ResetYearStats()
        {
            entrustItemCountDic_Complete_Year.Clear();
            entrustItemCountDic_Succeed_Year.Clear();
            entrustItemTotal_Complete_Year = 0;
            entrustItemTotal_Succeed_Year = 0;
        }
    }

    private EntrustModelInformationStats m_InformationStats;

    public void InformationStatsInit()
    {
        
    }

    public void InformationStatsLoadData(EntrustModelSaveData entrustModelSaveData)
    {
        if (entrustModelSaveData != null && entrustModelSaveData.informationStats != null)
        {
            m_InformationStats = entrustModelSaveData.informationStats;
        }
        else
        {
            m_InformationStats = new EntrustModelInformationStats();
        }
    }

    public void InformationStatsSaveData(EntrustModelSaveData entrustModelSaveData)
    {
        entrustModelSaveData.informationStats = m_InformationStats;
    }

    private void OnEntrustItemComplete_InformationStats(EntrustItemHandler entrustItemHandler)
    {
        //完成委托计数
        if (!m_InformationStats.entrustItemCountDic_Complete.ContainsKey(entrustItemHandler.Rank))
            m_InformationStats.entrustItemCountDic_Complete.Add(entrustItemHandler.Rank, 0);
        m_InformationStats.entrustItemCountDic_Complete[entrustItemHandler.Rank]++;
        m_InformationStats.entrustItemTotal_Complete++;

        if (!m_InformationStats.entrustItemCountDic_Complete_Year.ContainsKey(entrustItemHandler.Rank))
            m_InformationStats.entrustItemCountDic_Complete_Year.Add(entrustItemHandler.Rank, 0);
        m_InformationStats.entrustItemCountDic_Complete_Year[entrustItemHandler.Rank]++;
        m_InformationStats.entrustItemTotal_Complete_Year++;

        //成功委托计数
        entrustItemHandler.GetEntrustResult(out EntrustResultInfo outEntrustResultInfo);
        if(outEntrustResultInfo.GetFirstResultId(out int resultId))
        {
            if((EEntrustResultType)resultId != EEntrustResultType.Failed)
            {
                if (!m_InformationStats.entrustItemCountDic_Succeed.ContainsKey(entrustItemHandler.Rank))
                    m_InformationStats.entrustItemCountDic_Succeed.Add(entrustItemHandler.Rank, 0);
                m_InformationStats.entrustItemCountDic_Succeed[entrustItemHandler.Rank]++;
                m_InformationStats.entrustItemTotal_Succeed++;

                if (!m_InformationStats.entrustItemCountDic_Succeed_Year.ContainsKey(entrustItemHandler.Rank))
                    m_InformationStats.entrustItemCountDic_Succeed_Year.Add(entrustItemHandler.Rank, 0);
                m_InformationStats.entrustItemCountDic_Succeed_Year[entrustItemHandler.Rank]++;
                m_InformationStats.entrustItemTotal_Succeed_Year++;
            }
        }
    }

    /// <summary>
    /// 重置按年统计数据
    /// </summary>
    public void ResetYearStats()
    {
        m_InformationStats.ResetYearStats();
    }

    /// <summary>
    /// 完成的委托总数
    /// </summary>
    /// <param name="rank"></param>
    /// <returns></returns>
    public int GetCompleteEntrustCount(int rank)
    {
        m_InformationStats.entrustItemCountDic_Complete.TryGetValue(rank, out int count);
        return count;
    }

    /// <summary>
    /// 成功的委托总数
    /// </summary>
    /// <param name="rank"></param>
    /// <returns></returns>
    public int GetSucceedEntrustCount(int rank)
    {
        m_InformationStats.entrustItemCountDic_Succeed.TryGetValue(rank, out int count);
        return count;
    }

    /// <summary>
    /// 今年完成的委托总数
    /// </summary>
    /// <param name="rank"></param>
    /// <returns></returns>
    public int GetCompleteEntrustCountCurYear(int rank)
    {
        m_InformationStats.entrustItemCountDic_Complete_Year.TryGetValue(rank, out int count);
        return count;
    }

    /// <summary>
    /// 今年成功的委托总数
    /// </summary>
    /// <param name="rank"></param>
    /// <returns></returns>
    public int GetSucceedEntrustCountCurYear(int rank)
    {
        m_InformationStats.entrustItemCountDic_Succeed_Year.TryGetValue(rank, out int count);
        return count;
    }

    /// <summary>
    /// 获取 受理委托完成率 百分比
    /// </summary>
    /// <returns></returns>
    public int GetEntrustSucceedPercent()
    {
        float percent = m_InformationStats.entrustItemTotal_Succeed / m_InformationStats.entrustItemTotal_Complete;
        return (int)(percent * 100f);
    }

    /// <summary>
    /// 获取 今年 受理委托完成率 百分比
    /// </summary>
    /// <returns></returns>
    public int GetEntrustSucceedPercentCurYear()
    {
        float percent = m_InformationStats.entrustItemTotal_Succeed_Year / m_InformationStats.entrustItemTotal_Complete_Year;
        return (int)(percent * 100f);
    }

    #endregion
}
