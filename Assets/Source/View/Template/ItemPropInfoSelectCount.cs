using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemPropInfoSelectCount : ItemPropInfo
{
    [Header("选择数量标签")]
    [SerializeField] private GameObject m_GoLabelCurCount; //标签 当前选择数量
    [SerializeField] private GameObject m_BtnSubCount = null; //按钮 减少数量
    [SerializeField] private TextMeshProUGUI m_TxtCurCount = null; //文本 当前数量

    private Action<uint> m_EventCountChange; //事件 选中数量改变
    public Action<uint> EventCountChange
    {
        get { return m_EventCountChange; }
        set { m_EventCountChange = value; }
    }

    private uint m_CurCount; //当前数量
    public uint CurCount
    {
        get { return m_CurCount; }
    }

    protected override void Init()
    {
        base.Init();

        ClickListener.Get(m_BtnSubCount).SetClickHandler(OnSubCount);

        m_ClickEvent = OnAddCount;
        m_GoLabelCurCount.SetActive(false);
    }

    private void OnAddCount(ItemPropInfo itemPropInfo) //点击 增加数量
    {
        //是否超过拥有数量
        if (m_CurCount >= m_Count)
        {
            WindowSystem.Instance.ShowMsg("已达到最大数量");
            return;
        }

        if (m_CurCount <= 0u)
        {
            m_GoLabelCurCount.SetActive(true);
        }

        m_CurCount++;
        m_TxtCurCount.text = m_CurCount.ToString();

        m_EventCountChange?.Invoke(m_CurCount);
    }

    private void OnSubCount(UnityEngine.EventSystems.PointerEventData eventData) //点击 减少数量
    {
        m_CurCount--;

        if (m_CurCount <= 0u)
        {
            m_GoLabelCurCount.SetActive(false);
        }

        m_TxtCurCount.text = m_CurCount.ToString();

        m_EventCountChange?.Invoke(m_CurCount);
    }
}
