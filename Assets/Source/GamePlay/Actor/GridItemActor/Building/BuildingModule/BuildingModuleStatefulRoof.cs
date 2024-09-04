using com.ootii.Messages;
using FsGridCellSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingModuleStatefulRoof : BuildingModuleStateful
{
    /// <summary>
    /// 建筑模块显示，完整。
    /// </summary>
    [SerializeField]
    private GameObject entirety;

    /// <summary>
    /// 关联的屋顶，当自身状态变化时，会设置关联屋顶的状态和自身保持一致
    /// </summary>
    [SerializeField]
    private BuildingModuleStatefulRoof[] m_StaySameRoofs;

    /// <summary>
    /// 屋顶头部名称
    /// 用于确认屋顶是不是属于一个组
    /// </summary>
    [SerializeField]
    private string headName;

    /// <summary>
    /// 屋顶头部名称
    /// 用于确认屋顶是不是属于一个组
    /// </summary>
    public string GetHeadName { get { return headName; } }

    protected override void OnAwake()
    {
        base.OnAwake();

        m_BuildingModuleType = GuildGridModel.EBuildingModuleType.Roof;

        //设置 网格系统 定位功能参数
        m_GridItemComponent.SetLocationCenter(ELocationCenterType.MiddleCenter); //定位中心点
        m_GridItemComponent.SetLocationReceiverRange(ELocationReceiverRangeType.Down, m_GridItemComponent.GridItemSize.X * 2.5f); //定位检查范围
    }

    //回调 定位发起者信息 改变
    protected override void OnLocationEmitterInfoChange(GridItemComponent gridItem)
    {
        //检查 范围内定位发起者信息
        if (gridItem.CheckLocationEmitterInfoDirection(ELocationCheckMode.Have, ELocationDirection.Down))
        {
            //处于屋顶下方 隐藏屋顶
            SetState(BuildingModuleState.Hide);
        }
        else
        {
            //不处于屋顶下方 显示屋顶
            SetState(BuildingModuleState.Entirety);
        }
    }

    /// <summary>
    /// 设置信息
    /// </summary>
    /// <param name="entirety">完整</param>
    public void SetInfo(GameObject entirety)
    {
        this.entirety = entirety;

        headName = GameObjectGet.name.Split('_')[0];
    }

    /// <summary>
    /// 设置保持状态一致的屋顶
    /// </summary>
    /// <param name="staySameRoofs"></param>
    public void SetStaySameRoofs(BuildingModuleStatefulRoof[] staySameRoofs)
    {
        this.m_StaySameRoofs = staySameRoofs;
    }

    protected override bool StateChangeSetting(BuildingModuleState newState, BuildingModuleState oldState)
    {
        base.StateChangeSetting(newState, oldState);

        bool newSet = false;
        //显示或隐藏
        switch (newState)
        {
            case BuildingModuleState.Entirety:
                SetViewRootActive(true);
                break;
            case BuildingModuleState.Hide:
                SetViewRootActive(false);
                break;
        }
        newSet = true;

        return newSet;
    }

    protected override void OnStateChange(BuildingModuleState newState, BuildingModuleState oldState)
    {
        base.OnStateChange(newState, oldState);

        //设置关联屋顶状态和自身一致
        for (int i = 0; i < m_StaySameRoofs.Length; i++)
        {
            m_StaySameRoofs[i]?.SetState(GetState);
        }
    }
}
