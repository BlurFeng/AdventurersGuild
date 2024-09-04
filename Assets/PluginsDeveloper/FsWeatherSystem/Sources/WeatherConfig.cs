using ParadoxNotion;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering;

namespace FsWeatherSystem
{
    using IProvider = FilterWindow.IProvider;

    /// <summary>
    /// 气象配置
    /// 配置了可用的气象项目列表
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherConfig", menuName = "FsWeatherSystem/WeatherConfig")]
    public class WeatherConfig : ScriptableObject
    {
        public List<WeatherItemBase> m_ListWeatherItem;

        /// <summary>
        /// Adds a <see cref="WeatherItemBase"/> to this Volume Profile.
        /// </summary>
        /// <remarks>
        /// You can only have a single component of the same type per Volume Profile.
        /// </remarks>
        /// <param name="type">A type that inherits from <see cref="WeatherItemBase"/>.</param>
        /// <param name="overrides">Specifies whether Unity should automatically override all the settings when
        /// you add a <see cref="WeatherItemBase"/> to the Volume Profile.</param>
        /// <returns>The instance created for the given type that has been added to the profile</returns>
        /// <see cref="Add{T}"/>
        public WeatherItemBase Add(Type type, bool overrides = false)
        {
            if (CheckHas(type))
                throw new InvalidOperationException("Component already exists in the volume");

            var component = type.CreateObject() as WeatherItemBase;
#if UNITY_EDITOR
            //component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            //component.name = type.Name;
#endif
            //component.SetAllOverridesTo(overrides);
            m_ListWeatherItem.Add(component);
            //isDirty = true;
            return component;
        }

        private bool CheckHas(Type type)
        {
            foreach (var item in m_ListWeatherItem)
            {
                if (item.GetType() == type)
                    return true;
            }

            return false;
        }
    }

    #region 自定义Inspector界面
    /// <summary>
    /// 气象配置
    /// 自定义Inspector界面
    /// </summary>
    [CustomEditor(typeof(WeatherConfig))]
    public class WeatherConfigEditor : Editor 
    {
        private WeatherConfig m_WeatherConfig;

        private void OnEnable()
        {
            m_WeatherConfig = target as WeatherConfig;
        }

        //public override void OnInspectorGUI()
        //{
        //    if (target == null)
        //        return;

        //    //列表 当前配置的气象项目
        //    for (int i = 0; i < m_WeatherConfig.m_WeatherItemArray.Count; i++)
        //    {
        //        var item = m_WeatherConfig.m_WeatherItemArray[i];
        //        var title = item.GetType().Name;
        //        int id = i; // Needed for closure capture below
        //    }

        //    EditorGUILayout.Space();
            
        //    //按钮 添加气象项目
        //    using (var hscope = new EditorGUILayout.HorizontalScope())
        //    {
        //        if (GUILayout.Button("AddWeatherItem"))
        //        {
        //            var r = hscope.rect;
        //            var pos = new Vector2(r.x + r.width / 2f, r.yMax + 18f);
        //            //FilterWindow.Show(pos, new VolumeComponentProvider(asset, this));
        //        }
        //    }
        //}
    }

    class WeatherItemProvider : IProvider
    {
        Vector2 IProvider.position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        void IProvider.CreateComponentTree(List<FilterWindow.Element> tree)
        {
            throw new NotImplementedException();
        }

        bool IProvider.GoToChild(FilterWindow.Element element, bool addIfComponent)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}