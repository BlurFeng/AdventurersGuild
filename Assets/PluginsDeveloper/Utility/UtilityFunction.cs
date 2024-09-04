using Deploy;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable once CheckNamespace
public static class UtilityFunction
{
    private static readonly StringBuilder _myStringBuilder = new StringBuilder();

    public static string StringBuilder(params object[] arg)
    {
        _myStringBuilder.Clear();
        foreach (var t in arg) { _myStringBuilder.Append(t); }
        return _myStringBuilder.ToString();
    }

    /// <summary>
    /// 字符串转MD5
    /// </summary>
    /// <param name="inputString"></param>
    /// <returns></returns>
    public static string StringToMD5Hash(string inputString)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] encryptedBytes = md5.ComputeHash(Encoding.Default.GetBytes(inputString));
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < encryptedBytes.Length; i++)
        {
            sb.AppendFormat("{0:x2}", encryptedBytes[i]);
        }
        return sb.ToString();
    }

    private static DateTime m_NormalDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

    /// <summary>
    /// 秒数转换成正常时间显示
    /// </summary>
    /// <param name="sec">秒数</param>
    /// <param name="type">返回类型 1 h:mm:ss 2 mm:ss 3 h:mm</param>
    /// <returns></returns>
    public static string GetTimeBySec(int sec, int type = 1)
    {
        var day = sec % (60 * 60 * 24 * 7) / (60 * 60 * 24);
        var dd = sec / (60 * 60 * 24);
        var hh = sec % (60 * 60 * 24) / (60 * 60);
        var mm = sec % (60 * 60) / 60;
        var ss = sec % 60;

        // string sDay = day.ToString();
        var d = dd.ToString();
        var h = hh.ToString("D2");
        var m = mm.ToString("D2");
        var s = ss.ToString("D2");

        switch (type)
        {
            case 1: return $"{h}:{m}:{s}";
            case 2: return $"{m}:{s}";
            case 3: return $"{h}:{m}";
            case 4:
                return StringBuilder(d, "天", h, "时");
            case 5:
                return StringBuilder(dd, "天", $"{ h}:{ m}:{ s}");
            case 6:
                return day > 0
                    ? StringBuilder(dd, "天", h, "时", m, "分")
                    : StringBuilder(h, "时", m, "分", s, "秒");
            case 7:
                return day > 0
                    ? StringBuilder(dd, "天", $"{ h}:{ m}:{ s}")
                    : StringBuilder($"{ h}:{ m}:{ s}");
            case 8:
                return dd > 0
                    ? StringBuilder(dd, "天", hh, "时", mm, "分")
                    : StringBuilder(hh, "时", mm, "分");
            default: return string.Empty;
        }
    }

    /// <summary>
    /// 秒数转换成 0时0分0秒
    /// </summary>
    /// <param name="seconds">秒数</param>
    /// <returns></returns>
    public static string GetSecondsTimeFormat(uint seconds)
    {
        uint s = seconds % 60; //秒
        uint m = seconds / 60; //分
        uint h = seconds / 3600; //时
        string format = $"{h}时{m}分{s}秒";

        return format;
    }

    /// <summary>
    /// 获取 当前时区日期
    /// </summary>
    /// <param name="time"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetDayTime(int time, int type = 1)
    {
        DateTime startTime = TimeZoneInfo.ConvertTime(m_NormalDateTime, TimeZoneInfo.Local);

        long unixTimeStamp = time;
        var dt = startTime.AddSeconds(unixTimeStamp);
        string format;
        switch (type)
        {
            case 1:
                format = "yyyy-mm-dd";
                break;
            case 2:
                format = "mm-dd";
                break;
            case 3:
                format = "mm月dd日";
                break;
            default:
                format = "yyyy年mm月dd日";
                break;
        }

        return dt.ToString(format);
    }

    /// <summary>
    /// 获取 今日星期
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    private static int GetWeekByNowTime(DateTime dateTime)
    {
        var weekStr = dateTime.DayOfWeek.ToString();
        
        switch (weekStr)
        {
            case "Monday": return 1;
            case "Tuesday": return 2;
            case "Wednesday": return 3;
            case "Thursday": return 4;
            case "Friday": return 5;
            case "Saturday": return 6;
            case "Sunday": return 7;
        }

        return 0;
    }

    /// <summary>
    /// 当前时间戳
    /// </summary>
    /// <returns></returns>
    public static long GetTimestampCurrent()
    {
        TimeSpan ts = DateTime.UtcNow - m_NormalDateTime;
        return Convert.ToInt64(ts.TotalSeconds);
    }

    /// <summary>  
    /// 时间戳转换成日期  
    /// </summary>  
    /// <param name="timeStamp"></param>  
    /// <returns></returns>  
    public static DateTime GetDateTime(long timeStamp)
    {
        DateTime dtStart = TimeZoneInfo.ConvertTime(m_NormalDateTime, TimeZoneInfo.Local);
        long timeStampTicks = timeStamp * TimeSpan.TicksPerSecond;
        TimeSpan toNow = new TimeSpan(timeStampTicks);
        DateTime targetDt = dtStart.Add(toNow);
        return targetDt;
    }

    public static string GetRemainingTime(long durationSeconds)
    {
        string result = string.Empty;
        if (durationSeconds > 0 && durationSeconds < 60)
        {
            result = durationSeconds.ToString() + "秒";
        }
        else if (durationSeconds >= 60 && durationSeconds < 3600)
        {
            result = (durationSeconds / 60).ToString() + "分钟" + (durationSeconds % 60).ToString() + "秒";
        }
        else if (durationSeconds >= 3600 && durationSeconds < 86400)
        {
            result = (durationSeconds / 3600).ToString() + "小时" + ((durationSeconds % 3600) / 60).ToString() + "分钟";
        }
        else if (durationSeconds >= 86400)
        {
            result = (durationSeconds / 86400).ToString() + "天" + ((durationSeconds % 86400) / 3600).ToString() + "小时";
        }

        return result;
    }
}
