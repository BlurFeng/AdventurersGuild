
namespace FInteractionSystem
{
    /// <summary>
    /// 方向
    /// </summary>
    public enum EDirection
    {
        None,
        Forward,
        Back,
        Right,
        Left,
        Up,
        Down
    }

    /// <summary>
    /// 探测中心源点类型
    /// </summary>
    public enum DetectionOriginPoint
    {
        /// <summary>
        /// 组件中设置的中心点
        /// </summary>
        CenterPoint,
        /// <summary>
        /// 组件根节点
        /// </summary>
        Root,
        /// <summary>
        /// 玩家相机
        /// </summary>
        PlayerCamera,
    }

    /// <summary>
    /// 交互方式传入类型
    /// 和EInteractionOutType交互方式传出类型对应，交互者传入In类型给可交互对象，可交互对象返回Out给交互者指导交互者如何行动
    /// 比如人和门的交互，人传入Hand时门返回OpenDoor（或CloseDoor），人传入Foot时返回KickDoor（或KickCloseDoor）
    /// </summary>
    public enum EInteractionInType
    {
        None,

        Hand,
        Foot,
        Head,
        Body
    }

    /// <summary>
    /// 交互方式传出类型
    /// 和EInteractionInType交互方式传出类型对应，交互者传入In类型给可交互对象，可交互对象返回Out给交互者指导交互者如何行动
    /// 比如人和门的交互，人传入Hand时门返回OpenDoor（或CloseDoor），人传入Foot时返回KickDoor（或KickCloseDoor）
    /// </summary>
    public enum EInteractionOutType
    {
        None,

        //门
        OpenDoor,
        CloseDoor,

        //地面开关
        FloorSwitchLeft,
        FloorSwitchRight,

        //墙面开关
        WallSwitchUp,
        WallSwitchDown,

        //拾取
        PickUp,

        //阀门开关
        ValveOpen,
        ValveClose,
    }
}