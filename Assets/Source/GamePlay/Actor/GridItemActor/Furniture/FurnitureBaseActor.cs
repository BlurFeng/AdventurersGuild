using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework.InputSystem;
using Deploy;
using UnityEngine.EventSystems;
using System;
using FsGridCellSystem;
using FsGameFramework;

/// <summary>
/// 场景物体 家具 基类
/// </summary>
public class FurnitureBaseActor : GridItemActor
{
    [SerializeField] private GameObject m_GoOperateTip = null; //按钮 操作

    protected override void OnAwake()
    {
        base.OnAwake();

        Init(FWorldContainer.GetWorld());
    }

    protected override void OnEnableThis()
    {
        base.OnEnableThis();

        //添加至 网格区域
        AddAreaInfoIntraGridItem();
    }

    protected override void OnDisableThis()
    {
        base.OnDisableThis();

        //从网格区域中移除
        RemoveAreaInfoIntraGridItem();
    }

    public override bool Init(object owner = null)
    {
        //bool succeed = base.Init(owner);

        //默认设置
        ShowOperateTip(false);

        return true;
    }

    /// <summary>
    /// 显示 可操作提示
    /// </summary>
    public virtual void ShowOperateTip(bool isShow)
    {
        if (m_GoOperateTip == null) { return; }

        m_GoOperateTip.SetActive(isShow);
    }

    /// <summary>
    /// 执行操作
    /// </summary>
    /// <returns>成功执行</returns>
    public bool ExecuteOperate()
    {
        if (!CheckAreaRequireIsMeet()) { return false; }

        OnExecuteOperate();

        return true;
    }

    /// <summary>
    /// 执行操作 家具子类重写各自执行逻辑
    /// </summary>
    protected virtual void OnExecuteOperate()
    {

    }

    #region 网格系统

    /// <summary>
    /// 家具 配置表数据
    /// </summary>
    protected Prop_Furniture m_CfgFurniture;
    /// <summary>
    /// 家具类型 配置表数据
    /// </summary>
    protected Prop_FurnitureType m_CfgFurnitureType;

    public override void SetGridItemComponentData(GridItemData gridItemData)
    {
        base.SetGridItemComponentData(gridItemData);

        //获取 家具配置表数据
        m_CfgFurniture = ConfigSystem.Instance.GetConfig<Prop_Furniture>(gridItemData.Value, false);
        if (m_CfgFurniture == null)
            m_CfgFurnitureType = null;
        else
            m_CfgFurnitureType = ConfigSystem.Instance.GetConfig<Prop_FurnitureType>(m_CfgFurniture.Type);
    }

    protected override int GetAreaInfoIntraGridItemKey()
    {
        int key = -1;

        var cfgFurniture = ConfigSystem.Instance.GetConfig<Prop_Furniture>(m_GridItemComponent.Value, false);
        if (cfgFurniture != null)
        {
            //家具类型 作为区域内部网格项目的key
            key = cfgFurniture.Type;
        }

        return key;
    }

    //检查 家具所在区域 家具组合要求 是否满足
    protected bool CheckAreaRequireIsMeet()
    {
        //无区域类型要求
        if (m_CfgFurnitureType.UseAreaId == 0) { return true; }

        //判断 当前家具的区域要求是否满足
        bool isMeetArea = false;
        if (m_CfgFurniture != null && m_AreaInfoCur != null)
            isMeetArea = m_AreaInfoCur.Value == m_CfgFurnitureType.UseAreaId;
        if (!isMeetArea)
        {
            var areaName = ConfigSystem.Instance.GetConfig<Building_Area>(m_CfgFurnitureType.UseAreaId).Name;
            WindowSystem.Instance.ShowMsg($"需要放置在{areaName}，才能够使用该家具！");
            return false;
        }

        //判断 当前区域的家具组合要求是否满足
        var cfgBuildingArea = ConfigSystem.Instance.GetConfig<Building_Area>(m_AreaInfoCur.Value);
        foreach (var kv in cfgBuildingArea.FurnitureRequire)
        {
            var furnitureType = kv.Key;
            var countRequire = kv.Value;
            var countCur = m_AreaInfoCur.GetIntraGridItemListCount(furnitureType);
            if (countCur < countRequire)
            {
                WindowSystem.Instance.ShowMsg("需要当前功能区完成家具组合后，才能够使用该家具！");
                return false;
            }
        }

        return true;
    }

    #endregion
}
