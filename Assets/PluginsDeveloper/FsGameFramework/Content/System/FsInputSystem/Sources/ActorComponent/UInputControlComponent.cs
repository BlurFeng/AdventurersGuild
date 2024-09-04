using System;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework;

namespace FsGameFramework.InputSystem
{
    /// <summary>
    /// 操作方式 使用哪些控制器
    /// </summary>
    public enum ControlType
    {
        None,
        All,//所有方式

        KeyBoard,//键盘
        KeyBoardAndMouse,//键盘和鼠标
        //MobilePhone,//手机操作UI界面
        //JoyPad,//游戏手柄
    }

    /// <summary>
    /// 输入控制器功能组件
    /// </summary>
    public class UInputControlComponent : UActorComponent
    {
        public bool enable;

        /// <summary>
        /// 带方向的轴输入事件
        /// </summary>
        private Action<InputObjectType, InputEventType, Vector3, float, float> m_OnAxisInputEvent;
        /// <summary>
        /// 按钮输入事件
        /// </summary>
        private Action<InputObjectType, InputEventType> m_OnButtonInputEvent;

        public ControlType ControlType { get; private set; }//控制方式
        private KeyboardInput m_KeyboardInput;//键盘输入
        private MouseInput m_MouseInput;//鼠标输入

        public UInputControlComponent(AActor actor) : base (actor)
        {
            enable = true;

            //UInputSystem.AddInputControl(this);
        }

        /// <summary>
        /// 输入控制组件初始化
        /// </summary>
        /// <param name="onAxisInputEventCall">当摇杆操作输入时回调</param>
        /// <param name="onButtonInputEventCall">当按钮操作输入时回调</param>
        /// <param name="controlType">控制器类型 设置具体响应那些硬件输入</param>
        public void Init(
            Action<InputObjectType, InputEventType, Vector3, float, float> onAxisInputEventCall, 
            Action<InputObjectType, InputEventType> onButtonInputEventCall, 
            ControlType controlType = ControlType.All)
        {
            m_OnAxisInputEvent += onAxisInputEventCall;
            m_OnButtonInputEvent += onButtonInputEventCall;
            ControlInit(controlType);
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            if (!enable) return;

            //更新所有的控制器
            if (m_KeyboardInput != null)
                m_KeyboardInput.UpdateInput();
            if (m_MouseInput != null)
                m_MouseInput.UpdateInput();
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            base.FixedTick(fixedDeltaTime);
        }

        /// <summary>
        /// 初始化操作方式 使用何种控制器
        /// </summary>
        /// <param name="controlType"></param>
        private void ControlInit(ControlType controlType = ControlType.All)
        {
            if (ControlType == controlType) return;

            ControlType = controlType;
            m_KeyboardInput = null;
            m_MouseInput = null;

            switch (controlType)
            {
                case ControlType.All:
                    m_KeyboardInput = new KeyboardInput(this);
                    m_MouseInput = new MouseInput(this);
                    break;
                case ControlType.KeyBoard:
                    m_KeyboardInput = new KeyboardInput(this);
                    break;
                case ControlType.KeyBoardAndMouse:
                    m_KeyboardInput = new KeyboardInput(this);
                    m_MouseInput = new MouseInput(this);
                    break;
            }
        }

        const float mClickIntervalTime = 0.5f;//单击判断间隔时间设定（即Down到Up的间隔时间限制

        //输入信息的缓存和事件调用
        //分为带vector3信息的输入和bool信息的按钮输入
        #region Axis 带方向输入操作
        private readonly Dictionary<InputObjectType, AxisInfo> mCachedAxisInput = new Dictionary<InputObjectType, AxisInfo>();

        /// <summary>
        /// 摇杆输入
        /// </summary>
        /// <param name="inputObjectType"></param>
        /// <param name="inputDir"></param>
        public void OnAxisEvent(InputObjectType inputObjectType, Vector3 inputDir, float hor, float ver)
        {
            if (!enable) return;

            bool haveInputDir = inputDir != Vector3.zero;

            //首次输入 添加到缓存
            if (!mCachedAxisInput.ContainsKey(inputObjectType))
            {
                if (haveInputDir)
                {
                    m_OnAxisInputEvent?.Invoke(inputObjectType, InputEventType.Down, inputDir, hor, ver);
                }
                else
                {
                    m_OnAxisInputEvent?.Invoke(inputObjectType, InputEventType.UnHover, inputDir, 0f, 0f);
                }

                mCachedAxisInput.Add(inputObjectType, new AxisInfo(inputDir));
            }
            else
            {
                //判断当前输入的事件并调用事件

                var info = mCachedAxisInput[inputObjectType];
                bool haveOldInputDir = info.Direction != Vector3.zero;

                //摇杆有输入
                if(haveInputDir)
                {
                    //上一帧摇杆有输入
                    if (haveOldInputDir)
                    {
                        m_OnAxisInputEvent?.Invoke(inputObjectType, InputEventType.Hover, inputDir, hor, ver);
                    }
                    //上一帧摇杆无输入
                    else
                    {
                        m_OnAxisInputEvent?.Invoke(inputObjectType, InputEventType.Down, inputDir, hor, ver);
                        info.TimePointRefresh();
                    }
                }
                //摇杆无输入
                else
                {
                    //上一帧摇杆有输入
                    if (haveOldInputDir)
                    {
                        m_OnAxisInputEvent?.Invoke(inputObjectType, InputEventType.Up, inputDir, 0f, 0f);

                        if (info.DeltaTime() <= mClickIntervalTime)
                            m_OnAxisInputEvent?.Invoke(inputObjectType, InputEventType.Click, inputDir, 0f, 0f);
                    }
                    //上一帧摇杆无输入
                    else
                    {
                        m_OnAxisInputEvent?.Invoke(inputObjectType, InputEventType.UnHover, inputDir, 0f, 0f);
                    }
                }

                mCachedAxisInput[inputObjectType].Direction = inputDir;
            }
        }
        #endregion

        #region button 按键点击操作
        private readonly Dictionary<InputObjectType, PointInfo> mCachedBtnInput = new Dictionary<InputObjectType, PointInfo>();

        /// <summary>
        /// 按钮输入
        /// </summary>
        /// <param name="inputObjectType"></param>
        /// <param name="inputValue"></param>
        public void OnButtonEvent(InputObjectType inputObjectType, bool inputValue)
        {
            if (!enable) return;

            //首次输入 添加到缓存
            if (!mCachedBtnInput.ContainsKey(inputObjectType))
            {
                if (inputValue)
                {
                    m_OnButtonInputEvent?.Invoke(inputObjectType, InputEventType.Down);
                }

                mCachedBtnInput.Add(inputObjectType, new PointInfo(inputValue));
            }
            else
            {
                var info = mCachedBtnInput[inputObjectType];

                //按钮按下了
                if (inputValue)
                {
                    if (info.Value)
                    {
                        m_OnButtonInputEvent?.Invoke(inputObjectType, InputEventType.Hover);
                    }
                    else
                    {
                        m_OnButtonInputEvent?.Invoke(inputObjectType, InputEventType.Down);
                        info.TimePointRefresh();
                    }
                }
                //按钮未按下
                else
                {
                    if (info.Value)
                    {
                        m_OnButtonInputEvent?.Invoke(inputObjectType, InputEventType.Up);

                        if (info.DeltaTime() <= mClickIntervalTime)
                             m_OnButtonInputEvent?.Invoke(inputObjectType, InputEventType.Click);
                    }
                    else
                    {
                        m_OnButtonInputEvent?.Invoke(inputObjectType, InputEventType.UnHover);
                    }
                }

                info.Value = inputValue;
            }
        }

        ///// <summary>
        ///// click的单独调用 方便在OnClick事件中调用（鼠标单机屏幕按钮时 需要满足抬起时鼠标还在按钮范围内的条件）
        ///// </summary>
        ///// <param name="inputType"></param>
        //public void OnButtonClick(InputType inputType)
        //{
        //    OnButtonInputEvent?.Invoke(inputType, InputEvent.Click);
        //}
        #endregion
    }
}
