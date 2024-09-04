using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FsGameFramework;
using TMPro;
using com.ootii.Messages;
using DG.Tweening;
using Deploy;
using UnityEngine.EventSystems;
using System;

public class BuildingInfoWindow : WindowBase
{
    [SerializeField] private GameObject m_BtnClose = null; //按钮 关闭
    [SerializeField] private GameObject m_BtnLevelUp = null; //按钮 等级提升
    [SerializeField] private GameObject m_BtnDemolition = null; //按钮 拆除
    [SerializeField] private TextMeshProUGUI m_TxtName = null; //文本 名称
    [SerializeField] private TextMeshProUGUI m_TxtLevel = null; //文本 等级

    private GuildGridModel.BuildingInfo m_BuildingInfo; //建筑信息

    public override void OnLoaded()
    {
        base.OnLoaded();

        ClickListener.Get(m_BtnClose).SetClickHandler(BtnClose);
        ClickListener.Get(m_BtnLevelUp).SetClickHandler(BtnLevelUp);
        ClickListener.Get(m_BtnDemolition).SetClickHandler(BtnDemolition);

        MemoryLock = true;
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        //参数解析
        int buildingId = (int)userData;
        m_BuildingInfo = GuildGridModel.Instance.GetBuildingInfo(buildingId);
        if (m_BuildingInfo == null) { return; }

        var cfg = ConfigSystem.Instance.GetConfig<Building_Config>(m_BuildingInfo.CfgBuildingId);

        //设置 建筑信息
        m_TxtName.text = cfg.Name;
        m_TxtLevel.text = m_BuildingInfo.Level.ToString();
    }

    public override void OnRelease()
    {
        base.OnRelease();
    }

    #region 按钮
    //按钮 关闭界面
    private void BtnClose(PointerEventData obj)
    {
        CloseWindow();
    }

    //按钮 等级提升
    private void BtnLevelUp(PointerEventData obj)
    {
        GuildGridModel.Instance.SetBuildingLevel(m_BuildingInfo.Id, m_BuildingInfo.Level + 1);
    }

    //按钮 拆除
    private void BtnDemolition(PointerEventData obj)
    {
        GuildGridModel.Instance.RemoveBuildingInfo(m_BuildingInfo.Id);
        CloseWindow();
    }
    #endregion
}