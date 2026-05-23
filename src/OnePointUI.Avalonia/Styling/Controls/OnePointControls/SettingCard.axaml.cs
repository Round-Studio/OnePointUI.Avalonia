using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives; // 引入 TemplatedControl 相关的命名空间
using Avalonia.Input;
using Avalonia.Media;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls;

public class SettingCard : Button
{
    public static readonly StyledProperty<string> GlyphProperty =
        AvaloniaProperty.Register<SettingCard, string>(nameof(Glyph), "");

    public static readonly StyledProperty<object> HeaderProperty =
        AvaloniaProperty.Register<SettingCard, object>(nameof(Header));

    public static readonly StyledProperty<object> DescriptionProperty =
        AvaloniaProperty.Register<SettingCard, object>(nameof(Description));

    public static readonly StyledProperty<bool> IsClickableProperty =
        AvaloniaProperty.Register<SettingCard, bool>(nameof(IsClickable));

    public static readonly StyledProperty<bool> IsShowActionIconProperty =
        AvaloniaProperty.Register<SettingCard, bool>(nameof(IsShowActionIcon), true);

    public static readonly StyledProperty<bool> IsFontIconProperty =
        AvaloniaProperty.Register<SettingCard, bool>(nameof(IsFontIcon), true);

    public static readonly StyledProperty<bool> IsNotFontIconProperty =
        AvaloniaProperty.Register<SettingCard, bool>(nameof(IsNotFontIcon));

    public static readonly StyledProperty<IImage> ImageIconProperty =
        AvaloniaProperty.Register<SettingCard, IImage>(nameof(ImageIcon));

    public string Glyph
    {
        get => GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public bool IsClickable
    {
        get => GetValue(IsClickableProperty);
        set => SetValue(IsClickableProperty, value);
    }

    public bool IsShowActionIcon
    {
        get => GetValue(IsShowActionIconProperty);
        set => SetValue(IsShowActionIconProperty, value);
    }

    public bool IsFontIcon
    {
        get => GetValue(IsFontIconProperty);
        set
        {
            SetValue(IsFontIconProperty, value);
            SetValue(IsNotFontIconProperty, !value);
        }
    }

    public bool IsNotFontIcon
    {
        get => GetValue(IsNotFontIconProperty);
        set => SetValue(IsNotFontIconProperty, value);
    }

    public IImage? ImageIcon
    {
        get => GetValue(ImageIconProperty);
        set => SetValue(ImageIconProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // 查找模板中的内容控制组件（通过我们在 XAML 中指定的 Name）
        var contentPresenter = e.NameScope.Find<ContentControl>("ActionContentControl");
        if (contentPresenter != null)
        {
            // 订阅指针按下事件，并直接标记为已处理
            contentPresenter.PointerPressed += (sender, args) =>
            {
                args.Handled = true; 
            };
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsFontIconProperty) SetValue(IsNotFontIconProperty, !(bool)change.NewValue!);
    }
}