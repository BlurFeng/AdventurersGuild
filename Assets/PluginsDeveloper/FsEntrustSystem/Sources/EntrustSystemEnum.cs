using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntrustSystem
{
    /// <summary>
    /// 委托状态
    /// </summary>
    public enum EEntrustState
    {
        None,

        /// <summary>
        /// 未受理
        /// </summary>
        Unaccepted,

        /// <summary>
        /// 等待分配
        /// 此状态的委托属于Accepted已受理，在AcceptedPool中
        /// </summary>
        WaitDistributed,

        /// <summary>
        /// 进行中
        /// 此状态的委托属于Accepted已受理，在AcceptedPool中
        /// </summary>
        Underway,

        /// <summary>
        /// 完成
        /// 此状态的委托属于Accepted已受理，在AcceptedPool中
        /// Underway的委托，达到需求回合后会变为Complete。
        /// 业务层可在接收到OnComplete后对委托进行结算
        /// </summary>
        Complete,

        /// <summary>
        /// 结算
        /// 此状态的委托属于Accepted已受理，在AcceptedPool中
        /// Complete后的委托需要业务主动进行结算并设置委托到结算状态
        /// </summary>
        Statement,

        /// <summary>
        /// 超时
        /// 此状态的委托属于Accepted已受理，在AcceptedPool中
        /// 委托超时后会切换到此状态，超时的委托会自动进行销毁
        /// </summary>
        Timeout,

        /// <summary>
        /// 销毁
        /// 此状态的委托属于Accepted已受理，在AcceptedPool中
        /// 允许主动销毁某个委托
        /// </summary>
        Destroy,
    }

    /// <summary>
    /// 表示委托切换按钮当前点击后需要切换到哪个委托池
    /// </summary>
    public enum EEntrustPoolType
    {
        None,

        /// <summary>
        /// 未受理 在世界池中的委托允许进行受理
        /// </summary>
        UnacceptedPool,

        /// <summary>
        /// 受理的委托 包括已经分配并执行的和未分配的
        /// </summary>
        AcceptedPool,

        /// <summary>
        /// 所有
        /// </summary>
        UnacceptedAndAcceptedPool,

        /// <summary>
        /// 最大值，仅用于计数判断
        /// </summary>
        Max,
    }
}