
namespace FsGameFramework.InputSystem
{
    /// <summary>
    /// 输入的对象类型 摇杆或按钮
    /// </summary>
    public enum InputObjectType
    {
        /// <summary>
        /// 主摇杆 一般再左边 用作移动
        /// </summary>
        JoystickMain,
        /// <summary>
        /// 副摇杆 一般在右边 用作瞄准
        /// </summary>
        JoystickAim,
        //JoystickMinor1,//次级摇杆 一般用作可以瞄准的技能等操作

        /// <summary>
        /// 主按钮 一般用作攻击
        /// </summary>
        Main,
        /// <summary>
        /// 次按钮 一般用作跳或其他动作或技能
        /// </summary>
        Minor1,
        Minor2,
        Minor3,
        Minor4,
        Minor5,
        Minor6,
        Minor7,

        /// <summary>
        /// 鼠标左键
        /// </summary>
        MouseLeft,
        /// <summary>
        /// 鼠标右键
        /// </summary>
        MouseRight,
        /// <summary>
        /// 鼠标中键
        /// </summary>
        MouseMiddle,
    }

    /// <summary>
    /// 输入的事件类型，按下 按住 抬起等
    /// </summary>
    public enum InputEventType
    {
        None,

        /// <summary>
        /// 未按住 没有任何输入
        /// </summary>
        UnHover,

        /// <summary>
        /// 按下
        /// </summary>
        Down,

        /// <summary>
        /// 抬起
        /// </summary>
        Up,

        /// <summary>
        /// 停留 持续按住
        /// </summary>
        Hover,

        /// <summary>
        /// 点击
        /// </summary>
        Click
    }
}