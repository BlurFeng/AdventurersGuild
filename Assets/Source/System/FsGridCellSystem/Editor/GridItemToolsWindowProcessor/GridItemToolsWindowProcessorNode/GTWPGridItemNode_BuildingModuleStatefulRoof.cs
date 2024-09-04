using System.Collections.Generic;
using FsGridCellSystem;
using System;
using UnityEngine;

public class GTWPGridItemNode_BuildingModuleStatefulRoof : GTWPGridItemNode_BuildingModule
{
    public override Type GetTargetGridItemType()
    {
        return typeof(BuildingModuleStatefulRoof);
    }

    public override bool OnChangeGridItemPrefab(GameObject gridItem, Component u3dComponent, GameObject viewRoot, GameObject colliderRoot)
    {
        if (!base.OnChangeGridItemPrefab(gridItem, u3dComponent, viewRoot, colliderRoot)) return false;

        BuildingModuleStatefulRoof buildingModuleStatefulDoor = u3dComponent as BuildingModuleStatefulRoof;

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

    public override bool OnCreatePreformedUnitAfter(GameObject preformedUnit)
    {
        if (!base.OnCreatePreformedUnitAfter(preformedUnit)) return false;

        //获取和自己关联的屋顶
        BuildingModuleStatefulRoof[] allRoofs = preformedUnit.GetComponentsInChildren<BuildingModuleStatefulRoof>();
        Dictionary<int, List<BuildingModuleStatefulRoof>> BuildingModuleStatefulRoofsDic = new Dictionary<int, List<BuildingModuleStatefulRoof>>();

        for (int i = 0; i < allRoofs.Length; i++)
        {
            var roofSet = allRoofs[i];

            for (int j = 0; j < allRoofs.Length; j++)
            {
                if (i == j) continue;
                var roofTemp = allRoofs[j];

                if (roofSet.GetHeadName.Equals(roofTemp.GetHeadName))
                {
                    if (!BuildingModuleStatefulRoofsDic.ContainsKey(i))
                        BuildingModuleStatefulRoofsDic.Add(i, new List<BuildingModuleStatefulRoof>());

                    BuildingModuleStatefulRoofsDic[i].Add(roofTemp);
                }
            }
        }

        foreach (var item in BuildingModuleStatefulRoofsDic)
        {
            allRoofs[item.Key].SetStaySameRoofs(item.Value.ToArray());
        }

        return true;
    }
}
