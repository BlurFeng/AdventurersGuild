using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FsGameFramework
{
    /// <summary>
    /// 框架基础类 带生命周期
    /// </summary>
    public abstract class UObjectLife : UObject
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// 重置
        /// </summary>
        public abstract void ResetSelf();

        /// <summary>
        /// 销毁自身
        /// </summary>
        public abstract void DestroySelf();

        /// <summary>
        /// 启动 在初始化之后
        /// </summary>
        public abstract void Begin();

        /// <summary>
        /// 每帧执行，意义同Update
        /// </summary>
        public abstract void Tick(float deltaTime);

        /// <summary>
        /// 在所有Update执行结束后执行，意义同LateUpdate
        /// </summary>
        /// <param name="deltaTime"></param>
        public abstract void LateTick(float deltaTime);

        /// <summary>
        /// 固定时间间隔执行，意义同FixedUpdate
        /// </summary>
        /// <param name="fixedDeltaTime"></param>
        public abstract void FixedTick(float fixedDeltaTime);
    }
}
