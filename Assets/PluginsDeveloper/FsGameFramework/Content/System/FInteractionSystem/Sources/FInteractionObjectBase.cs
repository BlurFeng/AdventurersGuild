using System.Collections;
using UnityEngine;
using FsGameFramework;//F游戏框架 为了将AActor作为基类方便管理，如果你没有此插件可将基类替换为MonoBehaviour

namespace FInteractionSystem
{
    /// <summary>
    /// 可交互对象类型基类
    /// </summary>
    public class FInteractionObjectBase : /*MonoBehaviour*/ AActor, IFInteractionInterface
    {
        /// <summary>
        /// 是否在进行交互中
        /// </summary>
        bool m_IsOnInteraction;

        /// <summary>
        /// 是否在近距离范围内
        /// 此值在联网时最好只作为对应本地客户端的值，因为距离是相对每个玩家角色而言的，此处只能缓存和一个玩家角色的关系
        /// </summary>
        bool m_IsOnClose;

        /// <summary>
        /// 是否在非常近范围内
        /// 此值在联网时最好只作为对应本地客户端的值，因为距离是相对每个玩家角色而言的，此处只能缓存和一个玩家角色的关系
        /// </summary>
        bool m_IsOnVeryClose;

        [SerializeField]
        private Transform m_centerPoint;
        /// <summary>
        /// 获得中心点方位
        /// </summary>
        public Transform CenterPoint { get { return m_centerPoint; } }

        /// <summary>
        /// 是否打开了描边
        /// 此值在联网时最好只作为对应本地客户端的值，因为距离是相对每个玩家角色而言的，此处只能缓存和一个玩家角色的关系
        /// </summary>
        bool m_IsOutline;

        /// <summary>
        /// 是否i允许打开描边 用于在某些情况下禁止打开描边（比如正在进行交互时）
        /// 此值在联网时最好只作为对应本地客户端的值，因为距离是相对每个玩家角色而言的，此处只能缓存和一个玩家角色的关系
        /// </summary>
        bool m_IsCanOutLine = true;

        public virtual bool OnInteraction(Component other, EInteractionInType EInteractionOutType)
        {
            if (m_IsOnInteraction) return false;
            if (!CanWork(other)) return false;

            m_IsOnInteraction = true;

            return true;
        }

        public virtual bool OnStopInteraction(Component other)
        {
            if (!m_IsOnInteraction) return false;

            m_IsOnInteraction = false;

            return true;
        }

        public virtual bool OnDistanceClose(Component other, bool isOn)
        {
            //联网时应当做判断 只有当other交互者是客户端玩家角色时才改变此值，否则返回False
            if (m_IsOnClose == isOn) return false;//此设置只针对一个玩家
            m_IsOnClose = isOn;

            return true;
        }

        public virtual bool OnDistanceVeryClose(Component other, bool isOn)
        {
            //联网时应当做判断 只有当other交互者是客户端玩家角色时才改变此值，否则返回False
            if (m_IsOnVeryClose == isOn) return false;//此设置只针对一个玩家
            m_IsOnVeryClose = isOn;

            return true;
        }

        public virtual bool OnOutline(Component other, bool isOn, out bool conditionAllowed)
        {
            conditionAllowed = false;
            if (!m_IsCanOutLine && isOn) return false;
            if (isOn && !CanWork(other)) return false;//想打开描边时必须是允许工作的

            conditionAllowed = true;

            //联网时应当做判断 只有当other交互者是客户端玩家角色时才改变此值，否则返回False
            if (m_IsOutline == isOn) return false;//此设置只针对一个玩家
            m_IsOutline = isOn;

            return true;
        }

        public virtual EInteractionOutType GetInteractionType(Component other, EInteractionInType interactionInType)
        {
            return EInteractionOutType.None;
        }

        public virtual Vector3 GetInteractionPosition()
        {
            //默认返回主碰撞器中心点位置 子类按需调整
            return GetCollider.bounds.center;
        }

        public virtual bool CanWork(Component other)
        {
            return true;
        }

        public virtual bool SetCanOutline(Component other, bool newSet, bool refresh)
        {
            if (m_IsCanOutLine == newSet) return false;

            m_IsCanOutLine = newSet;
            if (refresh)
            {
                OnOutline(other, newSet, out bool conditionAllowed);
            }

            return true;
        }

        public virtual void AdjustOutlineLimit(ref float limitRange, ref float limitAngle, ref float toleranceRange)
        {
            //按需调整外发光限制条件 比如角色死亡时允许在更远的位置被发现尸体等
        }

        public bool IsOutline()
        {
            return m_IsOutline;
        }
    }
}