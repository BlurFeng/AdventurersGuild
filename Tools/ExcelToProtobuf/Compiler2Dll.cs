using System;
using System.Diagnostics;
using System.IO;

namespace ExcelToProtobuf
{
    // .cs文件转dll 
    public class Compiler2Dll
    {
        // 目标文件夹、输出文件夹
        public static bool Compiler(string csPath, string batPath)
        {
            if (!Directory.Exists(csPath))
            {
                Console.WriteLine($">> 转换失败 >> 需要编译的C#文件夹不存在-{csPath}");
                return false;
            }

			// 调用CMD编译cs文件 --> dll
			try
			{
                Process proc = new Process();
                proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(batPath);
                proc.StartInfo.FileName = Path.GetFileName(batPath);
                proc.Start();
                proc.WaitForExit();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($">> 转换失败 >> BuildDLL.bat调用异常：{e.ToString()}");
                return false;
            }
        }
    }
}
