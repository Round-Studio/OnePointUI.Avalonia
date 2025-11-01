using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls.Notice;

public class InfoCard : TemplatedControl
{
    public static readonly StyledProperty<string> GlyphProperty =
        AvaloniaProperty.Register<InfoCard, string>(nameof(Glyph), "");

    public string Glyph
    {
        get => GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }
    
    public static readonly StyledProperty<string> BigTitleProperty =
        AvaloniaProperty.Register<InfoCard, string>(nameof(BigTitle), "Title");

    public string BigTitle
    {
        get => GetValue(BigTitleProperty);
        set => SetValue(BigTitleProperty, value);
    }
    
    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<InfoCard, string>(nameof(Message), "Message");

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
}