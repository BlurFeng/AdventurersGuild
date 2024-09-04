using FsGridCellSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingModuleStatefulDoor : BuildingModuleStateful
{
    [Header("门-状态部件")]
    /// <summary>
    /// 建筑模块显示，完整。关闭状态。
    /// </summary>
    [SerializeField]
    private GameObject entiretyShowClosed;

    /// <summary>
    /// 建筑模块显示，完整。打开状态。
    /// </summary>
    [SerializeField]
    private GameObject entiretyShowOpened;

    /// <summary>
    /// 建筑模块显示，底部横切面。
    /// </summary>
    [SerializeField]
    private GameObject transectionBottomShow;

    protected override void OnAwake()
    {
        base.OnAwake();

        m_BuildingModuleType = GuildGridModel.EBuildingModuleType.Door;

        //设置 网格系统 定位功能参数
        m_GridItemComponent.SetLocationCenter(ELocationCenterType.MiddleCenter); //定位中心点
        m_GridItemComponent.SetLocationReceiverRange(ELocationReceiverRangeType.FrontBack, m_GridItemComponent.GridItemSize.Z * 0.3f); //定位检查范围
    }

    //回调 定位发起者信息 改变
    protected override void OnLocationEmitterInfoChange(GridItemComponent gridItem)
    {
        //检查 范围内定位发起者信息
        if (gridItem.CheckLocationEmitterInfoDirection(ELocationCheckMode.Have, ELocationDirection.Back))
        {
            //处于后方 隐藏门
            SetState(BuildingModuleState.TransectionBottom);
            m_GridItemComponent.SetRealVolumeCur("Open");
        }
        else if (gridItem.CheckLocationEmitterInfoDirection(ELocationCheckMode.Have, ELocationDirection.Front))
        {
            //处于前方 打开门
            SetState(BuildingModuleState.EntiretyOpened);
            m_GridItemComponent.SetRealVolumeCur("Open");
        }
        else
        {
            //无定位发起者 关闭门
            SetState(BuildingModuleState.EntiretyClosed);
            m_GridItemComponent.SetRealVolumeCur("Close");
        }
    }

    /// <summary>
    /// 设置信息
    /// </summary>
    /// <param name="entiretyShowClosed">完整，关闭。</param>
    /// <param name="entiretyShowOpened">完整，打开。</param>
    /// <param name="transectionBottomShow">横切面</param>
    public void SetInfo(GameObject entiretyShowClosed, GameObject entiretyShowOpened, GameObject transectionBottomShow)
    {
        this.entiretyShowClosed = entiretyShowClosed;
        this.entiretyShowOpened = entiretyShowOpened;
        this.transectionBottomShow = transectionBottomShow;
    }

    protected override bool StateChangeSetting(BuildingModuleState newState, BuildingModuleState oldState)
    {
        base.StateChangeSetting(newState, oldState);

        //在完整显示和显示底部横街面之间切换
        switch (newState)
        {
            case BuildingModuleState.EntiretyClosed:
                if (entiretyShowClosed != null)
                    entiretyShowClosed.SetActive(true);
                if (entiretyShowOpened != null)
                    entiretyShowOpened.SetActive(false);
                if (transectionBottomShow != null)
                    transectionBottomShow.SetActive(false);
                return true;
            case BuildingModuleState.EntiretyOpened:
                if (entiretyShowClosed != null)
                    entiretyShowClosed.SetActive(false);
                if (entiretyShowOpened != null)
                    entiretyShowOpened.SetActive(true);
                if (transectionBottomShow != null)
                    transectionBottomShow.SetActive(false);
                return true;
            case BuildingModuleState.TransectionBottom:
                if (entiretyShowClosed != null)
                    entiretyShowClosed.SetActive(false);
                if (entiretyShowOpened != null)
                    entiretyShowOpened.SetActive(false);
                if (transectionBottomShow != null)
                    transectionBottomShow.SetActive(true);
                return true;
        }

        return false;
    }
}
