using System.Runtime.InteropServices.JavaScript;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls;

public class SettingCard : ContentControl
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

    public event EventHandler<RoutedEventArgs> Click;

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

    private Border _rootBorder;
    private bool _isPointerPressed;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _rootBorder = e.NameScope.Find<Border>("PART_Root");
        if (_rootBorder != null)
        {
            _rootBorder.PointerPressed += RootBorder_PointerPressed;
            _rootBorder.PointerReleased += RootBorder_PointerReleased;
            _rootBorder.PointerCaptureLost += RootBorder_PointerCaptureLost;
        }

        var contentPresenter = e.NameScope.Find<ContentControl>("ActionContentControl");
        if (contentPresenter != null)
        {
            contentPresenter.PointerPressed += (sender, args) => { args.Handled = true; };
            contentPresenter.PointerReleased += (sender, args) => { args.Handled = true; };
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsFontIconProperty) SetValue(IsNotFontIconProperty, !(bool)change.NewValue!);
    }

    private void RootBorder_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _isPointerPressed = true;
        e.Pointer.Capture(_rootBorder);
    }

    private void RootBorder_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (!_isPointerPressed)
            return;

        _isPointerPressed = false;

        var pos = e.GetPosition(this);
        var bounds = new Rect(Bounds.Size);
        if (!bounds.Contains(pos))
            return;

        if (!IsClickable)
            return;

        Click?.Invoke(this, new RoutedEventArgs());
        e.Handled = true;
    }

    private void RootBorder_PointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
    {
        _isPointerPressed = false;
    }
}