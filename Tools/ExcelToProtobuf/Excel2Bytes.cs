using Google.Protobuf;
using Google.Protobuf.Collections;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;

namespace ExcelToProtobuf
{
    // excel数据转二进制数据
    public class Excel2Bytes
    {
        private static string m_ExcelFilePath = string.Empty;
		private static string m_CfgBytesPathDir = string.Empty;
		private static string m_CfgTxtPathDir = string.Empty;
		private static Assembly m_ConfigDllAssembly;

		public static void Compiler(string srcDllPathFile, string srcExcelPathDir, string destCfgBytesPathDir, string destCfgTxtPathDir)
        {
            if (!File.Exists(srcDllPathFile))
            {
                Console.WriteLine($">> 转换失败 >> 源dll文件路径不存在-{srcDllPathFile}");
                return;
            }

            if (!Directory.Exists(srcExcelPathDir))
            {
                Console.WriteLine($">> 转换失败 >> 源Excel配置文件夹路径不存在-{srcExcelPathDir}");
                return;
            }

            if (!Directory.Exists(destCfgBytesPathDir))
            {
                Console.WriteLine($">> 转换失败 >> 目标配置二进制文件夹路径不存在-{destCfgBytesPathDir}");
                return;
            }

            if (!Directory.Exists(destCfgTxtPathDir))
            {
                Console.WriteLine($">> 转换失败 >> 源配置Json文件夹路径不存在-{destCfgTxtPathDir}");
                return;
            }

            m_ConfigDllAssembly = Assembly.LoadFile(srcDllPathFile);

			m_CfgBytesPathDir = destCfgBytesPathDir;
			m_CfgTxtPathDir = destCfgTxtPathDir;
			//清空旧Config二进制文件
			if (Directory.Exists(m_CfgBytesPathDir))
            {
				DirectoryInfo dir = new DirectoryInfo(m_CfgBytesPathDir);
				if (dir.Exists)
				{
					FileInfo[] files = dir.GetFiles();
					foreach (var file in files)
					{
                        if (file.Name.EndsWith(".meta")) { continue; }

                        file.Delete();
					}
				}
            }
			//清空旧Config文本文件
			if (Directory.Exists(m_CfgTxtPathDir))
			{
				DirectoryInfo dir = new DirectoryInfo(m_CfgTxtPathDir);
				if (dir.Exists)
				{
					FileInfo[] files = dir.GetFiles();
					foreach (var file in files)
					{
                        if (file.Name.EndsWith(".meta")) { continue; }

                        file.Delete();
					}
				}
			}

            string[] excelFilePaths = Directory.GetFiles(srcExcelPathDir, "*.xlsx", SearchOption.AllDirectories);
            for (int i = 0; i < excelFilePaths.Length; i++)
            {
                OpenExcel(excelFilePaths[i]);
            }
        }

        private static void OpenExcel(string filePath)
        {
            if (Path.GetFileName(filePath).StartsWith("~$")) return;

            m_ExcelFilePath = filePath;
            using (FileStream fs = new FileStream(m_ExcelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IWorkbook workbook = new XSSFWorkbook(fs);

                if (workbook != null)
                {
                    int sheetNums = workbook.NumberOfSheets;
                    for (int i = 0; i < sheetNums; i++)
                    {
                        ISheet sheet = workbook.GetSheetAt(i);
                        WriteData(sheet);
                    }
                }
            }
        }

        // 三个文件：序列化后二进制文件、位置文件、明文数据
        private static void WriteData(ISheet sheet)
        {
			List<string> repeatedList = new List<string>(); //已经处理过的数组
			int Nums = sheet.LastRowNum; // 行数
            if (Nums > 4)
            {
				object configIns = m_ConfigDllAssembly.CreateInstance("Deploy." + sheet.SheetName + "_Map"); // 创建数据容器类
                if (configIns == null)
                {
                    Console.WriteLine($">> 转换失败 >> 可能存在重复的Excel表Sheet名称！ExcelPath-{m_ExcelFilePath} SheetName-{sheet.SheetName}");
                    return;
                }

                Type configType = configIns.GetType();
                PropertyInfo propertyInfo = configType.GetProperty("Items"); // 取到容器字段
                object ItemsVal = propertyInfo.GetValue(configIns); // 取到容器

                IRow row = sheet.GetRow(0); // 第一行数据
                int cellNum = row.LastCellNum;

                List<int> validColIndex = new List<int>(); //有效列 #是注释列
                for (int i = 0; i < cellNum; i++)
                {
                    if (row.GetCell(i).StringCellValue == "#") continue;
                    if (row.GetCell(i).StringCellValue != "__END__")
                    {
                        validColIndex.Add(i);
                    }
                    else
                    {
                        break;
                    }
                }

                IRow nameRow = sheet.GetRow(1); // 字段行数据
                IRow typeRow = sheet.GetRow(3); // 类型行数据
                for (int i = 4; i < Nums; i++)
                {
                    IRow rowData = sheet.GetRow(i);
                    if (rowData.GetCell(0).ToString() == "__END__") { break; }  // 结束标志放在第一列 

                    //检查并记录ID
                    int id = 0;
                    var idCell = rowData.GetCell(validColIndex[0]);
                    string idString = idCell != null ? idCell.ToString().Trim() : string.Empty;
                    if (string.IsNullOrEmpty(idString)) { continue; }
                    id = int.Parse(idString);
                    
					object dataIns = m_ConfigDllAssembly.CreateInstance("Deploy." + sheet.SheetName); // 数据实例
                    for (int j = 0; j < validColIndex.Count; j++)
                    {
                        int index = validColIndex[j];
                        var typeCell = typeRow.GetCell(index);
                        string type = typeCell != null ? typeCell.ToString().Trim() : string.Empty;
                        var fieldNameCell = nameRow.GetCell(index);
                        string fieldName = fieldNameCell != null ? fieldNameCell.ToString().Trim() : string.Empty;
                        var valueCell = rowData.GetCell(index);
                        string value = valueCell != null ? valueCell.ToString().Trim() : string.Empty;

						//是否为repeated数据
						if (fieldName.Contains("_"))
						{
							fieldName = fieldName.Split('_')[0];
							if (repeatedList.Contains(fieldName))
							{
								continue;
							}
							else
							{
								repeatedList.Add(fieldName);
								type = $"{type}Array";
								value = $"[ {value}";
								//遍历获取所有repeated数据
								for (int k = 0; k < validColIndex.Count; k++)
								{
									string fieldNameRpt = nameRow.GetCell(validColIndex[k]) != null ? nameRow.GetCell(validColIndex[k]).ToString() : string.Empty;
									if (fieldNameRpt.Contains("_") && fieldNameRpt.StartsWith(fieldName))
									{
										string valueRpt = rowData.GetCell(validColIndex[k]) != null ? rowData.GetCell(validColIndex[k]).ToString() : string.Empty;
										value = $"{value},{valueRpt}";
									}
								}
								value = $"{value} ]";
							}
						}

						if (value.Length > 0)
						{
							//采用字段不使用属性是因为repeated没有set方法
							string first = fieldName.First().ToString().ToLower();
							string sub = fieldName.Substring(1);
							FieldInfo fieldInfo = dataIns.GetType().GetField($"{first}{sub}_", BindingFlags.NonPublic | BindingFlags.Instance);
                            try
                            {
                                object realVal = GetRealVal(type, value); //获取真实值
                                fieldInfo.SetValue(dataIns, realVal);
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine($">> 转换失败 >> 值或类型不合法 filePath-{m_ExcelFilePath} value-{value} type-{type}");
                            }
						}
					}

					//Add进Map
					MethodInfo addMethod = propertyInfo.PropertyType.GetMethod("Add", new Type[] { typeof(int), dataIns.GetType() }); // 获取容器Add方法,需要标明具体添加哪种类型
                    try
                    {
                        addMethod?.Invoke(ItemsVal, new[] { id, dataIns });
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine($">> 转换失败 >> 重复的ID filePath-{m_ExcelFilePath} ID-{id}");
                    }
				}
				SaveCfgSerializeFile(configIns, sheet.SheetName);
				SaveCfgTxtFile(configIns, sheet.SheetName);
            }
        }

        // 根据类型获取真实值
        //  *********** 根据自己的填表规则进行修改 ***********
        private static object GetRealVal(string type, string value)
        {
            switch (type)
            {
                case "string":
                    return value;
                case "stringArray":
                    return HandlerArray(value, (sVal) => { return sVal.Trim(); });
                case "int":
                    return int.Parse(value);
                case "intArray":
                    return HandlerArray(value, (sVal) => { return int.Parse(sVal); });
                case "float":
                    return float.Parse(value);
                case "floatArray":
                    return HandlerArray(value, (sVal) => { return float.Parse(sVal); });
                case "map<int,int>":
                    return HandlerMap(value, (sVal1) => { return int.Parse(sVal1); }, (sVal2) => { return int.Parse(sVal2); });
                case "map<int,string>":
                    return HandlerMap(value, (sVal1) => { return int.Parse(sVal1); }, (sVal2) => { return sVal2.Trim(); });
                case "map<string,string>":
                    return HandlerMap(value, (sVal1) => { return sVal1.Trim(); }, (sVal2) => { return sVal2.Trim(); });
                case "map<string,int>":
                    return HandlerMap(value, (sVal1) => { return sVal1.Trim(); }, (sVal2) => { return int.Parse(sVal2); });
                default:
                    Console.WriteLine($">> 转换失败 >> 类型不合法 filePath-{m_ExcelFilePath} type-{type}");
                    return value;
            }
        }

        // 返回数组，[]包裹 ,分隔
        private static RepeatedField<T> HandlerArray<T>(string value, Func<string,T> func)
        {
            if (null == value) { return null; }

            string val = value.TrimStart('[');
            val = val.TrimEnd(']');
            string[] datas = val.Split(',');
            RepeatedField<T> valArray = new RepeatedField<T>();
            for (int i = 0; i < datas.Length; i++)
            {
                valArray.Add(func(datas[i]));
            }
            return valArray;
        }

        // 返回Map，{}包裹 ,分隔
        private static MapField<T1, T2> HandlerMap<T1, T2>(string value, Func<string, T1> func1, Func<string, T2> func2)
        {
            if (null == value) { return null; }

            string val = value.TrimStart('{');
            val = val.TrimEnd('}');
            string[] datas = val.Split(',');
            MapField<T1, T2> valMap = new MapField<T1, T2>();
            for (int i = 0; i < datas.Length; i++)
            {
                string[] sval = datas[i].Split(':');
                T1 k = func1(sval[0]);
                T2 v = func2(sval[1]);
                valMap.Add(k, v);
            }
            return valMap;
        }

        // 序列化后二进制文件
        private static void SaveCfgSerializeFile(object obj, string sheetName)
        {
            string fileName = $"{sheetName}.bytes";
            string filePath = Path.Combine(m_CfgBytesPathDir, fileName);
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                MessageExtensions.WriteTo(obj as IMessage, fs);
				Console.WriteLine($">> 转换完成 >> {fileName}");
			}
        }

        // 明文数据
        private static void SaveCfgTxtFile(object obj, string sheetName)
        {
            string fileName = sheetName + ".txt";
            using (FileStream fs = new FileStream(Path.Combine(m_CfgTxtPathDir, fileName), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                string content = obj.ToString();
                byte[] datas = Encoding.Default.GetBytes(content);
                byte[] newDatas = Encoding.Convert(Encoding.Default, Encoding.UTF8, datas); // 不转换一下就出现乱码的情况 
                fs.Write(newDatas, 0, newDatas.Length);
            }
        }
    }
}
