using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T>
{
	/// <summary>
	/// 对象列表 所有
	/// </summary>
	private List<ObjectPoolItem<T>> m_ItemsAll;
	/// <summary>
	/// 对象列表 使用中
	/// </summary>
	private Dictionary<T, ObjectPoolItem<T>> m_ItemsUsing;

	private Func<T> m_FuncFactory; //工厂 实例化方法
	private int m_CountInit; //数量 初始化
	private bool m_CanOverLimitCount; //是否 超过数量上限使用对象
	private int m_IndexLast = 0; //下标 上次使用到的对象

	/// <summary>
	/// 初始化对象池
	/// </summary>
	/// <param name="factoryFunc">实例化对象的工厂方法</param>
	/// <param name="initCount">初始实例数量</param>
	/// <param name="canOverLimitCount">是否 超过数量上限</param>
	public ObjectPool(Func<T> factoryFunc, int initCount, bool canOverLimitCount = false)
	{
		this.m_FuncFactory = factoryFunc;
		this.m_CountInit = initCount;
		this.m_CanOverLimitCount = canOverLimitCount;

		//实例化 对象池
		m_ItemsAll = new List<ObjectPoolItem<T>>(m_CountInit);
		m_ItemsUsing = new Dictionary<T, ObjectPoolItem<T>>(m_CountInit);
		for (int i = 0; i < m_CountInit; i++)
		{
			InstantiateItem();
		}
	}

	/// <summary>
	/// 使用
	/// </summary>
	/// <param name="overLimitUseEarly">是否 超上限 复用最早对象</param>
	/// <returns></returns>
	public T Get(bool overLimitUseEarly = false)
	{
		ObjectPoolItem<T> objectPoolItem = null;
		//获取 下一个未使用的对象
		for (int i = 0; i < m_ItemsAll.Count; i++)
		{
			m_IndexLast++;
			if (m_IndexLast > m_ItemsAll.Count - 1) { m_IndexLast = 0; }

			if (m_ItemsAll[m_IndexLast].InUse)
			{
				continue;
			}
			else
			{
				objectPoolItem = m_ItemsAll[m_IndexLast];
				break;
			}
		}

		if (objectPoolItem == null)
		{
			if (m_ItemsAll.Count < m_CountInit)
			{
				//无闲置对象 未超过数量上限 实例化
				objectPoolItem = InstantiateItem();
				m_IndexLast = m_ItemsAll.Count - 1;
			}
			else
			{
				//无闲置对象 超过数量上限
				if (m_CanOverLimitCount) //可 超上限扩容
				{

					objectPoolItem = InstantiateItem();
					Debug.Log($"ObjectPool.Get() >> 对象池扩容 name-{objectPoolItem.Item} curCount-{m_ItemsAll.Count}");
				}
				else //不可 超上限扩容
				{
					if (overLimitUseEarly) //可 复用最早对象
					{
						objectPoolItem = m_ItemsAll[0];
						m_ItemsAll.Remove(objectPoolItem);
						m_ItemsAll.Add(objectPoolItem);
						objectPoolItem.Use();
						Debug.LogWarning($"ObjectPool.Get() >> 对象池用尽 name-{objectPoolItem.Item}  curCount-{m_ItemsAll.Count} 执行复用");

						return objectPoolItem.Item;
					}
					else //不可 复用最早对象
					{
						Debug.LogWarning($"ObjectPool.Get() >> 对象池用尽 name-{m_ItemsAll[m_IndexLast].Item} curCount-{m_ItemsAll.Count} 不执行复用");

						return default(T);
					}
				}
			}
		}

		//移至最后
		m_ItemsAll.Remove(objectPoolItem);
		m_ItemsAll.Add(objectPoolItem);
		//记录使用
		objectPoolItem.Use();
		m_ItemsUsing.Add(objectPoolItem.Item, objectPoolItem);

		return objectPoolItem.Item;
	}

	/// <summary>
	/// 归还 不会销毁对象
	/// </summary>
	/// <param name="instance">归还的对象</param>
	/// <param name="reduceCount">是否减少容量</param>
	public void Return(T instance, bool reduceCount = false)
	{
		ObjectPoolItem<T> objectPoolItem = null;
		if (m_ItemsUsing.TryGetValue(instance, out objectPoolItem))
		{
			//记录 移除
			m_ItemsUsing.Remove(instance);
			//是否 减少容量
			if (reduceCount)
			{
				m_ItemsAll.Remove(objectPoolItem);
			}
			else
			{
				objectPoolItem.Return();
			}
		}
		else
		{
			Debug.LogWarning($"ObjectPool.Return() >> 此对象不属于对象池 name-{instance}");
		}
	}

	/// <summary>
	/// 获取 对象 使用中
	/// </summary>
	/// <returns></returns>
	public Dictionary<T, ObjectPoolItem<T>>.KeyCollection GetUsingInstances()
	{
		return m_ItemsUsing.Keys;
	}

	/// <summary>
	/// 获取 对象 所有
	/// </summary>
	/// <returns></returns>
	public List<ObjectPoolItem<T>> GetAllInstances()
	{
		return m_ItemsAll;
	}

	/// <summary>
	/// 清空 所有对象
	/// </summary>
	public void Clear()
	{
		m_ItemsAll.Clear();
		m_ItemsUsing.Clear();
	}

	/// <summary>
	/// 实例化 对象
	/// </summary>
	/// <returns></returns>
	private ObjectPoolItem<T> InstantiateItem()
	{
		//实例化 对象
		ObjectPoolItem<T> objectPoolItem = new ObjectPoolItem<T>();
		objectPoolItem.Item = m_FuncFactory();
		//记录 对象
		m_ItemsAll.Add(objectPoolItem);

		return objectPoolItem;
	}
}
