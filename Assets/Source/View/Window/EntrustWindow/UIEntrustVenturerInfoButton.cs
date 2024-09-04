using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Deploy;

public class UIEntrustVenturerInfoButton : CachedMonoBehaviour
{
    public enum EButtonState
    {
        None,

        Join,
        Leave
    }

    [SerializeField] private GameObject m_GobjJoin; //按钮 加入
    [SerializeField] private GameObject m_GobjLeave; //按钮 离开
    [SerializeField] private TextMeshProUGUI m_TxtDes; //文本 描述

    [SerializeField] private GameObject m_GobjLight; //描边外发光

    public Action OnClickBtnJoinAction;
    public Action OnClickBtnLeaveAction;

    private EButtonState m_ButtonStateCur;

    public void Init(EButtonState buttonState = EButtonState.Join)
    {
        ClickListener.Get(m_GobjJoin).SetPointerEnterHandler(OnEnterBtnJoin);
        ClickListener.Get(m_GobjJoin).SetPointerExitHandler(OnExitBtnJoin);
        ClickListener.Get(m_GobjJoin).SetClickHandler(OnClickBtnJoin);

        ClickListener.Get(m_GobjLeave).SetPointerEnterHandler(OnEnterBtnLeave);
        ClickListener.Get(m_GobjLeave).SetPointerExitHandler(OnExitBtnLeave);
        ClickListener.Get(m_GobjLeave).SetClickHandler(OnClickBtnLeave);

        m_GobjLight.SetActive(false);

        SetState(buttonState);
    }

    public void SetState(EButtonState buttonState)
    {
        if (m_ButtonStateCur == buttonState) return;

        switch (buttonState)
        {
            case EButtonState.None:
                GameObjectGet.SetActive(false);
                m_GobjLight.SetActive(false);
                break;
            case EButtonState.Join:
                GameObjectGet.SetActive(true);

                m_GobjJoin.SetActive(true);
                m_GobjLeave.SetActive(false);

                m_TxtDes.text = "加入";
                break;
            case EButtonState.Leave:
                GameObjectGet.SetActive(true);

                m_GobjJoin.SetActive(false);
                m_GobjLeave.SetActive(true);

                m_TxtDes.text = "离开";
                break;
            default:
                break;
        }

        m_ButtonStateCur = buttonState;
    }

    //按钮 鼠标进入 加入按钮
    private void OnEnterBtnJoin(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjLight.SetActive(true);
    }

    //按钮 鼠标离开 加入按钮
    private void OnExitBtnJoin(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjLight.SetActive(false);
    }

    //按钮 鼠标点击 加入按钮
    private void OnClickBtnJoin(UnityEngine.EventSystems.PointerEventData eventData)
    {
        OnClickBtnJoinAction?.Invoke();
    }


    //按钮 鼠标进入 离开按钮
    private void OnEnterBtnLeave(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjLight.SetActive(true);
    }

    //按钮 鼠标离开 离开按钮
    private void OnExitBtnLeave(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjLight.SetActive(false);
    }

    //按钮 鼠标点击 离开按钮
    private void OnClickBtnLeave(UnityEngine.EventSystems.PointerEventData eventData)
    {
        OnClickBtnLeaveAction?.Invoke();
    }
}
