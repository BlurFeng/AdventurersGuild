using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace FsGridCellSystem
{
    /// <summary>
    /// 网格物品工具窗口配置
    /// 可缓存和存储用户在窗口进行的配置
    /// </summary>
    [Serializable]
    public class GridItemToolsWindowConfig : ScriptableObject
    {
        //网格物品配置KeyVaule
        [Serializable]
        public struct GridItemScriptSetting
        {
            /// <summary>
            /// 脚本Tag，在Aseprite中编辑资源时设置，通过导出的数据Text获取并进行验证
            /// </summary>
            public string scriptTag;

            /// <summary>
            /// 需要挂载在GridItem预制体上的脚本
            /// </summary>
            public MonoScript gridItemScript;

            public GridItemScriptSetting(string scriptTag, MonoScript gridItemScript)
            {
                this.scriptTag = scriptTag;
                this.gridItemScript = gridItemScript;
            }
            public static bool operator ==(GridItemScriptSetting a, GridItemScriptSetting b)
            {
                if (a.scriptTag != b.scriptTag) return false;
                if (a.gridItemScript != b.gridItemScript) return false;

                return true;
            }

            public static bool operator !=(GridItemScriptSetting a, GridItemScriptSetting b)
            {
                return !(a == b);
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (obj.GetType() != GetType()) return false;

                GridItemScriptSetting objData = (GridItemScriptSetting)obj;
                return objData == this;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        public MonoScript processorScript;//工作流处理器脚本GridItemToolsWindowProcessor

        //生成预制件及批量网格物品
        public TextAsset gridItemsDataTxt_CreatePrefab;//网格物品数据文本
        public string imagesFolderPath_CreatePrefab;//网格物品图片存放文件夹路径
        public string gridItemsExportFolder_CreatePrefab;//网格物品预制体导出文件夹路径
        public MonoScript gridItemMonoScript_CreatePrefab;//网格物品脚本基类
        [SerializeField]
        public List<GridItemScriptSetting> gridItemScriptSettings = new List<GridItemScriptSetting>();//网格物品脚本详细配置
        public bool useExistedPrefab_CreatePrefab = true;//使用已经存在的预制体
        public string gridItemPrefabSearchPath_CreatePrefab;//网格物品预制体搜索路径
        public bool gridItemPrefabUpdate_CreatePrefab = true;//更新已有预制体的数据
        public bool gridItemPrefabUpdate_GridItemMonoScript = true;//更新Mono脚本
        public bool gridItemPrefabUpdate_GridItemComponent_CreatePrefab = true;//更新GridItem上的GridItemComponent数据
        public bool gridItemPrefabUpdate_ViewRoot_CreatePrefab = true;//更新GridItem中ViewRoot显示用节点
        public bool gridItemPrefabUpdate_ColliderRoot_CreatePrefab = false;//更新碰撞器节点，默认关闭。因为可能错误的覆盖了经过手工调整的碰撞器。

        //更新单个网格物品预制体
        public GameObject singleGridItemPrefabUpdate_Prefab;//网格物品预制体
        public TextAsset singleGridItemPrefabUpdate_DataText;//网格物品数据文本
        public bool singleGridItemPrefabUpdate_GridItemMonoScript = true;//更新Mono脚本
        public bool singleGridItemPrefabUpdate_GridItemComponent = true;//更新GridItem上的GridItemComponent数据
        public bool singleGridItemPrefabUpdate_ViewRoot = true;//更新GridItem中ViewRoot显示用节点
        public bool singleGridItemPrefabUpdate_ColliderRoot = true;//更新碰撞器节点，默认关闭。因为可能错误的覆盖了经过手工调整的碰撞器。

        //网格物品整体组装件更新
        public GameObject gridItemPreformedUnitPrefab_Update_Prefab;//网格物品预制件的预制体
        public TextAsset gridItemsDataTxt_Update_DataText;//网格物品数据文本

        //批量设置网格物品组装件内所有网格物品的ViewRoot
        public GameObject gridItemPreformedUnitPrefab_ViewRootSet;//网格物品预制件的预制体
        public TextAsset gridItemsDataTxt_ViewRootSet;//网格物品数据文本
    }
}