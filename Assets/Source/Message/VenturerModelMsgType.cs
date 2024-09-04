using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VenturerModelMsgType
{
    /// <summary>
    /// 冒险者 载入 成功
    /// </summary>
    public const string VENTURERMODEL_LOAD_SUCCESS = "VENTURERMODEL_LOAD_SUCCESS";

    /// <summary>
    /// 冒险者池 增加
    /// </summary>
    public const string VENTURERMODEL_VENTURERPOOL_ADD = "VENTURERMODEL_VENTURERPOOL_ADD";

    /// <summary>
    /// 冒险者池 减少
    /// </summary>
    public const string VENTURERMODEL_VENTURERPOOL_RED = "VENTURERMODEL_VENTURERPOOL_RED";

    /// <summary>
    /// 冒险者信息 变更
    /// </summary>
    public const string VENTURERMODEL_INFO_CHANGE = "VENTURERMODEL_INFO_CHANGE";

    /// <summary>
    /// 冒险者信息 等级 改变
    /// </summary>
    public const string VENTURERMODEL_INFO_LEVEL_CHANGE = "VENTURERMODEL_INFO_LEVEL_UPDATE";

    /// <summary>
    /// 冒险者信息 经验值 改变
    /// </summary>
    public const string VENTURERMODEL_INFO_EXP_CHANGE = "VENTURERMODEL_INFO_EXP_UPDATE";

    /// <summary>
    /// 冒险者信息 生命回合当前 改变
    /// </summary>
    public const string VENTURERMODEL_INFO_LIFETURNCUR_CHANGE = "VENTURERMODEL_INFO_LIFETURNCUR_UPDATE";

    /// <summary>
    /// 冒险者信息 状态 改变
    /// </summary>
    public const string VENTURERMODEL_INFO_STATE_CHANGE = "VENTURERMODEL_INFO_STATE_UPDATE";
}
