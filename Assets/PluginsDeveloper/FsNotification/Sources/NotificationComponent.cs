using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FsNotificationSystem
{
    public class NotificationComponent : NotificationComponentBase
    {
        [SerializeField] private SpriteRenderer m_SpBg; //Ã˘Õº ±≥æ∞
        [SerializeField] private RectTransform m_RectTransSpBg; //RectTrans Ã˘Õº±≥æ∞

        protected override void Init()
        {
            base.Init();

            m_RectTransSpBg = m_SpBg.GetComponent<RectTransform>();
            m_PixelsPerUnit = 0.01f;
            m_FontSizePerUnit = 0.1f;
        }

        protected override void SetSize(Vector2 size)
        {
            base.SetSize(size);

            //…Ë÷√ ±≥æ∞Õº≥ﬂ¥Á
            m_RectTransSpBg.sizeDelta = size;
            m_SpBg.size = size;
        }
    }
}

