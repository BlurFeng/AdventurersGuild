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

public class VenturerListWindow : WindowBase
{
    /// <summary>
    /// 冒险者类型
    /// </summary>
    private enum EVenturerLabelType
    {
        None = 0,
        /// <summary>
        /// 所有
        /// </summary>
        All,
        /// <summary>
        /// 旅行
        /// </summary>
        Travel,
        /// <summary>
        /// 常驻
        /// </summary>
        Resident,
    }

    [SerializeField] private GameObject m_BtnClose = null; //按钮 关闭

    [SerializeField] private GameObject m_BtnLabelAll = null; //按钮 所有
    [SerializeField] private GameObject m_BtnLabelTravel = null; //按钮 旅行
    [SerializeField] private GameObject m_BtnLabelResident = null; //按钮 常驻
    [SerializeField] private ListItemPagesComponent m_ListItemVenturerInfo = null; //翻页项目列表 冒险者信息

    private EVenturerLabelType m_EVenturerLabelTypeCur; //标签类型 当前

    public override void OnLoaded()
    {
        base.OnLoaded();

        ClickListener.Get(m_BtnClose).SetClickHandler(BtnClose);
        ClickListener.Get(m_BtnLabelAll).SetClickHandler(BtnLabelAll);
        ClickListener.Get(m_BtnLabelTravel).SetClickHandler(BtnLabelTravel);
        ClickListener.Get(m_BtnLabelResident).SetClickHandler(BtnLabelResident);
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        //默认标签类型 所有
        SetVenturerLabelType(EVenturerLabelType.All);
    }

    public override void OnRelease()
    {
        base.OnRelease();
    }

    #region 按钮

    //按钮 关闭
    private void BtnClose(PointerEventData eventData)
    {
        CloseWindow();
    }

    //按钮 所有
    private void BtnLabelAll(PointerEventData obj)
    {
        SetVenturerLabelType(EVenturerLabelType.All);
    }

    //按钮 旅行
    private void BtnLabelTravel(PointerEventData obj)
    {
        SetVenturerLabelType(EVenturerLabelType.Travel);
    }

    //按钮 常驻
    private void BtnLabelResident(PointerEventData obj)
    {
        SetVenturerLabelType(EVenturerLabelType.Resident);
    }

    #endregion

    //切换 冒险者标签类型
    private void SetVenturerLabelType(EVenturerLabelType labelType)
    {
        if (m_EVenturerLabelTypeCur == labelType) { return; }
        m_EVenturerLabelTypeCur = labelType;

        switch (m_EVenturerLabelTypeCur)
        {
            case EVenturerLabelType.All:
                //显示 所有的冒险者
                m_ListItemVenturerInfo.Init(VenturerModel.Instance.GetAllVenturerInfoData(), OnSelectItemChange, false);
                break;
            case EVenturerLabelType.Travel:
                break;
            case EVenturerLabelType.Resident:
                break;
        }
    }

    //选中的项目改变
    private void OnSelectItemChange(IItemPagesData data)
    {
        var venturerInfo = VenturerModel.Instance.GetVenturerInfo(data.GetId());
        if (venturerInfo == null) { return; }

        
    }
}