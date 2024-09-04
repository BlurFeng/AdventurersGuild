using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// bool带Action结构类 当bool变化时调用内部的Action
/// </summary>
public class BoolAction
{
    private bool m_boolCached;
    /// <summary>
    /// 是否可以拆除
    /// </summary>
    public bool BoolCached
    {
        get { return m_boolCached; }
        set
        {
            if (m_boolCached != value)
            {
                m_boolCached = value;
                OnChange?.Invoke(m_boolCached);
            }
        }
    }
    /// <summary>
    /// 当拆除状态发生变化时
    /// </summary>
    public Action<bool> OnChange;
}