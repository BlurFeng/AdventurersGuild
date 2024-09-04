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

public class MenuSettingWindow : WindowBase
{
    [SerializeField] private GameObject m_GoWindow = null; //物体 面板
    [SerializeField] private GameObject m_BtnClose = null; //按钮 关闭
    [SerializeField] private Animator m_AnimMenu = null; //动画控制器

    public override void OnLoaded()
    {
        base.OnLoaded();

        ClickListener.Get(m_BtnClose).SetClickHandler(BtnClose);
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        m_AnimMenu.SetInteger("State", 1);
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

    //按钮 关闭
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
