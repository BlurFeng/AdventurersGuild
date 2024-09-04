using Deploy;
using FsGameFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGridCellSystem;

/// <summary>
/// 模板预制体模块 克隆通用模板预制体
/// </summary>
public class AssetTemplateSystem : Singleton<AssetTemplateSystem>, IDestroy
{
    /// <summary>
    /// 模板信息
    /// </summary>
    private struct TemplateInfo
    {
        public TemplateInfo(GameObject prefab, bool useGameObjectPool)
        {
            Prefab = prefab;
            UseGameObjectPool = useGameObjectPool;
        }

        /// <summary>
        /// 模板预制体
        /// </summary>
        public GameObject Prefab;
        
        /// <summary>
        /// 是否 试用对象池
        /// </summary>
        public bool UseGameObjectPool;
    }

    private Dictionary<string, TemplateInfo> m_TemplateLoadedPrafab = new Dictionary<string, TemplateInfo>(); //模板预制体 已加载
    private List<string> m_TemplateInitPreload = new List<string>() //模板预制体 初始化预加载
    {
        "ItemPropInfo",
        "ItemPropInfoSelectCount",
        "ItemBuildingInfo",
    };

    public override void Init()
    {
        //预加载 模板预制体
        //对象池 初始化
        for (int i = 0; i < m_TemplateInitPreload.Count; i++)
        {
            string templateName = m_TemplateInitPreload[i];
            string address = AssetAddressUtil.GetPrefabTemplateAddress(templateName);
            LoadPrafabInitPool(address);
        }
    }

    /// <summary>
    /// 克隆 道具预制体 普通
    /// </summary>
    /// <param name="itemId">道具ID</param>
    /// <param name="itemRoot">预制体根节点</param>
    /// <param name="count">显示道具数量 -1为动态显示背包道具数量</param>
    /// <param name="callBack"></param>
    public void CloneItemPropInfo(int itemId, Transform itemRoot, int count = -1, bool showName = false, Action<ItemPropInfo> callBack = null)
    {
        string address = AssetAddressUtil.GetPrefabTemplateAddress("ItemPropInfo");
        CloneTemplatePrefab(address, itemRoot, (prefabClone) =>
        {
            var item = prefabClone.GetComponent<ItemPropInfo>();
            if (item != null)
            {
                item.SetInfo(itemId, count, showName);
                callBack?.Invoke(item);
            }
        });
    }

    /// <summary>
    /// 克隆 道具预制体 选择数量
    /// </summary>
    /// <param name="itemId">道具ID</param>
    /// <param name="itemRoot">预制体根节点</param>
    /// <param name="count">显示道具数量</param>
    /// <param name="callBack"></param>
    public void CloneItemPropInfoChoiceCount(int itemId, Transform itemRoot, int count = -1, Action<ItemPropInfo> callBack = null)
    {
        string address = AssetAddressUtil.GetPrefabTemplateAddress("ItemPropInfoSelectCount");
        CloneTemplatePrefab(address, itemRoot, (prefabClone) =>
        {
            var item = prefabClone.GetComponent<ItemPropInfoSelectCount>();
            if (item != null)
            {
                item.SetInfo(itemId, count);
                callBack?.Invoke(item);
            }
        });
    }

    /// <summary>
    /// 克隆 Item建筑信息
    /// </summary>
    /// <param name="buildingId">建筑ID</param>
    /// <param name="itemRoot">预制体根节点</param>
    /// <param name="count">显示道具数量 -1为动态显示背包道具数量</param>
    /// <param name="callBack"></param>
    public void CloneItemBuildingInfo(int buildingId, Transform itemRoot, Action<ItemBuildingInfo> callBack = null)
    {
        string address = AssetAddressUtil.GetPrefabTemplateAddress("ItemBuildingInfo");
        CloneTemplatePrefab(address, itemRoot, (prefabClone) =>
        {
            var item = prefabClone.GetComponent<ItemBuildingInfo>();
            if (item != null)
            {
                item.SetInfo(buildingId);
                callBack?.Invoke(item);
            }
        });
    }

    /// <summary>
    /// 克隆 家具预制体
    /// </summary>
    /// <param name="gridItemData"></param>
    /// <param name="transRoot"></param>
    /// <param name="callBack"></param>
    public void CloneFurniturePrefab(GridItemData gridItemData, Transform transRoot = null, Action<FurnitureBaseActor> callBack = null)
    {
        Action<GameObject> action = (prefabClone) =>
        {
            var furnitureBase = prefabClone.GetComponent<FurnitureBaseActor>();
            if (furnitureBase != null)
            {
                furnitureBase.SetGridItemComponentData(gridItemData);
                callBack?.Invoke(furnitureBase);
            }
            else
            {
                Debug.LogError($"AssetTemplateSystem.CloneFurniturePrefab() Error! >> 家具预制体没有FurnitureBaseActor组件 PrefabName-{prefabClone.name}");
            }
        };

        //根据家具的类型 实例化对应的预制体
        var cfg = ConfigSystem.Instance.GetConfig<Prop_Furniture>(gridItemData.Value);
        var type = (PropModel.EFurnitureType)cfg.Type;
        string prefabName = $"{cfg.Id}_{type}_{gridItemData.Direction}";
        var prefabAddress = AssetAddressUtil.GetPrefabFurnitureAddress($"{type}/{cfg.Id}/{prefabName}");
        CloneTemplatePrefab(prefabAddress, transRoot, action);
    }

    /// <summary>
    /// 克隆 建筑预制体
    /// </summary>
    /// <param name="gridItemData"></param>
    /// <param name="itemRoot"></param>
    /// <param name="callBack"></param>
    public void CloneBuildingPrefab(GridItemData gridItemData, Transform itemRoot = null, Action<BuildingBaseActor> callBack = null)
    {
        Action<GameObject> action = (prefabClone) =>
        {
            var buildingBase = prefabClone.GetComponent<BuildingBaseActor>();
            if (buildingBase != null)
            {
                buildingBase.SetGridItemComponentData(gridItemData); //设置 网格单元格数据
                callBack?.Invoke(buildingBase);
            }
            else
            {
                Debug.LogError($"AssetTemplateSystem.CloneBuildingPrefab() Error! >> 建筑预制体没有BuildingBaseActor组件 PrefabName-{prefabClone.name}");
            }
        };

        var buildingInfo = GuildGridModel.Instance.GetBuildingInfo(gridItemData.Value);
        int buildingLevelId;
        if (buildingInfo == null)
        {
            //不是已建造的建筑 Value为buildingLevelId
            buildingLevelId = gridItemData.Value;
        }
        else
        {
            //已建造的建筑 使用建造建筑的数据
            buildingLevelId = buildingInfo.CfgBuildingLevelId;
        }

        var cfgBuildingLevel = ConfigSystem.Instance.GetConfig<Building_Level>(buildingLevelId);
        string prefabName = $"{buildingLevelId}_Building";
        var prefabAddress = AssetAddressUtil.GetPrefabBuildingAddress($"{cfgBuildingLevel.BuildId}/{prefabName}");
        CloneTemplatePrefab(prefabAddress, itemRoot, action, false);
    }

    /// <summary>
    /// 获取 模板预制体的克隆实例
    /// </summary>
    /// <param name="prefabAddress"></param>
    /// <param name="parent"></param>
    /// <param name="callBack"></param>
    /// <param name="useGameObjectPool">是否使用对象池</param>
    public void CloneTemplatePrefab(string prefabAddress, Transform parent, Action<GameObject> callBack, bool useGameObjectPool = true)
    {
        TemplateInfo templateInfo;
        if (m_TemplateLoadedPrafab.TryGetValue(prefabAddress, out templateInfo))
        {
            //获取 模板预制体 克隆实例
            GameObject prefabClone = null;
            if (templateInfo.UseGameObjectPool)
            {
                prefabClone = GameObjectPool.Instance.Get(templateInfo.Prefab);
            }
            else
            {
                prefabClone = GameObject.Instantiate(templateInfo.Prefab);
            }
            
            //初始化 设置激活 与 父物体
            prefabClone.SetActive(true);
            Transform prefabTrans = prefabClone.transform;
            prefabTrans.SetParent(parent);
            prefabTrans.localPosition = Vector3.zero;

            //回调返回Prefab实例
            callBack?.Invoke(prefabClone);
        }
        else
        {
            //未加载的预制体 进行加载后回调返回
            m_TemplateLoadedPrafab.Remove(prefabAddress);
            LoadPrafabInitPool(prefabAddress, 1, (prefabNew) =>
            {
                //克隆 Prefab实例
                GameObject prefabClone = null;
                if (useGameObjectPool)
                {
                    prefabClone = GameObjectPool.Instance.Get(prefabNew);
                }
                else
                {
                    prefabClone = GameObject.Instantiate(prefabNew);
                }
                //初始化 设置激活 与 父物体
                prefabClone.SetActive(true);
                Transform prefabTrans = prefabClone.transform;
                prefabTrans.SetParent(parent);
                prefabTrans.localPosition = Vector3.zero;

                //回调返回Prefab实例
                callBack?.Invoke(prefabClone);
            }, useGameObjectPool);
        }
    }

    /// <summary>
    /// 返还 模板预制体克隆实例
    /// </summary>
    /// <param name="templateClone"></param>
    public void ReturnTemplatePrefab(GameObject templateClone)
    {
        if (!GameObjectPool.Instance.Return(templateClone))
        {
            //非对象池物体 直接销毁
            GameObject.Destroy(templateClone);
        }
    }

    /// <summary>
    /// 加载 新的预制体
    /// </summary>
    /// <param name="prefanAddress"></param>
    /// <param name="poolInitCount"></param>
    /// <param name="callback"></param>
    /// <param name=""></param>
    /// <param name="useGameObjectPool"></param>
    private void LoadPrafabInitPool(string prefanAddress, int poolInitCount = 6, Action<GameObject> callback = null, bool useGameObjectPool = true)
    {
        AssetSystem.Instance.LoadPrefab(prefanAddress, (prefabNew) =>
        {
            if (m_TemplateLoadedPrafab.ContainsKey(prefanAddress))
            {
                //重复加载 直接释放
                AssetAddressSystem.Instance.UnloadAsset(prefanAddress);
            }
            else
            {
                //第一次加载
                m_TemplateLoadedPrafab.Add(prefanAddress, new TemplateInfo(prefabNew, useGameObjectPool));
                //缓存池 初始化
                if (useGameObjectPool)
                {
                    GameObjectPool.Instance.InitGameObject(prefabNew, poolInitCount, true);
                }
            }

            callback?.Invoke(prefabNew);
        });
    }

    /// <summary>
    /// 卸载 模板预制体
    /// </summary>
    /// <param name="templateName">模板预制体名 默认为null卸载所有</param>
    /// <returns>是否卸载成功</returns>
    private bool UnloadmTemplatePrafab(string templateName = null)
    {
        if (string.IsNullOrEmpty(templateName))
        {
            //卸载 所有模板预制体
            foreach (var name in m_TemplateLoadedPrafab.Keys)
            {
                string address = AssetAddressUtil.GetPrefabTemplateAddress(name);
                AssetAddressSystem.Instance.UnloadAsset(address);
            }
            m_TemplateLoadedPrafab.Clear();

            return true;
        }
        else
        {
            //卸载 指定模板预制体
            if (m_TemplateLoadedPrafab.ContainsKey(templateName))
            {
                //未加载Prefab 加载后实例化
                m_TemplateLoadedPrafab.Remove(templateName);
                string address = AssetAddressUtil.GetPrefabTemplateAddress(templateName);
                AssetAddressSystem.Instance.UnloadAsset(address);
            }
        }

        return false;
    }
}
