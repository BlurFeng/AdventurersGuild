public class ObjectPoolItem<T>
{
	private T item;

	public T Item
	{
		get
		{
			return item;
		}
		set
		{
			item = value;
		}
	}

	/// <summary>
	/// 是否在使用中
	/// </summary>
	public bool InUse { get; private set; }

	/// <summary>
	/// 使用
	/// </summary>
	public void Use()
	{
		InUse = true;
	}

	/// <summary>
	/// 回收
	/// </summary>
	public void Return()
	{
		InUse = false;
	}
}