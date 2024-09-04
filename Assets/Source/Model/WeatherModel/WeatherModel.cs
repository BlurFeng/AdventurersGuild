using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using com.ootii.Messages;
using Deploy;
using FsWeatherSystem;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 气象模块
/// </summary>
public class WeatherModel : MonoBehaviourSingleton<WeatherModel>, IDestroy, ISaveData
{
    private WeatherSystemManager m_WeatherSystemManager; //气象系统管理器
    private bool m_GlobalEnableStateCur = false; //全局开启状态 当前

    private bool m_IsLoad = false;

    /// <summary>
    /// 初始化
    /// </summary>
    public override void Init()
    {
        base.Init();

        //气象系统
        if (m_IsLoad == false)
        {
            m_IsLoad = true;
            string addr = AssetAddressUtil.GetPrefabGamePlayAddress("WeatherSystem");
            AssetSystem.Instance.LoadPrefab(addr, (prefab) =>
            {
                prefab.hideFlags = HideFlags.DontSaveInEditor;
                m_WeatherSystemManager = prefab.GetComponent<WeatherSystemManager>();
                //设置当前的全局开启状态
                m_WeatherSystemManager.SetGlobalEnableState(m_GlobalEnableStateCur);

                OpenWeatherDayLoop();
            }, transform);
        }
    }

    public override void Destroy()
    {
        base.Destroy();

        CloseWeatherDayLoop();
    }

    public void SaveData(ES3File saveData)
    {
        
    }

    public void LoadData(ES3File saveData)
    {

    }

    /// <summary>
    /// 设置 开启状态
    /// </summary>
    /// <param name="isEnable"></param>
    public void SetGlobalEnableState(bool isEnable)
    {
        m_GlobalEnableStateCur = isEnable;

        if (m_WeatherSystemManager == null) return;

        m_WeatherSystemManager.SetGlobalEnableState(isEnable);
    }

    /// <summary>
    /// 获取 气象项目
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetWeatherItem<T>() where T : WeatherItemBase
    {
        if (m_WeatherSystemManager == null) return null;

        return m_WeatherSystemManager.GetWeatherItem<T>();
    }

    /// <summary>
    /// 打开 气象项目
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    public bool SetWeatherItemOpenState<T>(bool isOpen) where T : WeatherItemBase
    {
        if (m_WeatherSystemManager == null) return false;

        return m_WeatherSystemManager.SetWeatherItemOpenState<T>(isOpen);
    }

    /// <summary>
    /// 检查 气象项目是否打开
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool CheckWeatherItemOpenState<T>() where T : WeatherItemBase
    {
        if (m_WeatherSystemManager == null) return false;

        return m_WeatherSystemManager.CheckWeatherItemOpenState<T>();
    }

    #region 基础循环
    private Coroutine m_CorWeatherDayLoop = null;
    private WaitForSeconds m_WaitSecDayLoopUpdate = new WaitForSeconds(0.5f);

    //打开 气象今日循环
    private void OpenWeatherDayLoop()
    {
        if (m_CorWeatherDayLoop != null)
        {
            StopCoroutine(m_CorWeatherDayLoop);
            m_CorWeatherDayLoop = null;
        }
        m_CorWeatherDayLoop = StartCoroutine(CorWeatherDayLoop());

        if (m_WeatherSystemManager != null)
        {
            m_WeatherSystemManager.SetWeatherItemOpenState<WeatherItemGlobalLight>(true);
            m_WeatherSystemManager.SetWeatherItemOpenState<WeatherItemGlobalBloom>(true);
        }
    }

    //关闭 气象今日循环
    private void CloseWeatherDayLoop()
    {
        if (m_CorWeatherDayLoop != null)
        {
            StopCoroutine(m_CorWeatherDayLoop);
            m_CorWeatherDayLoop = null;
        }

        if (m_WeatherSystemManager != null)
        {
            m_WeatherSystemManager.SetWeatherItemOpenState<WeatherItemGlobalLight>(false);
            m_WeatherSystemManager.SetWeatherItemOpenState<WeatherItemGlobalBloom>(false);
        }
    }

    private IEnumerator CorWeatherDayLoop()
    {
        while (true)
        {
            yield return m_WaitSecDayLoopUpdate;

            //获取 时间信息
            var timeInfo = TimeModel.Instance.TimeInfoCur;

            //更新光照
            var globalLight = GetWeatherItem<WeatherItemGlobalLight>();
            if (globalLight != null)
                globalLight.SetLocationValue(timeInfo.GameTimeDayProgress);

            //更新Bloom
            var globalBloom = GetWeatherItem<WeatherItemGlobalBloom>();
            if (globalBloom != null)
                globalBloom.SetLocationValue(timeInfo.GameTimeDayProgress);
        }
    }
    #endregion
}
