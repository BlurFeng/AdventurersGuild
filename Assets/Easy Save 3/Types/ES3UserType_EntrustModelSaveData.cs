using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("entrustItems", "guildRank")]
	public class ES3UserType_EntrustModelSaveData : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_EntrustModelSaveData() : base(typeof(EntrustModel.EntrustModelSaveData)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (EntrustModel.EntrustModelSaveData)obj;
			
			writer.WriteProperty("entrustItems", instance.entrustItems, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(EntrustSystem.EntrustItem[])));
			writer.WriteProperty("guildRank", instance.guildRank, ES3Type_int.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (EntrustModel.EntrustModelSaveData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "entrustItems":
						instance.entrustItems = reader.Read<EntrustSystem.EntrustItem[]>();
						break;
					case "guildRank":
						instance.guildRank = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new EntrustModel.EntrustModelSaveData();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_EntrustModelSaveDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_EntrustModelSaveDataArray() : base(typeof(EntrustModel.EntrustModelSaveData[]), ES3UserType_EntrustModelSaveData.Instance)
		{
			Instance = this;
		}
	}
}