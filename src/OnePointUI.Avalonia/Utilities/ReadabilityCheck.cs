using Avalonia.Media;
namespace OnePointUI.Avalonia.Utilities;

public static class ReadabilityCheck
{
    /// <summary>
    /// DM: 手写函数，很唐。
    /// 该函数传入一个 Avalonia.Media.Color 类型，返回 bool 值。
    /// 该参数为文本背景色，该函数判断文本前景颜色为黑色时，在参数背景色下的可读性是否达标并返回对应 bool 值。
    /// </summary>
    /// <param name="textBackgroundColor">目标文本背景色</param>
    /// <returns></returns>
    public static bool BlackTextReadabilityCheck(this Color textBackgroundColor)
    {
        var r = textBackgroundColor.R / 255.0;
        var  g = textBackgroundColor.G / 255.0;
        var  b = textBackgroundColor.B / 255.0;

        r = (r <= 0.03928) ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
        g = (g <= 0.03928) ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
        b = (b <= 0.03928) ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);
        
        var  backgroundLuminance = 0.2126 * r + 0.7152 * g + 0.0722 * b;
        
        var  lighter = Math.Max(backgroundLuminance, 0.0);
        var  darker  = Math.Min(backgroundLuminance, 0.0);
        var  contrastRatio = (lighter + 0.05) / (darker + 0.05);

        return contrastRatio >= 4.5;
    }
}