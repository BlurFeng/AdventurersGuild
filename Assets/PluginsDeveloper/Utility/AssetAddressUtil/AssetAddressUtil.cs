using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AssetAddressUtil
{
    public static string GetPrefabGamePlayAddress(string assetName)
    {
        return string.Format("Assets/ProductAssets/Prefab/GamePlay/{0}.prefab", assetName);
    }

    /// <summary>
    /// 预制体 UI界面
    /// </summary>
    /// <param name="windowName"></param>
    /// <returns></returns>
    public static string GetPrefabWindowAddress(string windowName)
    {
        var prefabAsset = windowName;
        var prefabPath = windowName.Split('/');
        if (prefabPath.Length > 1)
        {
            prefabAsset = prefabPath[1];
        }
        return string.Format("Assets/ProductAssets/Prefab/Window/{0}/{1}.prefab", windowName, prefabAsset);
    }

    /// <summary>
    /// 预制体 UI界面模板
    /// </summary>
    /// <param name="prefabName"></param>
    /// <returns></returns>
    public static string GetPrefabTemplateAddress(string prefabName)
    {
        return string.Format("Assets/ProductAssets/Prefab/Template/{0}.prefab", prefabName);
    }

    /// <summary>
    /// 预制体 角色
    /// </summary>
    /// <param name="prefabName"></param>
    /// <returns></returns>
    public static string GetPrefabCharacterAddress(string prefabName)
    {
        return string.Format("Assets/ProductAssets/Prefab/Character/{0}.prefab", prefabName);
    }

    /// <summary>
    /// 预制体 角色Spine
    /// </summary>
    /// <param name="prefabName"></param>
    /// <returns></returns>
    public static string GetPrefabCharacterSpineAddress(string prefabName)
    {
        return string.Format("Assets/ProductAssets/Spine/Character/{0}/{0}.prefab", prefabName);
    }

    /// <summary>
    /// 预制体 家具
    /// </summary>
    /// <param name="prefabName"></param>
    /// <returns></returns>
    public static string GetPrefabFurnitureAddress(string prefabName)
    {
        return string.Format("Assets/ProductAssets/Prefab/Furniture/{0}.prefab", prefabName);
    }

    /// <summary>
    /// 预制体 建筑
    /// </summary>
    /// <param name="prefabName"></param>
    /// <returns></returns>
    public static string GetPrefabBuildingAddress(string prefabName)
    {
        return string.Format("Assets/ProductAssets/Prefab/Building/{0}.prefab", prefabName);
    }

    /// <summary>
    /// 图片 图标
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public static string GetTextureCharacterSkinSpriteAddress(string folder, string assetName)
    {
        return string.Format("Assets/ProductAssets/Texture/Character/SkinSprite/{0}/{1}.png", folder, assetName);
    }

    /// <summary>
    /// 图片 图标
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public static string GetTextureIconAddress(string folder, string assetName)
    {
        return string.Format("Assets/ProductAssets/Texture/Icon/{0}/{1}.png", folder, assetName);
    }

    /// <summary>
    /// 图片 大图
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public static string GetTextureLargeAddress(string assetName)
    {
        return string.Format("Assets/ProductAssets/Texture/Large/{0}.png", assetName);
    }

    /// <summary>
    /// 音频
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public static string GetAudioAddress(string folder, string assetName)
    {
        return string.Format("Assets/ProductAssets/Audio/{0}/{1}.wav", folder, assetName);
    }
}

