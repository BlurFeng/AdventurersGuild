using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using FsGridCellSystem;
using Deploy;

public class ItemOperateBuilding : ItemOperateBase
{
    private Building_Config m_CfgBuilding; //配置表 建筑
    private Building_Level m_CfgGuildBuildingLevel; //配置表 建筑

    public override GuildGridModel.EGridItemSetType SetType
    {
        get
        {
            GuildGridModel.EGridItemSetType setType = GuildGridModel.EGridItemSetType.None;
            var cfgBuildingLevel = ConfigSystem.Instance.GetConfig<Building_Level>(ValueId);
            if (cfgBuildingLevel != null)
            {
                var cfgBuild = ConfigSystem.Instance.GetConfig<Building_Config>(cfgBuildingLevel.BuildId);
                if (cfgBuild != null)
                {
                    setType = (GuildGridModel.EGridItemSetType)cfgBuild.SetType;
                }
            }

            return setType;
        }
    }

    protected override void Init()
    {
        base.Init();
    }

    protected override GridCoord GetGridItemActorSize(int id)
    {
        //建筑尺寸
        int buildingId = 0;
        var cfgBuildingLevel = ConfigSystem.Instance.GetConfig<Building_Level>(id);
        if (cfgBuildingLevel != null)
        {
            buildingId = cfgBuildingLevel.BuildId;
        }
        var gridItemSize = GuildGridModel.GetGridItemSize(GuildGridModel.EGridLayer.Building, buildingId);

        return gridItemSize;
    }

    protected override void RefreshGridItemActorPrefab(GridItemData gridItemData)
    {
        //立即返还 当前的建筑项目物体
        ReturnGridItemActor();

        //实例化 建筑渲染实例
        m_CfgGuildBuildingLevel = ConfigSystem.Instance.GetConfig<Building_Level>(gridItemData.Value);
        if (m_CfgGuildBuildingLevel == null) { return; }
        m_CfgBuilding = ConfigSystem.Instance.GetConfig<Building_Config>(m_CfgGuildBuildingLevel.BuildId);
        string prefabName = $"{m_CfgGuildBuildingLevel.Id}_Building";
        var prefabAddr = AssetAddressUtil.GetPrefabBuildingAddress($"{m_CfgBuilding.Id}/{prefabName}");
        AssetTemplateSystem.Instance.CloneBuildingPrefab(gridItemData, transform, EvtCloneGridItemActorComplete);
    }

    protected override void ReturnGridItemActor()
    {
        base.ReturnGridItemActor();

        m_CfgBuilding = null;
        m_CfgGuildBuildingLevel = null;
    }
}
