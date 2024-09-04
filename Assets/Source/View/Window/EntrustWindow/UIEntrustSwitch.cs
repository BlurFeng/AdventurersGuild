using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Deploy;
using System.Text;

public class UIEntrustSwitch : CachedMonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_TxtDes; //文本 名称
    [SerializeField] protected GameObject m_GobjLight; //图片 外发光

    //所属的委托窗口
    private EntrustWindow m_EntrustWindow;

    public Action OnClickAction;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="entrustSwitchType">点击后切换到的目标委托池</param>
    public void Init(EntrustWindow entrustWindow)
    {
        m_EntrustWindow = entrustWindow;

        ClickListener.Get(GameObjectGet).SetPointerEnterHandler(OnEnter);
        ClickListener.Get(GameObjectGet).SetPointerExitHandler(OnExit);
        ClickListener.Get(GameObjectGet).SetClickHandler(OnClick);

        m_GobjLight.SetActive(false);
    }

    /// <summary>
    /// 设置信息
    /// </summary>
    /// <param name="des"></param>
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
