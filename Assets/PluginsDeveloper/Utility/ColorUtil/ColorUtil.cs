using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorUtil
{
    /// <summary>
    /// 白色半透明
    /// </summary>
    public static Color WhiteHalfAlpha = new Color(1, 1, 1, 0.5f);

    /// <summary>
    /// 富文本 字体颜色
    /// </summary>
    /// <param name="color"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string RichTextColor(Color color, string content, bool useAlpha = false)
    {
        //转换为RBG十六进制
        string hex;
        if (useAlpha)
        {
            hex = ColorUtility.ToHtmlStringRGBA(color);
        }
        else
        {
            hex = ColorUtility.ToHtmlStringRGB(color);
        }

        return $"<color=#{hex}>{content}</color>";
    }

    /// <summary>
    /// 富文本 字体颜色
    /// </summary>
    /// <param name="hex">RGB十六进制</param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string RichTextColor(string hex, string content)
    {
        //有效性验证
        Color color = Color.white;
        if (!ColorUtility.TryParseHtmlString($"#{hex}", out color))
        {
            hex = "FFFFFF";
        }

        return $"<color=#{hex}>{content}</color>";
    }

    /// <summary>
    /// RGB十六进制 转 Color
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public static Color HexToColor(string hex)
    {
        //有效性验证
        Color color = Color.white;
        ColorUtility.TryParseHtmlString($"#{hex}", out color);

        return color;
    }
}

