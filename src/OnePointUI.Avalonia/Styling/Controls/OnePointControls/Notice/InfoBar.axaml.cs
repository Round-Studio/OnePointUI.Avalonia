using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls.Notice;

public class InfoBar : TemplatedControl
{
    public enum InfoBarType
    {
        Error,
        Warning,
        Info,
        Success
    }
    
    public static readonly StyledProperty<string> GlyphProperty =
        AvaloniaProperty.Register<InfoBar, string>(nameof(Glyph), "");

    public string Glyph
    {
        get => GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }
    
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<InfoBar, string>(nameof(Title), "Title");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<InfoBar, string>(nameof(Message), "Message");

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
    
    public static readonly StyledProperty<InfoBarType> StateProperty =
        AvaloniaProperty.Register<InfoBar, InfoBarType>(nameof(State), InfoBarType.Info);

    public InfoBarType State
    {
        get => GetValue(StateProperty);
        set
        {
            SetValue(StateProperty, value);
            SetValue(BackcolorProperty, GetColor());
        }
    }

    public static readonly StyledProperty<IBrush> BackcolorProperty =
        AvaloniaProperty.Register<InfoBar, IBrush>(nameof(Backcolor));

    public IBrush Backcolor
    {
        get => GetValue(BackcolorProperty);
        set => SetValue(BackcolorProperty, value); // 即使内部不存储，也需要setter
    }

    public IBrush GetColor() => State switch
    {
        InfoBarType.Error => Brushes.DarkRed,
        InfoBarType.Warning => Brushes.Orange,
        InfoBarType.Info => Brushes.Transparent,
        InfoBarType.Success => Brushes.Green
    };
}