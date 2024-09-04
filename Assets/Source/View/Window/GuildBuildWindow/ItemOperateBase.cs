using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using FsGridCellSystem;
using Deploy;

public class ItemOperateBase : MonoBehaviour
{
    private GridItemActor m_GridItemActor = null; //网格项目物体
    private GridCoord m_MainGridCoord = GridCoord.invalid; //当前坐标

    /// <summary>
    /// 改变预制体 完成
    /// </summary>
    public Action OnChangePrefabComplete { get { return m_OnChangePrefabComplete; } set { m_OnChangePrefabComplete = value; } }
    private Action m_OnChangePrefabComplete;

    /// <summary>
    /// ID
    /// </summary>
    public int ValueId { get { return m_ValueId; } }
    private int m_ValueId;

    /// <summary>
    /// 获取 朝向
    /// </summary>
    public EDirection Direction { get { return m_Direction; } }
    private EDirection m_Direction;

    /// <summary>
    /// 获取 占地尺寸
    /// </summary>
    public GridCoord GetGridItemSize 
    { 
        get 
        {
            if (m_GridItemActor == null) { return GridCoord.one; }
            return m_GridItemActor.GridItemComponent.GetGridItemSizeAtDirection; 
        } 
    }

    /// <summary>
    /// 放置类型
    /// </summary>
    public virtual GuildGridModel.EGridItemSetType SetType
    {
        get
        {
            return GuildGridModel.EGridItemSetType.None;
        }
    }

    /// <summary>
    /// 渲染根节点 世界位置
    /// </summary>
    public Vector3 PosViewRootTrans
    {
        get
        {
            if (m_GridItemActor == null || m_GridItemActor.GridItemComponent.ViewRootTrans == null)
            {
                var posBottomCenter = GuildGridModel.Instance.GetGridItemSizeFrontCenterPos(m_MainGridCoord, GridCoord.one);
                var posViewRoot = GuildGridModel.Instance.GetWorldPosToViewPos(posBottomCenter);
                return posViewRoot;
            }
            else
            {
                return m_GridItemActor.GridItemComponent.ViewRootTrans.position;
            }
        }
    }

    private void Awake()
    {
        Init();
    }

    private void OnDestroy()
    {
        //返还 当前的网格项目物体
        ReturnGridItemActor();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    protected virtual void Init()
    {

    }

    /// <summary>
    /// 设置 网格项目物体
    /// </summary>
    /// <param name="valueId"></param>
    /// <param name="direction"></param>
    public void SetGridItemActorData(int valueId, EDirection direction = EDirection.Down, GridCoord mainGridCoord = default)
    {
        //ID、朝向 相同
        if (m_GridItemActor != null && m_ValueId == valueId && m_Direction == direction) { return; }
        m_ValueId = valueId;
        m_Direction = direction;

        //设置为0 立即返还 网格项目物体
        if (m_ValueId == 0)
        {
            ReturnGridItemActor();
            m_OnChangePrefabComplete?.Invoke();
            return;
        }

        GridItemData griditemData = new GridItemData();
        griditemData.Value = m_ValueId;
        griditemData.Direction = m_Direction;
        griditemData.GridItemType = EGridItemType.Main;
        //主坐标
        if (mainGridCoord != default)
            griditemData.MainGridCoord = mainGridCoord;
        else if (m_GridItemActor != null)
            //网格坐标未传参 使用当前的网格坐标
            griditemData.MainGridCoord = m_GridItemActor.GridItemComponent.MainGridCoord;

        //物体占用尺寸
        griditemData.GridItemSize = GetGridItemActorSize(m_ValueId);

        RefreshGridItemActorPrefab(griditemData);
    }

    /// <summary>
    /// 获取 网格项目物体 占用尺寸
    /// </summary>
    /// <returns></returns>
    protected virtual GridCoord GetGridItemActorSize(int id)
    {
        return GridCoord.one;
    }

    /// <summary>
    /// 设置 主网格坐标
    /// </summary>
    /// <param name="gridCoord"></param>
    public void SetMainGridCoord(GridCoord gridCoord)
    {
        if (m_MainGridCoord == gridCoord) return;
        m_MainGridCoord = gridCoord;

        if (m_GridItemActor != null)
        {
            m_GridItemActor.SetMainGridCoord(gridCoord);
            m_OnChangePrefabComplete?.Invoke();
        }
    }

    /// <summary>
    /// 设置 网格项目朝向 旋转
    /// </summary>
    public void SetDirectionRotate()
    {
        if (m_GridItemActor.GridItemComponent == null) { return; }

        int directionIndex = (int)m_Direction + 1;
        m_Direction = (EDirection)directionIndex;
        if (m_Direction > EDirection.Right)
        {
            m_Direction = (EDirection)1;
        }
        var gridItemData = m_GridItemActor.GridItemComponent.GridItemData;
        gridItemData.Direction = m_Direction;

        RefreshGridItemActorPrefab(gridItemData);
    }

    //刷新 网格项目预制体
    protected virtual void RefreshGridItemActorPrefab(GridItemData gridItemData)
    {
        
    }

    /// <summary>
    /// 回调 网格项目物体 克隆完成
    /// </summary>
    /// <param name="gridItemActor"></param>
    protected virtual void EvtCloneGridItemActorComplete(GridItemActor gridItemActor)
    {
        //返还 当前的网格项目物体
        ReturnGridItemActor();

        //记录 新网格项目物体
        m_GridItemActor = gridItemActor;
        //立即设置一次新预制体的位置
        if (m_GridItemActor != null)
        {
            //设置坐标
            m_GridItemActor.SetMainGridCoord(m_MainGridCoord);
            //关闭 碰撞根节点
            m_GridItemActor.GridItemComponent.SetColliderRootActive(false);
            //关闭 自动添加或移除 从某个区域的内部网格项目组
            m_GridItemActor.EnableAutoRecordAreaInfo = false;
        }

        m_OnChangePrefabComplete?.Invoke();
    }

    /// <summary>
    /// 返还 网格项目物体
    /// </summary>
    protected virtual void ReturnGridItemActor()
    {
        //返还 网格项目物体
        if (m_GridItemActor == null) { return; }

        AssetTemplateSystem.Instance.ReturnTemplatePrefab(m_GridItemActor.GameObjectGet);
        m_GridItemActor = null;
    }
}
