using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 气象系统
/// </summary>
namespace FsWeatherSystem
{
    public class WeatherSystemManager : MonoBehaviour
    {
        [Header("气象项目列表(相同气象组件只能放置1个)")]
        [SerializeField] private List<GameObject> m_ListWeatherItem = null;

        /// <summary>
        /// 气象Item字典
        /// </summary>
        private Dictionary<System.Type, WeatherItemBase> m_DicWeatherItem = new Dictionary<Type, WeatherItemBase>();
        private Dictionary<System.Type, bool> m_DicWeatherItemOpenState = new Dictionary<Type, bool>(); //记录开启状态
        // 气象项目根节点
        private GameObject m_RootWeatherItem;
        private Transform m_RootTransWeatherItem;

        private void Awake()
        {
            Init();
        }

        //初始化
        private void Init()
        {
            //实例化根节点
            if (m_RootWeatherItem == null)
            {
                m_RootWeatherItem = new GameObject();
                m_RootWeatherItem.name = "RootWeatherItem";
                m_RootTransWeatherItem = m_RootWeatherItem.transform;
                m_RootTransWeatherItem.SetParent(transform);
            }

            //添加气象项目进字典
            m_DicWeatherItem.Clear();
            for (int i = 0; i < m_ListWeatherItem.Count; i++)
            {
                var gameObj = GameObject.Instantiate(m_ListWeatherItem[i], m_RootTransWeatherItem);
                var weatherItem = gameObj.GetComponent<WeatherItemBase>();
                if (weatherItem == null)
                {
                    Debug.LogError($"WeatherSystemManager.Init() Error!! >> 预制体上未挂载WeatherItemBase的子类组件! Index-{i} PrefabName-{gameObj.name}");
                    continue;
                }

                var type = weatherItem.GetType();
                if (m_DicWeatherItem.ContainsKey(type))
                {
                    Debug.LogError($"WeatherSystemManager.Init() Error!! >> 重复的气象组件-{type.Name}! Index-{i} PrefabName-{gameObj.name}");
                    continue;
                }

                m_DicWeatherItem.Add(type, weatherItem);
                m_DicWeatherItemOpenState.Add(type, false);
            }

            CloseAllWeatherItem(true);
        }

        /// <summary>
        /// 设置 全局开启状态
        /// 不会影响气象项目的OpenState。再全局开启状态再次开启时，打开状态的气象Item依然是打开状态。
        /// </summary>
        /// <param name="isEnable"></param>
        public void SetGlobalEnableState(bool isEnable)
        {
            m_RootWeatherItem.SetActive(isEnable);

            if (isEnable)
            {
                //恢复所有气象项目的开启状态
                foreach (var kv in m_DicWeatherItem)
                {
                    var type = kv.Key;
                    var item = kv.Value;
                    item.SetOpenState(m_DicWeatherItemOpenState[type]);
                }
            }
            else
                CloseAllWeatherItem(false);
        }

        /// <summary>
        /// 获取 气象项目
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetWeatherItem<T>() where T : WeatherItemBase
        {
            WeatherItemBase weatherItem = null;
            m_DicWeatherItem.TryGetValue(typeof(T), out weatherItem);
            if (weatherItem == null) return null;

            return weatherItem as T;
        }

        /// <summary>
        /// 打开 气象项目
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        public bool SetWeatherItemOpenState<T>(bool isOpen) where T : WeatherItemBase
        {
            var item = GetWeatherItem<T>();
            if (item == null) { return false; }

            item.SetOpenState(isOpen);
            m_DicWeatherItemOpenState[item.GetType()] = item.IsOpne;

            return true;
        }

        /// <summary>
        /// 检查 气象项目是否打开
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool CheckWeatherItemOpenState<T>() where T : WeatherItemBase
        {
            var type = typeof(T);
            bool isOpen = false;
            m_DicWeatherItemOpenState.TryGetValue(type, out isOpen);

            return isOpen;
        }

        /// <summary>
        /// 关闭所有 气象项目
        /// </summary>
        /// <param name="recordState">是否记录状态</param>
        /// <returns></returns>
        public bool CloseAllWeatherItem(bool recordState)
        {
            foreach (var item in m_DicWeatherItem.Values)
            {
                item.SetOpenState(false);
                if (recordState)
                    m_DicWeatherItemOpenState[item.GetType()] = false;
            }

            return true;
        }
    }
}
