using Avalonia.Media;

public static class ColorExtensions
{
    // 颜色变暗
    public static Color Darken(this Color color, double factor)
    {
        factor = Clamp(factor, 0, 1);
        return Color.FromArgb(
            color.A,
            (byte)(color.R * (1 - factor)),
            (byte)(color.G * (1 - factor)),
            (byte)(color.B * (1 - factor)));
    }

    // 颜色变亮
    public static Color Lighten(this Color color, double factor)
    {
        factor = Clamp(factor, 0, 1);
        return Color.FromArgb(
            color.A,
            (byte)(color.R + (255 - color.R) * factor),
            (byte)(color.G + (255 - color.G) * factor),
            (byte)(color.B + (255 - color.B) * factor));
    }

    private static double Clamp(double value, double min, double max)
    {
        return value < min ? min : value > max ? max : value;
    }
}