using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Deploy;
using UnityEngine.EventSystems;

public class ItemProfessionInfo : MonoBehaviour
{
    [SerializeField] private GameObject m_BtnClick = null; //按钮 点击
    [SerializeField] private TextMeshProUGUI m_TxtName = null; //文本 名称

    private Profession_Config m_Config; //配置表

    private void Awake()
    {
        ClickListener.Get(m_BtnClick).SetPointerEnterHandler(BtnEnter);
        ClickListener.Get(m_BtnClick).SetPointerExitHandler(BtnExit);
    }

    /// <summary>
    /// 设置 信息
    /// </summary>
    /// <param name="professionId"></param>
    public void SetInfo(int professionId)
    { 
        //职业名称
        m_Config = ConfigSystem.Instance.GetConfig<Profession_Config>(professionId);
        m_TxtName.text = m_Config.Name;
    }

    //按钮 鼠标进入
    private void BtnEnter(PointerEventData obj)
    {
        var arg = new InfoWindow.InfoWindowArg();
        arg.Type = InfoWindow.EInfoType.Profession;
        arg.Id = m_Config.Id;
        WindowSystem.Instance.OpenWindow(WindowEnum.InfoWindow, arg);
    }

    //按钮 鼠标离开
    private void BtnExit(PointerEventData obj)
    {
        WindowSystem.Instance.CloseWindow(WindowEnum.InfoWindow);
    }
}
