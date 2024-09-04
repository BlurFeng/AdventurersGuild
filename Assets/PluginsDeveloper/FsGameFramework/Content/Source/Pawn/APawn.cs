using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework.InputSystem;
using System;

namespace FsGameFramework
{
    /// <summary>
    /// 小兵演员 具有基本的一些通用行为
    /// </summary>
    public class APawn : AActor
    {
        protected UInputInfoComponent m_InputInfoComponent;

        /// <summary>
        /// 开关 输入控制角色
        /// </summary>
        public bool EnableInputAction 
        {
            get { return m_EnableInputAction; }
            set
            {
                if (value == false)
                    OnInputMoveDirection(Vector3.zero, InputEventType.Up, 0f, 0f); //立刻设置角色 不移动

                m_EnableInputAction = value;
            }
        }
        private  bool m_EnableInputAction = true;

        public override bool Init(System.Object outer = null)
        {
            bool succeed = base.Init(outer);

            m_InputInfoComponent = new UInputInfoComponent(this);

            return succeed;
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            base.FixedTick(fixedDeltaTime);
        }

        /// <summary>
        /// 动作行为 相应输入信息，根据输入信息自己执行相应的行为（轴输入信息，带方向）
        /// </summary>
        /// <param name="inputObjectType">输入对象类型（哪个摇杆）</param>
        /// <param name="inputEventType">输入事件类型（抬起 按下...）</param>
        /// <param name="inputDir">输入方向信息</param>
        public virtual void OnInputAction(InputObjectType inputObjectType, InputEventType inputEventType, Vector3 inputDir, float hor, float ver)
        {
            if (EnableInputAction == false) return;

            switch (inputObjectType)
            {
                case InputObjectType.JoystickMain:
                    OnInputMoveDirection(inputDir, inputEventType, hor, ver);
                    break;
                case InputObjectType.JoystickAim:
                    OnInputAim(inputDir, inputEventType, hor, ver);
                    break;
            }
        }

        /// <summary>
        /// 动作行为 相应输入信息，根据输入信息自己执行相应的行为(点输入信息，按钮)
        /// </summary>
        /// <param name="inputObjectType">输入对象信息（哪个按钮）</param>
        /// <param name="inputEventType">输入事件类型（抬起 按下...）</param>
        public virtual void OnInputAction(InputObjectType inputObjectType, InputEventType inputEventType)
        {
            if (EnableInputAction == false) return;

            switch (inputObjectType)
            {
                case InputObjectType.Main:
                    OnInputMainBtn(inputEventType);
                    break;
                case InputObjectType.Minor3:
                    OnInputJump(inputEventType);
                    break;
                case InputObjectType.MouseLeft:
                    OnInputMouseLeftBtn(inputEventType);
                    break;
                case InputObjectType.MouseRight:
                    OnInputMouseRightBtn(inputEventType);
                    break;
                case InputObjectType.MouseMiddle:
                    OnInputMouseMiddleBtn(inputEventType);
                    break;
            }
        }

        /// <summary>
        /// 当移动时
        /// 可根据需求重写方法并划分出更详细的输入事件方法
        /// </summary>
        /// <param name="inputDirection">输入方向</param>
        /// <param name="inputEventType">输入事件 Down Up等</param>
        public virtual void OnInputMoveDirection(Vector3 inputDirection, InputEventType inputEventType, float hor, float ver)
        {
            if (m_InputInfoComponent == null) return;

            m_InputInfoComponent.MoveDir.Set(inputDirection, inputEventType, hor, ver);
        }

        /// <summary>
        /// 当瞄准时
        /// 可根据需求重写方法并划分出更详细的输入事件方法
        /// </summary>
        /// <param name="inputDirection">输入方向</param>
        /// <param name="inputEventType">输入事件 Down Up等</param>
        public virtual void OnInputAim(Vector3 inputDirection, InputEventType inputEventType, float hor, float ver)
        {
            if (null == m_InputInfoComponent) return;

            m_InputInfoComponent.AimDir.Set(inputDirection, inputEventType, hor, ver);
        }

        /// <summary>
        /// 当主按钮操作
        /// 可根据需求重写方法并划分出更详细的输入事件方法
        /// </summary>
        /// <param name="inputEventType">输入事件 Down Up等</param>
        public virtual void OnInputMainBtn(InputEventType inputEventType)
        {
            if (null == m_InputInfoComponent) return;

            m_InputInfoComponent.MainBtn.Set(inputEventType);
        }

        /// <summary>
        /// 当跳跃时
        /// 可根据需求重写方法并划分出更详细的输入事件方法
        /// </summary>
        /// <param name="inputEventType">输入事件 Down Up等</param>
        public virtual void OnInputJump(InputEventType inputEventType)
        {
            if (null == m_InputInfoComponent) return;

            m_InputInfoComponent.JumpBtn.Set(inputEventType);
        }

        public virtual void OnInputMouseLeftBtn(InputEventType inputEventType)
        {
            if (null == m_InputInfoComponent) return;

            m_InputInfoComponent.MouseLeftBtn.Set(inputEventType);
        }

        public virtual void OnInputMouseRightBtn(InputEventType inputEventType)
        {
            if (null == m_InputInfoComponent) return;

            m_InputInfoComponent.MouseRightBtn.Set(inputEventType);
        }

        public virtual void OnInputMouseMiddleBtn(InputEventType inputEventType)
        {
            if (null == m_InputInfoComponent) return;

            m_InputInfoComponent.MouseMiddleBtn.Set(inputEventType);
        }
    }
}
