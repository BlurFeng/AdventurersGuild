using FsGridCellSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingModuleStatefulTransection: BuildingModuleStateful
{
    /// <summary>
    /// 建筑模块显示，完整。
    /// </summary>
    [SerializeField]
    private GameObject entiretyShow;

    /// <summary>
    /// 建筑模块显示，底部横切面。
    /// </summary>
    [SerializeField]
    private GameObject transectionBottomShow;

    protected override void OnAwake()
    {
        base.OnAwake();

        m_BuildingModuleType = GuildGridModel.EBuildingModuleType.Transection;

        //设置 网格系统 定位功能参数
        m_GridItemComponent.SetLocationCenter(ELocationCenterType.MiddleCenter); //定位中心点
        m_GridItemComponent.SetLocationReceiverRange(ELocationReceiverRangeType.Back, m_GridItemComponent.GridItemSize.Z * 0.6f); //定位检查范围
    }

    //回调 定位发起者信息 改变
    protected override void OnLocationEmitterInfoChange(GridItemComponent gridItem)
    {
        //检查 范围内定位发起者信息
        if (gridItem.CheckLocationEmitterInfoDirection(ELocationCheckMode.Have, ELocationDirection.Back))
        {
            //处于后方 隐藏墙
            SetState(BuildingModuleState.TransectionBottom);
        }
        else
        {
            //显示墙
            SetState(BuildingModuleState.Entirety);
        }
    }

    /// <summary>
    /// 设置信息
    /// </summary>
    /// <param name="entiretyShow">完整</param>
    /// <param name="transectionBottomShow">横切面</param>
    public void SetInfo(GameObject entiretyShow, GameObject transectionBottomShow)
    {
        this.entiretyShow = entiretyShow;
        this.transectionBottomShow = transectionBottomShow;
    }

    protected override bool StateChangeSetting(BuildingModuleState newState, BuildingModuleState oldState)
    {
        base.StateChangeSetting(newState, oldState);

        //在完整显示和显示底部横街面之间切换
        switch (newState)
        {
            case BuildingModuleState.Entirety:
                if (entiretyShow != null)
                    entiretyShow.SetActive(true);
                if (transectionBottomShow != null)
                    transectionBottomShow.SetActive(false);
                return true;
            case BuildingModuleState.TransectionBottom:
                if (entiretyShow != null)
                    entiretyShow.SetActive(false);
                if (transectionBottomShow != null)
                    transectionBottomShow.SetActive(true);
                return true;
        }

        return false;
    }
}
