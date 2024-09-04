using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 资源模块 明确类型的资源 加载、卸载
/// </summary>
public class AssetSystem : Singleton<AssetSystem>, IDestroy
{
    /// <summary>
    /// 纹理贴图2D 默认
    /// </summary>
    public Texture2D Tex2DDefault { get { return m_Tex2DDefault; } }
    private Texture2D m_Tex2DDefault;

    public override void Init()
    {
        base.Init();

        m_Tex2DDefault = new Texture2D(1, 1);
        m_Tex2DDefault.name = "Tex2DDefault";
        m_Tex2DDefault.SetPixel(0, 0, new Color(1, 1, 1, 0));
        m_Tex2DDefault.Apply();
    }

    /// <summary>
    /// 加载 预制体
    /// </summary>
    /// <param name="address"></param>
    /// <param name="callBack"></param>
    /// <param name="parent">父物体 默认null-返回Prefab非实例化的模板 非null-返回Prefab实例化的对象</param>
    /// <param name="SetActive">是否设置激活 默认激活</param>
    public void LoadPrefab(string address, Action<GameObject> callBack = null, Transform parent = null, bool SetActive = true)
    {
        AssetAddressSystem.Instance.LoadAsset<GameObject>(address, (asset) =>
        {
            GameObject prefab = asset as GameObject;

            GameObject prefabResult = prefab;
            if (parent != null)
            {
                prefabResult = GameObject.Instantiate(prefab, parent, false);
                if (prefabResult.activeSelf != SetActive)
                    prefabResult.SetActive(SetActive);
            }

#if UNITY_EDITOR  // 编辑器模式 从AB包加载Shader时 因Shader格式不支持 导致显示异常
            if (UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilderIndex == 2)
            {
                TextMeshProUGUI[] textMeshs = prefabResult.GetComponentsInChildren<TextMeshProUGUI>();

                for (int i = 0; i < textMeshs.Length; i++)
                {
                    TextMeshProUGUI textMeshProUGUI = textMeshs[i];
                    textMeshProUGUI.fontSharedMaterial.shader = Shader.Find(textMeshProUGUI.fontSharedMaterial.shader.name);
                }

                ParticleSystem[] particleSystems = prefabResult.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < particleSystems.Length; i++)
                {
                    ParticleSystem particleSystem = particleSystems[i];
                    Renderer render = particleSystem.GetComponent<Renderer>();

                    if (render != null)
                        render.sharedMaterial.shader = Shader.Find(render.sharedMaterial.shader.name);
                }

                MeshRenderer[] meshRenderers = prefabResult.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < meshRenderers.Length; i++)
                {
                    MeshRenderer meshRenderer = meshRenderers[i];
                    meshRenderer.sharedMaterial.shader = Shader.Find(meshRenderer.sharedMaterial.shader.name);
                }
            }
#endif

            callBack?.Invoke(prefabResult);
        });
    }

    /// <summary>
    /// 加载 Sprite
    /// </summary>
    /// <param name="address"></param>
    /// <param name="callBack"></param>
    public void LoadSprite(string address, Action<Sprite> callBack)
    {
        AssetAddressSystem.Instance.LoadAsset<Sprite>(address, (result) =>
        {
            Sprite sprite = result as Sprite;
            callBack?.Invoke(sprite);
        });
    }

    /// <summary>
    /// 加载Json
    /// </summary>
    /// <param name="address"></param>
    /// <param name="callBack"></param>
    public void LoadJson(string address, Action<TextAsset> callBack)
    {
        AssetAddressSystem.Instance.LoadAsset<TextAsset>(address, (result) =>
        {
            TextAsset textAsset = result as TextAsset;
            callBack?.Invoke(textAsset);
        });
    }
}
