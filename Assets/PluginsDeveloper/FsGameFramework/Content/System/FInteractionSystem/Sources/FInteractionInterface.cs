using UnityEngine;

namespace FInteractionSystem
{
    public interface IFInteractionInterface
    {
        /// <summary>
        /// 开始交互
        /// </summary>
        /// <typeparam name="T">交互者类型</typeparam>
        /// <param name="other">交互者</param>
        /// <param name="EInteractionOutType">交互传入方式</param>
        /// <returns>是否成功执行了交互</returns>
        public bool OnInteraction(Component other, EInteractionInType EInteractionOutType);

        /// <summary>
        /// 结束交互
        /// </summary>
        /// <typeparam name="T">交互者类型</typeparam>
        /// <param name="other">交互者</param>
        /// <returns>是否成功执行了停止交互</returns>
        public bool OnStopInteraction(Component other);

        /// <summary>
        /// 当交互者靠近时
        /// 一般用于在中距离时显示可交互对象信息标签 告知交互者自己为可交互对象
        /// </summary>
        /// <typeparam name="T">交互者类型</typeparam>
        /// <param name="other">交互者</param>
        /// <param name="isOn">靠近或不靠近</param>
        /// <returns>是否成功进行了状态变化</returns>
        public bool OnDistanceClose(Component other, bool isOn);

        /// <summary>
        /// 当交互者非常靠近时
        /// 一般用于在非常靠近时高亮可交互对象 告知交互者现在能点击开始交互
        /// </summary>
        /// <typeparam name="T">交互者类型</typeparam>
        /// <param name="other">交互者</param>
        /// <param name="isOn">非常靠近或不非常靠近</param>
        /// <returns>是否成功进行了状态变化</returns>
        public bool OnDistanceVeryClose(Component other, bool isOn);

        /// <summary>
        /// 返回交互方式类型
        /// 告知交互者该如何进行交互
        /// 比如人和门的交互，人传入Hand时门返回OpenDoor（或CloseDoor），人传入Foot时返回KickDoor（或KickCloseDoor）
        /// </summary>
        /// <returns></returns>
        public EInteractionOutType GetInteractionType(Component other, EInteractionInType interactionInType);

        /// <summary>
        /// 获取交互位置
        /// </summary>
        /// <returns>交互位置</returns>
        public Vector3 GetInteractionPosition();

        /// <summary>
        /// 确认是否能工作 在调用其他功能接口时会先确认
        /// 子类重写此方法来添加一些其他限制
        /// </summary>
        /// <param name="other">交互者</param>
        /// <returns></returns>
        bool CanWork(Component other);

        /// <summary>
        /// 打开或关闭描边
        /// 描边是为了明显的标记某个可交互对象
        /// </summary>
        /// <typeparam name="T">交互者类型</typeparam>
        /// <param name="other">交互者</param>
        /// <param name="isOn">打开或关闭描边效果</param>
        /// <param name="conditionAllowed">是否通过了条件限制 允许打开外发光（但实际在联网情况下可能并不会真的打开描边）</param>
        /// <returns>是否成功进行了状态变化</returns>
        public bool OnOutline(Component other, bool isOn, out bool conditionAllowed);

        /// <summary>
        /// 当前是否打开了描边外发光
        /// </summary>
        /// <returns></returns>
        public bool IsOutline();

        /// <summary>
        /// 调整外发光限制条件
        /// 在某些情况下允许自身在更远的位置被发现并打开描边
        /// </summary>
        /// <param name="limitRange">限制距离</param>
        /// <param name="limitAngle">限制角度</param>
        /// <param name="toleranceRange">容许误差范围 在两者距离差在此范围内时优先选择位置更接近正面的</param>
        public void AdjustOutlineLimit(ref float limitRange, ref float limitAngle, ref float toleranceRange);
    }
}