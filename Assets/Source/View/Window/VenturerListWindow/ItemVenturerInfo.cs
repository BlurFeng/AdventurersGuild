using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Deploy;
using UnityEngine.UI;
using System;
using com.ootii.Messages;
using FsListItemPages;
using UnityEngine.EventSystems;

public class ItemVenturerInfo : ListItemPagesItemBase
{
    [Header("冒险者信息")]
    //[SerializeField] private Image m_ImgHead = null; //图片 头像
    [SerializeField] private TextMeshProUGUI m_TxtName = null; //文本 名称
    [SerializeField] private TextMeshProUGUI m_TxtRaceClan = null; //文本 种族
    [SerializeField] private TextMeshProUGUI m_TxtGender = null; //文本 性别
    //[SerializeField] private TextMeshProUGUI m_TxtAge = null; //文本 年龄
    [SerializeField] private TextMeshProUGUI m_TxtMainProfession = null; //文本 主职业
    [SerializeField] private TextMeshProUGUI m_TxtLevel = null; //文本 等级

    private VenturerInfo m_VenturerInfo;

    protected override void Awake()
    {
        base.Awake();

        MessageDispatcher.AddListener(VenturerModelMsgType.VENTURERMODEL_INFO_LEVEL_CHANGE, MsgRefreshLevel);
        //MessageDispatcher.AddListener(VenturerModelMsgType.VENTURERMODEL_INFO_LIFETURNCUR_CHANGE, MsgRefreshLifeTurn);
    }

    protected override void OnDestroy()
    {
        MessageDispatcher.RemoveListener(VenturerModelMsgType.VENTURERMODEL_INFO_LEVEL_CHANGE, MsgRefreshLevel);
        //MessageDispatcher.RemoveListener(VenturerModelMsgType.VENTURERMODEL_INFO_LIFETURNCUR_CHANGE, MsgRefreshLifeTurn);
    }

    public override void SetInfo(IItemPagesData data, int pageIndex, int stripIndex)
    {
        base.SetInfo(data, pageIndex, stripIndex);

        m_VenturerInfo = VenturerModel.Instance.GetVenturerInfo(data.GetId());
        RefreshHeroInfo();
    }

    //按钮 鼠标点击
    protected override void BtnClick(PointerEventData eventData)
    {
        base.BtnClick(eventData);

        WindowSystem.Instance.OpenWindow(WindowEnum.VenturerInfoWindow, m_VenturerInfo);
    }

    //消息 刷新 等级
    private void MsgRefreshLevel(IMessage msg)
    {
        m_TxtLevel.text = m_VenturerInfo.Level.ToString();
    }

    //消息 刷新 寿命回合数
    //private void MsgRefreshLifeTurn(IMessage msg)
    //{
    //    m_TxtAge.text = ((int)(m_VenturerInfo.LifeTurnCur * 0.25f)).ToString();
    //}

    //刷新 英雄信息
    private void RefreshHeroInfo()
    {
        if (m_VenturerInfo == null) { return; }

        //姓名
        m_TxtName.text = m_VenturerInfo.FullName;
        //种族
        var cfgRaceClan = ConfigSystem.Instance.GetConfig<Venturer_RaceClan>(m_VenturerInfo.RaceClanId);
        m_TxtRaceClan.text = cfgRaceClan.Name;
        //性别
        var cfgGender = ConfigSystem.Instance.GetConfig<Venturer_Gender>(m_VenturerInfo.GenderId);
        m_TxtGender.text = cfgGender.Name;
        //主职业
        var cfgProfession = ConfigSystem.Instance.GetConfig<Profession_Config>(m_VenturerInfo.MainProfessionID);
        m_TxtMainProfession.text = cfgProfession.Name;
        //年龄
        //var timeInfo = TurnModel.Instance.GetTimeInfo(m_VenturerInfo.LifeTurnCur);
        //m_TxtAge.text = timeInfo.GameTimeEraYear.ToString();

        MsgRefreshLevel(null);
        //MsgRefreshLifeTurn(null);
    }
}
