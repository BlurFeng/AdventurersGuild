using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Unity的扩展方法
/// </summary>
static public class UnityExtension
{
    #region Dictionary
    /// <summary>
    /// 增量添加 Vaule为Int时
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dic"></param>
    /// <param name="key"></param>
    /// <param name="vaule"></param>
    public static void AddIncrementInt<T>(this Dictionary<T, int> dic, T key, int vaule)
    {
        if (!dic.ContainsKey(key))
        {
            dic.Add(key, vaule);
        }
        else
        {
            dic[key] += vaule;
        }
    }

    /// <summary>
    /// 替换添加
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="dic"></param>
    /// <param name="key"></param>
    /// <param name="vaule"></param>
    public static void AddReplace<T,U>(this Dictionary<T, U> dic, T key, U vaule)
    {
        if (!dic.ContainsKey(key))
        {
            dic.Add(key, vaule);
        }
        else
        {
            dic[key] = vaule;
        }
    }
    #endregion

    /// <summary>
    /// 获取或增加组件。
    /// </summary>
    /// <typeparam name="T">要获取或增加的组件。</typeparam>
    /// <param name="gameObject">目标对象。</param>
    /// <returns>获取或增加的组件。</returns>
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }

    /// <summary>
    /// 获取或增加组件。
    /// </summary>
    /// <param name="gameObject">目标对象。</param>
    /// <param name="type">要获取或增加的组件类型。</param>
    /// <returns>获取或增加的组件。</returns>
    public static Component GetOrAddComponent(this GameObject gameObject, Type type)
    {
        Component component = gameObject.GetComponent(type);
        if (component == null)
        {
            component = gameObject.AddComponent(type);
        }

        return component;
    }

    public static Component[] GetComponentsInChildren(this Component c, string type)
    {
        return c.GetComponentsInChildren(Type.GetType(type));
    }

    public static void AddRange<T>(this List<T> list, Component[] array) where T : Component
    {
        for (int i = 0; i < array.Length; i++)
        {
            list.Add(array[i] as T);
        }
    }

    /// <summary>
    /// 获取 GameObject 是否在场景中。
    /// </summary>
    /// <param name="gameObject">目标对象。</param>
    /// <returns>GameObject 是否在场景中。</returns>
    /// <remarks>若返回 true，表明此 GameObject 是一个场景中的实例对象；若返回 false，表明此 GameObject 是一个 Prefab。</remarks>
    public static bool InScene(this GameObject gameObject)
    {
        return gameObject.scene.name != null;
    }

    /// <summary>
    /// 递归设置游戏对象的层次。
    /// </summary>
    /// <param name="gameObject"><see cref="UnityEngine.GameObject" /> 对象。</param>
    /// <param name="layer">目标层次的编号。</param>
    public static void SetLayerRecursively(this GameObject gameObject, int layer)
    {
        Transform[] transforms = gameObject.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].gameObject.layer = layer;
        }
    }

    /// <summary>
    /// 创建子物体。
    /// </summary>
    public static T CreateChild<T>(GameObject itemPref, GameObject parent = null)
    {
        var obj = GameObject.Instantiate(itemPref);
        if (parent != null)
        {
            obj.transform.SetParent(parent.transform);
        }
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
        return obj.GetComponent<T>();
    }

    public static GameObject CreateChild(GameObject itemPref, GameObject parent = null)
    {
        var obj = GameObject.Instantiate(itemPref);
        if (parent != null)
        {
            obj.transform.SetParent(parent.transform);
        }
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
        return obj;
    }

    public static void ResetAnimation(Animation ani, string aniName)
    {
        AnimationState state = ani[aniName];
        state.speed = 1;
        ani.Play(aniName);
        state.time = 0;
        ani.Sample();
        state.enabled = false;
    }

    /// <summary>
    /// 注意，谨慎使用该方法 只时某些特殊情况需要使用，可能一个节点拥有重复组件
    /// 不管本身是否有这个组件，都添加,
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    // static public T AddComponent<T>(this GameObject go) where T:Component
    // {
    //     return go.transform.AddComponent<T>();
    // }

    /// <summary>
    /// 注意，谨慎使用该方法 只时某些特殊情况需要使用，可能一个节点拥有重复组件
    /// 不管本身是否有这个组件，都添加
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    // static public T AddComponent<T>(this Component go) where T:Component
    // {
    //     return go.AddComponent<T>();
    // }
}
