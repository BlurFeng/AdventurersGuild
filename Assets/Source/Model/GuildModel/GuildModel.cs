using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deploy;
using com.ootii.Messages;
using FsListItemPages;

public class GuildModel : Singleton<GuildModel>, IDestroy, ISaveData
{
    /// <summary>
    /// 初始化
    /// </summary>
    public override void Init()
    {
        base.Init();

        MessageDispatcher.AddListener(EntrustModelMsgType.ENTRUSTMODEL_ENTRUST_STATEMENT, MsgEntrustStatement);
    }

    public override void Destroy()
    {
        base.Destroy();

    }

    public void SaveData(ES3File saveData)
    {
        saveData.Save<string>("GuildModel_GuildInfo", PlayerPrefsUtil.SerializeData(m_GuildInfo));
    }

    public void LoadData(ES3File saveData)
    {
        string strGuildInfo = saveData.Load<string>("GuildModel_GuildInfo", string.Empty);

        //公会信息
        if (!string.IsNullOrEmpty(strGuildInfo))
        {
            //有存档 使用存档数据
            m_GuildInfo = PlayerPrefsUtil.DeserializeData<GuildInfo>(strGuildInfo);
        }
        else
        {
            //初始数据
            m_GuildInfo.GuildRank = 1;
            m_GuildInfo.PrestigeValue = 500;
        }
    }

    #region 养成数值

    [Serializable]
    /// <summary>
    /// 公会信息
    /// </summary>
    public struct GuildInfo
    {
        /// <summary>
        /// 公会阶级
        /// </summary>
        public int GuildRank;

        /// <summary>
        /// 规模值
        /// </summary>
        public int BuildingScaleValue;

        /// <summary>
        /// 设施值
        /// </summary>
        public int FacilityValue;

        /// <summary>
        /// 声望值
        /// </summary>
        public int PrestigeValue;
    }

    //公会信息
    private GuildInfo m_GuildInfo;

    /// <summary>
    /// 公会阶级
    /// </summary>
    public int GuildRank { get { return m_GuildInfo.GuildRank; } }

    /// <summary>
    /// 规模值
    /// </summary>
    public int BuildingScaleValue { get { return m_GuildInfo.BuildingScaleValue; } }

    /// <summary>
    /// 设施值
    /// </summary>
    public int FacilityValue { get { return m_GuildInfo.FacilityValue; } }

    /// <summary>
    /// 声望值
    /// </summary>
    public int PrestigeValue { get { return m_GuildInfo.PrestigeValue; } }

    /// <summary>
    /// 增加 规模值
    /// </summary>
    /// <param name="buildingLevelId">某等级的建筑ID</param>
    /// <param name="sendMessage"></param>
    public void AddBuildingScaleValue(int buildingLevelId, bool sendMessage = true)
    {
        var cfg = ConfigSystem.Instance.GetConfig<Building_Level>(buildingLevelId);

        m_GuildInfo.BuildingScaleValue += cfg.BuildingScaleValue;

        //发送消息 公会信息改变
        if (sendMessage)
            MessageDispatcher.SendMessage(GuildModelMsgType.GUILDMODEL_GUILDINFO_CHANGE);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="buildingLevelId"></param>
    /// <param name="sendMessage"></param>
    public void RemoveBuildingScaleValue(int buildingLevelId, bool sendMessage = true)
    {
        var cfg = ConfigSystem.Instance.GetConfig<Building_Level>(buildingLevelId);

        m_GuildInfo.BuildingScaleValue -= cfg.BuildingScaleValue;

        //发送消息 公会信息改变
        if (sendMessage)
            MessageDispatcher.SendMessage(GuildModelMsgType.GUILDMODEL_GUILDINFO_CHANGE);
    }

    /// <summary>
    /// 增加 设施值
    /// </summary>
    /// <param name="furnitureId"></param>
    /// <param name="sendMessage"></param>
    public void AddFacilityValue(int furnitureId, bool sendMessage = true)
    {
        var cfgFurniture = ConfigSystem.Instance.GetConfig<Prop_Furniture>(furnitureId);

        m_GuildInfo.FacilityValue += cfgFurniture.FacilityValue;

        //发送消息 公会信息改变
        if (sendMessage)
            MessageDispatcher.SendMessage(GuildModelMsgType.GUILDMODEL_GUILDINFO_CHANGE);
    }

    /// <summary>
    /// 减少 设施值
    /// </summary>
    /// <param name="furnitureId"></param>
    /// <param name="sendMessage"></param>
    public void RemoveFacilityValue(int furnitureId, bool sendMessage = true)
    {
        var cfgFurniture = ConfigSystem.Instance.GetConfig<Prop_Furniture>(furnitureId);

        m_GuildInfo.FacilityValue -= cfgFurniture.FacilityValue;

        //发送消息 公会信息改变
        if (sendMessage)
            MessageDispatcher.SendMessage(GuildModelMsgType.GUILDMODEL_GUILDINFO_CHANGE);
    }

    /// <summary>
    /// 改变 声望值
    /// </summary>
    /// <param name="valueChange">增减的值</param>
    public void ChangePrestigeValue(int valueChange)
    {
        m_GuildInfo.PrestigeValue += valueChange;

        //发送消息 公会信息改变
        MessageDispatcher.SendMessage(GuildModelMsgType.GUILDMODEL_GUILDINFO_CHANGE);
    }

    /// <summary>
    /// 改变 公会等级
    /// </summary>
    /// <param name="valueChange">增减的值</param>
    public void ChangeGuildLevel(int valueChange)
    {
        m_GuildInfo.GuildRank += valueChange;

        //发送消息 公会信息改变
        MessageDispatcher.SendMessage(GuildModelMsgType.GUILDMODEL_GUILDINFO_CHANGE);
    }

    //消息 委托完成
    private void MsgEntrustStatement(IMessage rMessage)
    {
        EntrustCommonInfo entrustCompleteInfo = (EntrustCommonInfo)rMessage.Data;

        Entrust_Reward entrustReward = ConfigSystem.Instance.GetConfig<Entrust_Reward>(entrustCompleteInfo.entrustId);
    }

    #endregion

    #region 公会评定

    /// <summary>
    /// 公会 评定信息
    /// </summary>
    public struct GuildEvaluateInfo
    {
        /// <summary>
        /// 规模值 得分
        /// </summary>
        public int BuildingScaleScore;

        /// <summary>
        /// 设施值 得分
        /// </summary>
        public int FacilityScore;

        /// <summary>
        /// 声望值 得分
        /// </summary>
        public int PrestigeScore;

        /// <summary>
        /// 委托得分
        /// </summary>
        public int EntrustScore;

        /// <summary>
        /// 委托完成率
        /// </summary>
        public int EntrustCompletePercent;

        /// <summary>
        /// 评定总分
        /// </summary>
        public int EvaluateTotalScore;
    }

    private int m_ScoreRatioBuildingScale = 1; //分数比例 建筑规模值
    private int m_ScoreRatioFacility = 1; //分数比例 设施值
    private int m_ScoreRatioPrestige = 1; //分数比例 声望值
    private int[] m_ScoreRatioEntrustArray = { 1, 4, 8, 16, 32, 64, 128 }; //分数比例 7个等级的委托

    /// <summary>
    /// 获取 公会评定信息
    /// </summary>
    /// <returns></returns>
    public GuildEvaluateInfo GetGuildEvaluateInfo()
    {
        var info = new GuildEvaluateInfo();
         
        info.BuildingScaleScore = m_GuildInfo.BuildingScaleValue * m_ScoreRatioBuildingScale; //规模值 得分
        info.FacilityScore = m_GuildInfo.FacilityValue * m_ScoreRatioFacility; //设施值 得分
        info.PrestigeScore = m_GuildInfo.PrestigeValue * m_ScoreRatioPrestige; //声望值 得分
        //委托得分
        int entrustScoreTotal = 0;
        for (int i = 0; i < m_ScoreRatioEntrustArray.Length; i++)
        {
            var entrustLevel = i + 1;
            var entrustScoreRatio = m_ScoreRatioEntrustArray[i];
            var entrustCompleteCount = EntrustModel.Instance.GetCompleteEntrustCountCurYear(entrustLevel);
            entrustScoreTotal += entrustCompleteCount * entrustScoreRatio;
        }
        info.EntrustScore = entrustScoreTotal;
        //委托完成率
        info.EntrustCompletePercent = EntrustModel.Instance.GetEntrustSucceedPercentCurYear();

        return info;
    }

    #endregion

    #region 功能区域

    /// <summary>
    /// 获取 可用功能区域 列表
    /// </summary>
    /// <returns></returns>
    public List<IItemPagesData> GetUsableListAreaCfgData()
    {
        //玩家当前所在区域
        if (GuildGridModel.Instance.PlayerAreaInfoCur == null) { return null; }

        //当前区域所属建筑 可用的功能区
        var listAreaData = new List<IItemPagesData>();
        listAreaData.Add(new ItemPagesData(0)); //默认可用功能区 空房间

        //根据区域组ID 获取建筑信息
        var buildingInfo = GuildGridModel.Instance.GetBuildingInfo(GuildGridModel.Instance.PlayerAreaInfoCur.AreaGroupId);
        if (buildingInfo != null) 
        {
            var cfgBuildingLevel = ConfigSystem.Instance.GetConfig<Building_Level>(buildingInfo.CfgBuildingLevelId);
            if (cfgBuildingLevel != null) 
            {
                
                for (int i = 0; i < cfgBuildingLevel.UsableAreaIdList.Count; i++)
                {
                    listAreaData.Add(new ItemPagesData(cfgBuildingLevel.UsableAreaIdList[i]));
                }
            }
        }

        return listAreaData;
    }

    #endregion
}
