using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGridCellSystem;
using System;

public class GTWPGridItemNode_BuildingModule : GridItemToolsWindowProcessorGridItemNode
{
    public override Type GetTargetGridItemType()
    {
        return typeof(BuildingModule);
    }

    public override bool OnChangeGridItemPrefab(GameObject gridItem, Component u3dComponent, GameObject viewRoot, GameObject colliderRoot)
    {
        if (!base.OnChangeGridItemPrefab(gridItem, u3dComponent, viewRoot, colliderRoot)) return false;

        bool isGroundLayer = false;
        if (
            gridItem.name.Contains("Ground")
            || gridItem.name.Contains("Floor")
            || gridItem.name.Contains("Wall")
            || gridItem.name.Contains("Step")
            || gridItem.name.Contains("Stairs"))
        {
            isGroundLayer = true;
        }

        if (isGroundLayer)
            colliderRoot.SetLayerRecursively(LayerMask.NameToLayer("Ground"));
        else
            colliderRoot.SetLayerRecursively(LayerMask.NameToLayer("Default"));

        return true;
    }
}
