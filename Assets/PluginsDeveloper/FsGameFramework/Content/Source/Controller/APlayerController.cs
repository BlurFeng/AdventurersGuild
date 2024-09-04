using UnityEngine;
using FsGameFramework.InputSystem;

namespace FsGameFramework
{
    /// <summary>
    /// 玩家控制器 负责将硬件输入信息转化成操作指令信息并发送给控制的Pawn单位
    /// </summary>
    public class APlayerController : AController
    {
        UPlayerState m_PlayerState;
        /// <summary>
        /// 玩家数据类
        /// </summary>
        public UPlayerState PlayerState
        {
            get
            {
                return m_PlayerState;
            }

            set
            {
                m_PlayerState = value;
            }
        }

        bool m_InputEnable;
        /// <summary>
        /// 输入是否打开
        /// </summary>
        public bool InputEnable
        {
            get
            {
                return m_InputEnable;
            }
            set
            {
                m_InputEnable = value;
                m_InputControl.enable = m_InputEnable;
            }
        }

        UInputControlComponent m_InputControl;//输入控制器


        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

#if UNITY_EDITOR
            //测试代码 possess功能
            if (Input.GetKeyDown(KeyCode.F8))
            {
                var actors = FWorldContainer.GetActors<APawn>();
                if(actors != null && actors.Count > 0)
                    Possess(actors[0]);
            }
#endif
        }

        public override bool Init(System.Object outer)
        {
            bool succeed = base.Init(outer);

            m_InputEnable = true;
            //初始化一个输入控制器 用于接收控制器的输入
            m_InputControl = new UInputControlComponent(this);
            m_InputControl.Init(OnAxisInputEvent, OnButtonInputEvent);

            return succeed;
        }

        /// <summary>
        /// 带方向信息的输入事件
        /// </summary>
        /// <param name="inputObjectType">输入对象类型（哪个摇杆）</param>
        /// <param name="inputEventType">输入事件类型（抬起 按下...）</param>
        /// <param name="inputDir">输入方向信息</param>
        protected virtual void OnAxisInputEvent(InputObjectType inputObjectType, InputEventType inputEventType, Vector3 inputDir, float hor, float ver)
        {
            if (!m_Possess || !InputEnable) return;

            //pawn响应玩家的操作做出反应
            Pawn.OnInputAction(inputObjectType, inputEventType, inputDir, hor, ver);
        }

        /// <summary>
        /// 按钮输入事件
        /// </summary>
        /// <param name="inputObjectType">输入对象信息（哪个按钮）</param>
        /// <param name="inputEventType">输入事件类型（抬起 按下...）</param>
        protected virtual void OnButtonInputEvent(InputObjectType inputObjectType, InputEventType inputEventType)
        {
            if (!m_Possess || !InputEnable) return;

            //pawn响应玩家的操作做出反应
            Pawn.OnInputAction(inputObjectType, inputEventType);
        }
    }
}