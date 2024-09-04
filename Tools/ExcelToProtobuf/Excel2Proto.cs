using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExcelToProtobuf
{
    public class Excel2Proto //excel转.proto
    {
        private static string m_DestPathDirProto = string.Empty;
        private static string m_ContentFormat = "syntax = \"proto3\";\npackage deploy;\n\nmessage fileName\n{\ncontent}\n\nmessage fileName_Map\n{\n\tmap<int32, fileName> Items = 1;\n}\n";

        private static string m_SrcPathDirExcel = string.Empty;

        //创建客户端Proto文件
        public static void Compiler(string srcPathDirExcel, string destPathDirProto)
        {
            m_SrcPathDirExcel = srcPathDirExcel;

            if (!Directory.Exists(m_SrcPathDirExcel))
            {
                Console.WriteLine($">> 转换失败 >> 文件夹不存在-{m_SrcPathDirExcel}");
                return;
            }

            m_DestPathDirProto = destPathDirProto;

            //清空旧Proto文件
            if (Directory.Exists(m_DestPathDirProto))
            {
                FileInfo[] files = new DirectoryInfo(m_DestPathDirProto).GetFiles();
                foreach (var file in files)
                {
                    file.Delete();
                }
            }
            else
            {
                Directory.CreateDirectory(m_DestPathDirProto);
            }

            //生产新Proto文件
            string[] paths = Directory.GetFiles(m_SrcPathDirExcel, "*.xlsx", SearchOption.AllDirectories);
            for (int i = 0; i < paths.Length; i++)
            {
                CompilerProto(paths[i]);
            }
        }

        //读取Excel文件数据 生成Proto文件
        private static void CompilerProto(string inPathExcel)
        {
            if (Path.GetFileName(inPathExcel).StartsWith("~$")) return;

            using (FileStream fs = new FileStream(inPathExcel, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IWorkbook workbook = new XSSFWorkbook(fs);

                if (workbook != null)
                {
                    int sheetNums = workbook.NumberOfSheets;
                    for (int i = 0; i < sheetNums; i++)
                    {
                        ISheet sheet = workbook.GetSheetAt(i);
                        CreateProtoFile(sheet);
                    }
                }
            }
        }

        //创建.proto文件
        private static void CreateProtoFile(ISheet sheet)
        {
			List<string> repeatedList = new List<string>(); //已经处理过的数组
            StringBuilder stringBuilder = new StringBuilder();

            int Nums = sheet.LastRowNum;
            if (Nums >= 4) //加上结束标志行大于规则数量才算是有效的表
            {
                IRow row = sheet.GetRow(0);
                int cellNum = row.LastCellNum;
                int index = 0;
                for (int i = 0; i < cellNum; i++)
                {
                    if (row.GetCell(i).StringCellValue == "#") continue; // 客户端、服务器所需的字段不一样，筛选条件也不一样，自己做调整
                    if (row.GetCell(i).StringCellValue != "__END__")
                    {
                        string typeName = GetProtoType(sheet.GetRow(3).GetCell(i).StringCellValue.Trim()); // 类型名字
                        string valName = sheet.GetRow(1).GetCell(i).StringCellValue; // 字段名字

						//是否为repeated
						if (valName.Contains("_"))
						{
							valName = valName.Split('_')[0];
							if (repeatedList.Contains(valName))
							{
								continue;
							}
							else
							{
								repeatedList.Add(valName);
								typeName = $"repeated {typeName}";
							}
						}

                        index++;
                        stringBuilder.Append($"\t{typeName}");
                        stringBuilder.Append($"\t{valName}");
                        stringBuilder.Append($"\t=\t{index};\n");
                    }
                    else
                    {
                        break;
                    }
                }

                if (stringBuilder.Length > 0 && Directory.Exists(m_DestPathDirProto))
                {
                    string proto = m_ContentFormat.Replace("fileName", sheet.SheetName);
                    proto = proto.Replace("content", stringBuilder.ToString());
                    string outPathFile = Path.Combine(m_DestPathDirProto, sheet.SheetName) + ".proto"; //生成的proto文件路径

                    using (FileStream fs = new FileStream(outPathFile, FileMode.Append, FileAccess.Write))
                    {
                        byte[] datas = Encoding.Default.GetBytes(proto);
                        fs.Write(datas, 0, datas.Length);
                    }

					Console.WriteLine($">> 转换完成 >> {sheet.SheetName}");
                }
            }
        }

        //获取proto3类型
        private static string GetProtoType(string typeName)
        {
            switch (typeName)
            {
                case "int":
                case "key":
                    return "int32";
                case "intArray":
                    return "repeated int32";
                case "string":
                    return "string";
                case "stringArray":
                    return "repeated string";
                case "float":
                    return "float";
                case "floatarray":
                    return "repeated float";
                case "map<int,int>":
                    return "map<int32,int32>";
                case "map<int,string>":
                    return "map<int32,string>";
                case "map<string,string>":
                    return "map<string,string>";
                case "map<string,int>":
                    return "map<string,int32>";
                default:
                    Console.WriteLine($">> 转换失败 >> 数据类型未定义-{typeName} 配置表-{m_SrcPathDirExcel}");
                    Console.ReadKey();
                    return typeName;
            }
        }
    }
}
