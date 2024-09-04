using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using FsGridCellSystem;
using Deploy;

public class ItemOperateFurniture : ItemOperateBase
{
    public override GuildGridModel.EGridItemSetType SetType
    {
        get
        {
            GuildGridModel.EGridItemSetType setType = GuildGridModel.EGridItemSetType.None;
            var cfg = ConfigSystem.Instance.GetConfig<Prop_Furniture>(ValueId);
            if (cfg != null)
                setType = (GuildGridModel.EGridItemSetType)cfg.SetType;

            return setType;
        }
    }

    protected override GridCoord GetGridItemActorSize(int id)
    {
        return GuildGridModel.GetGridItemSize(GuildGridModel.EGridLayer.Furniture, id);
    }

    protected override void RefreshGridItemActorPrefab(GridItemData gridItemData)
    {
        //实例化 网格项目物体
        AssetTemplateSystem.Instance.CloneFurniturePrefab(gridItemData, transform, EvtCloneGridItemActorComplete);
    }
}
