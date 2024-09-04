using Deploy;
using FsGridCellSystem;
using Pathfinding;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FsWeatherSystem
{
    /// <summary>
    /// 气象项目-辉光
    /// </summary>
    [Serializable]
    public class WeatherItemGlobalBloom : WeatherItemBase
    {
        [Header("配置")]
        [SerializeField] private List<GlobalBloomNode> m_ListGlobalBloomNode = new List<GlobalBloomNode>(); //全局光照节点列表

#if UNITY_EDITOR
        [Header("调试-点位值")]
        [SerializeField] bool m_Enable = false;
        [SerializeField, Range(0f, 1f)] float m_LocationValue = 0;

        private void OnValidate()
        {
            //效果开关
            SetOpenState(m_Enable);
            //效果点位值
            if (m_LocationValueCur != m_LocationValue)
                SetLocationValue(m_LocationValue);
        }
#endif

        private Bloom m_Bloom;

        public override void Init()
        {
            base.Init();

            m_Bloom = PostProcessSystem.Instance.GetEffectComponent<Bloom>();
        }

        public override bool SetOpenState(bool isOpen)
        {
            base.SetOpenState(isOpen);

            if (m_Bloom != null)
            {
                if (isOpen)
                    PostProcessSystem.Instance.OpenEffect<Bloom>();
                else
                    PostProcessSystem.Instance.CloseEffect<Bloom>();
            }  

            return true;
        }

        #region 设置Bloom效果
        /// <summary>
        /// 点位值 当前
        /// </summary>
        public float LocationValue { get { return m_LocationValue; } }
        private float m_LocationValueCur = -1f;

        /// <summary>
        /// 设置 位置值(0.0 - 1.0)
        /// 最后的Bloom数据 会从Bloom节点列表中进行插值
        /// </summary>
        /// <param name="value"></param>
        public void SetLocationValue(float value)
        {
            value = Mathf.Clamp01(value);
            if (m_LocationValueCur == value) return;
            m_LocationValueCur = value;

            //寻找位置值的前后Bloom节点
            GlobalBloomNode nodeLerp = GlobalBloomNode.Default;
            for (int i = 0; i < m_ListGlobalBloomNode.Count; i++)
            {
                var node = m_ListGlobalBloomNode[i];

                if (m_LocationValueCur <= node.LocationValue)
                {
                    var nodeStart = node;
                    if (i > 0)
                    {
                        //进行插值
                        var nodeEnd = m_ListGlobalBloomNode[i - 1];
                        float total = nodeEnd.LocationValue - nodeStart.LocationValue;
                        float part = m_LocationValueCur - nodeStart.LocationValue;
                        float t = total == 0f ? 1f : part / total;
                        nodeLerp = GlobalBloomNode.Lerp(nodeStart, nodeEnd, t);
                    }
                    else
                        nodeLerp = node;
                    break;
                }
            }

            Bloom(nodeLerp);
        }

        //设置 灯光
        private void Bloom(GlobalBloomNode node)
        {
            if (m_Bloom == null) return;

            //设置参数
            m_Bloom.tint.value = node.Color;
            m_Bloom.threshold.value = node.Threshold;
            m_Bloom.intensity.value = node.Intensity;
            m_Bloom.scatter.value = node.Scatter;
        }
        #endregion

        /// <summary>
        /// Bloom 节点
        /// </summary>
        [Serializable]
        public struct GlobalBloomNode
        {
            public GlobalBloomNode(bool isDefault = true)
            {
                Color = Color.white;
                Threshold = 1f;
                Intensity = 10f;
                Scatter = 0.3f;
                LocationValue = 0f;
            }

            /// <summary>
            /// 颜色
            /// </summary>
            public Color Color;

            /// <summary>
            /// 阈值
            /// </summary>
            public float Threshold;

            /// <summary>
            /// 强度
            /// </summary>
            public float Intensity;

            /// <summary>
            /// 扩散
            /// </summary>
            [Range(0f, 1f)]
            public float Scatter;

            /// <summary>
            /// 点位(0.0 - 1.0)
            /// </summary>
            [Range(0f, 1f)]
            public float LocationValue;

            public static GlobalBloomNode Default { get { return new GlobalBloomNode(); } }

            /// <summary>
            /// 将数据进行插值
            /// </summary>
            /// <param name="nodeA"></param>
            /// <param name="nodeB"></param>
            /// <returns></returns>
            public static GlobalBloomNode Lerp(GlobalBloomNode nodeA, GlobalBloomNode nodeB, float t)
            {
                GlobalBloomNode node = GlobalBloomNode.Default;

                node.Color = Color.Lerp(nodeA.Color, nodeB.Color, t);
                node.Threshold = Mathf.Lerp(nodeA.Threshold, nodeB.Threshold, t);
                node.Intensity = Mathf.Lerp(nodeA.Intensity, nodeB.Intensity, t);
                node.Scatter = Mathf.Lerp(nodeA.Scatter, nodeB.Scatter, t);
                node.LocationValue = Mathf.Lerp(nodeA.LocationValue, nodeB.LocationValue, t);

                return node;
            }
        }
    }
}