using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FsGameFramework
{
    /// <summary>
    /// 功能系统的管理类 所有自己写的System功能系统模块的总管理类继承此类
    /// 需要执行生命周期的系统类继承此类方便统一管理生命周期 其他系统或模块管理类可以使用单例更加灵活和独立（）
    /// 用于管理功能系统的生命周期
    /// </summary>
    public abstract class USystem : UObject
    {
        private string m_TypeName;
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get { return m_TypeName; } }

        public USystem()
        {
            m_TypeName = this.GetType().ToString();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init() { }

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
        /// 添加事件
        /// </summary>
        protected virtual void AddListeners() { }

        /// <summary>
        /// 移除事件
        /// </summary>
        protected virtual void RemoveListeners() { }
    }
}