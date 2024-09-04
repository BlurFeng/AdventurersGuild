using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Google.Protobuf;
using System.Reflection;

public class Singleton<T> where T : IDestroy, new()
{
    private static T instance;
    private static readonly object syslock = new object();

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock (syslock)
                {
                    if (instance == null)
                    {
                        instance = new T();
                        SingletonModel.AddSingleton(instance);

                        //初始化
                        (instance as Singleton<T>).Init();

                        //在Instance时加载数据，这样能保证Model被调用时数据必定加载了。否则我们需要确保加载顺序
                        //仅在游戏启动时 才会加载存档
                        if (GameInstance.IsPlayingGame)
                        {
                            //加载存档数据
                            var iSaveData = instance as ISaveData;
                            if (iSaveData != null)
                            {
                                iSaveData.LoadData(SaveDataModel.Instance.SaveDataCur);
                            }
                        }
                    }
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// 创建，用于在要求的固定时机生成单例，而不是一个不确定的调用时机
    /// </summary>
    public virtual void Create()
    {

    }

    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init()
    {

    }

    /// <summary>
    /// 销毁
    /// </summary>
    public virtual void Destroy()
    {
        instance = default;
    }
}

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static GameObject GameObject;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();

                if (instance == null)
                {
                    GameObject = new GameObject { name = typeof(T).Name };
                    instance = GameObject.AddComponent<T>();
                    DontDestroyOnLoad(GameObject);
                    SingletonModel.AddSingleton(instance);
                }

                //在Instance时加载数据，这样能保证Model被调用时数据必定加载了。否则我们需要确保加载顺序
                //加载存档数据
                var iSaveData = instance as ISaveData;
                if (iSaveData != null)
                {
                    iSaveData.LoadData(SaveDataModel.Instance.SaveDataCur);
                }

                //初始化
                (instance as MonoBehaviourSingleton<T>).Init();
            }
            return instance;
        }
    }

    /// <summary>
    /// 创建，用于在要求的固定时机生成单例，而不是一个不确定的调用时机
    /// </summary>
    public virtual void Create()
    {

    }

    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init()
    {

    }

    /// <summary>
    /// 销毁
    /// </summary>
    public virtual void Destroy()
    {
        instance = null;
    }
}

/// <summary>
/// 单例
/// </summary>
public class SingletonModel
{
    private static List<object> m_Singletons = new List<object>();
    private static List<MonoBehaviour> m_MonoSingletons = new List<MonoBehaviour>();

    /// <summary>
    /// 添加 单例
    /// </summary>
    /// <param name="singleton"></param>
    public static void AddSingleton(object singleton)
    {
        m_Singletons.Add(singleton);
    }

    /// <summary>
    /// 添加 单例
    /// </summary>
    /// <param name="singleton"></param>
    public static void AddSingleton(MonoBehaviour singleton)
    {
        m_MonoSingletons.Add(singleton);
    }

    /// <summary>
    /// 获取 单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<T> GetSingletons<T>() where T: class
    {
        var list = new List<T>();

        //单例列表
        for (int i = 0; i < m_Singletons.Count; i++)
        {
            T singleton = m_Singletons[i] as T;

            if (singleton != null )
            {
                list.Add(singleton);
            }
        }

        return list;
    }

    /// <summary>
    /// 获取 单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<T> GetMonoSingletons<T>() where T : class
    {
        var list = new List<T>();

        //Mono单例列表
        for (int i = 0; i < m_MonoSingletons.Count; i++)
        {
            T singleton = m_MonoSingletons[i].GetComponent<T>();

            if (singleton != null)
            {
                list.Add(singleton);
            }
        }

        return list;
    }

    /// <summary>
    /// 清除 单例
    /// </summary>
    /// <param name="singleton"></param>
    /// <param name="immediatelyGC"></param>
    public static void ClearSingleton(object singleton, bool immediatelyGC = false)
    {
        if(m_Singletons.Contains(singleton))
        {
            IDestroy singletonDestroy = singleton as IDestroy;
            m_Singletons.Remove(singleton);
            singletonDestroy.Destroy();
            if (immediatelyGC)
            {
                GC.Collect();
            }
        }

        var monoSingleton = singleton as MonoBehaviour;
        if (m_MonoSingletons.Contains(monoSingleton))
        {
            m_MonoSingletons.Remove(monoSingleton);
            GameObject.Destroy(monoSingleton.gameObject);
            if (immediatelyGC)
            {
                GC.Collect();
            }
        }
    }

    /// <summary>
    /// 清除 单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="immediatelyGC">立即执行 GC</param>
    public static void ClearSingleton<T>(bool immediatelyGC = false) where T : IDestroy, new()
    {
        Type checkType = typeof(T);

        for (int i = 0; i < m_Singletons.Count; i++)
        {
            IDestroy singleton = m_Singletons[i] as IDestroy;

            if (singleton != null && checkType == singleton.GetType())
            {
                m_Singletons.Remove(singleton);
                singleton.Destroy();
                if (immediatelyGC)
                {
                    GC.Collect();
                }
                break;
            }
        }

        for (int i = 0; i < m_MonoSingletons.Count; i++)
        {
            var singleton = m_MonoSingletons[i];

            if (singleton != null && checkType == singleton.GetType())
            {
                m_MonoSingletons.Remove(singleton);
                (singleton as IDestroy).Destroy();
                if (immediatelyGC)
                {
                    GC.Collect();
                }
                break;
            }
        }
    }

    /// <summary>
    /// 清理所有Singleton
    /// </summary>
    public static void ClearAllSingleton()
    {
        for (int i = 0; i < m_Singletons.Count; i++)
        {
            var singleton = m_Singletons[i];
            IDestroy singletonDestroy = singleton as IDestroy;
            singletonDestroy.Destroy();
        }
        m_Singletons.Clear();

        for (int i = 0; i < m_MonoSingletons.Count; i++)
        {
            var singleton = m_MonoSingletons[i];
            IDestroy singletonDestroy = singleton as IDestroy;
            singletonDestroy.Destroy();
        }
        m_MonoSingletons.Clear();

        GC.Collect();
    }
}