using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Deploy;
using System.Text;

public class UIEntrustVenturerLibraryPageTurning : CachedMonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_TxtDes; //文本 描述

    [SerializeField] protected GameObject m_GobjLight; //图片 外发光

    public Action OnClickAction;

    private bool m_IsShow;

    public void Init()
    {
        ClickListener.Get(GameObjectGet).SetPointerEnterHandler(OnEnter);
        ClickListener.Get(GameObjectGet).SetPointerExitHandler(OnExit);
        ClickListener.Get(GameObjectGet).SetClickHandler(OnClick);

        m_GobjLight.SetActive(false);
    }

    public void Show(bool show)
    {
        if (show == m_IsShow) return;
        m_IsShow = show;

        GameObjectGet.SetActive(m_IsShow);

        if (m_IsShow)
        {
            
        }
        else
        {
            m_GobjLight.SetActive(false);
        }
    }

    /// <summary>
    /// 设置信息
    /// </summary>
    /// <param name="venturerLibraryItemInfo"></param>
    public void SetInfo(string des)
    {
        m_TxtDes.text = des;
    }

    //按钮 鼠标进入
    private void OnEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjLight.SetActive(true);
    }

    //按钮 鼠标离开
    private void OnExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjLight.SetActive(false);
    }

    //按钮 鼠标点击
    private void OnClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        OnClickAction?.Invoke();
    }
}
