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
using FsListItemPages;

public class PanelGuildFurniture : PanelGuildBase
{
    private Dictionary<int, GameObject> m_DicFurnitureItemPropGo = new Dictionary<int, GameObject>(); //字典 家具ID,Item物体

    protected override void Awake()
    {
        base.Awake();

        MessageDispatcher.AddListener(PlayerModelMsgType.PROP_BACKPACK_CHANGE_FURNITURE, MsgPropBackpackChangeFurniture);

        m_GridLayer = GuildGridModel.EGridLayer.Furniture;

        //家具列表 实例化所有家具Item道具
        var propsFurniture = PlayerModel.Instance.GetPropInfos(PropModel.EPropType.Furniture);
        foreach (var kv in propsFurniture)
        {
            int furnitureId = kv.Key;
            AssetTemplateSystem.Instance.CloneItemPropInfo(furnitureId, m_RootListItem, -1, true, (propInfo) =>
            {
                propInfo.transform.localScale = new Vector3(0.4f, 0.4f, 1);
                propInfo.ClickEvent = EvtClickItem;
                m_DicFurnitureItemPropGo.Add(furnitureId, propInfo.gameObject);
            });
        }

        AwakeAreaInfo();  //功能区 家具需求
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        MessageDispatcher.RemoveListener(PlayerModelMsgType.PROP_BACKPACK_CHANGE_FURNITURE, MsgPropBackpackChangeFurniture);

        //返还 所有Item道具模板
        foreach (var itemGo in m_DicFurnitureItemPropGo.Values)
        {
            AssetTemplateSystem.Instance.ReturnTemplatePrefab(itemGo);
        }

        //操作点 已有家具 归还至背包
        if (m_ItemOperateBase.ValueId != 0)
            ChangeBackpackFurnitureCount(m_ItemOperateBase.ValueId, 1);

        DestroyAreaInfo(); //功能区 家具需求
    }

    public override void OnOpen()
    {
        base.OnOpen();

        OnOpenAreaInfo(); //功能区 家具需求
    }

    public override void OnClose()
    {
        base.OnClose();

        //返还 操作点的家具
        ReturenHandlerObject();
    }

    #region 事件

    //点击 背包中的家具项目
    private void EvtClickItem(ItemPropInfo itemPropInfo)
    {
        var count = PlayerModel.Instance.GetPropCount(itemPropInfo.ItemConfig.Id);
        if (count <= 0)
        {
            return;
        }

        //操作点 已有家具 归还至背包
        if (m_ItemOperateBase.ValueId != 0)
        {
            ChangeBackpackFurnitureCount(m_ItemOperateBase.ValueId, 1);
        }

        //操作点 设置新家具 背包中减少家具数量
        SetHandlerGridItemActorId(itemPropInfo.ItemConfig.Id);
        ChangeBackpackFurnitureCount(m_ItemOperateBase.ValueId, -1);
    }

    #endregion

    //改变 背包中的 家具数量
    private void ChangeBackpackFurnitureCount(int furnitureId, int changeCount)
    {
        if (changeCount > 0)
            PlayerModel.Instance.AddPropInfo(furnitureId, changeCount); //背包 家具数量 增加
        else if (changeCount < 0)
            PlayerModel.Instance.RemovePropInfo(m_ItemOperateBase.ValueId, 1); //背包 家具数量 减少
    }

    #region 消息

    //消息 背包道具变化 家具
    private void MsgPropBackpackChangeFurniture(IMessage rMessage)
    {
        PropInfo propInfo = rMessage.Data as PropInfo;

        //道具数量为0时 Item道具实例不显示
        int countCur = propInfo.Count;
        int furnitureId = propInfo.Id;
        GameObject itemPropGo = null;
        if (countCur <= 0 && m_DicFurnitureItemPropGo.TryGetValue(furnitureId, out itemPropGo))
        {
            if (itemPropGo != null)
            {
                AssetTemplateSystem.Instance.ReturnTemplatePrefab(itemPropGo);
                m_DicFurnitureItemPropGo.Remove(furnitureId);
            }
        }
        else
        {
            //道具增加时 若无Item道具实例 则显示
            if (!m_DicFurnitureItemPropGo.ContainsKey(furnitureId))
            {
                AssetTemplateSystem.Instance.CloneItemPropInfo(furnitureId, m_RootListItem, -1, true, (propInfo) =>
                {
                    propInfo.transform.localScale = new Vector3(0.4f, 0.4f, 1);
                    propInfo.ClickEvent = EvtClickItem;
                    m_DicFurnitureItemPropGo.Add(furnitureId, propInfo.gameObject);
                });
            }
        }
    }

    #endregion

    #region 操作点
    public override void GetGridCoordToHandlerObject()
    {
        //操作点 有家具
        if (m_ItemOperateBase.ValueId != 0) { return; }

        //弹出点击单元格的家具信息
        var furnitureInfo = GuildGridModel.Instance.PopFurnitureInfo(GuildGridModel.EGridLayer.Furniture, m_OperateGridCoordCur);
        if (furnitureInfo != null)
            SetHandlerGridItemActorId(furnitureInfo.FurnitureId, furnitureInfo.Direction); //挂载至操作点
    }

    public override void SetHandlerObjectToGridCoord()
    {
        //操作点 无有效值 或 不可设置
        if (m_ItemOperateBase.ValueId == 0 || !CanSet) { return; }

        //尝试放置于点击单元格
        if (GuildGridModel.Instance.PushFurnitureInfo(GuildGridModel.EGridLayer.Furniture, m_ItemOperateBase.ValueId, m_OperateGridCoordCur, m_ItemOperateBase.Direction))
            SetHandlerGridItemActorId(0); //清空 操作点上的数据
        else
            WindowSystem.Instance.ShowMsg("该位置不可放置家具！");
    }

    /// <summary>
    /// 旋转 操作点的建筑
    /// </summary>
    public override void RotateHandlerObject()
    {
        base.RotateHandlerObject();

        //家具朝向 旋转
        m_ItemOperateBase.SetDirectionRotate();
    }

    /// <summary>
    /// 返还 操作点的建筑
    /// </summary>
    public override void ReturenHandlerObject()
    {
        //操作点 无有效值
        if (m_ItemOperateBase.ValueId == 0) { return; }

        //归还至背包
        ChangeBackpackFurnitureCount(m_ItemOperateBase.ValueId, 1);
        //清空 操作点
        SetHandlerGridItemActorId(0);
    }
    #endregion

    #region 功能区家具要求

    [Header("功能区")]
    [SerializeField] private ListItemPagesComponent m_ListItemFurnitureRequire = null; //翻页项目列表 家具要求

    private Building_Area m_CfgBuildingAreaCur; //当前功能区配置文件

    //打开 功能区
    private void OnOpenAreaInfo()
    {
        RefreshListItemFurnitureRequire();
    }

    private void AwakeAreaInfo()
    {
        MessageDispatcher.AddListener(GuildModelMsgType.GUILDGRIDMODEL_AREAITEMCUR_CHANGE, MsgGuildGridAreaCurChange); //当前区域 改变
        MessageDispatcher.AddListener(GuildModelMsgType.GUILDGRIDMODEL_AREAITEMCUR_VALUE_CHANGE, MsgGuildGridAreaCurChange); //当前区域 值 改变
    }

    private void DestroyAreaInfo()
    {
        MessageDispatcher.RemoveListener(GuildModelMsgType.GUILDGRIDMODEL_AREAITEMCUR_CHANGE, MsgGuildGridAreaCurChange);
        MessageDispatcher.RemoveListener(GuildModelMsgType.GUILDGRIDMODEL_AREAITEMCUR_VALUE_CHANGE, MsgGuildGridAreaCurChange);
    }

    //刷新 需求家具数量列表
    private void RefreshListItemFurnitureRequire()
    {
        if (GuildGridModel.Instance.PlayerAreaInfoCur == null)
        {
            m_ListItemFurnitureRequire.Init(null);
            return;
        }

        var cfg = ConfigSystem.Instance.GetConfig<Building_Area>(GuildGridModel.Instance.PlayerAreaInfoCur.Value);
        if (cfg == null) { return; }
        m_CfgBuildingAreaCur = cfg;

        //设置 当前功能区的家具需求数量
        var listData = new List<IItemPagesData>();
        foreach (var kv in m_CfgBuildingAreaCur.FurnitureRequire)
            listData.Add(new ItemPagesData(kv.Key, kv.Value.ToString()));
        m_ListItemFurnitureRequire.Init(listData, null, false);
    }

    //消息 功能区改变
    private void MsgGuildGridAreaCurChange(IMessage rMessage)
    {
        RefreshListItemFurnitureRequire();
    }

    #endregion
}
