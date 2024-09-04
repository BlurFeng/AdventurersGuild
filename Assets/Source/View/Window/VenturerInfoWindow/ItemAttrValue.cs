using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Deploy;
using UnityEngine.EventSystems;

public class ItemAttrValue : MonoBehaviour
{
    [SerializeField] private GameObject m_BtnClick = null; //按钮 点击
    [SerializeField] private Image m_ImgIcon = null; //图片 图标
    [SerializeField] private TextMeshProUGUI m_TxtName = null; //文本 名称
    [SerializeField] private TextMeshProUGUI m_TxtValue = null; //文本 值

    private Common_Attribute m_Config; //配置表

    private void Awake()
    {
        ClickListener.Get(m_BtnClick).SetPointerEnterHandler(BtnEnter);
        ClickListener.Get(m_BtnClick).SetPointerExitHandler(BtnExit);
    }

    /// <summary>
    /// 设置 信息
    /// </summary>
    /// <param name="id">Common_Attribute配置表ID</param>
    public void SetInfo(int id)
    {
        if (m_Config != null && m_Config.Id == id) { return; }

        m_Config = ConfigSystem.Instance.GetConfig<Common_Attribute>(id);

        //信息
        if (!string.IsNullOrEmpty(m_Config.Icon))
        {
            AssetIconSystem.Instance.SetIcon(m_ImgIcon, "Attribute", m_Config.Icon);
        }
        m_TxtName.text = m_Config.Name;
    }

    /// <summary>
    /// 设置 值
    /// </summary>
    /// <param name="value"></param>
    /// <param name="color"></param>
    public void SetValue(int value, Color color = default)
    {
        m_TxtValue.text = value.ToString();
        if (color != default)
        {
            m_TxtValue.color = color;
        }
    }

    //按钮 鼠标进入
    private void BtnEnter(PointerEventData obj)
    {
        var arg = new InfoWindow.InfoWindowArg();
        arg.Type = InfoWindow.EInfoType.Attribute;
        arg.Id = m_Config.Id;
        WindowSystem.Instance.OpenWindow(WindowEnum.InfoWindow, arg);
    }

    //按钮 鼠标离开
    private void BtnExit(PointerEventData obj)
    {
        WindowSystem.Instance.CloseWindow(WindowEnum.InfoWindow);
    }
}
