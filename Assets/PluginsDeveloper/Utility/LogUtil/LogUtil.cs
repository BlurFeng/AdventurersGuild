using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LogUtil
{
    static ILogger debug = Debug.unityLogger;

    static public void InitLogger()
    {
        debug = Debug.unityLogger;
        debug.filterLogType = LogType.Log;
        debug.logEnabled = true;
    }

    static public void Log(string str)
    {
        debug.Log(ColorUtil.RichTextColor(Color.white, str));
    }

    static public void Log(string str, Color color)
    {
        debug.Log(ColorUtil.RichTextColor(color, str));
    }

    static public void Log(string str, string str2, Color color)
    {
        debug.Log(ColorUtil.RichTextColor(color, str + str2));
    }

    static public void LogNetFormat(string strFormat, string str1, string str2, string str3, Color color)
    {
        debug.Log(ColorUtil.RichTextColor(color, string.Format(strFormat, str1, str2, str3)));
    }

    static public void LogNetFormat(string strFormat, string str1, string str2, string str3, string str4, Color color)
    {
        debug.Log(ColorUtil.RichTextColor(color, string.Format(strFormat, str1, str2, str3, str4)));
    }
}
