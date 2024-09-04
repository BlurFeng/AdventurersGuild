using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EEntrustType
{
    None,

    /// <summary>
    /// 杂务
    /// </summary>
    Chore,

    /// <summary>
    /// 调查
    /// </summary>
    Investigate,

    /// <summary>
    /// 护卫
    /// </summary>
    Guard,

    /// <summary>
    /// 狩猎
    /// </summary>
    Hunt,

    /// <summary>
    /// 捕获
    /// </summary>
    Capture,

    /// <summary>
    /// 交付
    /// 要求交付一定数量的货物 推荐的方式可能是采集或者狩猎
    /// </summary>
    Deliver,
}

/// <summary>
/// 委托结果类型
/// </summary>
public enum EEntrustResultType
{
    None,

    /// <summary>
    /// 失败
    /// </summary>
    Failed,

    /// <summary>
    /// 成功
    /// </summary>
    Succeed,

    /// <summary>
    /// 大成功
    /// </summary>
    GreateSucceed,

    /// <summary>
    /// 极大成功
    /// </summary>
    ExcellentSucceed,
}