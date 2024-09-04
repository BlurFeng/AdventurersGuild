using com.ootii.Messages;
using Deploy;
using FsGameFramework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using FsGridCellSystem;

public class PanelGuildBuild : PanelGuildBase
{
    //列表 道具信息
    private List<GameObject> m_ListItemTemplate = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();

        m_GridLayer = GuildGridModel.EGridLayer.Building;

        //建筑列表 实例化所有 配置表中的建筑
        var cfgGuildBuild = ConfigSystem.Instance.GetConfigMap<Building_Config>();
        foreach (var buildingId in cfgGuildBuild.Keys)
        {
            AssetTemplateSystem.Instance.CloneItemBuildingInfo(buildingId, m_RootListItem, (item) =>
            {
                item.transform.localScale = new Vector3(0.4f, 0.4f, 1);
                item.ClickEvent = EvtClickItem;
                m_ListItemTemplate.Add(item.gameObject);
            });
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        //返还 所有Item道具模板
        for (int i = 0; i < m_ListItemTemplate.Count; i++)
        {
            AssetTemplateSystem.Instance.ReturnTemplatePrefab(m_ListItemTemplate[i]);
        }
    }

    public override void OnClose()
    {
        base.OnClose();

        //清空 操作点上的建筑
        SetHandlerGridItemActorId(0);
    }

    protected override int GetGridItemActorValue(int id)
    {
        //获取 当前建筑1级的 对应配置ID
        var buildingLevelId = 0;
        if (id != 0)
            buildingLevelId = ConfigSystem.Instance.GetConfigIdGuildBuildingLevel(id, 1);

        return buildingLevelId;
    }

    #region 事件

    private void EvtClickItem(ItemBuildingInfo item)
    {
        //操作点 设置新建筑
        SetHandlerGridItemActorId(item.cfgBuilding.Id);
    }

    #endregion

    #region 操作点
    public override void GetGridCoordToHandlerObject()
    {
        //操作点 有家具
        if (m_ItemOperateBase.ValueId != 0) { return; }

        //获取 点击单元格的建筑ID
        var buildingId = GuildGridModel.Instance.GetMainGridItemValue(GuildGridModel.EGridLayer.Building, m_OperateGridCoordCur);
        if (buildingId != 0)
            WindowSystem.Instance.OpenWindow(WindowEnum.BuildingInfoWindow, buildingId); //打开弹窗 建筑操作界面
    }

    public override void SetHandlerObjectToGridCoord()
    {
        //操作点 无有效值 或 不可设置
        if (m_ItemOperateBase.ValueId == 0 || !CanSet) { return; }

        //尝试放置于点击单元格
        var cfgBuildingLevel = ConfigSystem.Instance.GetConfig<Building_Level>(m_ItemOperateBase.ValueId);
        if (GuildGridModel.Instance.PushBuildingInfo(cfgBuildingLevel.BuildId, m_OperateGridCoordCur))
            SetHandlerGridItemActorId(0); //清空 操作点上的数据
        else
            WindowSystem.Instance.ShowMsg("该位置不可放置建筑！");
    }

    /// <summary>
    /// 旋转 操作点的建筑
    /// </summary>
    public override void RotateHandlerObject()
    {
        base.RotateHandlerObject();

    }

    /// <summary>
    /// 返还 操作点的建筑
    /// </summary>
    public override void ReturenHandlerObject()
    {
        //操作点 无有效值
        if (m_ItemOperateBase.ValueId == 0) { return; }

        //清空 操作点
        SetHandlerGridItemActorId(0);
    }
    #endregion
}
