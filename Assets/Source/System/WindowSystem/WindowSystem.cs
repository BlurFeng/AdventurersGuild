using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Deploy;
using FsGameFramework;
using System;
using com.ootii.Messages;

public class WindowSystem : MonoBehaviourSingleton<WindowSystem>
{
    [SerializeField] private CanvasGroup m_GroupsMain = null; //主逻辑界面 全屏界面、常驻状态栏、弹窗、引导系统、遮罩、点击特效等
    [SerializeField] private CanvasGroup m_GroupsAdd = null; //其他界面 层级>=5

    public enum EWindowGroupType
    {
        None,
        /// <summary>
        /// 全屏窗口 显示层级1
        /// </summary>
        FullScreen,
        /// <summary>
        /// 常驻窗口 显示层级2
        /// </summary>
        Resident,
        /// <summary>
        /// 弹出窗口 显示层级3
        /// </summary>
        PopUp,
        /// <summary>
        /// 其他 显示层级4
        /// </summary>
        Other,
    }

    /// <summary>
    /// Window组
    /// </summary>
    public Dictionary<EWindowGroupType, WindowGroupComponent> GroupComponents { get { return m_GroupComponents; } }
    private readonly Dictionary<EWindowGroupType, WindowGroupComponent> m_GroupComponents = new Dictionary<EWindowGroupType, WindowGroupComponent>(); //窗口分组Map Group深度,Group组件
    private HashSet<WindowEnum> m_LoadingWindows = new HashSet<WindowEnum>(); //界面预制体 加载中

    /// <summary>
    /// Window分组 初始化
    /// </summary>
    public void InitWindowGroup()
    {
        //自适应 UI尺寸
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Screen.width / CameraModel.Instance.ScaleRatio, Screen.height / CameraModel.Instance.ScaleRatio);

        //Window_Config配置表 获取Group分组
        var configMap = ConfigSystem.Instance.GetConfigMap<Window_Config>();
        //排序Group分组
        SortedSet<int> groupTypes = new SortedSet<int>();
        foreach (Window_Config window_Config in configMap.Values)
        {
            groupTypes.Add(window_Config.Group);
        }
        //实例化Group组件
        foreach (int groupType in groupTypes)
        {
            AddWindowGroup((EWindowGroupType)groupType);
        }
    }

    /// <summary>
    /// 打开 窗口
    /// </summary>
    /// <param name="windowEnum">界面枚举</param>
    /// <param name="userData">数据传参</param>
    public void OpenWindow(WindowEnum windowEnum, object userData = null, bool closeAllUnFullWindow = false)
    {
        if (m_LoadingWindows.Contains(windowEnum))
        {
            Debug.LogWarning("所有界面单例，不可重复 " + windowEnum);
            return;
        }

        //获取 窗口配置表
        Window_Config config = ConfigSystem.Instance.GetConfig<Window_Config>((int)windowEnum);
        if (config == null) return;

        //获取 当前Group
        WindowGroupComponent groupComponent = m_GroupComponents[(EWindowGroupType)config.Group];
        WindowBase windowBase = groupComponent.GetWindow(windowEnum);
        if (windowBase != null) //已实例化的Window
        {
            OnOpenWindow(windowBase, userData, closeAllUnFullWindow);
        }
        else //未实例化的Window
        {
            string assetPath = AssetAddressUtil.GetPrefabWindowAddress(config.Asset);
            m_LoadingWindows.Add(windowEnum);
            AssetSystem.Instance.LoadPrefab(assetPath, (gameObj) =>
            {
                gameObj.layer = 5;
                windowBase = gameObj.GetOrAddComponent<WindowBase>();

                //移除 加载中记录
                if (m_LoadingWindows.Contains(windowEnum))
                {
                    m_LoadingWindows.Remove(windowEnum);
                }
                else
                {
                    //未实例化 已被Close的界面 直接销毁
                    windowBase.OnRelease();
                    GameObject.Destroy(windowBase.gameObject);
                    AssetAddressSystem.Instance.UnloadAsset(assetPath);
                    return;
                }

                //新界面 初始化
                windowBase.OnLoaded();
                windowBase.WindowEnum = windowEnum;

                OnOpenWindow(windowBase, userData, closeAllUnFullWindow);

            }, groupComponent.transform, false);
        }
    }

    /// <summary>
    /// 在UI打开时再次尝试打开也会调用此方法
    /// 为了记录UI的前后覆盖顺序等信息
    /// </summary>
    /// <param name="windowBase"></param>
    /// <param name="userData"></param>
    /// <param name="closeAllOtherWindow"></param>
    private void OnOpenWindow(WindowBase windowBase, object userData, bool closeAllOtherWindow) //打开界面
    {
        //获取 窗口配置表
        Window_Config config = ConfigSystem.Instance.GetConfig<Window_Config>((int)windowBase.WindowEnum);
        //获取 当前Group
        WindowGroupComponent groupComponent = m_GroupComponents[(EWindowGroupType)config.Group];

        //关闭所有非全屏窗口
        if (closeAllOtherWindow)
        {
            CloseWindowAll(EWindowGroupType.Resident);
            CloseWindowAll(EWindowGroupType.PopUp);
            //CloseWindowAll(EWindowGroupType.Other);
        }

        //重设激活
        if (!windowBase.isActiveAndEnabled)
        {
            //全屏界面
            if ((EWindowGroupType)config.Group == EWindowGroupType.FullScreen)
            {
                //所有全屏界面 设置非激活
                foreach (WindowBase window in groupComponent.OrderWindows)
                {
                    if (window.isActiveAndEnabled)
                    {
                        window.gameObject.SetActive(false); //旧界面 disable先调用
                    }
                }

                //设置 当前界面的 背景图 背景音乐
                SetBGP(config.BGP);
                SetBGM(config.BGM);
            }

            //新界面的enable后调用
            windowBase.gameObject.SetActive(true);
        }

        //新界面 打开并记录
        groupComponent.AddWindow(windowBase);
        windowBase.transform.SetAsLastSibling();

        //自身没打开时，调用OnOpen事件
        if (!IsOpen(windowBase.WindowEnum))
        {
            AddCharacterOperateCounter(windowBase.WindowEnum);
            windowBase.OnOpen(userData);
        } 
    }

    /// <summary>
    /// 关闭 窗口
    /// </summary>
    /// <param name="windowEnum"></param>
    public void CloseWindow(WindowEnum windowEnum)
    {
        //未实例化完成的界面 直接移除记录
        if (m_LoadingWindows.Contains(windowEnum))
        {
            m_LoadingWindows.Remove(windowEnum);
            return;
        }

        Window_Config config = ConfigSystem.Instance.GetConfig<Window_Config>((int)windowEnum);
        if (config == null) { return; }
        WindowGroupComponent groupComponent = m_GroupComponents[(EWindowGroupType)config.Group];
        WindowBase windowBase = groupComponent.GetWindow(windowEnum);
        if (windowBase == null) { return; }

        bool needOpenPrevious = false; //是否 要打开上一个界面

        //是全屏界面 且 为最后一个界面
        if ((EWindowGroupType)config.Group == EWindowGroupType.FullScreen && groupComponent.GetLastWindow().WindowEnum == windowEnum)
        {
            //该界面的上一个界面
            WindowBase windowPrevious = groupComponent.GetLastWindow(1);
            if (windowPrevious != null)
            {
                windowPrevious.transform.SetAsLastSibling();
                if (!windowPrevious.isActiveAndEnabled)
                {
                    needOpenPrevious = true;
                    //关闭界面的disable先调用
                    OnCloseWindow(groupComponent, windowBase);
                    // 打开界面的enable后调用
                    windowPrevious.gameObject.SetActive(true);
                }

                SetBGP(config.BGP);
                SetBGM(config.BGM);
            }
        }

        if (!needOpenPrevious)
            OnCloseWindow(groupComponent, windowBase);
    }

    /// <summary>
    /// 关闭所有窗口
    /// </summary>
    /// <param name="groupType"></param>
    public void CloseWindowAll(EWindowGroupType groupType)
    {
        WindowGroupComponent windowGroup = null;
        if (m_GroupComponents.TryGetValue(groupType, out windowGroup))
        {
            //关闭所有 组中的窗口
            var listNodeCur = windowGroup.OrderWindows.Last;
            if (listNodeCur == null) { return; }

            OnCloseWindow(windowGroup, listNodeCur.Value);

            var listNodePre = listNodeCur.Previous;
            while (listNodePre != null)
            {
                listNodeCur = listNodePre;
                listNodePre = listNodeCur.Previous;
                OnCloseWindow(windowGroup, listNodeCur.Value);
            }
        }
    }

    private void OnCloseWindow(WindowGroupComponent groupComponent, WindowBase windowBase, bool ignoreMemoryLock = false)
    {
        int useCount = groupComponent.GetWindowUseCount(windowBase.WindowEnum);

        if (IsOpen(windowBase.WindowEnum))
        {
            RemoveCharacterOperateCounter(windowBase.WindowEnum);
            windowBase.OnClose();
        }

        if (useCount <= 1) //界面数量仅1个
        {
            if (!windowBase.MemoryLock || ignoreMemoryLock) //释放资源
            {
                //释放资源
                GameObject.Destroy(windowBase.gameObject);
                AssetAddressSystem.Instance.UnloadAsset(AssetAddressUtil.GetPrefabWindowAddress(windowBase.Config.Asset));
                windowBase.OnRelease();

                groupComponent.RemoveWindow(windowBase);
            }
            else //不释放资源
            {
                windowBase.gameObject.SetActive(false);
            }
        }
        else
        {
            //移除窗口记录
            groupComponent.RemoveWindow(windowBase);
            windowBase.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 获取 窗口
    /// </summary>
    /// <param name="windowEnum"></param>
    /// <returns></returns>
    public WindowBase GetWindow(WindowEnum windowEnum)
    {
        Window_Config config = ConfigSystem.Instance.GetConfig<Window_Config>((int)windowEnum);
        WindowGroupComponent groupComponent = m_GroupComponents[(EWindowGroupType)config.Group];
        WindowBase windowBase = groupComponent.GetWindow(windowEnum);
        return windowBase;
    }

    /// <summary>
    /// 当前窗口是否打开
    /// </summary>
    /// <param name="windowEnum"></param>
    /// <returns></returns>
    public bool IsOpen(WindowEnum windowEnum)
    {
        WindowBase window = GetWindow(windowEnum);
        return window != null && window.IsOpen;
    }

    /// <summary>
    /// 打开或关闭窗口
    /// 如果窗口已经打开则关闭 如果窗口关闭则打开
    /// </summary>
    /// <param name="windowEnum"></param>
    public void OpenOrClose(WindowEnum windowEnum)
    {
        if(!IsOpen(windowEnum))
            OpenWindow(windowEnum);
        else
            CloseWindow(windowEnum);
    }

    /// <summary>
    /// 关闭 最上层的最后一个界面
    /// </summary>
    /// <param name="windowGroupTypeLimit"></param>
    /// <param name="unCloseWindoEnum"></param>
    /// <returns></returns>
    public bool CloseLastLayerWindow(WindowEnum unCloseWindowEnum = WindowEnum.MainGameWindow, EWindowGroupType windowGroupTypeLimit = EWindowGroupType.Other)
    {
        //从最上层界面组 向下遍历
        for (EWindowGroupType groupTypeCur = windowGroupTypeLimit; groupTypeCur > EWindowGroupType.FullScreen; groupTypeCur--)
        {
            WindowGroupComponent windowGroup = null;
            if (m_GroupComponents.TryGetValue(groupTypeCur, out windowGroup))
            {
                var windowBase = windowGroup.GetLastWindow();
                if (windowBase == null || !windowBase.IsOpen) { continue; }

                if (windowBase.WindowEnum == unCloseWindowEnum)
                    return false;
                else
                {
                    CloseWindow(windowBase.WindowEnum);
                    return true;
                }
            }
        }

        return false;
    }

    private void AddWindowGroup(EWindowGroupType groupType) //添加界面组
    {
        if (m_GroupComponents.ContainsKey(groupType)) { return; } //已有Group

        //Group命名
        GameObject windowGroupObj = new GameObject("WindowGroup_" + groupType);
        
        //RectTransform 初始化
        RectTransform rectTranform = windowGroupObj.GetOrAddComponent<RectTransform>();
        rectTranform.anchorMin = Vector2.zero;
        rectTranform.anchorMax = Vector2.one;
        rectTranform.offsetMax = Vector2.zero;
        rectTranform.offsetMin = Vector2.zero;

        if (groupType > EWindowGroupType.Other)
            rectTranform.SetParent(m_GroupsAdd.transform, false);
        else
            rectTranform.SetParent(m_GroupsMain.transform, false);
        rectTranform.gameObject.layer = 5;

        //Canvas 初始化
        Canvas canvas = windowGroupObj.GetOrAddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = (int)groupType * 1000;

        //GraphicRaycaster 初始化
        windowGroupObj.GetOrAddComponent<GraphicRaycaster>();

        //记录 Group深度,Group组件
        WindowGroupComponent groupComponent = windowGroupObj.GetOrAddComponent<WindowGroupComponent>();
        m_GroupComponents.Add(groupType, groupComponent);
    }

    private void SetBGP(string bgpName) //设置 背景图片
    {

    }

    private void SetBGM(string bgmName) //设置 背景音乐
    {
        if (string.IsNullOrEmpty(bgmName)) 
            return;
        else if (bgmName == "NO_BGM")
            AudioModel.Instance.Stop(AudioEnum.Music);
        else
            AudioModel.Instance.PlayMusic(bgmName);
    }

    #region 界面实例获取
    /// <summary>
    /// 游戏主界面
    /// </summary>
    public MainGameWindow MainGameWindow
    {
        get
        {
            if (m_MainGameWindow == null)
                m_MainGameWindow = WindowSystem.Instance.GetWindow(WindowEnum.MainGameWindow) as MainGameWindow;

            return m_MainGameWindow;
        }
    }
    private MainGameWindow m_MainGameWindow;
    #endregion

    #region 显示消息
    /// <summary>
    /// 显示消息
    /// </summary>
    /// <param name="msg">信息内容</param>
    /// <param name="time">信息展示时间</param>
    public void ShowMsg(string msg, float time = 1.5f)
    {
        NotificationWindow.NotificationWindowArg msgWindowArg = new NotificationWindow.NotificationWindowArg();
        msgWindowArg.Text = msg;
        msgWindowArg.ShowTime = time;
        var window = GetWindow(WindowEnum.NotificationWindow) as NotificationWindow;
        window.ShowMsg(msgWindowArg);
    }

    /// <summary>
    /// 显示消息 右上
    /// </summary>
    /// <param name="msg1"></param>
    /// <param name="msg2"></param>
    /// <param name="showType">显示类型 Icon 0:双环背景 1:单环背景</param>
    /// <param name="iconFolder"></param>
    /// <param name="iconName"></param>
    public void ShowMsgTopRight(string msg1, int showType, string iconFolder = default, string iconName = default, float showTime = 1.5f)
    {
        NotificationWindow.NotificationWindowArg msgWindowArg = new NotificationWindow.NotificationWindowArg();
        msgWindowArg.MsgType = NotificationWindow.EMsgType.TopRightTip;
        msgWindowArg.Text = msg1;
        msgWindowArg.ShowTime = showTime;
        msgWindowArg.MsgTypeParam = $"{showType}|{iconFolder}|{iconName}";
        var window = GetWindow(WindowEnum.NotificationWindow) as NotificationWindow;
        window.ShowMsg(msgWindowArg);
    }

    /// <summary>
    /// 显示消息 全屏
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="onShowComplete">显示完成 回调</param>
    public void ShowMsgFullScreen(string msg, Action onShowComplete = null)
    {
        NotificationWindow.NotificationWindowArg msgWindowArg = new NotificationWindow.NotificationWindowArg();
        msgWindowArg.MsgType = NotificationWindow.EMsgType.FullScreenTip;
        msgWindowArg.Text = msg;
        msgWindowArg.OnShowComplete = onShowComplete;
        var window = GetWindow(WindowEnum.NotificationWindow) as NotificationWindow;
        window.ShowMsg(msgWindowArg);
    }

    /// <summary>
    /// 显示消息 全屏
    /// </summary>
    /// <param name="storyDialogueId"></param>
    /// <param name="onShowComplete">显示完成 回调</param>
    public void ShowMsgFullScreen(int storyDialogueId, Action onShowComplete = null)
    {
        NotificationWindow.NotificationWindowArg msgWindowArg = new NotificationWindow.NotificationWindowArg();
        msgWindowArg.MsgType = NotificationWindow.EMsgType.FullScreenTip;
        msgWindowArg.MsgTypeParam = storyDialogueId.ToString();
        msgWindowArg.OnShowComplete = onShowComplete;
        var window = GetWindow(WindowEnum.NotificationWindow) as NotificationWindow;
        window.ShowMsg(msgWindowArg);
    }
    #endregion

    #region 玩家角色操作
    private int m_OperateCharacterMoveCounter; //操作 角色移动 计数器
    private int m_OperateMouseSceneCounter; //操作 鼠标场景交互 计数器

    /// <summary>
    /// 设置 玩家操作开关 计数
    /// </summary>
    /// <param name="windowEnum"></param>
    private void AddCharacterOperateCounter(WindowEnum windowEnum)
    {
        Window_Config cfg = ConfigSystem.Instance.GetConfig<Window_Config>((int)windowEnum);
        //操作角色移动
        if (cfg.OperateCharacterMove == 0)
        {
            if (m_OperateCharacterMoveCounter == 0)
                MessageDispatcher.SendMessageData(WindowSystemMsgType.WINDOWSYSTEM_OPERATE_CHARACTER_MOVE_CHANGE, false);
            m_OperateCharacterMoveCounter++;
        }
        //操作角色移动
        if (cfg.OperateMouseScene == 0)
        {
            if (m_OperateMouseSceneCounter == 0)
                MessageDispatcher.SendMessageData(WindowSystemMsgType.WINDOWSYSTEM_OPERATE_MOUSE_SCENE_CHANGE, false);
            m_OperateMouseSceneCounter++;
        }
    }

    /// <summary>
    /// 设置 玩家操作开关 计数
    /// </summary>
    /// <param name="windowEnum"></param>
    private void RemoveCharacterOperateCounter(WindowEnum windowEnum)
    {
        Window_Config cfg = ConfigSystem.Instance.GetConfig<Window_Config>((int)windowEnum);
        //操作角色移动
        if (cfg.OperateCharacterMove == 0)
        {
            if (m_OperateCharacterMoveCounter == 1)
                MessageDispatcher.SendMessageData(WindowSystemMsgType.WINDOWSYSTEM_OPERATE_CHARACTER_MOVE_CHANGE, true);
            m_OperateCharacterMoveCounter--;
        }
        //操作角色移动
        if (cfg.OperateMouseScene == 0)
        {
            if (m_OperateMouseSceneCounter == 1)
                MessageDispatcher.SendMessageData(WindowSystemMsgType.WINDOWSYSTEM_OPERATE_MOUSE_SCENE_CHANGE, true);
            m_OperateMouseSceneCounter--;
        }
    }
    #endregion
}

public class WindowLifeSystem : USystem
{
    public override void Init()
    {
        base.Init();
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        //执行 Window OnUpdate
        foreach (var group in WindowSystem.Instance.GroupComponents.Values)
        {
            foreach (var window in group.OrderWindows)
            {
                if (window.gameObject.activeInHierarchy)
                {
                    window.OnUpdate();
                }
            }
        }
    }
}

