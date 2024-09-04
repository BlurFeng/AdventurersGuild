using com.ootii.Messages;
using Deploy;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework;
using UnityEngine.EventSystems;
using System;

public class EscMenuWindow : WindowBase
{
    [SerializeField] private GameObject m_AreaWindow = null; //区域 主菜单
    [SerializeField] private GameObject m_BtnClose = null; //按钮 关闭
    [SerializeField] private GameObject m_BtnSaveGame = null; //按钮 储存游戏
    [SerializeField] private GameObject m_BtnLoadGame = null; //按钮 读取游戏
    [SerializeField] private GameObject m_BtnSetting = null; //按钮 设置
    [SerializeField] private GameObject m_BtnHelp = null; //按钮 帮助
    [SerializeField] private GameObject m_BtnReturnMainMenu = null; //按钮 返回主界面
    [SerializeField] private Animator m_AnimMenu = null; //动画控制器

    public override void OnLoaded()
    {
        base.OnLoaded();

        ClickListener.Get(m_BtnClose).SetClickHandler(BtnClose);
        ClickListener.Get(m_BtnSaveGame).SetClickHandler(BtnSaveGame);
        ClickListener.Get(m_BtnLoadGame).SetClickHandler(BtnLoadGame);
        ClickListener.Get(m_BtnSetting).SetClickHandler(BtnSetting);
        ClickListener.Get(m_BtnHelp).SetClickHandler(BtnHelp);
        ClickListener.Get(m_BtnReturnMainMenu).SetClickHandler(BtnReturnMainMenu);
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);
    }

    public override void OnRelease()
    {
        base.OnRelease();

        //Time.timeScale = 1;
    }

    private void Awake()
    {
        //初始化
        m_AreaWindow.SetActive(false);
    }

    private void OnEnable()
    {
        OpenWindowAnim();
    }

    //按钮 关闭
    private void BtnClose(PointerEventData obj)
    {
        CloseWindowAnim(() =>
        {
            CloseWindow();
        });
    }

    //按钮 储存游戏
    private void BtnSaveGame(PointerEventData obj)
    {
        SaveDataModel.Instance.SaveSaveDataCur();
        WindowSystem.Instance.ShowMsg("游戏储存成功！");
    }

    //按钮 读取游戏
    private void BtnLoadGame(PointerEventData obj)
    {
        CloseWindowAnim(() =>
        {
            WindowSystem.Instance.OpenWindow(WindowEnum.MenuLoadWindow);
        });
    }

    //按钮 设置
    private void BtnSetting(PointerEventData obj)
    {
        CloseWindowAnim(() =>
        {
            WindowSystem.Instance.OpenWindow(WindowEnum.MenuSettingWindow);
        });
    }

    //按钮 帮助
    private void BtnHelp(PointerEventData obj)
    {
        
    }

    //按钮 返回主菜单
    private void BtnReturnMainMenu(PointerEventData obj)
    {
        CloseWindowAnim(() =>
        {
            AsyncLoadWindow.FadeIn(() =>
            {
                WindowSystem.Instance.OpenWindow(WindowEnum.MainMenuWindow, null, true);
                WindowSystem.Instance.CloseWindow(WindowEnum.MainGameWindow);
                WindowSystem.Instance.CloseWindow(WindowEnum.EscMenuWindow);
            }, false);
        });
    }

    //打开界面
    private void OpenWindowAnim()
    {
        m_AnimMenu.SetInteger("State", 1);
        DOVirtual.DelayedCall(0.3f, null).OnComplete(() =>
        {
            //Time.timeScale = 0;
        });
    }

    //关闭界面
    private void CloseWindowAnim(TweenCallback callback = null)
    {
        m_AnimMenu.SetInteger("State", 0);
        DOVirtual.DelayedCall(0.3f, null).OnComplete(callback);
    }
}
