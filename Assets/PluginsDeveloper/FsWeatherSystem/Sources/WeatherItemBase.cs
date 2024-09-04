using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace FsWeatherSystem
{
    /// <summary>
    /// 气象项目
    /// 每种气象项目类型的基类 由WeatherSystemManager进行统一管理
    /// </summary>
    [Serializable]
    public class WeatherItemBase : MonoBehaviour
    {
        /// <summary>
        /// 是否打开（Init时关闭）
        /// </summary>
        public bool IsOpne { get { return m_IsOpen; } }
        private bool m_IsOpen = false;

        private void Awake()
        {
            Init();
        }

        /// <summary>
        /// 初始化（Awake时调用）
        /// </summary>
        public virtual void Init()
        {
            //默认关闭状态
            SetOpenState(false);
        }

        /// <summary>
        /// 设置 开启状态
        /// </summary>
        /// <returns>是否成功打开</returns>
        public virtual bool SetOpenState(bool isOpen)
        {
            m_IsOpen = isOpen;

            return true;
        }

        //基础配置
        #region 全局光照
        //[Header("全局光照")]
        //[SerializeField] private bool m_LightEnable = false; //是否开启
        //[SerializeField] private Color m_LightColor = Color.white; //光照颜色
        //[SerializeField, Range(0f, 1f)] private float m_LightIntensity = 0.5f; //光照强度
        //[SerializeField] private Vector3 m_LightRotation = new Vector3(50f, -30f, 0); //光照旋转
        //[SerializeField] private bool m_LightShadows = false; //是否有阴影

        #endregion
    }
}