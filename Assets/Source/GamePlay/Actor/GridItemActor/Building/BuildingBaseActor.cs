using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework.InputSystem;
using Deploy;
using UnityEngine.EventSystems;
using System;
using FsGridCellSystem;

namespace FsGameFramework
{
    /// <summary>
    /// 场景中的建筑
    /// </summary>
    public class BuildingBaseActor : GridItemActor
    {
        protected override void OnAwake()
        {
            base.OnAwake();

            Init(FWorldContainer.GetWorld());
        }

        public override bool Init(object owner = null)
        {
            return true;
        }

        /// <summary>
        /// 设置 网格系统 区域信息
        /// </summary>
        /// <param name="areaGroupInfoId"></param>
        public void SetGridCellSystemAreaInfo(int areaGroupInfoId)
        {
            var rootAreaGroup = TransformGet.Find("AreaGroup");
            for (int i = 0; i < rootAreaGroup.childCount; i++)
            {
                var areaItem = rootAreaGroup.GetChild(i);
                var areaActor = areaItem.GetComponent<AreaBaseActor>();
                areaActor.RefreshGridCoordOfWorldPos(); //根据世界坐标 刷新网格坐标
                //添加 区域信息
                var areaInfo = new AreaInfo(areaItem.name, areaActor.GridItemComponent.MainGridCoord, areaActor.GridItemComponent.GetGridItemSizeAtDirection);
                GuildGridModel.Instance.AddAreaInfo(areaGroupInfoId, areaInfo);
                //检查区域内的建筑模块或家具 添加至区域内
                foreach (var buildingModuleInfo in GuildGridModel.Instance.DicBuildingModuleInfo.Values)
                    areaInfo.AddIntraGridItem(buildingModuleInfo.GridItemComponent);
                foreach (var furnitureInfo in GuildGridModel.Instance.DicFurnitureInfo.Values)
                    areaInfo.AddIntraGridItem(furnitureInfo.GridItemComponent);
            }
        }

        public override void SetGridItemComponentData(GridItemData gridItemData)
        {
            base.SetGridItemComponentData(gridItemData);

        }

        /// <summary>
        /// 刷新 世界坐标 (建筑制作预制体时 是以左下为原点 无需进行坐标修正）
        /// </summary>
        protected override void RefreshWorldPosition()
        {
            var posOri = GuildGridModel.Instance.GetWorldPosition(m_GridItemComponent.MainGridCoord);
            TransformGet.position = posOri;
        }
    }
}
