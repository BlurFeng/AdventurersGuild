using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class PlayerPrefsUtil
{
	#region PlayerPrefsKeys
	/// <summary>
	/// 开发者模式
	/// </summary>
	public static string DEBUG_MODE = "DEBUG_MODE";

	#endregion

	private static string m_Account;

	public static void SetAccount(string account)
	{
		m_Account = account;
	}

	/// <summary>
	/// 设置 Int
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <param name="accountFilter">是否 账号专有数据</param>
	public static void SetInt(string key, int value, bool accountFilter = true)
	{
		if (accountFilter)
		{
			PlayerPrefs.SetInt(m_Account + key, value);
		}
		else
		{
			PlayerPrefs.SetInt(key, value);
		}
	}

	/// <summary>
	/// 获取 Int
	/// </summary>
	/// <param name="key"></param>
	/// <param name="defaultValue"></param>
	/// <param name="accountFilter">是否 账号专有数据</param>
	/// <returns></returns>
	public static int GetInt(string key, int defaultValue = 0, bool accountFilter = true)
	{
		if (accountFilter)
		{
			return PlayerPrefs.GetInt(m_Account + key, defaultValue);
		}
		else
		{
			return PlayerPrefs.GetInt(key, defaultValue);
		}
	}

	/// <summary>
	/// 设置 Float
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <param name="accountFilter">是否 账号专有数据</param>
	public static void SetFloat(string key, float value, bool accountFilter = true)
	{
		if (accountFilter)
		{
			PlayerPrefs.SetFloat(m_Account + key, value);
		}
		else
		{
			PlayerPrefs.SetFloat(key, value);
		}
	}

	/// <summary>
	/// 获取 Float
	/// </summary>
	/// <param name="key"></param>
	/// <param name="defaultValue"></param>
	/// <param name="accountFilter">是否 账号专有数据</param>
	/// <returns></returns>
	public static float GetFloat(string key, float defaultValue = 0, bool accountFilter = true)
	{
		if (accountFilter)
		{
			return PlayerPrefs.GetFloat(m_Account + key, defaultValue);
		}
		else
		{
			return PlayerPrefs.GetFloat(key, defaultValue);
		}
	}

	/// <summary>
	/// 设置 String
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <param name="accountFilter">是否 账号专有数据</param>
	public static void SetString(string key, string value, bool accountFilter = true)
	{
		if (accountFilter)
		{
			PlayerPrefs.SetString(m_Account + key, value);
		}
		else
		{
			PlayerPrefs.SetString(key, value);
		}
	}

	/// <summary>
	/// 获取 String
	/// </summary>
	/// <param name="key"></param>
	/// <param name="defaultValue"></param>
	/// <param name="accountFilter">是否 账号专有数据</param>
	/// <returns></returns>
	public static string GetString(string key, string defaultValue = null, bool accountFilter = true)
	{
		if (accountFilter)
		{
			return PlayerPrefs.GetString(m_Account + key, defaultValue);
		}
		else
		{
			return PlayerPrefs.GetString(key, defaultValue);
		}
	}

	/// <summary>
	/// 保存数据
	/// </summary>
	/// <param name="key">PlayerPrefs key</param>
	/// <param name="data"></param>
	public static void SaveData(string key, object data)
	{
		var dataByte = SerializeData(data);
		SetString(key, dataByte);
	}

	/// <summary>
	/// 读取数据
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="key"> PlayerPrefs key</param>
	/// <returns></returns>
	public static T LoadData<T>(string key) where T : class
	{
		var dataByte = GetString(key);
		if (string.IsNullOrEmpty(dataByte)) { return default(T); }

		return DeserializeData<T>(dataByte);
	}

	/// <summary>
	/// 保存数据
	/// </summary>
	/// <param name="filePath">文件路径</param>
	/// <param name="data"></param>
	public static void SaveDataFilePath(string filePath, object data)
	{
		var dataByte = SerializeData(data);
		using (StreamWriter sw = new StreamWriter(filePath))
		{
			sw.Write(dataByte);
			sw.Flush();
			sw.Close();
		}
	}

	/// <summary>
	/// 读取数据
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="filePath">文件路径</param>
	/// <returns></returns>
	public static T LoadDataFilePath<T>(string filePath) where T : class
	{
		if (!File.Exists(filePath)) { return default(T); }

		using (StreamReader sr = new StreamReader(filePath))
		{
			var dataByte = sr.ReadToEnd();
			sr.Close();
			if (string.IsNullOrEmpty(dataByte)) { return default(T); }

			return DeserializeData<T>(dataByte);
		}
	}

	/// <summary>
	/// 序列化数据
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	public static string SerializeData(object data)
	{
		IFormatter formatter = new BinaryFormatter();
		string dataByte = string.Empty;
		using (MemoryStream stream = new MemoryStream())
		{
			formatter.Serialize(stream, data);
			byte[] byt = new byte[stream.Length];
			byt = stream.ToArray();
			dataByte = Convert.ToBase64String(byt);
			stream.Flush();
		}

		return dataByte;
	}

	/// <summary>
	/// 反序列化数据
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="dataByte"></param>
	/// <returns></returns>
	public static T DeserializeData<T>(string dataByte)
	{
		IFormatter formatter = new BinaryFormatter();
		byte[] byt = Convert.FromBase64String(dataByte);
		object obj = null;
		using (Stream stream = new MemoryStream(byt, 0, byt.Length))
		{
			obj = formatter.Deserialize(stream);
			var classObj = (T)obj;
			stream.Close();

			return classObj;
		}
	}
}
