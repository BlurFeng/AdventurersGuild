using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeModelMsgType
{
    /// <summary>
    /// 游戏时间 小时 改变
    /// </summary>
    public const string TIMEMODEL_GAMETIME_HOURS_CHANGE = "TIMEMODEL_GAMETIME_HOURS_CHANGE";

    /// <summary>
    /// 回合数量 改变
    /// </summary>
    public const string TIMEMODEL_DAYCOUNT_CHANGE = "TIMEMODEL_DAYCOUNT_CHANGE";

    /// <summary>
    /// 季节 改变
    /// </summary>
    public const string TIMEMODEL_SEASON_CHANGE = "TIMEMODEL_SEASON_CHANGE";

    /// <summary>
    /// 纪元年 改变
    /// </summary>
    public const string TIMEMODEL_ERAYEAR_CHANGE = "TIMEMODEL_ERAYEAR_CHANGE";
}
