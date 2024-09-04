using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Deploy;
using System.Text;

public class UIEntrustSwitch : CachedMonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_TxtDes; //�ı� ����
    [SerializeField] protected GameObject m_GobjLight; //ͼƬ �ⷢ��

    //������ί�д���
    private EntrustWindow m_EntrustWindow;

    public Action OnClickAction;

    /// <summary>
    /// ��ʼ��
    /// </summary>
    /// <param name="entrustSwitchType">������л�����Ŀ��ί�г�</param>
    public void Init(EntrustWindow entrustWindow)
    {
        m_EntrustWindow = entrustWindow;

        ClickListener.Get(GameObjectGet).SetPointerEnterHandler(OnEnter);
        ClickListener.Get(GameObjectGet).SetPointerExitHandler(OnExit);
        ClickListener.Get(GameObjectGet).SetClickHandler(OnClick);

        m_GobjLight.SetActive(false);
    }

    /// <summary>
    /// ������Ϣ
    /// </summary>
    /// <param name="des"></param>
    public void SetInfo(string des)
    {
        m_TxtDes.text = des;
    }

    //��ť ������
    private void OnEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjLight.SetActive(true);
    }

    //��ť ����뿪
    private void OnExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjLight.SetActive(false);
    }

    //��ť �����
    private void OnClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        OnClickAction?.Invoke();
    }
}
