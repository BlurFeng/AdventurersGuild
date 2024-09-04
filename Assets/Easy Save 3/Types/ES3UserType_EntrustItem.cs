using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("<EntrustItemData>k__BackingField", "<State>k__BackingField", "<RoundsCounter>k__BackingField", "m_RoundsCounter_Underway", "m_VenturerTeam", "m_VenturerNumMax", "EntrustItemData", "State", "RoundsCounter")]
	public class ES3UserType_EntrustItem : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_EntrustItem() : base(typeof(EntrustSystem.EntrustItem)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (EntrustSystem.EntrustItem)obj;
			
			writer.WritePrivateField("<EntrustItemData>k__BackingField", instance);
			writer.WritePrivateField("<State>k__BackingField", instance);
			writer.WritePrivateField("<RoundsCounter>k__BackingField", instance);
			writer.WritePrivateField("m_RoundsCounter_Underway", instance);
			writer.WritePrivateField("m_VenturerTeam", instance);
			writer.WritePrivateField("m_VenturerNumMax", instance);
			writer.WritePrivateProperty("EntrustItemData", instance);
			writer.WritePrivateProperty("State", instance);
			writer.WritePrivateProperty("RoundsCounter", instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (EntrustSystem.EntrustItem)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "<EntrustItemData>k__BackingField":
					instance = (EntrustSystem.EntrustItem)reader.SetPrivateField("<EntrustItemData>k__BackingField", reader.Read<EntrustSystem.EntrustItemData>(), instance);
					break;
					case "<State>k__BackingField":
					instance = (EntrustSystem.EntrustItem)reader.SetPrivateField("<State>k__BackingField", reader.Read<EntrustSystem.EEntrustState>(), instance);
					break;
					case "<RoundsCounter>k__BackingField":
					instance = (EntrustSystem.EntrustItem)reader.SetPrivateField("<RoundsCounter>k__BackingField", reader.Read<System.Int32>(), instance);
					break;
					case "m_RoundsCounter_Underway":
					instance = (EntrustSystem.EntrustItem)reader.SetPrivateField("m_RoundsCounter_Underway", reader.Read<System.Int32>(), instance);
					break;
					case "m_VenturerTeam":
					instance = (EntrustSystem.EntrustItem)reader.SetPrivateField("m_VenturerTeam", reader.Read<System.Collections.Generic.List<System.Int32>>(), instance);
					break;
					case "m_VenturerNumMax":
					instance = (EntrustSystem.EntrustItem)reader.SetPrivateField("m_VenturerNumMax", reader.Read<System.Int32>(), instance);
					break;
					case "EntrustItemData":
					instance = (EntrustSystem.EntrustItem)reader.SetPrivateProperty("EntrustItemData", reader.Read<EntrustSystem.EntrustItemData>(), instance);
					break;
					case "State":
					instance = (EntrustSystem.EntrustItem)reader.SetPrivateProperty("State", reader.Read<EntrustSystem.EEntrustState>(), instance);
					break;
					case "RoundsCounter":
					instance = (EntrustSystem.EntrustItem)reader.SetPrivateProperty("RoundsCounter", reader.Read<System.Int32>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new EntrustSystem.EntrustItem();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_EntrustItemArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_EntrustItemArray() : base(typeof(EntrustSystem.EntrustItem[]), ES3UserType_EntrustItem.Instance)
		{
			Instance = this;
		}
	}
}