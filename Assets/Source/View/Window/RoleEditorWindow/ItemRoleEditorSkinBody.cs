using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Deploy;
using com.ootii.Messages;
using System;
using UnityEngine.EventSystems;

public class ItemRoleEditorSkinBody : MonoBehaviour
{
    [SerializeField] private GameObject m_BtnLeft; //按钮 向左
    [SerializeField] private GameObject m_BtnRight; //按钮 向右
    [SerializeField] private TextMeshProUGUI m_TxtSkinPartName = null; //文本 皮肤部位名称

    /// <summary>
    /// 点击事件
    /// </summary>
    public Action<ItemRoleEditorSkinBody> OnClick { get { return m_OnClick; } set { m_OnClick = value; } }
    private Action<ItemRoleEditorSkinBody> m_OnClick;

    /// <summary>
    /// 皮肤 部位ID
    /// </summary>
    public int SkinPartId 
    {
        get 
        {
            int id = 0;
            if (m_CfgVenturerSkinPart != null)
            {
                id = m_CfgVenturerSkinPart.Id;
            }

            return id;
        }
    }

    /// <summary>
    /// 皮肤 道具ID
    /// </summary>
    public int SkinPropId { get { return m_ListSkinPropId[m_IndexSelectCur]; } }

    private Venturer_SkinPart m_CfgVenturerSkinPart;
    private List<int> m_ListSkinPropId;
    private int m_IndexSelectCur; //选择的下标 当前

    private void Awake()
    {
        ClickListener.Get(m_BtnLeft).SetClickHandler(BtnLeft);
        ClickListener.Get(m_BtnRight).SetClickHandler(BtnRight);
    }

    /// <summary>
    /// 设置 信息
    /// </summary>
    /// <param name="skinPartId">皮肤部位ID</param>
    /// <param name="skinPropId">可选择的 皮肤道具ID 列表</param>
    public void SetInfo(int skinPartId, List<int> skinPropId)
    {
        var cfg = ConfigSystem.Instance.GetConfig<Venturer_SkinPart>(skinPartId);
        if (cfg == null) { return; }

        //记录 
        m_CfgVenturerSkinPart = cfg;
        m_ListSkinPropId = skinPropId;

        //设置 部位名称
        m_TxtSkinPartName.text = m_CfgVenturerSkinPart.Name;
    }

    //设置 选中的皮肤道具ID
    public void SetSkinPropId(int skinPropID)
    {
        for (int i = 0; i < m_ListSkinPropId.Count; i++)
        {
            var skinPropId = m_ListSkinPropId[i];
            if (skinPropId == skinPropID)
            {
                SetIndexSelectCur(i);
                break;
            }
        }
    }

    //设置 选中的下标
    private void SetIndexSelectCur(int index)
    {
        if (m_IndexSelectCur == index) { return; }

        m_IndexSelectCur = index;
        m_TxtSkinPartName.text = m_CfgVenturerSkinPart.Name + m_IndexSelectCur;

        m_OnClick?.Invoke(this);
    }

    //按钮 向左
    private void BtnLeft(PointerEventData obj)
    {
        int index = m_IndexSelectCur;
        index--;
        if (index < 0)
        {
            index = m_ListSkinPropId.Count - 1;
        }
        SetIndexSelectCur(index);
    }

    //按钮 向右
    private void BtnRight(PointerEventData obj)
    {
        int index = m_IndexSelectCur;
        index++;
        if (index >= m_ListSkinPropId.Count)
        {
            index = 0;
        }
        SetIndexSelectCur(index);
    }
}
