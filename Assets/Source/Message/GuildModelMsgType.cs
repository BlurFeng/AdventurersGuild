using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GuildModelMsgType
{
    /// <summary>
    /// 公会信息 改变
    /// </summary>
    public const string GUILDMODEL_GUILDINFO_CHANGE = "GUILDMODEL_GUILDINFO_CHANGE";

    /// <summary>
    /// 公会建造 网格单元格 改变
    /// </summary>
    public const string GUILDGRIDMODEL_GRIDITEM_CHANGE = "GUILDGRIDMODEL_GRIDITEM_CHANGE";

    /// <summary>
    /// 公会建造 建筑信息 改变
    /// </summary>
    public const string GUILDGRIDMODEL_BUILDINGINFO_CHANGE = "GUILDGRIDMODEL_BUILDINGINFO_CHANGE";

    /// <summary>
    /// 公会建造 家具信息 改变
    /// </summary>
    public const string GUILDGRIDMODEL_FURNITUREINFO_CHANGE = "GUILDGRIDMODEL_FURNITUREINFO_CHANGE";

    /// <summary>
    /// 公会建造 当前所在区域 改变
    /// </summary>
    public const string GUILDGRIDMODEL_AREAITEMCUR_CHANGE = "GUILDGRIDMODEL_AREAITEMCUR_CHANGE";

    /// <summary>
    /// 公会建造 当前所在区域 值 改变
    /// </summary>
    public const string GUILDGRIDMODEL_AREAITEMCUR_VALUE_CHANGE = "GUILDGRIDMODEL_AREAITEMCUR_VALUE_CHANGE";

    /// <summary>
    /// 公会建造 当前所在区域 内部网格项目 改变
    /// </summary>
    public const string GUILDGRIDMODEL_AREAITEMCUR_INTRAGRIDITEM_CHANGE = "GUILDGRIDMODEL_AREAITEMCUR_INTRAGRIDITEM_CHANGE";
}
