using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BuildingModelMsgType
{
    /// <summary>
    /// 开始建造 某个建筑层
    /// </summary>
    public const string BUILDING_START = "BUILDING_START";

    /// <summary>
    /// 停止建造 当前正在建造的建筑层
    /// </summary>
    public const string BUILDING_STOP = "BUILDING_STOP";

    /// <summary>
    /// 正在建造更新 当前建造中建筑物层
    /// </summary>
    public const string BUILDING_UNDER_BUILDING_UPDATE = "BUILDING_UNDER_BUILDING_UPDATE";

    /// <summary>
    /// 完成建造 当前建造中的建筑层
    /// </summary>
    public const string BUILDING_UNDER_BUILDING_COMPLETE = "BUILDING_UNDER_BUILDING_COMPLETE";

    /// <summary>
    /// 开始拆除 建筑层
    /// </summary>
    public const string BUILDING_DEMOLISH_START = "BUILDING_DEMOLISH_START";

    /// <summary>
    /// 正在拆除 建筑层
    /// </summary>
    public const string BUILDING_DEMOLISH_UPDATE = "BUILDING_DEMOLISH_UPDATE";

    /// <summary>
    /// 拆除完成 建筑层
    /// </summary>
    public const string BUILDING_DEMOLISH_COMPLETE = "BUILDING_DEMOLISH_COMPLETE";

    /// <summary>
    /// 当拥有的建筑物数据发生变化时
    /// </summary>
    public const string BUILDING_OWNED_DATA_CHANGE = "BUILDING_OWNED_DATA_CHANGE";

    /// <summary>
    /// 当总建筑层数变化时
    /// </summary>
    public const string BUILDING_FLOOR_NUM_TOTAL_CHANGE = "BUILDING_FLOOR_NUM_TOTAL_CHANGE";

    /// <summary>
    /// 当前的施工项目信息更新时
    /// 包括开始或停止建造 开始或停止拆除 建造或拆除进度变化时
    /// 建造回合拆除只能有一个在进行中
    /// </summary>
    public const string BUILDING_UNDER_WORK_INFO_UPDATE = "BUILDING_UNDER_WORK_INFO_UPDATE";
}
