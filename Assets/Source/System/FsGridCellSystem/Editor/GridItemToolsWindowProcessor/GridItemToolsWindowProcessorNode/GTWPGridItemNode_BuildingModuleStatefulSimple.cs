using System.Collections.Generic;
using FsGridCellSystem;
using System;
using UnityEngine;

public class GTWPGridItemNode_BuildingModuleStatefulSimple : GTWPGridItemNode_BuildingModule
{
    public override Type GetTargetGridItemType()
    {
        return typeof(BuildingModuleStatefulSimple);
    }

    public override bool OnChangeGridItemPrefab(GameObject gridItem, Component u3dComponent, GameObject viewRoot, GameObject colliderRoot)
    {
        if (!base.OnChangeGridItemPrefab(gridItem, u3dComponent, viewRoot, colliderRoot)) return false;

        BuildingModuleStatefulSimple buildingModuleStatefulDoor = u3dComponent as BuildingModuleStatefulSimple;

        GameObject entirety = null;


        Transform viewRootTs = viewRoot.transform;
        for (int i = 0; i < viewRootTs.childCount; i++)
        {
            if (entirety) break;

            var child = viewRootTs.GetChild(i);
            if (child.name.Contains("Entirety"))
            {
                entirety = child.gameObject;
            }
        }

        buildingModuleStatefulDoor.SetInfo(entirety);

        return true;
    }
}
