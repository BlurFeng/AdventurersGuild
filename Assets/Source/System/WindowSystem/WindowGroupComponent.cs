using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowGroupComponent : MonoBehaviour
{
    public LinkedList<WindowBase> OrderWindows { get { return m_OrderWindows; } }
    private readonly LinkedList<WindowBase> m_OrderWindows = new LinkedList<WindowBase>();

    /// <summary>
    /// 增加界面到最后
    /// </summary>
    /// <param name="window"></param>
    public void AddWindow(WindowBase window)
    {
        if (window.Config.Group != 0)
        {
            if (m_OrderWindows.Contains(window))
            {
                return;
            }
        }

        if (m_OrderWindows.Last != null)
        {
            if (m_OrderWindows.Last.Value != window)
            {
                m_OrderWindows.AddLast(window);
            }
        }
        else {
            m_OrderWindows.AddLast(window);
        }
    }

    /// <summary>
    /// 移除界面
    /// </summary>
    /// <param name="window"></param>
    /// <param name="lastOrAll">移除最近一个，还是所有</param>
    public void RemoveWindow(WindowBase window, bool lastOrAll = true)
    {
        LinkedListNode<WindowBase> crt = m_OrderWindows.Last;
        LinkedListNode<WindowBase> previous;
        LinkedListNode<WindowBase> previousPrevious;
        LinkedListNode<WindowBase> next;
        while (crt != null)
        {
            previous = crt.Previous;
            next = crt.Next;
            if (crt.Value == window)
            {
                m_OrderWindows.Remove(crt);
                if (previous != null && previous == next)
                {
                    previousPrevious = previous.Previous;
                    m_OrderWindows.Remove(previous);
                    previous = previousPrevious;
                }
                if (lastOrAll)
                {
                    return;
                }
            }
            crt = previous;
        }
    }

    /// <summary>
    /// 获取靠后界面
    /// </summary>
    /// <param name="toLastOffset">和最后的距离</param>
    /// <returns></returns>
    public WindowBase GetLastWindow(int toLastOffset = 0)
    {
        LinkedListNode<WindowBase> crt = m_OrderWindows.Last;

        //获取 倒数第i个 WindowBase
        for (int i = 0; i < toLastOffset; i++)
        {
            if (crt == null)
            {
                return null;
            }
            crt = crt.Previous;
        }

        if (crt == null) { return null; }

        return crt.Value;
    }

    /// <summary>
    /// 查找界面
    /// </summary>
    /// <param name="uiFormAssetName">界面资源名称。</param>
    /// <returns>界面实例</returns>
    public WindowBase GetWindow(WindowEnum windowEnum)
    {
        LinkedListNode<WindowBase> crt = m_OrderWindows.Last;
        LinkedListNode<WindowBase> next;
        while (crt != null)
        {
            next = crt.Previous;
            if (crt.Value.WindowEnum == windowEnum)
            {
                return crt.Value;
            }
            crt = next;
        }

        return null;
    }

    /// <summary>
    /// 界面在序列中的使用次数
    /// </summary>
    /// <param name="windowEnum"></param>
    /// <returns></returns>
    public int GetWindowUseCount(WindowEnum windowEnum)
    {
        int useCount = 0;
        LinkedListNode<WindowBase> crt = m_OrderWindows.Last;
        LinkedListNode<WindowBase> next;
        while (crt != null)
        {
            next = crt.Previous;
            if (crt.Value.WindowEnum == windowEnum)
            {
                useCount ++;
            }
            crt = next;
        }
        return useCount;
    }
}
