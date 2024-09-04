using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessSystem : MonoBehaviourSingleton<PostProcessSystem>
{
    public UniversalAdditionalCameraData m_UACameraDataMain;
    public UniversalAdditionalCameraData m_UACameraDataUI;
    public Volume m_URPVolume;

    private Dictionary<ECameraType, Dictionary<System.Type, uint>> m_EffectReferenceCount = new Dictionary<ECameraType, Dictionary<System.Type, uint>>(); //开启后效 引用计数

    /// <summary>
    /// 摄像机类型
    /// </summary>
    public enum ECameraType
    {
        /// <summary>
        /// 主摄像机
        /// </summary>
        Main,
        /// <summary>
        /// UI摄像机
        /// </summary>
        UI
    }

    public override void Init()
    {
        //urp中，给pp的renderFeature关闭
        //CustomPostProcess cpp = RenderFeaturesManager.Instance.GetFeature<CustomPostProcess>();
        //cpp.SetActive(false);
        //默认关闭 所有后效
        if (m_UACameraDataMain != null)
            m_UACameraDataMain.renderPostProcessing = false;
        if (m_UACameraDataUI != null)
            m_UACameraDataUI.renderPostProcessing = false;
        if (m_URPVolume != null && m_URPVolume.profile != null)
        {
            for (int i = 0; i < m_URPVolume.profile.components.Count; i++)
            {
                m_URPVolume.profile.components[i].active = false;
            }
        }

        m_EffectReferenceCount.Add(ECameraType.Main, new Dictionary<System.Type, uint>());
        m_EffectReferenceCount.Add(ECameraType.UI, new Dictionary<System.Type, uint>());
    }

    //获取 后效列表
    private VolumeProfile GetVolumeProfile(ECameraType type)
    {
        if (m_URPVolume == null) return null;

        //在Awake中可能为null
        return m_URPVolume.profile;
    }

    /// <summary>
    /// 获取 后效组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetEffectComponent<T>(ECameraType type = ECameraType.Main) where T : VolumeComponent
    {
        VolumeProfile volume = GetVolumeProfile(type);
        if (volume == null) { return null; }

        T effect = null;
        bool have = volume.TryGet<T>(out effect);
        return effect;
    }

    /// <summary>
    /// 打开 后效
    /// </summary>
    /// <param name="postEffectEnum"></param>
    public void OpenEffect<T>(ECameraType type = ECameraType.Main) where T : VolumeComponent
    {
        if (!m_EffectReferenceCount.TryGetValue(type, out Dictionary<System.Type, uint> effectList)) { return; }

        //增加 特效引用计数
        var effectType = typeof(T);
        uint refCount;
        if (effectList.TryGetValue(effectType, out refCount))
        {
            refCount += 1;
            effectList[effectType] = refCount;
        }
        else
            effectList.Add(effectType, 1);

        OnOpenEffect<T>(type);
    }

    //打开特效
    private void OnOpenEffect<T>(ECameraType type = ECameraType.Main) where T : VolumeComponent
    {
        var effect = GetEffectComponent<T>();
        if (effect == null) { return; }

        if (!effect.active)
            effect.active = true;

        EnableCameraRenderPostProcess(type, true);
    }

    /// <summary>
    /// 关闭 后效
    /// </summary>
    /// <param name="postEffectEnum"></param>
    /// <param name="removeAllCount"></param>
    public void CloseEffect<T>(bool removeAllCount = false, ECameraType type = ECameraType.Main) where T : VolumeComponent
    {
        if (!m_EffectReferenceCount.TryGetValue(type, out Dictionary<System.Type, uint> effectList)) { return; }

        var effectType = typeof(T);
        uint refCount;
        if (effectList.TryGetValue(effectType, out refCount))
        {
            if (removeAllCount)
                OnCloseEffect<T>(type);
            else
            {
                refCount -= 1;
                if (refCount == 0)
                    OnCloseEffect<T>(type);
                else
                    effectList[effectType] = refCount;
            }
        }
    }

    //关闭后效
    private void OnCloseEffect<T>(ECameraType type = ECameraType.Main) where T : VolumeComponent
    {
        if (!m_EffectReferenceCount.TryGetValue(type, out Dictionary<System.Type, uint> effectList)) { return; }

        var effectType = typeof(T);
        effectList.Remove(effectType);
        var effect = GetEffectComponent<T>();
        if (effect == null) { return; }

        effect.active = false;

        //关闭 后效开关
        if (effectList.Count == 0)
            EnableCameraRenderPostProcess(type, false);
    }

    /// <summary>
    /// 关闭所有 后效
    /// </summary>
    public void CloseAllEffect(ECameraType type = ECameraType.Main)
    {
        VolumeProfile volume = GetVolumeProfile(type);
        if (volume == null) { return; }

        for (int i = 0; i < volume.components.Count; i++)
        {
            volume.components[i].active = false;
        }
        m_EffectReferenceCount.Clear();
        EnableCameraRenderPostProcess(type, false);
    }

    private void EnableCameraRenderPostProcess(ECameraType type, bool isEnable)
    {
        //打开 后效开关
        switch (type)
        {
            case ECameraType.Main:
                m_UACameraDataMain.renderPostProcessing = isEnable;
                break;
            case ECameraType.UI:
                m_UACameraDataUI.renderPostProcessing = isEnable;
                break;
        }
    }
}
