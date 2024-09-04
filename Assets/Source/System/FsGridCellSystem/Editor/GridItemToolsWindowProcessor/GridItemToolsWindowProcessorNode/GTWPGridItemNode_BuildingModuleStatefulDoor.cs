using FsGridCellSystem;
using System;
using UnityEngine;

public class GTWPGridItemNode_BuildingModuleStatefulDoor : GTWPGridItemNode_BuildingModule
{
    public override Type GetTargetGridItemType()
    {
        return typeof(BuildingModuleStatefulDoor);
    }

    public override bool OnChangeGridItemPrefab(GameObject gridItem, Component u3dComponent, GameObject viewRoot, GameObject colliderRoot)
    {
        if (!base.OnChangeGridItemPrefab(gridItem, u3dComponent, viewRoot, colliderRoot)) return false;

        BuildingModuleStatefulDoor buildingModuleStatefulDoor = u3dComponent as BuildingModuleStatefulDoor;

        GameObject entiretyClosedGObj = null;
        GameObject entiretyOpenGObj = null;
        GameObject transectionGObj = null;

        Transform viewRootTs = viewRoot.transform;
        for (int i = 0; i < viewRootTs.childCount; i++)
        {
            if (entiretyClosedGObj && entiretyOpenGObj && transectionGObj) break;

            var child = viewRootTs.GetChild(i);
            if (child.name.Contains("EntiretyClosed"))
            {
                entiretyClosedGObj = child.gameObject;
            }
            else if (child.name.Contains("EntiretyOpened"))
            {
                entiretyOpenGObj = child.gameObject;
            }
            else if (child.name.Contains("Transection"))
            {
                transectionGObj = child.gameObject;
            }
        }

        buildingModuleStatefulDoor.SetInfo(entiretyClosedGObj, entiretyOpenGObj, transectionGObj);

        return true;
    }
}
