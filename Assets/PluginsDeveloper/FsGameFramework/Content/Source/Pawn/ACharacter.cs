using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FsGameFramework
{
    /// <summary>
    /// 继承自APawn，具有更复杂的角色行为能力的人形单位，配置了游戏中定义为角色的单位的常用能力。
    /// </summary>
    public class ACharacter : APawn
    {
        //TODO:character的基本功能实现
        CapsuleCollider m_CapsuleCollider;

        public override bool Init(System.Object outer = null)
        {
            bool succeed = base.Init(outer);

            //人形角色必须拥有一个碰撞器
            if (GetCollider == null)
            {
                m_CapsuleCollider = GameObjectGet.AddComponent<CapsuleCollider>();
            }

            return succeed;
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            base.FixedTick(fixedDeltaTime);
        }
    }
}
