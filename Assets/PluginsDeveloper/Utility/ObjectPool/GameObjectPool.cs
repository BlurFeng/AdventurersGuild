using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : Singleton<GameObjectPool>, IDestroy
{
	/// <summary>
	/// 对象池字典 闲置
	/// </summary>
	private Dictionary<GameObject, ObjectPool<GameObject>> m_GameObjectPoolIdle = new Dictionary<GameObject, ObjectPool<GameObject>>();
	/// <summary>
	/// 对象池字典 使用中
	/// </summary>
	private Dictionary<GameObject, ObjectPool<GameObject>> m_GameObjectPoolUsing = new Dictionary<GameObject, ObjectPool<GameObject>>();

	private Transform m_GameObjectPoolRoot = null; //缓存池 游戏对象实例 根节点

	/// <summary>
	/// 初始化 游戏物体
	/// </summary>
	/// <param name="prefab">游戏对象模板</param>
	/// <param name="initCount">初始实例数量</param>
	/// <param name="canOverLimitCount">是否 超过数量上限</param>
	public void InitGameObject(GameObject prefab, int initCount, bool canOverLimitCount = false)
	{
		if (m_GameObjectPoolRoot == null)
		{
			m_GameObjectPoolRoot = new GameObject("GameObjectPoolRoot").transform;
		}

		//是否 对象池已存在
		if (m_GameObjectPoolIdle.ContainsKey(prefab))
		{
			Debug.LogError($"GameObjectPool.InitGameObject() Error! >> 对象池已存在 prefabName-{prefab.name}");
			return;
		}

		//创建 对象池
		ObjectPool<GameObject> pool = new ObjectPool<GameObject>(() =>
		{
			GameObject go = GameObject.Instantiate(prefab) as GameObject;
			go.transform.SetParent(m_GameObjectPoolRoot);
			go.SetActive(false);
			return go;
		}, initCount, canOverLimitCount);

		//记录 对象池
		m_GameObjectPoolIdle[prefab] = pool;
	}

	/// <summary>
	/// 获取
	/// </summary>
	/// <param name="prefab">游戏对象 预制体模板</param>
	/// <param name="canOverLimitUseEarly">是否 超上限时复用最早对象（可超上限实例化时会直接实例化新GameObject）</param>
	/// <returns></returns>
	public GameObject Get(GameObject prefab, bool canOverLimitUseEarly = true)
	{
		//无对象池时 初始化对象池
		if (!m_GameObjectPoolIdle.ContainsKey(prefab))
		{
			Debug.LogError($"GameObjectPool.Get() Error! >> 不能获取未初始化的预制体 PrefabName-{prefab.name}");
			return null;
		}

		//获取 对象池中的实例
		ObjectPool<GameObject> gameObjectPool = m_GameObjectPoolIdle[prefab];
		GameObject instance = gameObjectPool.Get(canOverLimitUseEarly);

		//物体意外销毁时 重新实例化
		if (instance == null)
		{
			instance = GameObject.Instantiate(prefab) as GameObject;
		}

		//激活 实例对象
		instance.SetActive(true);
		if (!m_GameObjectPoolUsing.ContainsKey(instance))
		{
			m_GameObjectPoolUsing.Add(instance, gameObjectPool);
		}

		return instance;
	}

	/// <summary>
	/// 返还 不会销毁对象实例
	/// </summary>
	/// <param name="instance">归还的对象</param>
	/// <param name="reduceCount">是否 减少容量</param>
	public bool Return(GameObject instance, bool reduceCount = false)
	{
		instance.SetActive(false);

		if (m_GameObjectPoolUsing.ContainsKey(instance))
		{
			m_GameObjectPoolUsing[instance].Return(instance, reduceCount);
			m_GameObjectPoolUsing.Remove(instance);
			instance.transform.SetParent(m_GameObjectPoolRoot);

			return true;
		}
		else
		{
			return false;
		}
	}

	public void Clear(GameObject prefab)
	{
		if (!m_GameObjectPoolIdle.ContainsKey(prefab)) { return; }

		ObjectPool<GameObject> gameObjectPool = m_GameObjectPoolIdle[prefab];

		//回收 所有使用中的对象
		foreach (GameObject instance in gameObjectPool.GetUsingInstances())
		{
			m_GameObjectPoolUsing.Remove(instance);
		}

		//销毁 所有对象
		foreach (ObjectPoolItem<GameObject> item in gameObjectPool.GetAllInstances())
		{
			GameObject.Destroy(item.Item);
		}

		//清空对象池 移除记录
		gameObjectPool.Clear();
		m_GameObjectPoolIdle.Remove(prefab);
	}
}
