using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls;

public class LineTextBlock : TemplatedControl
{
    public static readonly StyledProperty<string> TextProperty =  AvaloniaProperty.Register<LineTextBlock, string>(nameof(Text));
    public string Text 
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}