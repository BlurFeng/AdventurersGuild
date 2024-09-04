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
using DG.Tweening;
using FsListItemPages;
using FsGridCellSystem;

public class BuildingAreaWindow : WindowBase
{
    [SerializeField] private GameObject m_BtnClose = null; //按钮 关闭
    [SerializeField] private GameObject m_BtnConfirm = null; //按钮 设置
    [SerializeField] private GameObject m_AreaSetedTip = null; //物体 已设置提示
    [SerializeField] private ListItemPagesComponent m_ListItemAreaInfo = null; //翻页项目列表
    [SerializeField] private Image m_ImgAreaInfo; //图片 区域信息
    [SerializeField] private TextMeshProUGUI m_TxtContent; //文本 内容
    [SerializeField] private ListItemPagesComponent m_ListItemFurnitureRequire = null; //翻页项目列表 家具要求

    private AreaInfo m_AreaInfoCur; //当前操作的区域信息
    private Building_Area m_CfgBuildingAreaCur; //当前查看的功能区配置文件

    public override void OnLoaded()
    {
        base.OnLoaded();

        ClickListener.Get(m_BtnClose).SetClickHandler(BtnClose);
        ClickListener.Get(m_BtnConfirm).SetClickHandler(BtnConfirm);
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        m_AreaInfoCur = userData as AreaInfo;

        //初始化 可选区域列表
        var listCfgId = GuildModel.Instance.GetUsableListAreaCfgData();
        m_ListItemAreaInfo.Init(listCfgId, OnSelectItemChange);
        //选中 当前的功能区ID
        m_ListItemAreaInfo.SelectItem(m_AreaInfoCur.Value);
    }

    public override void OnRelease()
    {
        base.OnRelease();
    }

    //按钮 关闭
    private void BtnClose(PointerEventData eventData)
    {
        CloseWindow();
    }

    //按钮 设置
    private void BtnConfirm(PointerEventData obj)
    {
        //将当前区域 设置为指定功能区
        m_AreaInfoCur.Value = m_CfgBuildingAreaCur.Id;
        RefreshSetedTip(); //刷新 设置按钮与提示
    }

    //选中的项目改表
    private void OnSelectItemChange(IItemPagesData itemData)
    {
        var cfg = ConfigSystem.Instance.GetConfig<Building_Area>(itemData.GetId());
        if (cfg == null) { return; }
        m_CfgBuildingAreaCur = cfg;

        //设置 区域信息
        m_TxtContent.text = m_CfgBuildingAreaCur.Desc;

        //设置 家具要求
        var listData = new List<IItemPagesData>();
        foreach (var kv in m_CfgBuildingAreaCur.FurnitureRequire)
        {
            listData.Add(new ItemPagesData(kv.Key, kv.Value.ToString()));
        }
        m_ListItemFurnitureRequire.Init(listData, null, false);

        //刷新 设置按钮与提示
        RefreshSetedTip();
    }

    //刷新 设置按钮与提示
    private void RefreshSetedTip()
    {
        bool isSeted = m_AreaInfoCur.Value == m_CfgBuildingAreaCur.Id;
        m_BtnConfirm.SetActive(!isSeted);
        m_AreaSetedTip.SetActive(isSeted);
    }
}