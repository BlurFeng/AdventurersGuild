using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FsGameFramework
{
    public class FPlayerStart : AActor
    {
        /// <summary>
        /// 玩家起始出生点位置
        /// </summary>
        public Vector3 position { get { return TransformGet.position; } }
    }
}