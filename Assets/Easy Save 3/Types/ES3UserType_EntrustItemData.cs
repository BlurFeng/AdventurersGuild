using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("isValid", "id", "title", "describeSimple", "describe", "type", "rank", "roundsLimit", "roundsNeedBase", "canTryMultipleTimes", "venturerNumMust", "venturerNumOptional")]
	public class ES3UserType_EntrustItemData : ES3Type
	{
		public static ES3Type Instance = null;

		public ES3UserType_EntrustItemData() : base(typeof(EntrustSystem.EntrustItemData)){ Instance = this; priority = 1;}


		public override void Write(object obj, ES3Writer writer)
		{
			var instance = (EntrustSystem.EntrustItemData)obj;
			
			writer.WriteProperty("isValid", instance.isValid, ES3Type_bool.Instance);
			writer.WriteProperty("id", instance.nameId, ES3Type_int.Instance);
			writer.WriteProperty("title", instance.title, ES3Type_string.Instance);
			writer.WriteProperty("describeSimple", instance.describeSimple, ES3Type_string.Instance);
			writer.WriteProperty("describe", instance.describe, ES3Type_string.Instance);
			writer.WriteProperty("type", instance.type, ES3Type_int.Instance);
			writer.WriteProperty("rank", instance.rank, ES3Type_int.Instance);
			writer.WriteProperty("roundsLimit", instance.roundsLimit, ES3Type_int.Instance);
			writer.WriteProperty("roundsNeedBase", instance.roundsNeedBase, ES3Type_int.Instance);
			writer.WriteProperty("canTryMultipleTimes", instance.canTryMultipleTimes, ES3Type_bool.Instance);
			writer.WriteProperty("venturerNumMust", instance.venturerNumMust, ES3Type_int.Instance);
			writer.WriteProperty("venturerNumOptional", instance.venturerNumOptional, ES3Type_int.Instance);
		}

		public override object Read<T>(ES3Reader reader)
		{
			var instance = new EntrustSystem.EntrustItemData();
			string propertyName;
			while((propertyName = reader.ReadPropertyName()) != null)
			{
				switch(propertyName)
				{
					
					case "isValid":
						instance.isValid = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					case "id":
						instance.nameId = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "title":
						instance.title = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "describeSimple":
						instance.describeSimple = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "describe":
						instance.describe = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "type":
						instance.type = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "rank":
						instance.rank = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "roundsLimit":
						instance.roundsLimit = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "roundsNeedBase":
						instance.roundsNeedBase = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "canTryMultipleTimes":
						instance.canTryMultipleTimes = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					case "venturerNumMust":
						instance.venturerNumMust = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "venturerNumOptional":
						instance.venturerNumOptional = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
			return instance;
		}
	}


	public class ES3UserType_EntrustItemDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_EntrustItemDataArray() : base(typeof(EntrustSystem.EntrustItemData[]), ES3UserType_EntrustItemData.Instance)
		{
			Instance = this;
		}
	}
}