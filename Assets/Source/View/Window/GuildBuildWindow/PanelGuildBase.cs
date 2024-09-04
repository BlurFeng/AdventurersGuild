using com.ootii.Messages;
using Deploy;
using FsGameFramework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using FsGridCellSystem;

public class PanelGuildBase : MonoBehaviour
{
    [SerializeField] private GameObject m_GoPanel = null; //物体 面板
    [SerializeField] protected ItemOperateBase m_ItemOperateBase; //当前家具
    [SerializeField] protected Transform m_RootListItem = null; //根节点 建筑列表

    /// <summary>
    /// 操作点 网格物体改变
    /// </summary>
    public Action OnChangePrefabComplete 
    { 
        get { return m_OnChangePrefabComplete; } 
        set 
        { 
            m_OnChangePrefabComplete = value;
            m_ItemOperateBase.OnChangePrefabComplete = m_OnChangePrefabComplete;
        }
    }
    private Action m_OnChangePrefabComplete;

    /// <summary>
    /// 是否 可以放置
    /// </summary>
    public bool CanSet { get; set; } = false;

    protected GuildGridModel.EGridLayer m_GridLayer;

    /// <summary>
    /// 操作的网格物体（家具、建筑等）
    /// </summary>
    public ItemOperateBase ItemOperateBase
    {
        get { return m_ItemOperateBase; }
    }

    /// <summary>
    /// 操作点 有物体
    /// </summary>
    public bool HandlerHadObject
    {
        get
        {
            return m_ItemOperateBase.ValueId != 0;
        }
    }

    protected virtual void Awake()
    {
        
    }

    protected virtual void OnDestroy()
    {
        
    }

    /// <summary>
    /// 打开
    /// </summary>
    public virtual void OnOpen()
    {
        m_GoPanel.SetActive(true);
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public virtual void OnClose()
    {
        m_GoPanel.SetActive(false);
    }

    /// <summary>
    /// 获取 网格项目物体 有效值
    /// </summary>
    /// <returns></returns>
    protected virtual int GetGridItemActorValue(int id)
    {
        return id;
    }

    #region 操作点
    protected GridCoord m_OperateGridCoordCur; //单元格坐标 当前

    /// <summary>
    /// 设置 操作点的网格坐标
    /// </summary>
    public void SetOperateGridCoord(GridCoord gridCoord)
    {
        m_OperateGridCoordCur = gridCoord;
        //刷新 操作点显示物体位置
        m_ItemOperateBase.SetMainGridCoord(m_OperateGridCoordCur);
    }

    /// <summary>
    /// 获取 网格物体进操作点
    /// </summary>
    public virtual void GetGridCoordToHandlerObject()
    {
        
    }

    /// <summary>
    /// 设置 操作点物体进网格
    /// </summary>
    public virtual void SetHandlerObjectToGridCoord()
    {
        
    }

    /// <summary>
    /// 旋转 操作点的物体
    /// </summary>
    public virtual void RotateHandlerObject()
    {

    }

    /// <summary>
    /// 返还 操作点的物体
    /// </summary>
    public virtual void ReturenHandlerObject()
    {
        
    }

    //设置 操作点的网格物体项目 有效值
    protected void SetHandlerGridItemActorId(int valueId, EDirection eDirection = EDirection.Down)
    {
        if (m_ItemOperateBase.ValueId == valueId) { return; }

        //设置 操作的家具
        m_ItemOperateBase.SetGridItemActorData(GetGridItemActorValue(valueId), eDirection, m_OperateGridCoordCur);
    }
    #endregion
}
