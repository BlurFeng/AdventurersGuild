using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEntrustInfoEntry : CachedMonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_TxtDes; //文本 描述

    public int Type { get; private set; }

    public void Init(int type)
    {
        Type = type;
    }

    public void SetInfo(string showDes)
    {
        m_TxtDes.text = showDes;
    }
}
