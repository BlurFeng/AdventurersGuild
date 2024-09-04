using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingModuleState
{
    None,

    /// <summary>
    /// 隐藏
    /// </summary>
    Hide,

    /// <summary>
    /// 完整的
    /// </summary>
    Entirety,

    /// <summary>
    /// 底部横切面
    /// </summary>
    TransectionBottom,

    /// <summary>
    /// 完整的，关闭。
    /// </summary>
    EntiretyClosed,

    /// <summary>
    /// 完整的，打开。
    /// </summary>
    EntiretyOpened,
}