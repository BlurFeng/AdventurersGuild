using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FsGameFramework
{
    /// <summary>
    /// 控制器 用于控制其他Pawn
    /// </summary>
    public abstract class AController : AActor
    {
        [SerializeField]
        private APawn m_Pawn;
        /// <summary>
        /// 控制对象
        /// </summary>
        public APawn Pawn { get { return m_Pawn; } }


        protected bool m_Possess;
        /// <summary>
        /// 是否有控制对象
        /// </summary>
        public bool IsPossess { get { return m_Possess; } }

        public override bool Init(System.Object outer)
        {
            bool succeed = base.Init(outer);

            m_Possess = null != m_Pawn;

            return succeed;
        }

        /// <summary>
        /// 接入控制Pawn
        /// </summary>
        /// <param name="aPawn"></param>
        public virtual void Possess(APawn aPawn)
        {
            if (aPawn == null || m_Pawn == aPawn) return;

            m_Possess = true;
            m_Pawn = aPawn;
        }

        /// <summary>
        /// 断开控制Pawn
        /// </summary>
        /// <param name="aPawn"></param>
        public virtual void UnPossess(APawn aPawn)
        {
            if (aPawn == null || m_Pawn == null || m_Pawn != aPawn) return;

            m_Possess = false;
            m_Pawn = null;
        }
    }
}
