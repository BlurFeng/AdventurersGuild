using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapModelMsgType
{
    /// <summary>
    /// 世界地图 旅行 停留
    /// 每回合结束移动完毕后通知当前停留的位置
    /// 有目的地时会进行移动 那么可能停留在地点或道路
    /// 否则会一直停留在地点
    /// </summary>
    public const string WORLDMAP_TRAVEL_STAY = "WORLDMAP_TRAVEL_STAY";
}
