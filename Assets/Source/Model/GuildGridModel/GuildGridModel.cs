using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using com.ootii.Messages;
using Deploy;
using FsGridCellSystem;

public class GuildGridModel : Singleton<GuildGridModel>, IDestroy, ISaveData
{
    /// <summary>
    /// 初始化
    /// </summary>
    public override void Init()
    {
        base.Init();

        m_GridCellSystemManager = new GridCellSystemManager();
        //单位单元格尺寸
        var unitCellSizePixel = ProjectConfigModel.GuildGridCellSizePixel;
        Vector3 cellUnitSize = new Vector3(unitCellSizePixel.X * 0.01f, unitCellSizePixel.Y * 0.01f, unitCellSizePixel.Z * 0.01f);
        //自定义Layer初始化的单元格数据 TODO
        m_GridCellSystemManager.Init(cellUnitSize.x, cellUnitSize.y, cellUnitSize.z,
            ProjectConfigModel.GuildGridCellCountX, ProjectConfigModel.GuildGridCellCountY, ProjectConfigModel.GuildGridCellCountZ, ProjectConfigModel.GuildLayerCount);
    }

    public void SaveData(ES3File saveData)
    {
        saveData.Save<string>("GuildGridModel_DicBuildingInfo", PlayerPrefsUtil.SerializeData(m_DicBuildingInfo));

        Dictionary<GridCoord, FurnitureInfo> dicNew = new Dictionary<GridCoord, FurnitureInfo>(m_DicFurnitureInfo);
        foreach (var furniturInfo in dicNew.Values)
            furniturInfo.GridItemComponent = null;
        saveData.Save<string>("GuildGridModel_DicFurnitureInfo", PlayerPrefsUtil.SerializeData(dicNew));
    }

    public void LoadData(ES3File saveData)
    {
        string guildGridModelDicBuildingInfo = saveData.Load<string>("GuildGridModel_DicBuildingInfo", string.Empty);
        string guildGridModelDicFurnitureInfo = saveData.Load<string>("GuildGridModel_DicFurnitureInfo", string.Empty);

        //建筑数据
        m_DicBuildingInfo.Clear();
        if (!string.IsNullOrEmpty(guildGridModelDicBuildingInfo))
        {
            //有存档 读取建筑
            var dicBuildingInfo = PlayerPrefsUtil.DeserializeData<Dictionary<int, BuildingInfo>>(guildGridModelDicBuildingInfo);
            if (dicBuildingInfo != null)
            {
                m_DicBuildingInfo = dicBuildingInfo;
                //将建筑数据填入网格系统
                foreach (var buildingInfo in m_DicBuildingInfo.Values)
                {
                    PushBuildingGridItem(buildingInfo.Id, buildingInfo.CfgBuildingId, buildingInfo.MainGridCoord);
                }
            }
        }

        //网格数据
        InitGridSystemManagerData();
        if (!string.IsNullOrEmpty(guildGridModelDicFurnitureInfo))
        {
            //有存档 读取网格数据
            var dicFurnitureInfo = PlayerPrefsUtil.DeserializeData<Dictionary<GridCoord, FurnitureInfo>>(guildGridModelDicFurnitureInfo);
            if (dicFurnitureInfo != null)
            {
                m_DicFurnitureInfo = dicFurnitureInfo;
                //将家具数据填入网格系统
                foreach (var furnitureInfo in m_DicFurnitureInfo.Values)
                {
                    OnPushFurnitureGridItem(furnitureInfo.Layer, furnitureInfo.FurnitureId, furnitureInfo.MainGridCoord, furnitureInfo.Direction);
                }
            }
        }
        else
        {
            //新存档 默认数据
            PushBuildingInfo(10001, new GridCoord(392, 386, 0)); //公会主楼
        }
    }

    #region 三维网格系统
    #region 基础功能
    /// <summary>
    /// 网格 层
    /// </summary>
    public enum EGridLayer
    {
        /// <summary>
        /// 建筑
        /// </summary>
        Building = 0,
        /// <summary>
        /// 家具
        /// </summary>
        Furniture,
        /// <summary>
        /// 地面铺设（地毯）
        /// </summary>
        GroundLay,
        /// <summary>
        /// 角色
        /// </summary>
        Character,
    }

    /// <summary>
    /// 网格项目 状态
    /// </summary>
    public enum EGridItemState
    {
        None = 0,

        /// <summary>
        /// 被阻挡
        /// </summary>
        IsObstruct,

        /// <summary>
        /// 可以行走
        /// </summary>
        CanWalk,

        /// <summary>
        /// 可放置
        /// </summary>
        CanPut,
    }

    /// <summary>
    /// 网格物体 设置类型
    /// </summary>
    public enum EGridItemSetType
    {
        /// <summary>
        /// 无要求
        /// </summary>
        None = 0,
        /// <summary>
        /// 落地
        /// </summary>
        Ground,
        /// <summary>
        /// 靠墙
        /// </summary>
        Wall,
    }

    /// <summary>
    /// 消息信息 网格数据改变
    /// </summary>
    public class MsgGridItemChangeInfo
    {
        public MsgGridItemChangeInfo(EGridLayer layer, GridCoord gridCoord)
        {
            GridLayer = layer;
            GridCoord = gridCoord;
        }

        /// <summary>
        /// 网格 层
        /// </summary>
        public EGridLayer GridLayer;
        /// <summary>
        /// 网格 坐标
        /// </summary>
        public GridCoord GridCoord;
    }

    /// <summary>
    /// 网格系统管理器
    /// </summary>
    public GridCellSystemManager GridCellSystemManager { get { return m_GridCellSystemManager; } }
    private GridCellSystemManager m_GridCellSystemManager; //网格系统

    /// <summary>
    /// 单元格世界坐标尺寸 X
    /// </summary>
    public float CellUnitSizeX { get { return m_GridCellSystemManager.CellUnitSizeX; } }
    /// <summary>
    /// 单元格世界坐标尺寸 Y
    /// </summary>
    public float CellUnitSizeY { get { return m_GridCellSystemManager.CellUnitSizeY; } }
    /// <summary>
    /// 单元格世界坐标尺寸 Z
    /// </summary>
    public float CellUnitSizeZ { get { return m_GridCellSystemManager.CellUnitSizeZ; } }

    /// <summary>
    /// 正在操作的 网格坐标 Z (渲染维度只有二维 需要指定高度Z轴)
    /// </summary>
    public int OperateGridCoordZ { get { return m_OperateGridCoordZ; } set { m_OperateGridCoordZ = value; } }
    private int m_OperateGridCoordZ;

    /// <summary>
    /// 初始化 网格内单元格数据
    /// </summary>
    private void InitGridSystemManagerData()
    {
        //建筑层 高度为0 设置状态
        //var layerBuildingInt = (int)EGridLayer.Building;
        //var gridItemArrayBuilding = m_GridCellSystemManager.GetGridItemArray(layerBuildingInt);
        //for (int x = 0; x < gridItemArrayBuilding.GetLength(0); x++)
        //{
        //    for (int y = 0; y < gridItemArrayBuilding.GetLength(1); y++)
        //    {
        //        var gridItem = m_GridCellSystemManager.GetGridItem(layerBuildingInt, new GridCoord(x, y, 0), true);
        //        gridItem.AddState((int)EGridItemState.CanPut);
        //    }
        //}

        //家具层 高度为0 设置状态
        var layerFurnitureInt = (int)EGridLayer.Furniture;
        var gridItemArrayFurniture = m_GridCellSystemManager.GetGridItemArray(layerFurnitureInt);
        for (int x = 0; x < gridItemArrayFurniture.GetLength(0); x++)
        {
            for (int y = 0; y < gridItemArrayFurniture.GetLength(1); y++)
            {
                var gridItem = m_GridCellSystemManager.GetGridItem(layerFurnitureInt, new GridCoord(x, y, 0), true);
                //gridItem.AddState((int)EGridItemState.CanPut);
                gridItem.AddState((int)EGridItemState.CanWalk);
            }
        }
    }

    /// <summary>
    /// 压入 主单元格 数值
    /// 如果当前单元格已有数值 不会修改单元格数值
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="gridItemData"></param>
    /// <returns>是否 压入成功</returns>
    public bool PushMainGridItemValue(EGridLayer layer, GridItemData gridItemData)
    {
        bool isSucceed = m_GridCellSystemManager.PushMainGridItemData((int)layer, gridItemData);
        if (isSucceed)
        {
            //发送消息 网格数据变化
            MessageDispatcher.SendMessageData(GuildModelMsgType.GUILDGRIDMODEL_GRIDITEM_CHANGE, new MsgGridItemChangeInfo(layer, gridItemData.MainGridCoord));
        }

        return isSucceed;
    }

    /// <summary>
    /// 弹出 主单元格 数值
    /// 会移除网格主单元格上的数值
    /// </summary>
    /// <param name="gridCoord"></param>
    /// <returns></returns>
    private GridItemComponent PopMainGridItemValue(EGridLayer layer, GridCoord gridCoord)
    {
        var gridItem = m_GridCellSystemManager.PopMainGridItemValue((int)layer, gridCoord);

        if (gridItem.Value != 0)
        {
            //修改单元格状态
            switch (layer)
            {
                case EGridLayer.Building:
                    break;
                case EGridLayer.Furniture:
                    //家具占据的单元格 阻挡状态 否
                    m_GridCellSystemManager.SetGridItemState((int)layer, gridItem.MainGridCoord, gridItem.GetGridItemSizeAtDirection, (int)EGridItemState.IsObstruct, false);
                    //公会信息 设施值 减少
                    GuildModel.Instance.RemoveFacilityValue(gridItem.Value);
                    break;
                case EGridLayer.Character:
                    break;
            }

            //发送消息 网格数据变化
            MessageDispatcher.SendMessageData(GuildModelMsgType.GUILDGRIDMODEL_GRIDITEM_CHANGE, new MsgGridItemChangeInfo(layer, gridItem.MainGridCoord));
        }

        return gridItem;
    }

    /// <summary>
    /// 移除 网格项目
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="gridCoord"></param>
    /// <returns></returns>
    public bool RemoveMainGridItemValue(EGridLayer layer, GridCoord gridCoord)
    {
        bool isSuccess = m_GridCellSystemManager.RemoveMainGridItemData((int)layer, gridCoord);

        //发送消息 网格数据变化
        MessageDispatcher.SendMessageData(GuildModelMsgType.GUILDGRIDMODEL_GRIDITEM_CHANGE, new MsgGridItemChangeInfo(layer, gridCoord));

        return isSuccess;
    }

    /// <summary>
    /// 获取 主单元格
    /// </summary>
    /// <param name="gridCoord"></param>
    /// <returns></returns>
    public GridItemComponent GetMainGridItem(EGridLayer layer, GridCoord gridCoord)
    {
        return m_GridCellSystemManager.GetMainGridItem((int)layer, gridCoord);
    }

    /// <summary>
    /// 获取 主单元格 数值
    /// </summary>
    /// <param name="gridCoord"></param>
    /// <returns></returns>
    public int GetMainGridItemValue(EGridLayer layer, GridCoord gridCoord)
    {
        return m_GridCellSystemManager.GetMainGridItemValue((int)layer, gridCoord);
    }

    /// <summary>
    /// 检查 物体尺寸单元格内 是否被阻挡
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="gridCoord"></param>
    /// <param name="gridItemSize"></param>
    /// <returns></returns>
    public bool CheckGridItemSizeIsObstruct(EGridLayer layer, GridCoord gridCoord, GridCoord gridItemSize)
    {
        //检查 网格区域内是否被阻挡
        if (CheckGridItemSizeStateIsMeet(layer, gridCoord, gridItemSize, EGridItemState.IsObstruct, ECheckStateType.Anyone)) return true;
        //检查 网格区域内是否已有网格物体
        if (m_GridCellSystemManager.CheckGridItemSizeHasGirdItem((int)layer, gridCoord, gridItemSize)) return true;

        return false;
    }

    /// <summary>
    /// 检查占用单元格内 状态是否满足
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="gridCoord"></param>
    /// <param name="gridItemSize"></param>
    /// <param name="direction"></param>
    /// <param name="setType"></param>
    /// <returns></returns>
    public bool CheckGridItemSizeSetTypeIsMeet(EGridLayer layer, GridCoord gridCoord, GridCoord gridItemSize, EDirection direction = EDirection.None,
        EGridItemSetType setType = EGridItemSetType.None)
    {
        bool isMeet = true;

        switch (setType)
        {
            case EGridItemSetType.None:
                break;
            case EGridItemSetType.Ground:
                //仅检查 最底部单元格下方 是否可放置
                gridItemSize.Z = 1;
                gridCoord.Z -= 1;
                //特殊处理 0高度未被放置时 可放置于0高度
                if (gridCoord.Z == -1)
                {
                    gridCoord.Z = 0;
                    isMeet = CheckGridItemSizeStateIsMeet(layer, gridCoord, gridItemSize, EGridItemState.CanPut, ECheckStateType.All);
                    if (!isMeet)
                        return true;
                    else
                        return false;
                }
                isMeet = CheckGridItemSizeStateIsMeet(layer, gridCoord, gridItemSize, EGridItemState.CanPut, ECheckStateType.All);
                break;
            case EGridItemSetType.Wall:
                //检查 背立面单元格后方 是否可放置
                switch (direction)
                {
                    case EDirection.Down:
                        gridCoord.Y += gridItemSize.Y;
                        gridItemSize.Y = 1;
                        break;
                    case EDirection.Left:
                        gridCoord.X += gridItemSize.X;
                        gridItemSize.X = 1;
                        break;
                    case EDirection.Up:
                        gridCoord.Y -= 1;
                        gridItemSize.Y = 1;
                        break;
                    case EDirection.Right:
                        gridCoord.X -= 1;
                        gridItemSize.X = 1;
                        break;
                }

                isMeet = CheckGridItemSizeStateIsMeet(layer, gridCoord, gridItemSize, EGridItemState.CanPut, ECheckStateType.All);
                break;
        }

        return isMeet;
    }

    /// <summary>
    /// 检查 物体尺寸单元格内 状态是否满足
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="gridCoord"></param>
    /// <param name="gridItemSize"></param>
    /// <returns></returns>
    private bool CheckGridItemSizeStateIsMeet(EGridLayer layer, GridCoord gridCoord, GridCoord gridItemSize, EGridItemState state, ECheckStateType checkType)
    {
        return m_GridCellSystemManager.CheckGridItemSizeState((int)layer, gridCoord, gridItemSize, (int)state, checkType);
    }

    /// <summary>
    /// 获取 单元格尺寸
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="id"></param>
    /// <param name="eDirection"></param>
    /// <returns></returns>
    public static GridCoord GetGridItemSize(EGridLayer layer, int id)
    {
        GridCoord gridCoord = GridCoord.one;
        if (layer == EGridLayer.Building)
        {
            //公会建筑 配置表
            var cfg = ConfigSystem.Instance.GetConfig<Building_Config>(id);
            if (cfg != null)
            {
                gridCoord.X = cfg.GridItemSizeX;
                gridCoord.Y = cfg.GridItemSizeY;
                gridCoord.Z = cfg.GridItemSizeZ;
            }
        }
        else
        {
            //家具 配置表
            var cfg = ConfigSystem.Instance.GetConfig<Prop_Furniture>(id);
            if (cfg != null)
            {
                gridCoord.X = cfg.GridItemSizeX;
                gridCoord.Y = cfg.GridItemSizeY;
                gridCoord.Z = cfg.GridItemSizeZ;
            }
        }

        return gridCoord;
    }

    #region 射线检测
    /// <summary>
    /// 屏幕射线检测
    /// 基于网格系统的屏幕射线检测
    /// </summary>
    /// <param name="gridLayer">网格层</param>
    /// <param name="posScreen">屏幕坐标</param>
    /// <param name="posWorldOffset">转换成世界坐标后的修正偏移</param>
    /// <param name="startGridCoordHeight">射线起始点的网格高度（无参时从摄像机高度起始）</param>
    /// <returns></returns>
    public GridItemComponent ScreenPointToRay(EGridLayer gridLayer, Vector3 posScreen, Vector3 posWorldOffset = default, int startGridCoordHeight = -1)
    {
        //转换为世界坐标
        var posWorldPoint = CameraModel.Instance.MainCameraScreenToWorldPoint(posScreen);
        //世界坐标偏移
        if (posWorldOffset != default)
            posWorldPoint += posWorldOffset;

        //射线起始点的网格高度
        int rayStartHeightGridCoord = startGridCoordHeight; //摄像机网格高度
        if (rayStartHeightGridCoord == -1)
            rayStartHeightGridCoord = (int)(CameraModel.Instance.CameraMain.transform.position.y / GuildGridModel.Instance.CellUnitSizeZ); //摄像机网格高度

        //摄像机高度坐标开始 从高到低检查 （类似射线检测）
        GridItemComponent gridItem = null;
        for (int heightGridCoord = rayStartHeightGridCoord; heightGridCoord >= 0; heightGridCoord--)
        {
            posWorldPoint.y = heightGridCoord * GuildGridModel.Instance.CellUnitSizeZ; //网格高度转世界坐标高度
            var posWorld = GuildGridModel.Instance.GetViewPosToWorldPos(posWorldPoint); //二维显示坐标 转 三维世界坐标
            var gridCoord = GuildGridModel.Instance.GetGridCoord(posWorld); //获取 网格坐标
            var gridItemCur = GuildGridModel.Instance.GetMainGridItem(gridLayer, gridCoord);
            if (gridItemCur == null || gridItemCur.Value == 0)
                continue;
            else
            {
                switch (gridLayer)
                {
                    case EGridLayer.Furniture:
                        //获取家具实例 检查是否可见
                        var gridItemInstance = WindowSystem.Instance.MainGameWindow.GetFurnitureBase(EGridLayer.Furniture, gridItemCur.MainGridCoord);
                        if (gridItemInstance != null && gridItemInstance.GridItemComponent.ViewRootIsVisible == false)
                            continue;
                        break;
                }
            }

            //记录有效的网格物体
            gridItem = gridItemCur;
            break;
        }

        return gridItem;
    }

    /// <summary>
    /// 屏幕坐标 转换 网格坐标
    /// </summary>
    /// <param name="posScreen">屏幕坐标</param>
    /// <param name="posWorldOffset">转换成世界坐标后的修正偏移</param>
    /// <param name="gridCoordHeight">网格高度</param>
    /// <returns></returns>
    public GridCoord ScreenPointToGridCoord(Vector3 posScreen, Vector3 posWorldOffset = default, int gridCoordHeight = 0)
    {
        //转换为世界坐标
        var posWorldPoint = CameraModel.Instance.MainCameraScreenToWorldPoint(posScreen);
        //世界坐标偏移
        if (posWorldOffset != default)
            posWorldPoint += posWorldOffset;

        posWorldPoint.y = gridCoordHeight * GuildGridModel.Instance.CellUnitSizeZ; //网格高度转世界坐标高度
        var posWorld = GuildGridModel.Instance.GetViewPosToWorldPos(posWorldPoint); //二维显示坐标 转 三维世界坐标
        var gridCoord = GuildGridModel.Instance.GetGridCoord(posWorld); //获取 网格坐标

        return gridCoord;
    }
    #endregion
    #endregion

    #region 坐标转换
    /// <summary>
    /// 获取 单元格的世界坐标
    /// </summary>
    /// <param name="gridCoord"></param>
    /// <returns></returns>
    public Vector3 GetWorldPosition(GridCoord gridCoord)
    {
        var gridPos = m_GridCellSystemManager.GetWorldPosition(gridCoord);
        //Unity中 高度是Y轴。网格系统中 高度是Z轴
        return new Vector3(gridPos.X, gridPos.Z, gridPos.Y);
    }

    /// <summary>
    /// 获取 单元格的世界坐标
    /// </summary>
    /// <param name="gridCoord"></param>
    /// <returns></returns>
    public Vector3 GetWorldPosition(GridCoordFloat gridCoord)
    {
        var gridPos = m_GridCellSystemManager.GetWorldPosition(gridCoord);
        //Unity中 高度是Y轴。网格系统中 高度是Z轴
        return new Vector3(gridPos.X, gridPos.Z, gridPos.Y);
    }

    /// <summary>
    /// 获取 世界坐标所在单元格坐标
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <returns></returns>
    public GridCoord GetGridCoord(Vector3 worldPosition)
    {
        //网格系统中 高度是Z轴。Unity中 高度是Y轴
        return m_GridCellSystemManager.GetGridCoord(new GridCoordFloat(worldPosition.x, worldPosition.z, worldPosition.y));
    }

    /// <summary>
    /// 获取 世界坐标所在单元格坐标 精确到小数
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <returns></returns>
    public GridCoordFloat GetGridCoordFloat(Vector3 worldPosition)
    {
        return m_GridCellSystemManager.GetGridCoordFloat(new GridCoordFloat(worldPosition.x, worldPosition.z, worldPosition.y));
    }

    /// <summary>
    /// 获取 三维网格坐标 转 二维显示坐标
    /// </summary>
    /// <param name="gridCoord"></param>
    /// <returns></returns>
    public Vector3 GetGridCoordToViewPos(GridCoord gridCoord)
    {
        var pos = m_GridCellSystemManager.GetGridCoordToViewPos(gridCoord);
        //网格系统中 高度是Z轴。Unity中 高度是Y轴
        return new Vector3(pos.X, pos.Z, pos.Y);
    }

    /// <summary>
    /// 获取 三维世界坐标 转 二维显示坐标
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector3 GetWorldPosToViewPos(Vector3 position)
    {
        var pos = m_GridCellSystemManager.GetWorldPosToViewPos(new GridCoordFloat(position.x, position.z, position.y));
        //网格系统中 高度是Z轴。Unity中 高度是Y轴
        return new Vector3(pos.X, pos.Z, pos.Y);
    }

    /// <summary>
    /// 获取 二维显示坐标 转 三维世界坐标
    /// </summary>
    /// <param name="position">Z值为给定的目标高度</param>
    /// <returns></returns>
    public Vector3 GetViewPosToWorldPos(Vector3 position)
    {
        //Unity中 高度是Y轴。网格系统中 高度是Z轴
        var pos = m_GridCellSystemManager.GetViewPosToWorldPos(new GridCoordFloat(position.x, position.z, position.y));
        return new Vector3(pos.X, pos.Z, pos.Y);
    }

    /// <summary>
    /// 获取 网格项目的坐标修正
    /// </summary>
    /// <param name="gridItemSize">家具尺寸</param>
    /// <returns></returns>
    public Vector3 GetGridItemSizePositionAmend(GridCoord gridItemSize)
    {
        var posAmend = new Vector3(gridItemSize.X * 0.5f * CellUnitSizeX, gridItemSize.Z * 0.5f * CellUnitSizeZ, gridItemSize.Y * 0.5f * CellUnitSizeY);
        return posAmend;
    }

    /// <summary>
    /// 获取 网格项目占地的前部中心
    /// </summary>
    /// <param name="gridCoord"></param>
    /// <param name="gridItemSize"></param>
    /// <returns></returns>
    public Vector3 GetGridItemSizeFrontCenterPos(GridCoord gridCoord, GridCoord gridItemSize)
    {
        var posSizeAmend = GetGridItemSizePositionAmend(gridItemSize);
        var posGridCoord = GetWorldPosition(gridCoord);
        //var posBottomCenter = posGridCoord + posSizeAmend; //占地网格尺寸的中心点位置
        //posBottomCenter.y = gridCoord.Z * CellUnitSizeZ + posSizeAmend.y; //高度使用网格坐标高度 底部中心点位置

        posGridCoord.x += posSizeAmend.x;
        posGridCoord.y += posSizeAmend.y;

        return posGridCoord;
    }

    /// <summary>
    /// 获取 单元格尺寸的网格真实尺寸
    /// </summary>
    /// <param name="gridItemSize"></param>
    /// <returns></returns>
    public Vector3 GetGridItemSizeToWorldScale(GridCoord gridItemSize)
    {
        var size = new Vector3(gridItemSize.X * CellUnitSizeX, gridItemSize.Y * CellUnitSizeY, gridItemSize.Z * CellUnitSizeZ);
        return size;
    }
    #endregion

    #region 区域功能
    /// <summary>
    /// 玩家当前处于的区域
    /// </summary>
    public AreaInfo PlayerAreaInfoCur { get { return m_PlayerAreaInfoCur; } }
    private AreaInfo m_PlayerAreaInfoCur;

    /// <summary>
    /// 添加 区域项目
    /// </summary>
    /// <param name="areaGroupId"></param>
    /// <param name="areaInfo"></param>
    /// <param name="isInherits">继承已存在的区域项目的数据</param>
    public void AddAreaInfo(int areaGroupId, AreaInfo areaInfo, bool isInherits = true)
    {
        m_GridCellSystemManager.AddAreaInfo(areaGroupId, areaInfo, isInherits);
    }

    /// <summary>
    /// 移除 区域项目
    /// </summary>
    /// <param name="gridItem"></param>
    public void RemoveAreaInfo(int areaGroupId, string areaItemKey)
    {
        m_GridCellSystemManager.RemoveAreaInfo(areaGroupId, areaItemKey);
    }

    /// <summary>
    /// 移除 区域组信息
    /// </summary>
    /// <param name="areaGroupId"></param>
    public void RemoveAreaGroupInfo(int areaGroupId)
    {
        m_GridCellSystemManager.RemoveAreaGroupInfo(areaGroupId);
    }

    /// <summary>
    /// 检查 是否在区域内
    /// </summary>
    public AreaInfo CheckInAreaInfo(GridCoordFloat gridCoord)
    {
        var areaInfo = m_GridCellSystemManager.CheckInAreaInfo(gridCoord);

        return areaInfo;
    }

    /// <summary>
    /// 刷新 玩家所在区域
    /// </summary>
    /// <param name="gridCoord"></param>
    public void RefreshPlayerInAreaInfo(GridCoordFloat gridCoord)
    {
        var areaInfo = CheckInAreaInfo(gridCoord);

        //当前玩家所在区域 是否改变
        if (m_PlayerAreaInfoCur != areaInfo)
        {
            if (m_PlayerAreaInfoCur != null)
            {
                m_PlayerAreaInfoCur.OnValueChange = null;
                m_PlayerAreaInfoCur.OnListIntraGridItemChange = null;
            }

            m_PlayerAreaInfoCur = areaInfo;

            //回调 区域信息 值 改变
            if (m_PlayerAreaInfoCur != null)
            {
                m_PlayerAreaInfoCur.OnValueChange = ActOnAreaInfoValueChange;
                m_PlayerAreaInfoCur.OnListIntraGridItemChange = ActOnAreaInfoIntraGridItemChange;
            }  

            //设置 焦点区域信息
            m_GridCellSystemManager.SetFocusAreaInfo(m_PlayerAreaInfoCur);

            MessageDispatcher.SendMessage(GuildModelMsgType.GUILDGRIDMODEL_AREAITEMCUR_CHANGE);
        }
    }

    //回调 区域信息 值改变
    private void ActOnAreaInfoValueChange(int value)
    {
        MessageDispatcher.SendMessageData(GuildModelMsgType.GUILDGRIDMODEL_AREAITEMCUR_VALUE_CHANGE, value);
    }

    //回调 区域信息 内部项目改变
    private void ActOnAreaInfoIntraGridItemChange(int key)
    {
        MessageDispatcher.SendMessageData(GuildModelMsgType.GUILDGRIDMODEL_AREAITEMCUR_INTRAGRIDITEM_CHANGE, key);
    }
    #endregion
    #endregion

    #region 建筑信息

    /// <summary>
    /// 消息数据 建筑信息改变
    /// </summary>
    public class MsgBuildingInfoChange
    {
        public MsgBuildingInfoChange(EGridLayer layer, BuildingInfo buildingInfo, bool isRemove = false)
        {
            Layer = layer;
            BuildingInfo = buildingInfo;
            IsRemove = isRemove;
        }

        /// <summary>
        /// 是否 移除信息
        /// </summary>
        public bool IsRemove;

        /// <summary>
        /// 网格层
        /// </summary>
        public EGridLayer Layer;

        /// <summary>
        /// 建筑信息
        /// </summary>
        public BuildingInfo BuildingInfo;
    }

    /// <summary>
    /// 建筑信息
    /// </summary>
    [Serializable]
    public class BuildingInfo
    {
        /// <summary>
        /// 建筑ID 场景中建筑的唯一标识
        /// </summary>
        public int Id;

        /// <summary>
        /// 建筑配置表
        /// </summary>
        public int CfgBuildingId;

        /// <summary>
        /// 建筑等级配置表
        /// </summary>
        public int CfgBuildingLevelId { get { return ConfigSystem.Instance.GetConfigIdGuildBuildingLevel(CfgBuildingId, Level); } }

        /// <summary>
        /// 建筑等级
        /// </summary>
        public int Level;

        /// <summary>
        /// 主坐标
        /// </summary>
        public GridCoord MainGridCoord;
    }

    /// <summary>
    /// 建筑ID:建筑信息
    /// </summary>
    public Dictionary<int, BuildingInfo> DicBuildingInfo { get { return m_DicBuildingInfo; } }
    private Dictionary<int, BuildingInfo> m_DicBuildingInfo = new Dictionary<int, BuildingInfo>();

    /// <summary>
    /// 压入 建筑信息
    /// </summary>
    /// <param name="buildingCfgId">建筑配置表Id</param>
    /// <param name="gridCoord">主坐标</param>
    /// <param name="buildingLevel">建筑等级</param>
    /// <returns></returns>
    public bool PushBuildingInfo(int buildingCfgId, GridCoord gridCoord, int buildingLevel = 1)
    {
        //检查 指定建筑的位置是否被阻挡
        var gridItemSize = GetGridItemSize(EGridLayer.Building, buildingCfgId);
        var isObstruct = CheckGridItemSizeIsObstruct(EGridLayer.Building, gridCoord, gridItemSize);
        if (isObstruct) return false;

        //建筑唯一ID
        int buildingId = 0;
        foreach (var id in m_DicBuildingInfo.Keys)
        {
            if (id > buildingId)
            {
                buildingId = id;
            }
        }
        buildingId += 1;
        //记录 建筑信息
        var buildingInfo = new BuildingInfo();
        buildingInfo.Id = buildingId;
        buildingInfo.CfgBuildingId = buildingCfgId;
        buildingInfo.MainGridCoord = gridCoord;
        buildingInfo.Level = buildingLevel;
        m_DicBuildingInfo.Add(buildingInfo.Id, buildingInfo);

        var isSuccess = PushBuildingGridItem(buildingId, buildingCfgId, gridCoord);
        if (isSuccess)
        {
            //新的建筑 公会规模值 增加
            GuildModel.Instance.AddBuildingScaleValue(buildingInfo.CfgBuildingLevelId);
            //设置 建筑等级 设置网格信息
            SetBuildingGridItemData(buildingId);

            MessageDispatcher.SendMessageData(GuildModelMsgType.GUILDGRIDMODEL_BUILDINGINFO_CHANGE, new MsgBuildingInfoChange(EGridLayer.Building, buildingInfo));
        }
        else
        {
            m_DicBuildingInfo.Remove(buildingInfo.Id);
        }

        return isSuccess;
    }

    /// <summary>
    /// 移除 建筑信息
    /// </summary>
    /// <param name="buildingId"></param>
    /// <returns></returns>
    public bool RemoveBuildingInfo(int buildingId)
    {
        BuildingInfo buildingInfo = null;
        if (!m_DicBuildingInfo.TryGetValue(buildingId, out buildingInfo)) { return false; }
        
        //移除 建筑信息
        m_DicBuildingInfo.Remove(buildingId);

        //移除 主网格数据
        RemoveMainGridItemValue((int)EGridLayer.Building, buildingInfo.MainGridCoord);

        //公会规模值 减少
        GuildModel.Instance.RemoveBuildingScaleValue(buildingInfo.CfgBuildingLevelId);

        MessageDispatcher.SendMessageData(GuildModelMsgType.GUILDGRIDMODEL_BUILDINGINFO_CHANGE, new MsgBuildingInfoChange(EGridLayer.Building, buildingInfo, true));

        return true;
    }

    /// <summary>
    /// 获取 建筑信息
    /// </summary>
    /// <param name="buildingId"></param>
    /// <returns></returns>
    public BuildingInfo GetBuildingInfo(int buildingId)
    {
        BuildingInfo buildingInfo = null;
        m_DicBuildingInfo.TryGetValue(buildingId, out buildingInfo);

        return buildingInfo;
    }

    /// <summary>
    /// 设置 建筑等级
    /// </summary>
    /// <param name="buildingId"></param>
    /// <param name="buildingLevel"></param>
    public bool SetBuildingLevel(int buildingId, int buildingLevel)
    {
        var buildingInfo = GetBuildingInfo(buildingId);
        if (buildingInfo == null || buildingInfo.Level == buildingLevel) { return false; }
        
        //建筑等级有效性
        var cfgId = ConfigSystem.Instance.GetConfigIdGuildBuildingLevel(buildingInfo.CfgBuildingId, buildingLevel);
        var cfgBuildingNew = ConfigSystem.Instance.GetConfig<Building_Level>(cfgId);
        if (cfgBuildingNew == null) return false;

        //当前建筑等级 公会规模值 减少
        GuildModel.Instance.RemoveBuildingScaleValue(buildingInfo.CfgBuildingLevelId, false);
        //新的建筑等级 公会规模值 增加
        GuildModel.Instance.AddBuildingScaleValue(cfgId);

        //记录 新的建筑等级
        buildingInfo.Level = buildingLevel;

        SetBuildingGridItemData(buildingId);

        //发送消息 建筑等级变化
        MessageDispatcher.SendMessageData(GuildModelMsgType.GUILDGRIDMODEL_BUILDINGINFO_CHANGE, new MsgBuildingInfoChange(EGridLayer.Building, buildingInfo));

        return true;
    }

    //压入 建筑网格数据
    private bool PushBuildingGridItem(int buildingId, int buildingCfgId, GridCoord gridCoord)
    {
        var gridItemData = new GridItemData();
        gridItemData.Value = buildingId;
        gridItemData.GridItemType = EGridItemType.Main;
        gridItemData.MainGridCoord = gridCoord;
        gridItemData.GridItemSize = GetGridItemSize(EGridLayer.Building, buildingCfgId);
        gridItemData.Direction = EDirection.Down;
        bool isSuccess = PushMainGridItemValue(EGridLayer.Building, gridItemData);

        return isSuccess;
    }

    //设置 建筑网格数据
    private void SetBuildingGridItemData(int buildingId)
    {
        var buildingInfo = GetBuildingInfo(buildingId);
        //功能禁用
        if (true || buildingInfo == null) { return; }

        //var cfgId = ConfigSystem.Instance.GetConfigIdGuildBuildingLevel(buildingInfo.CfgBuildingId, buildingInfo.Level);
        //var cfg = ConfigSystem.Instance.GetConfig<Guild_BuildingLevel>(cfgId);
        //if (cfg == null) return;

        ////修改建筑覆盖的 家具层组的 单元格状态
        //var gridItemDataList = ConfigSystem.Instance.GetConfigBuildingGrid(cfg.ConfigBuildingGridId);
        //for (int i = 0; i < gridItemDataList.Count; i++)
        //{
        //    var gridItemData = gridItemDataList[i];
        //    var gridCoord = buildingInfo.MainGridCoord + gridItemData.GridCoord;
        //    //设置 单元格状态
        //    m_GridCellSystemManager.SetGridItemState((int)EGridLayer.Furniture, gridCoord, (int)EGridItemState.IsObstruct, !gridItemData.CanSet);
        //    m_GridCellSystemManager.SetGridItemState((int)EGridLayer.Furniture, gridCoord, (int)EGridItemState.CanWalk, gridItemData.CanWalk);
        //    m_GridCellSystemManager.SetGridItemState((int)EGridLayer.Furniture, gridCoord, (int)EGridItemState.CanPut, gridItemData.IsGround);
        //    m_GridCellSystemManager.SetGridItemState((int)EGridLayer.Furniture, gridCoord, (int)EGridItemState.CanPut, gridItemData.IsWall);

        //    //单元格不可设置时 自动收回家具
        //    if (!gridItemData.CanSet)
        //    {
        //        //弹出点击单元格的家具Id
        //        var gridItem = PopMainGridItemValue(EGridLayer.Furniture, gridCoord);
        //        if (gridItem.Value != 0)
        //        {
        //            //背包 家具数量 增加
        //            PlayerModel.Instance.AddPropInfo(gridItem.Value, 1);
        //        }
        //    }
        //}
    }

    #endregion

    #region 建筑模块

    /// <summary>
    /// 建筑模块类型
    /// </summary>
    public enum EBuildingModuleType
    {
        /// <summary>
        /// 基础 无显示状态
        /// </summary>
        Base,
        /// <summary>
        /// 普通 有显示状态
        /// </summary>
        Simple,
        /// <summary>
        /// 墙面 截面 有显示状态
        /// </summary>
        Transection,
        /// <summary>
        /// 门 有显示状态
        /// </summary>
        Door,
        /// <summary>
        /// 屋顶 有显示状态
        /// </summary>
        Roof
    }

    /// <summary>
    /// 建筑模块信息
    /// </summary>
    public class BuildingModuleInfo
    {
        /// <summary>
        /// 建筑模块类型
        /// </summary>
        public EBuildingModuleType BuildingModuleType;
        /// <summary>
        /// 网格项目数据
        /// </summary>
        public GridItemComponent GridItemComponent;
    }

    /// <summary>
    /// 网格坐标:建筑模块信息
    /// </summary>
    public Dictionary<GridCoord, BuildingModuleInfo> DicBuildingModuleInfo { get { return m_DicBuildingModuleInfo; } }
    private Dictionary<GridCoord, BuildingModuleInfo> m_DicBuildingModuleInfo = new Dictionary<GridCoord, BuildingModuleInfo>();

    /// <summary>
    /// 压入 建筑模块信息
    /// </summary>
    /// <param name="eBuildingModuleType"></param>
    /// <param name="gridItem"></param>
    /// <returns></returns>
    public bool PushBuildingModuleInfo(EBuildingModuleType eBuildingModuleType, GridItemComponent gridItem)
    {
        if (m_DicBuildingModuleInfo.ContainsKey(gridItem.MainGridCoord)) { return false; }

        //添加 建筑模块信息
        var buildingModuleInfo = new BuildingModuleInfo();
        buildingModuleInfo.BuildingModuleType = eBuildingModuleType;
        buildingModuleInfo.GridItemComponent = gridItem;
        m_DicBuildingModuleInfo.Add(gridItem.MainGridCoord, buildingModuleInfo);

        //设置 家具层网格状态
        SetBuildingModuleState(EGridLayer.Furniture, gridItem.MainGridCoord, gridItem.GridItemSize, true);

        return true;
    }

    /// <summary>
    /// 移除 建筑模块信息
    /// </summary>
    /// <param name="gridCoord"></param>
    public void RemoveBuildingModuleInfo(GridCoord gridCoord)
    {
        BuildingModuleInfo buildingModuleInfo = null;
        if (!m_DicBuildingModuleInfo.TryGetValue(gridCoord, out buildingModuleInfo)) { return; }

        //移除 建筑模块信息
        m_DicBuildingModuleInfo.Remove(gridCoord);

        //设置 家具层网格状态
        SetBuildingModuleState(EGridLayer.Furniture, buildingModuleInfo.GridItemComponent.MainGridCoord, buildingModuleInfo.GridItemComponent.GridItemSize, false);
    }

    /// <summary>
    /// 设置 建筑模块 状态
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="gridCoord"></param>
    /// <param name="gridItemSize"></param>
    /// <returns></returns>
    private void SetBuildingModuleState(EGridLayer layer, GridCoord gridCoord, GridCoord gridItemSize, bool isTrue)
    {
        //设置 建筑模块的占用尺寸的单元格状态
        //阻挡
        m_GridCellSystemManager.SetGridItemState((int)layer, gridCoord, gridItemSize, (int)EGridItemState.IsObstruct, isTrue);
        //可放置
        m_GridCellSystemManager.SetGridItemState((int)layer, gridCoord, gridItemSize, (int)EGridItemState.CanPut, isTrue);
    }

    #endregion

    #region 家具信息
    /// <summary>
    /// 消息数据 家具信息改变
    /// </summary>
    public class MsgFurnitureInfoChange
    {
        public MsgFurnitureInfoChange(EGridLayer layer, FurnitureInfo furnitureInfo, bool isRemove = false)
        {
            Layer = layer;
            FurnitureInfo = furnitureInfo;
            IsRemove = isRemove;
        }

        /// <summary>
        /// 是否 移除信息
        /// </summary>
        public bool IsRemove;

        /// <summary>
        /// 网格层
        /// </summary>
        public EGridLayer Layer;

        /// <summary>
        /// 建筑信息
        /// </summary>
        public FurnitureInfo FurnitureInfo;
    }

    /// <summary>
    /// 家具信息
    /// </summary>
    [Serializable]
    public class FurnitureInfo
    {
        /// <summary>
        /// 家具ID
        /// </summary>
        public int FurnitureId;
        /// <summary>
        /// 网格层
        /// </summary>
        public EGridLayer Layer;
        /// <summary>
        /// 主坐标
        /// </summary>
        public GridCoord MainGridCoord;
        /// <summary>
        /// 朝向
        /// </summary>
        public EDirection Direction;
        /// <summary>
        /// 网格项目数据
        /// </summary>
        public GridItemComponent GridItemComponent;
    }

    /// <summary>
    /// 网格坐标:家具信息
    /// </summary>
    public Dictionary<GridCoord, FurnitureInfo> DicFurnitureInfo { get { return m_DicFurnitureInfo; } }
    private Dictionary<GridCoord, FurnitureInfo> m_DicFurnitureInfo = new Dictionary<GridCoord, FurnitureInfo>();

    /// <summary>
    /// 压入 家具信息
    /// </summary>
    /// <param name="furnitureId"></param>
    /// <param name="gridCoord"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public bool PushFurnitureInfo(EGridLayer layer, int furnitureId, GridCoord gridCoord, EDirection direction)
    {
        bool isSuccess = OnPushFurnitureGridItem(layer, furnitureId, gridCoord, direction);
        if (isSuccess)
        {
            //记录 家具信息
            var furnitureInfo = new FurnitureInfo();
            furnitureInfo.FurnitureId = furnitureId;
            furnitureInfo.Layer = layer;
            furnitureInfo.MainGridCoord = gridCoord;
            furnitureInfo.Direction = direction;
            m_DicFurnitureInfo.Add(gridCoord, furnitureInfo);

            MessageDispatcher.SendMessageData(GuildModelMsgType.GUILDGRIDMODEL_FURNITUREINFO_CHANGE, new MsgFurnitureInfoChange(layer, furnitureInfo));
        }

        return isSuccess;
    }

    /// <summary>
    /// 压入 家具网格项目
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="furnitureId"></param>
    /// <param name="gridCoord"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    private bool OnPushFurnitureGridItem(EGridLayer layer, int furnitureId, GridCoord gridCoord, EDirection direction)
    {
        var gridItemData = new GridItemData();
        gridItemData.Value = furnitureId;
        gridItemData.GridItemType = EGridItemType.Main;
        gridItemData.MainGridCoord = gridCoord;
        gridItemData.GridItemSize = GetGridItemSize(EGridLayer.Furniture, furnitureId);
        gridItemData.Direction = direction;

        bool isSuccess = PushMainGridItemValue(layer, gridItemData);
        if (isSuccess)
        {
            //家具占据的单元格 阻挡状态 是
            m_GridCellSystemManager.SetGridItemState((int)layer, gridItemData.MainGridCoord, gridItemData.GetGridItemSizeAtDirection, (int)EGridItemState.IsObstruct, true);
            //公会信息 设施值 增加
            GuildModel.Instance.AddFacilityValue(gridItemData.Value);
        }

        return isSuccess;
    }

    /// <summary>
    /// 弹出 家具信息
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="gridCoord"></param>
    /// <returns></returns>
    public FurnitureInfo PopFurnitureInfo(EGridLayer layer, GridCoord gridCoord)
    {
        FurnitureInfo furnitureInfo = null;

        //尝试弹出 网格坐标的家具
        var gridItem = PopMainGridItemValue(layer, gridCoord);
        if (gridItem.Value != 0 && m_DicFurnitureInfo.TryGetValue(gridItem.MainGridCoord, out furnitureInfo))
        {
            //移除 家具信息
            m_DicFurnitureInfo.Remove(gridItem.MainGridCoord);
            MessageDispatcher.SendMessageData(GuildModelMsgType.GUILDGRIDMODEL_FURNITUREINFO_CHANGE, new MsgFurnitureInfoChange(layer, furnitureInfo, true));
        }

        return furnitureInfo;
    }
    #endregion
}
