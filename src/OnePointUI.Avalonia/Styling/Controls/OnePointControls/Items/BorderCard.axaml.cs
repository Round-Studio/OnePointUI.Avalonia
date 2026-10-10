using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls.Items;

public class BorderCard : ContentControl
{
    public static readonly StyledProperty<Thickness> InnerMarginProperty =
        AvaloniaProperty.Register<BorderCard, Thickness>(nameof(InnerMargin), new Thickness(1));

    public static readonly StyledProperty<double> BorderOpacityProperty =
        AvaloniaProperty.Register<BorderCard, double>(nameof(BorderOpacity), 1.0);
    

    public Thickness InnerMargin
    {
        get => GetValue(InnerMarginProperty);
        set => SetValue(InnerMarginProperty, value);
    }

    public double BorderOpacity
    {
        get => GetValue(BorderOpacityProperty);
        set => SetValue(BorderOpacityProperty, value);
    }
}