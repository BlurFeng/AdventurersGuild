using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FsGameFramework.InputSystem
{
    /// <summary>
    /// 输入硬件抽象类
    /// </summary>
    public abstract class InputDevice
    {
        protected readonly UInputControlComponent m_InputControl;
        protected readonly bool m_Enable;

        public InputDevice(UInputControlComponent inputControl, bool enable = true)
        {
            m_InputControl = inputControl;
            m_Enable = enable;
        }

        /// <summary>
        /// 更新输入信息
        /// </summary>
        public virtual void UpdateInput() { }
    }
}