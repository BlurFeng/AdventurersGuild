using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace ExcelToProtobuf
{
    class Program
    {
        static void Main(string[] args)
        {
            string exeDir = Directory.GetCurrentDirectory();
            string programDir = Directory.GetParent(Directory.GetParent(exeDir).FullName).FullName;
			string unityDir = Directory.GetParent(Directory.GetParent(programDir).FullName).FullName;
            
			string srcConfigDir = Path.Combine(unityDir, "Config/");
			string srcCsharpDir = Path.Combine(programDir, "Csharp/");
			string destProtoDir = Path.Combine(programDir, "Protos/");
			string destConfigCsharpDir = Path.Combine(unityDir, "Assets/Source/System/ConfigSystem/Config/");
			string destConfigByteDir = Path.Combine(unityDir, "Assets/ProductAssets/Config/");
			string destConfigJsonDir = Path.Combine(unityDir, "Assets/UnProductAssets/Config/");
			string batExcel2ProtoPath = Path.Combine(programDir, "BuildProtos.bat");
			string batCsharp2DllPath = Path.Combine(programDir, "BuildDLL.bat");
			

			if (Directory.Exists(srcConfigDir))
			{
				Console.WriteLine("【Excel转Proto】开始...");
				Excel2Proto.Compiler(Path.Combine(srcConfigDir, "Excel"), destProtoDir);
				Console.WriteLine("【Excel转Proto】结束...\n");

				Console.WriteLine("【Proto拷贝】开始...");
				CopyProto2Tool(Path.Combine(srcConfigDir, "Proto"), destProtoDir);
				Console.WriteLine("【Proto拷贝】结束...\n");

				Console.WriteLine("【Proto转C#】开始...");
				bool isGenerat = GeneratClass.CallProtoc(batExcel2ProtoPath);
				if (isGenerat)
				{
					Console.WriteLine($">> 转换完成 >> {batExcel2ProtoPath}");
				}
				Console.WriteLine("【Proto转C#】结束...\n");

				Console.WriteLine("【C#拷贝至Unity】开始...");
				CopyCs2Unity(srcCsharpDir, destConfigCsharpDir);
				Console.WriteLine("【C#拷贝至Unity】结束...\n");

				if (isGenerat)
				{
					Console.WriteLine("【C#转DLL】开始...");
					bool isCompiler = Compiler2Dll.Compiler(srcCsharpDir, batCsharp2DllPath);
					if (isCompiler)
					{
						Console.WriteLine($">> 转换完成 >> {batCsharp2DllPath}");
					}
					Console.WriteLine("【C#转DLL】结束...\n", ConsoleColor.Green);

					if (isCompiler)
					{
						Console.WriteLine("【序列化保存配置表数据】开始...");
						Excel2Bytes.Compiler(Path.Combine(srcCsharpDir, "ConfigProto.dll"), Path.Combine(srcConfigDir, "Excel"), destConfigByteDir, destConfigJsonDir);
						Console.WriteLine("【序列化保存配置表数据】结束...\n", ConsoleColor.Green);
					}
				}
			}
			else
			{
				Console.WriteLine($"{srcConfigDir}源配置文件夹不存在，请填写正确的文件路径！！！");
			}

            Console.WriteLine("\n流程执行完毕，按任意键退出");
            Console.ReadKey();
        }

		private static void CopyProto2Tool(string srcDir, string DestDir)
		{
			if (!Directory.Exists(srcDir))
			{
				Console.WriteLine($"【Proto拷贝】失败! >> 源文件夹不存在-{srcDir}");
				return;
			}

			if (!Directory.Exists(DestDir))
			{
				Directory.CreateDirectory(DestDir);
			}

			FileInfo[] files = new DirectoryInfo(srcDir).GetFiles();
			foreach (var file in files)
			{
				string fileName = file.Name;
				if (fileName.EndsWith("proto"))
				{
					file.CopyTo(Path.Combine(DestDir, fileName), true);
					Console.WriteLine($">> 拷贝完成 >> {fileName}");
				}
			}
		}

		private static void CopyCs2Unity(string srcDir, string destDir)
		{
			if (!Directory.Exists(srcDir))
			{
				Console.WriteLine($">> 拷贝失败 >> 文件夹不存在-{srcDir}");
				return;
			}

			if (!Directory.Exists(destDir))
			{
				Console.WriteLine($">> 拷贝失败 >> 文件夹不存在-{destDir}");
				return;
			}

			//遍历旧C#文件
			DirectoryInfo destDirInfo = new DirectoryInfo(destDir);
			FileInfo[] destFiles = destDirInfo.GetFiles();
			Dictionary<string, FileInfo> filesMap = new Dictionary<string, FileInfo>();
			foreach (var file in destFiles)
			{
				filesMap.Add(file.Name, file);
			}

			//拷贝新C#文件
			HashAlgorithm hash = HashAlgorithm.Create();

			DirectoryInfo srcDirInfo = new DirectoryInfo(srcDir);
			FileInfo[] srcFiles = srcDirInfo.GetFiles();
			foreach (var newFile in srcFiles)
			{
				string newfileName = newFile.Name;
				if (!newfileName.EndsWith(".cs")) { return; }

				//判断新旧文件是否相同
				bool isSame = false;
				FileInfo oldFile;
				if (filesMap.TryGetValue(newfileName, out oldFile))
				{
					using (FileStream oldFs = new FileStream(oldFile.FullName, FileMode.Open), newFs = new FileStream(newFile.FullName, FileMode.Open))
					{
						byte[] oldHashByte = hash.ComputeHash(oldFs);
						byte[] newHashByte = hash.ComputeHash(newFs);
						string oldHashStr = BitConverter.ToString(oldHashByte);
						string newHashStr = BitConverter.ToString(newHashByte);
						if (oldHashStr.Equals(newHashStr))
						{
							isSame = true;
						}
					}
				}

				if (isSame)
				{
					Console.WriteLine($">> 文件相同 >> {newfileName}");
				}
				else
				{
					newFile.CopyTo(Path.Combine(destDir, newfileName), true);
					Console.WriteLine($">> 拷贝完成 >> {newfileName}");
				}

				filesMap.Remove(newfileName); //已处理文件从记录中移除
			}

			//删除多余或弃用的配置表C#
			foreach (var fileInfo in filesMap.Values)
			{
				string fileName = fileInfo.Name;
				if (fileName.EndsWith(".meta")) { continue; }

				fileInfo.Delete();
				Console.WriteLine($">> 删除弃用 >> {fileName}");
			}
		}
	}
}
