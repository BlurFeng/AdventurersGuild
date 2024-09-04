using EntrustSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 委托显示信息
/// </summary>
public struct EntrustShowInfo
{
    public int id;
    public EEntrustType type;
    public int level;
    public int daysLimit;
    public int daysNeedBase;
    public bool canTryMultipleTimes;
    public string title;
    public string describe;

    /// <summary>
    /// 执行此委托的冒险者队伍 存储了冒险者Id
    /// 具体冒险者信息通过Id和冒险者模块查询
    /// </summary>
    public int[] venturerTeam;

    public EntrustShowInfo(int id, EEntrustType type, int level, int daysLimit, int daysNeedBase, bool canTryMultipleTimes, string title, string des, int[] heroTeam = null)
    {
        this.id = id;
        this.type = type;
        this.level = level;
        this.daysLimit = daysLimit;
        this.daysNeedBase = daysNeedBase;
        this.canTryMultipleTimes = canTryMultipleTimes;
        this.title = title;
        this.describe = des;

        if(heroTeam != null)
        {
            this.venturerTeam = heroTeam;
        }
        else
        {
            this.venturerTeam = null;
        }
    }
}

/// <summary>
/// 委托模块消息类型
/// </summary>
public static class EntrustModelMsgType
{
    /// <summary>
    /// 委托开始
    /// </summary>
    public const string ENTRUSTMODEL_ENTRUST_START = "ENTRUSTMODEL_ENTRUST_START";

    /// <summary>
    /// 委托结算
    /// 委托在完成后等待玩家进行结算
    /// </summary>
    public const string ENTRUSTMODEL_ENTRUST_STATEMENT = "ENTRUSTMODEL_STATEMENT";
}

/// <summary>
/// 委托完成信息
/// </summary>
public struct EntrustCommonInfo
{
    /// <summary>
    /// 委托Id
    /// </summary>
    public int entrustId;

    /// <summary>
    /// 获取冒险者队伍，冒险者Id列表
    /// </summary>
    /// <returns></returns>
    public int[] GetVenturerTeam()
    {
        if (!EntrustModel.Instance.GetEntrustItemHandle(entrustId, out EntrustItemHandler entrustItemHandler)) return null;
        return entrustItemHandler.GetVenturerTeam();
    }
}