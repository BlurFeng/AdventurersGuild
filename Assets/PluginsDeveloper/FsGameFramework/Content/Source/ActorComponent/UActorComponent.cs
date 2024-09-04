using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FsGameFramework
{
    /// <summary>
    /// 功能组件 从属于Actor，调用Actor.AddComponent()安装于Actor上
    /// </summary>
    [Serializable]
    public class UActorComponent : UObject
    {
        /// <summary>
        /// 从属的演员
        /// </summary>
        private AActor m_Actor;
        private string m_Name;

        public UActorComponent(AActor actor)
        {
            m_Actor = actor;

            //添加自身到所属Actor中
            m_Name = m_Actor.AddActorComponent(this);
        }

        //根据实际情况 每个组件有自己的初始化要求
        //public void Init(...) { }

        /// <summary>
        /// 启动 在初始化之后
        /// </summary>
        public virtual void Begin() { }

        /// <summary>
        /// 每帧执行，被WorldContainer管理进行。意义同Update
        /// </summary>
        public virtual void Tick(float deltaTime) { }

        /// <summary>
        /// 在所有Update执行结束后执行，被WorldContainer管理进行。意义同LateUpdate
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void LateTick(float deltaTime) { }

        /// <summary>
        /// 固定时间间隔执行，被WorldContainer管理进行。意义同FixedUpdate
        /// </summary>
        /// <param name="fixedDeltaTime"></param>
        public virtual void FixedTick(float fixedDeltaTime) { }

        /// <summary>
        /// 获得自身的所有者
        /// </summary>
        /// <returns></returns>
        public AActor GetOwner()
        {
            return m_Actor;
        }
    }
}
