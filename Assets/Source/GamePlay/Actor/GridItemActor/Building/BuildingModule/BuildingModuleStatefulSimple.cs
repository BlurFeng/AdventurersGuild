using com.ootii.Messages;
using FsGridCellSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingModuleStatefulSimple : BuildingModuleStateful
{
    /// <summary>
    /// 建筑模块显示，完整。
    /// </summary>
    [SerializeField]
    private GameObject entirety;

    protected override void OnAwake()
    {
        base.OnAwake();

        m_BuildingModuleType = GuildGridModel.EBuildingModuleType.Simple;

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
            //处于下方 隐藏
            SetState(BuildingModuleState.Hide);
        }
        else
        {
            //不处于下方 显示
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
}
