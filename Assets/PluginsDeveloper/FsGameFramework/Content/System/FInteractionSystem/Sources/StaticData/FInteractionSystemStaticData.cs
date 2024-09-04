using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FInteractionSystem
{
    /// <summary>
    /// 探测筛选条件信息
    /// </summary>
    [Serializable]
    public struct FDetectionConditionInfo
    {
        /// <summary>
        /// 筛选图层
        /// </summary>
        public LayerMask layerMask;

        /// <summary>
        /// 筛选类
        /// </summary>
        public Type classFilter;

        /// <summary>
        /// 筛选范围
        /// </summary>
        public float range;

        public FDetectionConditionInfo(Type classFilter, float range = 10f, LayerMask layerMask = new LayerMask())
        {
            this.layerMask = layerMask;
            this.classFilter = classFilter;
            this.range = range;
        }
    }
}