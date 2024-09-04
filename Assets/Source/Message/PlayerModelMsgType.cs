using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerModelMsgType
{
    /// <summary>
    /// 主角 创建 成功
    /// </summary>
    public const string PLAYER_CREATE_SUCCESS = "PLAYER_CREATE_SUCCESS";

    /// <summary>
    /// 主角 创建 失败
    /// </summary>
    public const string PLAYER_CREATE_ERROR = "PLAYER_CREATE_ERROR";

    /// <summary>
    /// 主角 信息 改变
    /// </summary>
    public const string PLAYER_INFO_CHANGE = "PLAYER_INFO_CHANGE";

    /// <summary>
    /// 背包 道具 初始化
    /// </summary>
    public const string PROP_BACKPACK_INIT = "PROP_BACKPACK_INIT";

    /// <summary>
    /// 背包 道具改变 资产
    /// </summary>
    public const string PROP_BACKPACK_CHANGE_PROPERTY = "PROP_BACKPACK_CHANGE_PROPERTY";

    /// <summary>
    /// 背包 道具改变 材料
    /// </summary>
    public const string PROP_BACKPACK_CHANGE_MATERIAL = "PROP_BACKPACK_CHANGE_MATERIAL";

    /// <summary>
    /// 背包 道具改变 功能
    /// </summary>
    public const string PROP_BACKPACK_CHANGE_FUNCTION = "PROP_BACKPACK_CHANGE_FUNCTION";

    /// <summary>
    /// 背包 道具改变 身体
    /// </summary>
    public const string PROP_BACKPACK_CHANGE_BODY = "PROP_BACKPACK_CHANGE_BODY";

    /// <summary>
    /// 背包 道具改变 装备
    /// </summary>
    public const string PROP_BACKPACK_CHANGE_EQUIP = "PROP_BACKPACK_CHANGE_EQUIP";

    /// <summary>
    /// 背包 道具改变 家具
    /// </summary>
    public const string PROP_BACKPACK_CHANGE_FURNITURE = "PROP_BACKPACK_CHANGE_FURNITURE";

    /// <summary>
    /// 背包 道具改变 独特
    /// </summary>
    public const string PROP_BACKPACK_CHANGE_UNIQUE = "PROP_BACKPACK_CHANGE_UNIQUE";
}
