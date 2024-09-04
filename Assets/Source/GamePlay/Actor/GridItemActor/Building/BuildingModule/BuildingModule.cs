using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FsGameFramework;

/// <summary>
/// 建筑物模块
/// 根据前后层级关系，将相同显示层级的建筑模块定义为一个整体。
/// </summary>
public class BuildingModule : GridItemActor
{
    private BuildingBaseActor m_BuildingBaseActor;
    protected GuildGridModel.EBuildingModuleType m_BuildingModuleType; //建筑模块类型

    public override bool Init(object outer = null)
    {
        bool succeed = base.Init(outer);

        return succeed;
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        m_IsInitAutoGetGridCoord = true;

        //获取自身所属的建筑根节点类
        m_BuildingBaseActor = GetComponentInParent<BuildingBaseActor>();
        Init(m_BuildingBaseActor.GetOuter);
    }

    protected override void OnStart()
    {
        base.OnStart();

        //添加 建筑模块信息
        GuildGridModel.Instance.PushBuildingModuleInfo(m_BuildingModuleType, m_GridItemComponent);
        //添加至区域
        AddAreaInfoIntraGridItem();
    }

    protected override void OnDestroyThis()
    {
        base.OnDestroyThis();

        //移除 建筑模块信息
        GuildGridModel.Instance.RemoveBuildingModuleInfo(m_GridItemComponent.MainGridCoord);
        //从区域中移除
        RemoveAreaInfoIntraGridItem();
    }
}