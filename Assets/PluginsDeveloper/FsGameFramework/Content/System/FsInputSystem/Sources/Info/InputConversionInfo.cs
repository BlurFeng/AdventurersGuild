using UnityEngine;

//硬件输入信息转变为具体信息 供被控制对象使用
namespace FsGameFramework.InputSystem
{
    /// <summary>
    /// 带方向的输入信息
    /// </summary>
    public class DirectionInfo
    {
        public DirectionInfo()
        {

        }

        /// <summary>
        /// 获得的原始输入量
        /// </summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>
        /// 单位化向量
        /// </summary>
        public Vector3 Direction { get; private set; }
        bool m_MagnitudeRefresh;
        float m_Magnitude;

        /// <summary>
        /// 水平输入量
        /// </summary>
        public float Horizontal { get; private set; }

        /// <summary>
        /// 垂直输入量
        /// </summary>
        public float Vertical { get; private set; }

        /// <summary>
        /// 输入量的大小
        /// </summary>
        public float Magnitude
        {
            get
            {
                if (!m_MagnitudeRefresh)
                {
                    m_MagnitudeRefresh = true;
                    m_Magnitude = Velocity == Vector3.zero ? 0f : Mathf.Clamp(Velocity.magnitude, 0f, 1f);
                    if (m_Magnitude > 0.99f) m_Magnitude = 1f;
                }
                return m_Magnitude;
            }
        }
        /// <summary>
        /// 是否有输入
        /// </summary>
        public bool Have { get { return Velocity != Vector3.zero; } }

        /// <summary>
        /// 输入事件类型
        /// </summary>
        public InputEventType InputEventType { get; private set; }

        /// <summary>
        /// 按下的这一帧
        /// </summary>
        public bool IsDown
        {
            get { return InputEventType == InputEventType.Down; }
        }

        /// <summary>
        /// 抬起的这一帧
        /// </summary>
        public bool IsUp
        {
            get { return InputEventType == InputEventType.Up; }
        }

        /// <summary>
        /// 按住
        /// </summary>
        public bool IsStay
        {
            get { return InputEventType == InputEventType.Hover; }
        }

        /// <summary>
        /// 单击
        /// </summary>
        public bool IsClick
        {
            get { return InputEventType == InputEventType.Click; }
        }

        /// <summary>
        /// 设置信息
        /// </summary>
        /// <param name="velocity">原始输入量</param>
        public void Set(Vector3 velocity, InputEventType inputEventType, float hor, float ver)
        {
            if (velocity != Vector3.zero)
            {
                Velocity = velocity;
                Direction = velocity.normalized;
                m_MagnitudeRefresh = false;
                Horizontal = hor;
                Vertical = ver;
            }
            else
            {
                Direction = Velocity = Vector3.zero;
                Horizontal = 0f;
                Vertical = 0f;
            }

            InputEventType = inputEventType;
        }
    }

    /// <summary>
    /// 按钮输入信息
    /// </summary>
    public class ButtonInfo
    {
        public ButtonInfo()
        {

        }

        /// <summary>
        /// 输入事件类型
        /// </summary>
        public InputEventType InputEventType { get; private set; }

        /// <summary>
        /// 按下的这一帧
        /// </summary>
        public bool IsDown
        {
            get { return InputEventType == InputEventType.Down; }
        }

        /// <summary>
        /// 抬起的这一帧
        /// </summary>
        public bool IsUp
        {
            get { return InputEventType == InputEventType.Up; }
        }

        /// <summary>
        /// 按住
        /// </summary>
        public bool IsStay
        {
            get { return InputEventType == InputEventType.Hover; }
        }

        /// <summary>
        /// 单击
        /// </summary>
        public bool IsClick
        {
            get { return InputEventType == InputEventType.Click; }
        }

        public void Set(InputEventType inputEventType)
        {
            InputEventType = inputEventType;
        }
    }

    /// <summary>
    /// float输入信息
    /// </summary>
    public class FloatInfo
    {
        public FloatInfo()
        {

        }

        private float floatNum;

        /// <summary>
        /// 获取输入量float
        /// </summary>
        public float GetFloatNum { get { return floatNum; } }

        /// <summary>
        /// 输入事件类型
        /// </summary>
        public InputEventType InputEventType { get; private set; }

        /// <summary>
        /// 按下的这一帧
        /// </summary>
        public bool IsDown
        {
            get { return InputEventType == InputEventType.Down; }
        }

        /// <summary>
        /// 抬起的这一帧
        /// </summary>
        public bool IsUp
        {
            get { return InputEventType == InputEventType.Up; }
        }

        /// <summary>
        /// 按住
        /// </summary>
        public bool IsStay
        {
            get { return InputEventType == InputEventType.Hover; }
        }

        /// <summary>
        /// 单击
        /// </summary>
        public bool IsClick
        {
            get { return InputEventType == InputEventType.Click; }
        }

        public void Set(float floatNum, InputEventType inputEventType)
        {
            this.floatNum = floatNum;
            InputEventType = inputEventType;
        }
    }
}
