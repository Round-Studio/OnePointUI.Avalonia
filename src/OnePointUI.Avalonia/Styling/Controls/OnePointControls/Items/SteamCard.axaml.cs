using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls.Items;

public class SteamCard : ContentControl
{
    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<SteamCard, IImage?>(nameof(Source));

    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<SteamCard, object?>(nameof(Title));

    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<SteamCard, Stretch>(nameof(Stretch), Stretch.UniformToFill);

    public static readonly StyledProperty<double> CardCornerRadiusProperty =
        AvaloniaProperty.Register<SteamCard, double>(nameof(CardCornerRadius), 6.0);

    public static readonly StyledProperty<double> HoverScaleProperty =
        AvaloniaProperty.Register<SteamCard, double>(nameof(HoverScale), 1.5);

    public static readonly StyledProperty<double> ParallaxDepthProperty =
        AvaloniaProperty.Register<SteamCard, double>(nameof(ParallaxDepth), 1.0);

    public static readonly StyledProperty<double> RotateXProperty =
        AvaloniaProperty.Register<SteamCard, double>(nameof(RotateX));

    public static readonly StyledProperty<double> RotateYProperty =
        AvaloniaProperty.Register<SteamCard, double>(nameof(RotateY));

    public static readonly StyledProperty<double> ScaleValueProperty =
        AvaloniaProperty.Register<SteamCard, double>(nameof(ScaleValue), 1.0);

    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public double CardCornerRadius
    {
        get => GetValue(CardCornerRadiusProperty);
        set => SetValue(CardCornerRadiusProperty, value);
    }

    public double HoverScale
    {
        get => GetValue(HoverScaleProperty);
        set => SetValue(HoverScaleProperty, value);
    }

    public double ParallaxDepth
    {
        get => GetValue(ParallaxDepthProperty);
        set => SetValue(ParallaxDepthProperty, value);
    }

    public double RotateX
    {
        get => GetValue(RotateXProperty);
        set => SetValue(RotateXProperty, value);
    }

    public double RotateY
    {
        get => GetValue(RotateYProperty);
        set => SetValue(RotateYProperty, value);
    }

    public double ScaleValue
    {
        get => GetValue(ScaleValueProperty);
        set => SetValue(ScaleValueProperty, value);
    }

    private const double MaxRotationAngle = 6.0;
    private const double EdgeCompensation = 0.05;

    private readonly Rotate3DTransform _rotateTransform;
    private readonly ScaleTransform _scaleTransform;
    private bool _isHovered;

    public SteamCard()
    {
        RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);

        _rotateTransform = new Rotate3DTransform { Depth = 800 };
        _scaleTransform = new ScaleTransform();

        RenderTransform = new TransformGroup
        {
            Children = { _rotateTransform, _scaleTransform }
        };

        ClipToBounds = false;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == RotateXProperty)
            _rotateTransform.AngleX = change.GetNewValue<double>();
        else if (change.Property == RotateYProperty)
            _rotateTransform.AngleY = change.GetNewValue<double>();
        else if (change.Property == ScaleValueProperty)
        {
            double s = change.GetNewValue<double>();
            _scaleTransform.ScaleX = s;
            _scaleTransform.ScaleY = s;
        }
        else if (change.Property == ParallaxDepthProperty)
        {
            // 可选：当视差深度改变时，可以重新计算当前变换
            if (_isHovered)
            {
                // 触发热更新 - 可以选择重新应用当前鼠标位置的效果
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (TopLevel.GetTopLevel(this) is Window window)
        {
            window.PointerMoved += OnGlobalPointerMoved;
            window.PointerExited += OnGlobalPointerExited;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (TopLevel.GetTopLevel(this) is Window window)
        {
            window.PointerMoved -= OnGlobalPointerMoved;
            window.PointerExited -= OnGlobalPointerExited;
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _isHovered = true;
        ZIndex = 10000;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _isHovered = false;
        ResetTransform();
    }

    private void OnGlobalPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isHovered) return;

        var bounds = Bounds;
        if (!IsLoaded || bounds.Width <= 0 || bounds.Height <= 0) return;

        var pos = e.GetPosition(this);
        double percentX = (pos.X / bounds.Width - 0.5) * 2.0;
        double percentY = (pos.Y / bounds.Height - 0.5) * 2.0;

        if (Math.Abs(percentX) <= 1.1 && Math.Abs(percentY) <= 1.1)
        {
            // 使用视差深度值乘以最大旋转角度
            double effectiveMaxAngle = MaxRotationAngle * ParallaxDepth;
            RotateY = percentX * effectiveMaxAngle;
            RotateX = -percentY * effectiveMaxAngle;

            double intensity = Math.Max(Math.Abs(percentX), Math.Abs(percentY));
            ScaleValue = HoverScale + (EdgeCompensation * intensity);
        }
    }

    private void OnGlobalPointerExited(object? sender, PointerEventArgs e)
    {
        if (!_isHovered) return;
        ResetTransform();
    }

    private void ResetTransform()
    {
        _isHovered = false;
        ZIndex = 0;
        RotateX = 0;
        RotateY = 0;
        ScaleValue = 1.0;
    }
}