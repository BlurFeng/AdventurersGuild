using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework;
using FsGridCellSystem;
using System;
using Deploy;

/// <summary>
/// 网格单元系统，项目单元基类
/// 接入单元组件功能，其他单元物件都继承此类
/// </summary>
public class GridItemActor : AActor
{
    #region 网格系统
    /// <summary>
    /// 网格系统 组件
    /// </summary>
    public GridItemComponent GridItemComponent { get { return m_GridItemComponent; } }
    //网格系统 组件
    [SerializeField]
    //[Header("网格物体组件")]
    //[Tooltip("拥有此组件的物体拥有网格物体的特性和功能，会被FsGridCellSystem认定为一个网格物体")]
    protected GridItemComponent m_GridItemComponent = new GridItemComponent();

    /// <summary>
    /// 初始化时 自动获取网格坐标
    /// </summary>
    protected bool m_IsInitAutoGetGridCoord;

    /// <summary>
    /// 设置 单元格数据
    /// </summary>
    /// <param name="gridItemData"></param>
    public virtual void SetGridItemComponentData(GridItemData gridItemData)
    {
        //从当前区域中移除
        RemoveAreaInfoIntraGridItem();

        //记录 新的网格项目数据
        m_GridItemComponent.GridItemData = gridItemData;

        //添加至新的区域中
        AddAreaInfoIntraGridItem();

        //更新 自身的世界坐标
        RefreshWorldPosition();

#if UNITY_EDITOR
        //检查 是否设置有效的 占用单元格尺寸
        if (m_GridItemComponent.GridItemSize.isNoVolume())
            Debug.LogError($"GridItemComponent.GridItemSize Error! >> 未设置有效的GridItemSize 物体名称-{GameObjectGet.name} 当前值-{m_GridItemComponent.GridItemSize}");
#endif
    }

    /// <summary>
    /// 设置 坐标位置
    /// </summary>
    /// <param name="gridCoord"></param>
    public void SetMainGridCoord(GridCoord gridCoord)
    {
        var gridItemData = m_GridItemComponent.GridItemData;
        gridItemData.MainGridCoord = gridCoord;
        SetGridItemComponentData(gridItemData);
    }

    /// <summary>
    /// 刷新 世界坐标
    /// </summary>
    protected virtual void RefreshWorldPosition()
    {
        var posOri = GuildGridModel.Instance.GetWorldPosition(m_GridItemComponent.MainGridCoord);
        //根据占地尺寸 进行坐标修正
        var posAmend = GuildGridModel.Instance.GetGridItemSizePositionAmend(m_GridItemComponent.GetGridItemSizeAtDirection);
        TransformGet.position = posOri + posAmend;
    }

    /// <summary>
    /// 刷新 网格坐标 根据世界坐标
    /// </summary>
    public void RefreshGridCoordOfWorldPos()
    {
        //根据物体的世界位置 获取网格坐标
        var posSizeAmend = GuildGridModel.Instance.GetGridItemSizePositionAmend(m_GridItemComponent.GetGridItemSizeAtDirection);
        //网格坐标为物体占地尺寸左下角 世界坐标修正
        var gridCoord = GuildGridModel.Instance.GetGridCoord(TransformGet.position - posSizeAmend);
        m_GridItemComponent.MainGridCoord = gridCoord;
    }

    #region 定位功能

#if UNITY_EDITOR
    [Header("网格系统-定位功能-场景线框")]
    [Tooltip("定位中心点")]
    [SerializeField] private bool m_GizmosLocationCenter;
    [Tooltip("定位接收者检测范围中心点")]
    [SerializeField] private bool m_GizmosLocationReceiverRangeCenter;
    [Tooltip("定位接收者检测范围")]
    [SerializeField] private bool m_GizmosLocationReceiverRangeSize;

    private void OnDrawGizmos()
    {
        //定位中心点
        if (m_GizmosLocationCenter)
        {
            Gizmos.DrawSphere(GuildGridModel.Instance.GetWorldPosition(m_GridItemComponent.LocationCenter), 0.06f);
        }

        //定位接收者检测范围中心点
        if (m_GizmosLocationReceiverRangeCenter)
        {
            Gizmos.DrawSphere(GuildGridModel.Instance.GetWorldPosition(m_GridItemComponent.LocationReceiverRangeCenter), 0.06f);
        }

        //定位接收者检测范围
        if (m_GizmosLocationReceiverRangeSize)
        {
            var centerPos = GuildGridModel.Instance.GetWorldPosition(m_GridItemComponent.LocationReceiverRangeCenter);
            Gizmos.DrawWireCube(centerPos, GuildGridModel.Instance.GetWorldPosition(m_GridItemComponent.LocationReceiverRangeSize));
        }
    }

#endif

    #endregion

    #region 区域功能
    /// <summary>
    /// 自动添加或移除 从某个区域信息内的网格项目组
    /// </summary>
    public bool EnableAutoRecordAreaInfo
    {
        get { return m_EnableAutoRecordAreaInfo; }
        set
        {
            if (m_EnableAutoRecordAreaInfo == value) { return; }
            m_EnableAutoRecordAreaInfo = value;
            //立刻执行 添加或移除 区域信息
            if (value)
                AddAreaInfoIntraGridItem(true);
            else
                RemoveAreaInfoIntraGridItem(true);
        }
    }
    private bool m_EnableAutoRecordAreaInfo = true;

    protected AreaInfo m_AreaInfoCur; //当前所在的区域信息

    /// <summary>
    /// 添加至 区域内
    /// </summary>
    protected void AddAreaInfoIntraGridItem(bool isForce = false)
    {
        if (isForce == false && m_EnableAutoRecordAreaInfo == false) { return; }

        //从当前区域移除
        RemoveAreaInfoIntraGridItem();

        //检查当前所在区域
        m_AreaInfoCur = GuildGridModel.Instance.CheckInAreaInfo(m_GridItemComponent.MainGridCoord);
        if (m_AreaInfoCur != null)
            m_AreaInfoCur.AddIntraGridItem(m_GridItemComponent, GetAreaInfoIntraGridItemKey());
    }

    /// <summary>
    /// 从区域移除
    /// </summary>
    /// <param name="isForce"></param>
    protected void RemoveAreaInfoIntraGridItem(bool isForce = false)
    {
        if (isForce == false && m_EnableAutoRecordAreaInfo == false) { return; }

        if (m_AreaInfoCur == null) { return; }

        m_AreaInfoCur.RemoveIntraGridItem(m_GridItemComponent, GetAreaInfoIntraGridItemKey());
        m_AreaInfoCur = null;
    }

    /// <summary>
    /// 获取 区域网格项目 key
    /// </summary>
    /// <returns></returns>
    protected virtual int GetAreaInfoIntraGridItemKey()
    {
        return -1;
    }

    #endregion
    #endregion

    protected override void OnAwake()
    {
        base.OnAwake();

        m_GridItemComponent.Init(this);

        //设置 网格系统管理器
        m_GridItemComponent.SetGridCellSystemManager(GuildGridModel.Instance.GridCellSystemManager);

        //获取 显示根节点
        if (m_GridItemComponent.ViewRootTrans == null)
            m_GridItemComponent.ViewRootTrans = TransformGet.Find("ViewRoot");
        //获取 碰撞根节点
        if (m_GridItemComponent.ColliderRootGo == null)
        {
            var trans = TransformGet.Find("ColliderRoot");
            if (trans != null)
                m_GridItemComponent.ColliderRootGo = trans.gameObject;
        }
    }

    protected override void OnStart()
    {
        base.OnStart();

        if (m_IsInitAutoGetGridCoord)
            RefreshGridCoordOfWorldPos();
    }

    protected override void OnEnableThis()
    {
        base.OnEnableThis();

        m_GridItemComponent.SetEnableRefreshViewObjPosition(true);
    }

    protected override void OnDisableThis()
    {
        base.OnDisableThis();

        m_GridItemComponent.SetEnableRefreshViewObjPosition(false);
    }

    public override bool Init(object outer = null)
    {
        return base.Init(outer);

    }

    public override void Begin()
    {
        base.Begin();

    }
}