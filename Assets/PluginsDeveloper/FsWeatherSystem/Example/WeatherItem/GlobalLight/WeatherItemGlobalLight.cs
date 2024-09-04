using Deploy;
using FsGridCellSystem;
using Pathfinding;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FsWeatherSystem
{
    /// <summary>
    /// 气象项目-全局光照
    /// </summary>
    [Serializable]
    public class WeatherItemGlobalLight : WeatherItemBase
    {
        [Header("效果组件")]
        [SerializeField] private Light m_Light;
        [Header("配置")]
        [SerializeField] private List<GlobalLightNode> m_ListGlobalLightNode = new List<GlobalLightNode>(); //全局光照节点列表

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

        private GameObject m_LightGameObj;

        public override void Init()
        {
            base.Init();

            if (m_Light != null)
                m_LightGameObj = m_Light.gameObject;
        }

        public override bool SetOpenState(bool isOpen)
        {
            base.SetOpenState(isOpen);

            if (m_LightGameObj != null && m_LightGameObj.activeSelf != isOpen)
                m_LightGameObj.SetActive(isOpen);

            return true;
        }

        #region 设置灯光
        /// <summary>
        /// 点位值 当前
        /// </summary>
        public float LocationValue { get { return m_LocationValue; } }
        private float m_LocationValueCur = -1f;

        //委托定义 灯光改变
        public delegate void DelegateLightChange(Light light);
        /// <summary>
        /// 委托 灯光改变
        /// </summary>
        public event DelegateLightChange OnLightChange;

        /// <summary>
        /// 设置 位置值(0.0 - 1.0)
        /// 最后的光照数据 会从光照节点列表中进行插值
        /// </summary>
        /// <param name="value"></param>
        public void SetLocationValue(float value)
        {
            value = Mathf.Clamp01(value);
            if (m_LocationValueCur == value) return;
            m_LocationValueCur = value;

            //寻找位置值的前后光照节点
            GlobalLightNode nodeLerp = GlobalLightNode.Default;
            for (int i = 0; i < m_ListGlobalLightNode.Count; i++)
            {
                var node = m_ListGlobalLightNode[i];

                if (m_LocationValueCur <= node.LocationValue)
                {
                    var nodeStart = node;
                    if (i > 0)
                    {
                        //进行插值
                        var nodeEnd = m_ListGlobalLightNode[i - 1];
                        float total = nodeEnd.LocationValue - nodeStart.LocationValue;
                        float part = m_LocationValueCur - nodeStart.LocationValue;
                        float t = total == 0f ? 1f : part / total;
                        nodeLerp = GlobalLightNode.Lerp(nodeStart, nodeEnd, t);
                    }
                    else
                        nodeLerp = node;
                    break;
                }
            }

            SetLight(nodeLerp);
        }

        //设置 灯光
        private void SetLight(GlobalLightNode node)
        {
            //设置参数
            m_Light.color = node.Color;
            m_Light.intensity = node.LightIntensity;
            var rotation = node.LightRotation;
            m_Light.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
            m_Light.shadowStrength = node.ShadowStrength;

            //委托回调
            OnLightChange?.Invoke(m_Light);
        }
        #endregion

        /// <summary>
        /// 全局光照 节点
        /// </summary>
        [Serializable]
        public struct GlobalLightNode
        {
            public GlobalLightNode(bool isDefault = true)
            {
                Color = Color.white;
                LightIntensity = 0.5f;
                LightRotation = new Vector3(50f, -30f, 0);
                ShadowStrength = 0f;
                LocationValue = 0f;
            }

            /// <summary>
            /// 颜色
            /// </summary>
            public Color Color;

            /// <summary>
            /// 光照强度
            /// </summary>
            [Range(0f, 1f)]
            public float LightIntensity;

            /// <summary>
            /// 光照旋转
            /// </summary>
            public Vector3 LightRotation;

            /// <summary>
            /// 阴影强度
            /// </summary>
            [Range(0f, 1f)]
            public float ShadowStrength;

            /// <summary>
            /// 点位(0.0 - 1.0)
            /// </summary>
            [Range(0f, 1f)]
            public float LocationValue;

            public static GlobalLightNode Default { get { return new GlobalLightNode(); } }

            /// <summary>
            /// 将数据进行插值
            /// </summary>
            /// <param name="nodeA"></param>
            /// <param name="nodeB"></param>
            /// <returns></returns>
            public static GlobalLightNode Lerp(GlobalLightNode nodeA, GlobalLightNode nodeB, float t)
            {
                GlobalLightNode node = GlobalLightNode.Default;

                node.Color = Color.Lerp(nodeA.Color, nodeB.Color, t);
                node.LightIntensity = Mathf.Lerp(nodeA.LightIntensity, nodeB.LightIntensity, t);
                node.LightRotation = Vector3.Lerp(nodeA.LightRotation, nodeB.LightRotation, t);
                node.ShadowStrength = Mathf.Lerp(nodeA.ShadowStrength, nodeB.ShadowStrength, t);
                node.LocationValue = Mathf.Lerp(nodeA.LocationValue, nodeB.LocationValue, t);

                return node;
            }
        }
    }
}