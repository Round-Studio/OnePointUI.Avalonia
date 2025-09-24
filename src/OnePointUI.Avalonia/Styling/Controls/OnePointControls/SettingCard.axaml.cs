using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls;

public class SettingCard : Button
{
    public static readonly StyledProperty<string> GlyphProperty =
        AvaloniaProperty.Register<SettingCard, string>(nameof(Glyph), "");
    
    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<SettingCard, string>(nameof(Header));
    
    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<SettingCard, string>(nameof(Description));
    
    public static readonly StyledProperty<bool> IsClickableProperty =
        AvaloniaProperty.Register<SettingCard, bool>(nameof(IsClickable));

    public string Glyph
    {
        get => GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }
    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    public bool IsClickable
    {
        get => GetValue(IsClickableProperty);
        set => SetValue(IsClickableProperty, value);
    }
}