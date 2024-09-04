using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEntrustInfoMember : CachedMonoBehaviour
{
    [SerializeField] protected GameObject m_ImgBGMust; //底图 必须槽位
    [SerializeField] protected GameObject m_ImgBGOptional; //底图 可选槽位
    [SerializeField] protected Image m_ImgHead; //图片 冒险者头像
    [SerializeField] protected GameObject m_HeadMaskRoot; //头像图片遮罩根节点

    [SerializeField] private TextMeshProUGUI m_TxtDes; //文本 剩余时间

    private int m_VenturerId;

    /// <summary>
    /// 当点击时
    /// </summary>
    public Action OnClickAction;

    public void Init()
    {
        ClickListener.Get(GameObjectGet).SetPointerEnterHandler(OnEnter);
        ClickListener.Get(GameObjectGet).SetPointerExitHandler(OnExit);
        ClickListener.Get(GameObjectGet).SetClickHandler(OnClick);

       //初始化时关闭头像Mask 因为需要只有显示底图
       m_HeadMaskRoot.SetActive(false);
    }

    /// <summary>
    /// 设置成员槽位UI风格
    /// </summary>
    /// <param name="styleType">队员UI的类型 1=必须槽位 2=可选槽位</param>
    public void SetStyle(int styleType)
    {
        switch (styleType)
        {
            case 1:
                m_ImgBGMust.SetActive(true);
                m_ImgBGOptional.SetActive(false);
                break;
            case 2:
                m_ImgBGMust.SetActive(false);
                m_ImgBGOptional.SetActive(true);
                break;
            default:
                m_ImgBGMust.SetActive(true);
                m_ImgBGOptional.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// 设置成员信息
    /// </summary>
    /// <param name="entrustItemMemberInfo">要求传入的数据</param>
    public void SetInfo(int venturerId)
    {
        if (m_VenturerId == venturerId) return;

        m_VenturerId = venturerId;

        m_TxtDes.text = m_VenturerId.ToString();

        //20211016 Winhoo:加载冒险者头像Icon
        //IconSystem.Instance.SetIcon(m_ImgHead, "VenturerHeadPortrait", entrustItemMemberInfo.headImagePath);

        m_HeadMaskRoot.SetActive(true);
    }

    /// <summary>
    /// 清除信息
    /// </summary>
    public void ClearInfo()
    {
        m_VenturerId = -1;

        m_ImgHead.sprite = null;
        m_TxtDes.text = string.Empty;
        m_HeadMaskRoot.SetActive(false);
    }

    //按钮 鼠标进入
    private void OnEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {

    }

    //按钮 鼠标离开
    private void OnExit(UnityEngine.EventSystems.PointerEventData eventData)
    {

    }

    //按钮 鼠标点击
    private void OnClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        OnClickAction?.Invoke();
    }
}
