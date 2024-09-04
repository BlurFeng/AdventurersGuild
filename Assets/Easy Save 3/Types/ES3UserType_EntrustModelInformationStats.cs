using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("entrustItemCountDic_Complete", "entrustItemCountDic_Succeed", "entrustItemTotal_Complete", "entrustItemTotal_Succeed", "entrustItemCountDic_Complete_Year", "entrustItemCountDic_Succeed_Year", "entrustItemTotal_Complete_Year", "entrustItemTotal_Succeed_Year")]
	public class ES3UserType_EntrustModelInformationStats : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_EntrustModelInformationStats() : base(typeof(EntrustModel.EntrustModelInformationStats)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (EntrustModel.EntrustModelInformationStats)obj;
			
			writer.WriteProperty("entrustItemCountDic_Complete", instance.entrustItemCountDic_Complete, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.Dictionary<System.Int32, System.Int32>)));
			writer.WriteProperty("entrustItemCountDic_Succeed", instance.entrustItemCountDic_Succeed, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.Dictionary<System.Int32, System.Int32>)));
			writer.WriteProperty("entrustItemTotal_Complete", instance.entrustItemTotal_Complete, ES3Type_int.Instance);
			writer.WriteProperty("entrustItemTotal_Succeed", instance.entrustItemTotal_Succeed, ES3Type_int.Instance);
			writer.WriteProperty("entrustItemCountDic_Complete_Year", instance.entrustItemCountDic_Complete_Year, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.Dictionary<System.Int32, System.Int32>)));
			writer.WriteProperty("entrustItemCountDic_Succeed_Year", instance.entrustItemCountDic_Succeed_Year, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.Dictionary<System.Int32, System.Int32>)));
			writer.WriteProperty("entrustItemTotal_Complete_Year", instance.entrustItemTotal_Complete_Year, ES3Type_int.Instance);
			writer.WriteProperty("entrustItemTotal_Succeed_Year", instance.entrustItemTotal_Succeed_Year, ES3Type_int.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (EntrustModel.EntrustModelInformationStats)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "entrustItemCountDic_Complete":
						instance.entrustItemCountDic_Complete = reader.Read<System.Collections.Generic.Dictionary<System.Int32, System.Int32>>();
						break;
					case "entrustItemCountDic_Succeed":
						instance.entrustItemCountDic_Succeed = reader.Read<System.Collections.Generic.Dictionary<System.Int32, System.Int32>>();
						break;
					case "entrustItemTotal_Complete":
						instance.entrustItemTotal_Complete = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "entrustItemTotal_Succeed":
						instance.entrustItemTotal_Succeed = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "entrustItemCountDic_Complete_Year":
						instance.entrustItemCountDic_Complete_Year = reader.Read<System.Collections.Generic.Dictionary<System.Int32, System.Int32>>();
						break;
					case "entrustItemCountDic_Succeed_Year":
						instance.entrustItemCountDic_Succeed_Year = reader.Read<System.Collections.Generic.Dictionary<System.Int32, System.Int32>>();
						break;
					case "entrustItemTotal_Complete_Year":
						instance.entrustItemTotal_Complete_Year = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "entrustItemTotal_Succeed_Year":
						instance.entrustItemTotal_Succeed_Year = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new EntrustModel.EntrustModelInformationStats();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_EntrustModelInformationStatsArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_EntrustModelInformationStatsArray() : base(typeof(EntrustModel.EntrustModelInformationStats[]), ES3UserType_EntrustModelInformationStats.Instance)
		{
			Instance = this;
		}
	}
}