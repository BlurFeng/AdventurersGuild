using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Deploy;
using com.ootii.Messages;

[DisallowMultipleComponent]
public class WindowBase : MonoBehaviour
{
    /// <summary>
    /// 是否能被销毁释放
    /// </summary>
    public bool MemoryLock { set; get; } = false;

    /// <summary>
    /// 是否打开
    /// </summary>
    public bool IsOpen { get; set; } = false;

    /// <summary>
    /// UI界面配置文件
    /// </summary>
    public Window_Config Config { get { return m_Config; } }
    private Window_Config m_Config;

    /// <summary>
    /// UI界面 枚举
    /// </summary>
    public WindowEnum WindowEnum
    {
        get { return m_WindowEnum; }
        set
        {
            m_WindowEnum = value;
            m_Config = ConfigSystem.Instance.GetConfig<Window_Config>((int)m_WindowEnum);
        }
    }
    private WindowEnum m_WindowEnum = WindowEnum.Undefined;

    /// <summary>
    /// RectTransform
    /// </summary>
    public RectTransform RectTransform { get { return m_RectTransform; } }
    private RectTransform m_RectTransform;

    /// <summary>
    /// 是否 初始化完成
    /// </summary>
    [HideInInspector]
    public bool IsInit { get { return m_IsInit; } }
    private bool m_IsInit = false;

    #region Messege (Awake > OnEnable > OnLoaded > OnOpen > Start) (OnRelease > OnDisable > OnDestroy)

    private void Awake()
    {
        m_RectTransform = transform as RectTransform;

        m_RectTransform.anchorMin = Vector2.zero;
        m_RectTransform.anchorMax = Vector2.one;
        m_RectTransform.offsetMin = m_RectTransform.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// <para>prefab加载完成时调用</para> 
    /// <para>处理一次性的工作，例如事件监听、按钮回调</para> 
    /// <para>执行顺序：Awake>OnEnable>OnLoaded>OnOpen>Start</para>
    /// </summary>
    public virtual void OnLoaded()
    {
        m_IsInit = true;
    }

    /// <summary>
    /// <para>WindowModel.OpenWindow执行时调用</para> 
    /// <para>只有当UI从关闭状态到打开状态时才会调用</para> 
    /// <para>打开界面时 可传参</para>
    /// <para>执行顺序：Awake>OnEnable>OnLoaded>OnOpen>Start</para>
    /// </summary>
    /// <param name="userData">自定义参数</param>
    public virtual void OnOpen(object userData = null)
    {
        IsOpen = true;
    }

    /// <summary>
    /// 当自身被关闭时
    /// </summary>
    /// <param name="userData"></param>
    public virtual void OnClose()
    {
        IsOpen = false;
    }

    /// <summary>
    /// <para>prefab释放时调用</para> 
    /// <para>释放其他prefab或资源、移除事件监听</para> 
    /// <para>执行顺序：OnRelease>OnDisable>OnDestroy</para>
    /// </summary>
    public virtual void OnRelease()
    {
        m_IsInit = false;
    }

    /// <summary>
    /// <para>每帧调用</para> 
    /// <para>统一管理的Update</para> 
    /// </summary>
    public virtual void OnUpdate()
    {
        
    }

    #endregion

    #region Quick API

    /// <summary>
    /// 关闭界面 自身
    /// </summary>
    public void CloseWindow()
    {
        WindowSystem.Instance.CloseWindow(WindowEnum);
    }

    #endregion
}
