using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FsNotificationSystem
{
    public class NotificationUIComponent : NotificationComponentBase
    {
        protected override void Init()
        {
            base.Init();

            m_PixelsPerUnit = 1f;
            m_FontSizePerUnit = 1f;
        }
    }
}

