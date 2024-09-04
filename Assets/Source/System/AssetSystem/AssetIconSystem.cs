using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Icon模块 快捷加载单个Sprite的API
/// </summary>
public class AssetIconSystem : Singleton<AssetIconSystem>, IDestroy
{
    /// <summary>
    /// 默认最大加载数量
    /// </summary>
    private int m_MaxCountDefault = 20;

    /// <summary>
    /// 配置每组ICON的最大加载数量
    /// </summary>
    private Dictionary<string, int> iconGroupMaxCount = new Dictionary<string, int> 
    { 
        { "prop", 20 },
    };

    private Dictionary<string, List<string>> m_LoadedIconMap = new Dictionary<string, List<string>>();

    /// <summary>
    /// 设置Icon
    /// </summary>
    /// <param name="image">Image组件</param>
    /// <param name="folder">Icon文件夹名</param>
    /// <param name="IconName">Icon文件名</param>
    /// <param name="unCompleteDisable">未加载完成 隐藏Image</param>
    /// <param name="callBack"></param>
    public void SetIcon(Image image, string folder, string IconName, bool unCompleteDisable = true, Action callBack = null)
    {
        if (string.IsNullOrEmpty(IconName))
        {
            Debug.LogError("IconModel.SetIcon() Error >> IconName is null");
            return;
        }

        if (image == null)
        {
            Debug.LogError("IconModel.SetIcon() Error >> image component is null");
            return;
        }

        string address = AssetAddressUtil.GetTextureIconAddress(folder, IconName);

        if (unCompleteDisable)
        {
            image.enabled = false;
        }

        AssetSystem.Instance.LoadSprite(address, (result) =>
        {
            //异步操作 物体可能被销毁
            if (image == null) { return; }

            image.enabled = true;
            image.sprite = result;

            CheckIconVolume(folder, IconName);

            callBack?.Invoke();
        });
    }

    /// <summary>
    /// 设置ICON
    /// </summary>
    /// <param name="mat">Material材质球</param>
    /// <param name="folder">Icon文件夹名</param>
    /// <param name="IconName">Icon文件名</param>
    /// <param name="callBack"></param>
    public void SetIcon(Material mat, string folder, string IconName, Action callBack = null)
    {
        if (mat == null)
        {
            Debug.LogError("IconModel.SetIcon() Error >> material is null");
            return;
        }

        string address = AssetAddressUtil.GetTextureIconAddress(folder, IconName);

        AssetSystem.Instance.LoadSprite(address, (sprite) =>
        {
            mat.SetTexture("_MainTex", sprite.texture);

            CheckIconVolume(folder, IconName);

            callBack?.Invoke();
        });
    }

    private void CheckIconVolume(string folder, string IconName) //检查Icon容量
    {
        List<string> folderIcons;
        //新建 容量记录
        if (!m_LoadedIconMap.TryGetValue(folder, out folderIcons))
        {
            int maxCount;
            if (!iconGroupMaxCount.TryGetValue(folder, out maxCount))
            {
                maxCount = m_MaxCountDefault;
            }
            folderIcons = new List<string>(maxCount);
            m_LoadedIconMap.Add(folder, folderIcons);
        }

        //是否超过容量上限
        if (folderIcons.Count >= folderIcons.Capacity)
        {
            //卸载最早的Icon
            string address = AssetAddressUtil.GetTextureIconAddress(folder, folderIcons[0]);
            AssetAddressSystem.Instance.UnloadAsset(address);

            folderIcons.RemoveAt(0);
        }

        folderIcons.Add(IconName);
    }

    /// <summary>
    /// 手动卸载Icon
    /// </summary>
    /// <param name="folder">Icon文件夹名</param>
    /// <param name="IconName">Icon文件名</param>
    /// <param name="unloadCount">卸载数量 0卸载所有</param>
    public void UnloadIcon(string folder, string IconName, int unloadCount = 1)
    {
        List<string> folderIcons;
        if (m_LoadedIconMap.TryGetValue(folder, out folderIcons))
        {
            if (folderIcons.Contains(IconName))
            {
                string address = AssetAddressUtil.GetTextureIconAddress(folder, IconName);

                if (unloadCount > 0)
                {
                    //卸载Icon 指定数量
                    for (int i = folderIcons.Count - 1; i >= 0; i--)
                    {
                        if (folderIcons[i] == IconName)
                        {
                            //卸载Icon
                            AssetAddressSystem.Instance.UnloadAsset(address);
                            folderIcons.RemoveAt(i);

                            //检查 卸载数量
                            unloadCount--;
                            if (unloadCount <= 0)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    //卸载Icon 所有
                    for (int i = folderIcons.Count - 1; i >= 0; i--)
                    {
                        if (folderIcons[i] == IconName)
                        {
                            //卸载Icon
                            AssetAddressSystem.Instance.UnloadAsset(address);
                            folderIcons.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 手动卸载Icon 某folder所有
    /// </summary>
    /// <param name="folder">Icon文件夹名</param>
    public void UnloadIcon(string folder)
    {
        List<string> folderIcons;
        if (m_LoadedIconMap.TryGetValue(folder, out folderIcons))
        {
            for (int i = 0; i < folderIcons.Count; i++)
            {
                //卸载Icon
                string address = AssetAddressUtil.GetTextureIconAddress(folder, folderIcons[i]);
                AssetAddressSystem.Instance.UnloadAsset(address);
            }
            folderIcons.Clear();
        }
    }
}
