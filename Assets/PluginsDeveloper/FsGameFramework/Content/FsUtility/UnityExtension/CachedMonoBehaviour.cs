using UnityEngine;

public class CachedMonoBehaviour : MonoBehaviour
{
    Transform mCachedTransform;
    public Transform TransformGet
    {
        get
        {
            if (mCachedTransform == null) mCachedTransform = transform;
            return mCachedTransform;
        }
    }

    GameObject mCachedGameObject;
    public GameObject GameObjectGet
    {
        get
        {
            if (mCachedGameObject == null) mCachedGameObject = gameObject;
            return mCachedGameObject;
        }
    }

    //GetComponent的缓存字典 一般不用还是习惯Get后自己管理 注释咯
    //private Dictionary<System.Type, UnityEngine.Component> mCachedComponentMap = new Dictionary<System.Type, Component>();
    //public T GetCachedComponent<T>() where T: UnityEngine.Component
    //{
    //    if (GameObject == null) return null;

    //    System.Type t = typeof(T);
    //    UnityEngine.Component c;
    //    if (!mCachedComponentMap.ContainsKey(t))
    //    {
    //        T target = GameObject.GetComponent<T>();
    //        if (target == null) return null;
    //        mCachedComponentMap.Add(t, target);
    //        return target;
    //    }
    //    else
    //        c = mCachedComponentMap[t];

    //    return c as T;
    //}
}

