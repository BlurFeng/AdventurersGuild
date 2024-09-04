using FsGridCellSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 建筑物模块
/// 拥有状态，根据状态变化切换显示内容。
/// </summary>
public class BuildingModuleStateful : BuildingModule
{
    /// <summary>
    /// 当前显示状态
    /// </summary>
    private BuildingModuleState m_BuildingModuleState;

    /// <summary>
    /// 获取当前状态
    /// </summary>
    public BuildingModuleState GetState { get { return m_BuildingModuleState; } }

    protected override void OnAwake()
    {
        base.OnAwake();

        //设置 默认显示状态
        SetState(BuildingModuleState.Entirety);

        m_GridItemComponent.OnLocationEmitterInfoChange = OnLocationEmitterInfoChange;
    }

    protected override void OnEnableThis()
    { 
        base.OnEnableThis();

        m_GridItemComponent.EnableLocationReceiver = true;
    }

    protected override void OnDisableThis()
    {
        base.OnDisableThis();

        m_GridItemComponent.EnableLocationReceiver = false;
    }

    /// <summary>
    /// 范围内 定位发起者信息 改变
    /// </summary>
    /// <param name="gridItem"></param>
    protected virtual void OnLocationEmitterInfoChange(GridItemComponent gridItem)
    {

    }

    public override bool Init(object outer = null)
    {
        bool succeed = base.Init(outer);

        return succeed;
    }

    /// <summary>
    /// 设置显示状态
    /// </summary>
    /// <param name="newIsShowEntirety"></param>
    /// <param name="force"></param>
    /// <returns></returns>
    public bool SetState(BuildingModuleState newBuildingModuleState, bool force = false)
    {
        if (!force && newBuildingModuleState == m_BuildingModuleState) return false;

        if (StateChangeSetting(newBuildingModuleState, m_BuildingModuleState))
        {
            var oldState = m_BuildingModuleState;
            m_BuildingModuleState = newBuildingModuleState;
            OnStateChange(m_BuildingModuleState, oldState);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 状态发生变化时
    /// 返回True后会更新m_BuildingModuleState的值到NewState
    /// </summary>
    /// <param name="newState"></param>
    /// <param name="oldState"></param>
    /// <returns></returns>
    protected virtual bool StateChangeSetting(BuildingModuleState newState, BuildingModuleState oldState)
    {
        return true;
    }

    /// <summary>
    /// 当状态变化时
    /// </summary>
    /// <param name="newState"></param>
    /// <param name="oldState"></param>
    protected virtual void OnStateChange(BuildingModuleState newState, BuildingModuleState oldState)
    {

    }

    /// <summary>
    /// 隐藏显示物体
    /// 必须使用的接口 让GridItemComponent能记录显示物体的显示状态
    /// </summary>
    /// <param name="isActive"></param>
    protected void SetViewRootActive(bool isActive)
    {
        m_GridItemComponent.SetViewRootIsVisible(isActive);
    }
}