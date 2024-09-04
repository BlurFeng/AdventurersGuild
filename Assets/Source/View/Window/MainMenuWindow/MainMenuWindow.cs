using com.ootii.Messages;
using Deploy;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework;
using UnityEngine.EventSystems;
using System;

public class MainMenuWindow : WindowBase
{
    [SerializeField] private GameObject m_AreaMainMenu = null; //区域 主菜单
    [SerializeField] private GameObject m_BtnNewGame = null; //按钮 开始游戏
    [SerializeField] private GameObject m_BtnLoadGame = null; //按钮 读取游戏
    [SerializeField] private GameObject m_BtnSetting = null; //按钮 设置
    [SerializeField] private GameObject m_BtnQuit = null; //按钮 退出
    [SerializeField] private Animator m_AnimMainMenu = null; //动画控制器

    public override void OnLoaded()
    {
        base.OnLoaded();

        ClickListener.Get(m_BtnNewGame).SetClickHandler(BtnNewGame);
        ClickListener.Get(m_BtnLoadGame).SetClickHandler(BtnLoadGame);
        ClickListener.Get(m_BtnSetting).SetClickHandler(BtnSetting);
        ClickListener.Get(m_BtnQuit).SetClickHandler(BtnQuit);
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        //切换世界
        FWorldContainer.SwitchWorld(ConfigSystem.Instance.UWorldConfigContainer.Get("MainMenu_World"));

        //设置 摄像机坐标
        CameraModel.Instance.SetCameraMainPos(Vector3.zero, true);

        //加载界面 淡出
        AsyncLoadWindow.FadeOut();

        //清除当前的存档数据
        SaveDataModel.Instance.ClearData();

        //气象系统 关闭
        WeatherModel.Instance.SetGlobalEnableState(false);

        OnEnterWindow();
    }

    public override void OnRelease()
    {
        base.OnRelease();
    }

    private void Awake()
    {
        //初始化
        m_AreaMainMenu.SetActive(false);
    }

    /// <summary>
    /// 进入 主界面
    /// </summary>
    public void OnEnterWindow()
    {
        m_AnimMainMenu.SetInteger("State", 1);
    }

    //按钮 新游戏
    private void BtnNewGame(PointerEventData obj)
    {
        //异步加载界面
        AsyncLoadWindow.FadeIn(() =>
        {
            WindowSystem.Instance.CloseWindow(WindowEnum.MainMenuWindow);
            FWorldContainer.SwitchLevel(FWorldContainer.CurrentWorld.WorldConfig.levelConfigs[1]);

            //开发跳过 直接打开主游戏场景
            //新存档
            SaveDataModel.Instance.ClearData();
            //主游戏场景
            WindowSystem.Instance.OpenWindow(WindowEnum.MainGameWindow, null, true);

            //播放 开场叙事
            //WindowSystem.Instance.ShowMsgFullScreen(101010101, () =>
            //{
            //    AsyncLoadWindow.FadeIn(() =>
            //    {
            //        //新存档
            //        SaveDataModel.Instance.ClearData();
            //        //主游戏场景
            //        WindowSystem.Instance.OpenWindow(WindowEnum.MainGameWindow, null, true);
            //    }, false, false);
            //});
        }, true, true, 0.3f, 1.5f);
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

    //按钮 退出
    private void BtnQuit(PointerEventData obj)
    {
        Application.Quit();
    }

    //关闭界面
    private void CloseWindowAnim(TweenCallback callback = null)
    {
        m_AnimMainMenu.SetInteger("State", 0);
        DOVirtual.DelayedCall(0.3f, null).OnComplete(callback);
    }
}
