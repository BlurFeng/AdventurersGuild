using System.Collections;
using System.Collections.Generic;
using FsGridCellSystem;

/// <summary>
/// 项目配置数据
/// </summary>
public static class ProjectConfigModel
{
    /// <summary>
    /// 网格系统 单位单元格尺寸 像素长度
    /// </summary>
    public static GridCoord GuildGridCellSizePixel { get { return m_GuildGridCellSizePixel; } }
    private static GridCoord m_GuildGridCellSizePixel = new GridCoord(20, 20, 10);

    /// <summary>
    /// 网格系统 网格数量 X轴
    /// </summary>
    public static int GuildGridCellCountX { get { return m_GuildGridCellCountX; } }
    private static int m_GuildGridCellCountX = 800;

    /// <summary>
    /// 网格系统 网格数量 Y轴
    /// </summary>
    public static int GuildGridCellCountY { get { return m_GuildGridCellCountY; } }
    private static int m_GuildGridCellCountY = 800;

    /// <summary>
    /// 网格系统 网格数量 Z轴
    /// </summary>
    public static int GuildGridCellCountZ { get { return m_GuildGridCellCountZ; } }
    private static int m_GuildGridCellCountZ = 100;

    /// <summary>
    /// 网格系统 网格层数
    /// </summary>
    public static int GuildLayerCount { get { return m_GuildLayerCount; } }
    private static int m_GuildLayerCount = 3;
}
