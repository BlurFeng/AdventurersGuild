using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGridCellSystem;
using System;

public class GTWPGridItemNode_BuildingModuleStatefulTransection : GTWPGridItemNode_BuildingModule
{
    public override Type GetTargetGridItemType()
    {
        return typeof(BuildingModuleStatefulTransection);
    }

    public override bool OnChangeGridItemPrefab(GameObject gridItem, Component u3dComponent, GameObject viewRoot, GameObject colliderRoot)
    {
        if (!base.OnChangeGridItemPrefab(gridItem, u3dComponent, viewRoot, colliderRoot)) return false;

        BuildingModuleStatefulTransection buildingModuleStatefulTransection = u3dComponent as BuildingModuleStatefulTransection;

        GameObject entiretyGObj = null;
        GameObject transectionGObj = null;

        Transform viewRootTs = viewRoot.transform;
        for (int i = 0; i < viewRootTs.childCount; i++)
        {
            if (entiretyGObj && transectionGObj) break;

            var child = viewRootTs.GetChild(i);
            if (child.name.Contains("Entirety"))
            {
                entiretyGObj = child.gameObject;
            }
            else if (child.name.Contains("Transection"))
            {
                transectionGObj = child.gameObject;
            }
        }

        buildingModuleStatefulTransection.SetInfo(entiretyGObj, transectionGObj);

        return true;
    }
}
