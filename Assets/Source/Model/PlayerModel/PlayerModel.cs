using com.ootii.Messages;
using Google.Protobuf.Collections;
using Deploy;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 主角模块
/// </summary>
public class PlayerModel : Singleton<PlayerModel>, IDestroy, ISaveData
{
    public override void Init()
    {
        base.Init();
    }

    public void SaveData(ES3File saveData)
    {
        //保存 玩家信息
        saveData.Save<string>("PlayerModel_PlayerInfo", PlayerPrefsUtil.SerializeData(m_PlayerInfo));
    }

    public void LoadData(ES3File saveData)
    {
        string strPlayerModelPlayerInfo = saveData.Load<string>("PlayerModel_PlayerInfo", string.Empty);

        if (!string.IsNullOrEmpty(strPlayerModelPlayerInfo))
        {
            //有存档 使用存档数据
            m_PlayerInfo = PlayerPrefsUtil.DeserializeData<PlayerInfo>(strPlayerModelPlayerInfo);
        }
        else
        {
            //初始化玩家数据
            m_PlayerInfo = new PlayerInfo();
            m_PlayerInfo.Nickname = $"玩家名称{UnityEngine.Random.Range(0, 10)}";
            m_PlayerInfo.VenturerInfo = VenturerModel.Instance.SpawnNewVenturerInfo();
            m_PlayerInfo.VenturerInfo.Id = -1;
            m_PlayerInfo.VenturerInfo.RaceClanId = 19; //普莱茵人

            //初始道具
            AddPropInfo(10001, 35237); //钱币

            AddPropInfo(61001, 5);
            AddPropInfo(61002, 3);
            AddPropInfo(61003, 3);
            AddPropInfo(62001, 5);
            AddPropInfo(63001, 5);
            AddPropInfo(64001, 5);
            AddPropInfo(65001, 5);
            AddPropInfo(66001, 7);
        }
    }

    #region 玩家信息
    /// <summary>
    /// 玩家信息
    /// </summary>
    public PlayerInfo PlayerInfo { get { return m_PlayerInfo; } }
    private PlayerInfo m_PlayerInfo;

    /// <summary>
    /// 设置 玩家 冒险者信息
    /// </summary>
    /// <param name="info"></param>
    public void SetPlayerVenturerInfo(VenturerInfo info)
    {
        m_PlayerInfo.VenturerInfo = info;
        MessageDispatcher.SendMessage(PlayerModelMsgType.PLAYER_INFO_CHANGE);
    }

    /// <summary>
    /// 获取 玩家冒险者信息
    /// </summary>
    /// <returns></returns>
    public VenturerInfo GetPlayerVenturerInfo()
    {
        var venturerInfo = new VenturerInfo(m_PlayerInfo.VenturerInfo);
        return venturerInfo;
    }

    /// <summary>
    /// 主角昵称 修改
    /// </summary>
    /// <param name="nickname"></param>
    public void ChangeNickname(string nickname)
    {
        if (!string.IsNullOrEmpty(nickname))
        {
            m_PlayerInfo.Nickname = nickname;
        }
    }

    /// <summary>
    /// 获取 玩家世界坐标
    /// </summary>
    /// <returns></returns>
    public Vector3 GetPlayerPositionCur()
    {
        return new Vector3(m_PlayerInfo.PositionCur.X, m_PlayerInfo.PositionCur.Y, m_PlayerInfo.PositionCur.Z);
    }
    #endregion

    #region 玩家背包
    private Dictionary<PropModel.EPropType, string> m_DicPropBackpackChangeMsg = new Dictionary<PropModel.EPropType, string>() //道具背包的改变消息
    {
        {PropModel.EPropType.Property, PlayerModelMsgType.PROP_BACKPACK_CHANGE_PROPERTY},
        {PropModel.EPropType.Material, PlayerModelMsgType.PROP_BACKPACK_CHANGE_MATERIAL},
        {PropModel.EPropType.Function, PlayerModelMsgType.PROP_BACKPACK_CHANGE_FUNCTION},
        {PropModel.EPropType.Body, PlayerModelMsgType.PROP_BACKPACK_CHANGE_BODY},
        {PropModel.EPropType.Equip, PlayerModelMsgType.PROP_BACKPACK_CHANGE_EQUIP},
        {PropModel.EPropType.Furniture, PlayerModelMsgType.PROP_BACKPACK_CHANGE_FURNITURE},
        {PropModel.EPropType.Unique, PlayerModelMsgType.PROP_BACKPACK_CHANGE_UNIQUE},
    };

    /// <summary>
    /// 获取 背包道具变化 消息类型
    /// </summary>
    /// <param name="proptype">道具类型</param>
    /// <returns>消息类型</returns>
    public string GetPropBackpackChangeMsg(PropModel.EPropType proptype)
    {
        string eventType = string.Empty;
        m_DicPropBackpackChangeMsg.TryGetValue(proptype, out eventType);

        return eventType;
    }

    /// <summary>
    /// 获取 某ID的道具
    /// </summary>
    /// <param name="id">道具ID</param>
    /// <returns>道具信息</returns>
    public PropInfo GetPropInfo(int id)
    {
        return m_PlayerInfo.VenturerInfo.GetPropInfo(id);
    }

    /// <summary>
    /// 获取 某类型的所有道具
    /// </summary>
    /// <param name="type">道具类型</param>
    /// <returns>道具信息列表</returns>
    public Dictionary<int, PropInfo> GetPropInfos(PropModel.EPropType type)
    {
        return m_PlayerInfo.VenturerInfo.GetPropInfos(type);
    }

    /// <summary>
    /// 获取 某ID的道具的数量
    /// </summary>
    /// <param name="id">道具ID</param>
    /// <returns>道具数量</returns>
    public int GetPropCount(int id)
    {
        return m_PlayerInfo.VenturerInfo.GetPropCount(id);
    }

    /// <summary>
    /// 增加 某ID的道具
    /// </summary>
    /// <param name="id"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public PropInfo AddPropInfo(int id, int count = 0)
    {
        PropInfo propInfo = m_PlayerInfo.VenturerInfo.AddPropInfo(id, count);
        if (propInfo != null)
        {
            string msgType = GetPropBackpackChangeMsg((PropModel.EPropType)propInfo.Config.Type);
            MessageDispatcher.SendMessageData(msgType, propInfo);
        }

        return propInfo;
    }

    /// <summary>
    /// 移除 某ID的道具
    /// </summary>
    /// <param name="id"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public PropInfo RemovePropInfo(int id, int count = 0)
    {
        PropInfo propInfo = m_PlayerInfo.VenturerInfo.RemovePropInfo(id, count);
        if (propInfo != null)
        {
            string msgType = GetPropBackpackChangeMsg((PropModel.EPropType)propInfo.Config.Type);
            MessageDispatcher.SendMessageData(msgType, propInfo);
        }

        return propInfo;
    }

    /// <summary>
    /// 尝试消耗 某些道具指定数量
    /// </summary>
    /// <param name="propsMap">道具Map<propID,Count> </param>
    /// <returns></returns>
    public bool TryConsumePropsCount(Dictionary<int, int> propsMap)
    {
        bool success = m_PlayerInfo.VenturerInfo.TryConsumePropsCount(propsMap);

        return success;
    }

    /// <summary>
    /// 尝试消耗 某道具指定数量
    /// </summary>
    /// <param name="id">道具Id</param>
    /// <param name="count">道具数量</param>
    /// <returns></returns>
    public bool TryConsumePropCount(int id, int count)
    {
        bool success = m_PlayerInfo.VenturerInfo.TryConsumePropCount(id, count);

        return success;
    }

    /// <summary>
    /// 改变货币
    /// </summary>
    /// <param name="changeCount">改变数量</param>
    public void ChangeCoinCount(int changeCount)
    {
        m_PlayerInfo.VenturerInfo.ChangeCoinCount(changeCount);
    }
    #endregion
}

