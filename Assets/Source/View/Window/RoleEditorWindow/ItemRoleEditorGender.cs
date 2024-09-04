using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Deploy;
using com.ootii.Messages;
using System;

public class ItemRoleEditorGender : MonoBehaviour
{
    [SerializeField] private GameObject m_BtnClick = null; //按钮 点击
    [SerializeField] private GameObject m_GoBgHover; //物体 背景 悬停
    [SerializeField] private GameObject m_GoBgSelect; //物体 背景 选中
    [SerializeField] private Image m_ImgIcon = null; //图片 图标

    /// <summary>
    /// 点击事件
    /// </summary>
    public Action<ItemRoleEditorGender> OnClick { get { return m_OnClick; } set { m_OnClick = value; } }
    private Action<ItemRoleEditorGender> m_OnClick;

    /// <summary>
    /// 性别 配置表ID
    /// </summary>
    public VenturerModel.EGender Gender
    {
        get 
        {
            int id = 0;
            if (m_CfgGender != null)
            {
                id = m_CfgGender.Id;
            }

            return (VenturerModel.EGender)id;
        }
    }

    private Venturer_Gender m_CfgGender;
    private bool m_IsSelect = false; //选中

    private void Awake()
    {
        ClickListener.Get(m_BtnClick).SetClickHandler(BtnClick);
        ClickListener.Get(m_BtnClick).SetPointerEnterHandler(BtnEnter);
        ClickListener.Get(m_BtnClick).SetPointerExitHandler(BtnExit);

        SetSelect(false);
    }

    /// <summary>
    /// 设置 信息
    /// </summary>
    public void SetInfo(int id)
    {
        var cfg = ConfigSystem.Instance.GetConfig<Venturer_Gender>(id);
        if (cfg == null) { return; }

        //记录 配置表
        m_CfgGender = cfg;

        //设置 图标
        AssetIconSystem.Instance.SetIcon(m_ImgIcon, "Gender", m_CfgGender.Icon);
    }

    /// <summary>
    /// 设置 选中
    /// </summary>
    /// <param name="isSelect"></param>
    public void SetSelect(bool isSelect)
    {
        m_IsSelect = isSelect;
        if (m_IsSelect)
        {
            SetBgType(3);
        }
        else
        {
            SetBgType(1);
        }
    }

    //按钮 鼠标点击
    public void BtnClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_OnClick?.Invoke(this);
    }

    //按钮 鼠标进入
    private void BtnEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (m_IsSelect) { return; }

        SetBgType(2);
    }

    //按钮 鼠标离开
    private void BtnExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (m_IsSelect) { return; }

        SetBgType(1);
    }

    //设置 背景类型
    private void SetBgType(int type)
    {
        switch(type)
        {
            case 1:
                m_GoBgHover.SetActive(false);
                m_GoBgSelect.SetActive(false);
                break;
            case 2:
                m_GoBgHover.SetActive(true);
                m_GoBgSelect.SetActive(false);
                break;
            case 3:
                m_GoBgHover.SetActive(false);
                m_GoBgSelect.SetActive(true);
                break;
        }
    }
}
