using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Addressable模块 泛型资源的 加载、卸载
/// </summary>
public class AssetAddressSystem : Singleton<AssetAddressSystem>, IDestroy
{
    private Dictionary<string, List<AsyncOperationHandle>> m_LoadingAssetsMap = new Dictionary<string, List<AsyncOperationHandle>>(); //资源句柄 正在加载
    private Dictionary<string, List<AsyncOperationHandle>> m_LoadedAssetsMap = new Dictionary<string, List<AsyncOperationHandle>>(); //资源句柄 已加载

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="address">资源地址</param>
    /// <param name="callBack"></param>
    public void LoadAsset<T>(string address, Action<UnityEngine.Object> callBack) where T : UnityEngine.Object
    {
        List<AsyncOperationHandle> handleListLoading;
        if (!m_LoadingAssetsMap.TryGetValue(address, out handleListLoading))
        {
            handleListLoading = new List<AsyncOperationHandle>();
            m_LoadingAssetsMap.Add(address, handleListLoading);
        }

        LogUtil.Log("↓↓↓LoadAsset:", address, Color.yellow);

        var handle = Addressables.LoadAssetAsync<T>(address);
        handleListLoading.Add(handle); //记录 资源句柄 正在加载
        handle.Completed += (handleDone) =>
        {
            //移除 资源句柄 正在加载
            handleListLoading.Remove(handle);

            //资源加载失败
            if (handleDone.Status != AsyncOperationStatus.Succeeded)
            {
                //Debug.LogError($"---LoadAsset: Error!!! Address-{address}");
                //Debug.LogError(handleDone.OperationException);
                return;
            }

            //记录 资源句柄 已加载
            List<AsyncOperationHandle> handleListLoaded;
            if (!m_LoadedAssetsMap.TryGetValue(address, out handleListLoaded))
            {
                handleListLoaded = new List<AsyncOperationHandle>();
                m_LoadedAssetsMap.Add(address, handleListLoaded);
            }
            handleListLoaded.Add(handleDone);

            //回调 返回资源
            callBack(handleDone.Result);
        };
    }

    /// <summary>
    /// 卸载资源
    /// </summary>
    /// <param name="address">资源地址</param>
    /// <param name="unloadCount">卸载数量 0卸载所有</param>
    /// <returns>是否成功卸载</returns>
    public bool UnloadAsset(string address, int unloadCount = 1)
    {
        //从 已加载资源句柄Map 卸载资源
        List<AsyncOperationHandle> handleListLoaded;
        if (m_LoadedAssetsMap.TryGetValue(address, out handleListLoaded))
        {
            if (unloadCount > 0)
            {
                //卸载 指定数量
                while (unloadCount > 0)
                {
                    Addressables.Release(handleListLoaded[0]);
                    handleListLoaded.RemoveAt(0);
                    LogUtil.Log("---UnloadAsset: single-", address, Color.cyan);

                    unloadCount--;
                }
            }
            else
            {
                //卸载 所有
                for (int i = 0; i < handleListLoaded.Count; i++)
                {
                    Addressables.Release(handleListLoaded[i]);
                }
                handleListLoaded.Clear();

                LogUtil.Log("---UnloadAsset: all-", address, Color.cyan);
            }

            return true;
        }
        else
        {
            return false;
        }
    }
}
