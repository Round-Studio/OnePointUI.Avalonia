using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace OnePointUI.Avalonia.Converters
{
    public class ScrollViewerOpacityMaskConverter : IMultiValueConverter
    {
        public double FadeSize { get; set; } = 30.0;

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            // ⚠️OpacityMask 非 null 就会让渲染器 PushOpacityMask 开 SaveLayer,
            // 把层内所有 BackdropEffect(毛玻璃)隔离失效。所以任何"渐隐不起作用"的
            // 场景必须返回 null(等价于无 mask),绝不能返回全不透明笔刷充数。
            if (values.Count >= 3 &&
                values[0] is double offsetY &&
                values[1] is double extentHeight &&
                values[2] is double viewportHeight &&
                viewportHeight > 0)
            {
                double maxScroll = extentHeight - viewportHeight;

                // 内容放得下:任何位置的渐隐都不可能生效,不挂 mask
                if (maxScroll <= 0)
                {
                    return null;
                }

                double topRatio = Math.Min(FadeSize / viewportHeight, 0.5);
                double bottomRatio = Math.Min(FadeSize / viewportHeight, 0.5);

                double topAlphaFactor = Math.Min(1.0, offsetY / FadeSize);

                double remaining = maxScroll - offsetY;
                double bottomAlphaFactor = Math.Min(1.0, Math.Max(0.0, remaining / FadeSize));

                var brush = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop(Color.FromArgb((byte)(255 * (1 - topAlphaFactor)), 0, 0, 0), 0.0),
                        new GradientStop(Colors.Black, topRatio),

                        new GradientStop(Colors.Black, 1.0 - bottomRatio),

                        new GradientStop(Color.FromArgb((byte)(255 * (1 - bottomAlphaFactor)), 0, 0, 0), 1.0)
                    }
                };

                return brush;
            }

            // 数据未就绪(布局前/NaN):不挂 mask
            return null;
        }
    }
}