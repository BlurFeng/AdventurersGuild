using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework.InputSystem;
using Deploy;
using UnityEngine.EventSystems;
using System;

namespace FsGameFramework
{
    /// <summary>
    /// 场景物体 家具 前台
    /// </summary>
    public class FurnitureDeskActor : FurnitureBaseActor
    {
        public override bool Init(object outer = null)
        {
            return base.Init(outer);
        }

        protected override void OnExecuteOperate()
        {
            base.OnExecuteOperate();

            WindowSystem.Instance.OpenWindow(WindowEnum.EntrustWindow);
        }
    }
}
