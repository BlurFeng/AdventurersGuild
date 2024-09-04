using com.ootii.Messages;
using Deploy;
using FsGameFramework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class MenuLoadWindow : WindowBase
{
    [SerializeField] private GameObject m_GoWindow = null; //物体 面板
    [SerializeField] private GameObject m_BtnClose = null; //按钮 关闭
    [SerializeField] private Animator m_AnimMenu = null; //动画控制器
    [SerializeField] private List<ItemSaveData> m_ListItemSaveData; //项目存档

    public override void OnLoaded()
    {
        base.OnLoaded();

        ClickListener.Get(m_BtnClose).SetClickHandler(BtnClose);
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        m_AnimMenu.SetInteger("State", 1);

        //设置 存档数据
        for (int i = 0; i < m_ListItemSaveData.Count; i++)
        {
            m_ListItemSaveData[i].SetInfo(null);
        }

        foreach (var kv in SaveDataModel.Instance.DicSaveDataInfo)
        {
            var num = kv.Key;
            var saveDataInfo = kv.Value;

            //仅显示3个存档
            if (num > 3 || num > m_ListItemSaveData.Count) { continue; }

            var index = num - 1;
            index = index < 0 ? 0 : index;
            var item = m_ListItemSaveData[index];
            item.SetInfo(saveDataInfo);
        }
    }

    public override void OnRelease()
    {
        base.OnRelease();

    }

    private void Awake()
    {
        m_GoWindow.SetActive(false);
    }

    #region 按钮

    //按钮 点击场景
    private void BtnClose(PointerEventData eventData)
    {
        m_AnimMenu.SetInteger("State", 0);
        DOVirtual.DelayedCall(0.3f, null).OnComplete(() =>
        {
            CloseWindow();

            var mainMenuWindow = WindowSystem.Instance.GetWindow(WindowEnum.MainMenuWindow) as MainMenuWindow;
            mainMenuWindow.OnEnterWindow();
        });
    }

    #endregion
}
