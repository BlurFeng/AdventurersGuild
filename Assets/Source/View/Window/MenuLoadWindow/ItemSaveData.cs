using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSaveData : MonoBehaviour
{
    [SerializeField] private GameObject m_BtnClick = null; //按钮 点击
    [SerializeField] private GameObject m_BtnDelete = null; //按钮 删除
    [SerializeField] private Image m_ImgBtnClick = null; //贴图 按钮点击
    [SerializeField] private GameObject m_AreaTxtName = null; //区域 文本名称
    [SerializeField] private TextMeshProUGUI m_TxtName = null; //文本 名称
    [SerializeField] private GameObject m_AreaTxtDate = null; //区域 文本游戏纪年
    [SerializeField] private TextMeshProUGUI m_TxtDate = null; //文本 游戏纪年
    [SerializeField] private GameObject m_AreaTxtTime = null; //区域 文本游玩时间
    [SerializeField] private TextMeshProUGUI m_TxtPlayTime = null; //文本 游玩时间

    private SaveDataModel.SaveDataInfo m_SaveDataInfo; //存档数据信息

    private void Awake()
    {
        ClickListener.Get(m_BtnClick).SetClickHandler(BtnClick);
        ClickListener.Get(m_BtnDelete).SetClickHandler(BtnDelete);
    }

    //按钮 点击
    private void BtnClick(PointerEventData obj)
    {
        if (m_SaveDataInfo == null) { return; }

        AsyncLoadWindow.FadeIn(() =>
        {
            //载入存档 开始游戏
            SaveDataModel.Instance.LoadSaveDataCur(m_SaveDataInfo.Num);

            WindowSystem.Instance.CloseWindow(WindowEnum.MainGameWindow);
            WindowSystem.Instance.CloseWindow(WindowEnum.MainMenuWindow);
            WindowSystem.Instance.CloseWindow(WindowEnum.MenuLoadWindow);
            WindowSystem.Instance.CloseWindow(WindowEnum.EscMenuWindow);

            WindowSystem.Instance.OpenWindow(WindowEnum.MainGameWindow, null, true);
        }, false);
    }

    //按钮 删除
    private void BtnDelete(PointerEventData obj)
    {
        SaveDataModel.Instance.DeleteSaveData(m_SaveDataInfo.Num);
        SetInfo(null);
    }

    public void SetInfo(SaveDataModel.SaveDataInfo saveDataInfo)
    {
        //存档数据 是否为空
        if (saveDataInfo == null)
        {
            m_SaveDataInfo = null;

            //不显示信息 置灰
            m_ImgBtnClick.color = Color.gray;
            m_ImgBtnClick.raycastTarget = false;
            m_BtnDelete.SetActive(false);
            m_AreaTxtName.SetActive(false);
            m_AreaTxtDate.SetActive(false);
            m_AreaTxtTime.SetActive(false);

            return;
        }
        else
        {
            //显示信息
            m_ImgBtnClick.color = Color.white;
            m_ImgBtnClick.raycastTarget = true;
            m_BtnDelete.SetActive(true);
            m_AreaTxtName.SetActive(true);
            m_AreaTxtDate.SetActive(true);
            m_AreaTxtTime.SetActive(true);
        }

        //存档数据 是否相同
        if (m_SaveDataInfo == saveDataInfo) { return; }
        m_SaveDataInfo = saveDataInfo;

        //设置 存档数据信息
        m_TxtName.text = m_SaveDataInfo.PlayerName;
        m_TxtDate.text = m_SaveDataInfo.GameTimeDate;
        m_TxtPlayTime.text = UtilityFunction.GetSecondsTimeFormat(m_SaveDataInfo.PlayTimeSeconds);
    }
}
