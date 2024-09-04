using Deploy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PropInfo
{
	/// <summary>
	/// 道具ID
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// 数量
	/// </summary>
	public int Count { get; set; }

	/// <summary>
	/// 自定义参数
	/// </summary>
	public string Param { get; set; }

	/// <summary>
	/// 配置表
	/// </summary>
	public Prop_Config Config { get; }

    public PropInfo(int itemId, int count)
	{
		Id = itemId;
		Count = count;

        Config = ConfigSystem.Instance.GetConfig<Prop_Config>(Id);
		if (Config == null)
            Debug.LogError($"ItemInfo new Error >> Excel Item_Config not have ID-{Id}");
    }
}

