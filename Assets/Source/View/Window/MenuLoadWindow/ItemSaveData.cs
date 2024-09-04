using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSaveData : MonoBehaviour
{
    [SerializeField] private GameObject m_BtnClick = null; //��ť ���
    [SerializeField] private GameObject m_BtnDelete = null; //��ť ɾ��
    [SerializeField] private Image m_ImgBtnClick = null; //��ͼ ��ť���
    [SerializeField] private GameObject m_AreaTxtName = null; //���� �ı�����
    [SerializeField] private TextMeshProUGUI m_TxtName = null; //�ı� ����
    [SerializeField] private GameObject m_AreaTxtDate = null; //���� �ı���Ϸ����
    [SerializeField] private TextMeshProUGUI m_TxtDate = null; //�ı� ��Ϸ����
    [SerializeField] private GameObject m_AreaTxtTime = null; //���� �ı�����ʱ��
    [SerializeField] private TextMeshProUGUI m_TxtPlayTime = null; //�ı� ����ʱ��

    private SaveDataModel.SaveDataInfo m_SaveDataInfo; //�浵������Ϣ

    private void Awake()
    {
        ClickListener.Get(m_BtnClick).SetClickHandler(BtnClick);
        ClickListener.Get(m_BtnDelete).SetClickHandler(BtnDelete);
    }

    //��ť ���
    private void BtnClick(PointerEventData obj)
    {
        if (m_SaveDataInfo == null) { return; }

        AsyncLoadWindow.FadeIn(() =>
        {
            //����浵 ��ʼ��Ϸ
            SaveDataModel.Instance.LoadSaveDataCur(m_SaveDataInfo.Num);

            WindowSystem.Instance.CloseWindow(WindowEnum.MainGameWindow);
            WindowSystem.Instance.CloseWindow(WindowEnum.MainMenuWindow);
            WindowSystem.Instance.CloseWindow(WindowEnum.MenuLoadWindow);
            WindowSystem.Instance.CloseWindow(WindowEnum.EscMenuWindow);

            WindowSystem.Instance.OpenWindow(WindowEnum.MainGameWindow, null, true);
        }, false);
    }

    //��ť ɾ��
    private void BtnDelete(PointerEventData obj)
    {
        SaveDataModel.Instance.DeleteSaveData(m_SaveDataInfo.Num);
        SetInfo(null);
    }

    public void SetInfo(SaveDataModel.SaveDataInfo saveDataInfo)
    {
        //�浵���� �Ƿ�Ϊ��
        if (saveDataInfo == null)
        {
            m_SaveDataInfo = null;

            //����ʾ��Ϣ �û�
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
            //��ʾ��Ϣ
            m_ImgBtnClick.color = Color.white;
            m_ImgBtnClick.raycastTarget = true;
            m_BtnDelete.SetActive(true);
            m_AreaTxtName.SetActive(true);
            m_AreaTxtDate.SetActive(true);
            m_AreaTxtTime.SetActive(true);
        }

        //�浵���� �Ƿ���ͬ
        if (m_SaveDataInfo == saveDataInfo) { return; }
        m_SaveDataInfo = saveDataInfo;

        //���� �浵������Ϣ
        m_TxtName.text = m_SaveDataInfo.PlayerName;
        m_TxtDate.text = m_SaveDataInfo.GameTimeDate;
        m_TxtPlayTime.text = UtilityFunction.GetSecondsTimeFormat(m_SaveDataInfo.PlayTimeSeconds);
    }
}
