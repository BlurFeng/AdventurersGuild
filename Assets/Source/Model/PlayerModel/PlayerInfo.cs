using Deploy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Messages;
using System;
using FsGridCellSystem;

[Serializable]
public class PlayerInfo
{
    /// <summary>
    /// 玩家ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 创建时间 时间戳
    /// </summary>
    public long CreateTimestamp { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// 位置 当前
    /// </summary>
    public GridCoordFloat PositionCur { get; set; }

    /// <summary>
    /// 玩家的 冒险者信息
    /// </summary>
    public VenturerInfo VenturerInfo { get; set; }
}

