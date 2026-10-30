using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls;

public class SettingExpander : Expander
{
    public static readonly StyledProperty<string> GlyphProperty =
        AvaloniaProperty.Register<SettingExpander, string>(nameof(Glyph), "");

    public static readonly StyledProperty<object> DescriptionProperty =
        AvaloniaProperty.Register<SettingExpander, object>(nameof(Description));

    public static readonly StyledProperty<bool> IsFontIconProperty =
        AvaloniaProperty.Register<SettingExpander, bool>(nameof(IsFontIcon), true);

    public static readonly StyledProperty<bool> IsNotFontIconProperty =
        AvaloniaProperty.Register<SettingExpander, bool>(nameof(IsNotFontIcon));

    public static readonly StyledProperty<IImage> ImageIconProperty =
        AvaloniaProperty.Register<SettingExpander, IImage>(nameof(ImageIcon));

    public static readonly StyledProperty<object> RightContentProperty =
        AvaloniaProperty.Register<SettingExpander, object>(nameof(RightContent));

    private static readonly TimeSpan ExpandDuration = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan CollapseDuration = TimeSpan.FromMilliseconds(200);

    private Border _contentClip;
    private Border _header;
    private CancellationTokenSource _animationCts;
    private bool _isPointerPressed;
    private bool _templateReady;

    public string Glyph
    {
        get => GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public object Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public bool IsFontIcon
    {
        get => GetValue(IsFontIconProperty);
        set => SetValue(IsFontIconProperty, value);
    }

    public bool IsNotFontIcon
    {
        get => GetValue(IsNotFontIconProperty);
        set => SetValue(IsNotFontIconProperty, value);
    }

    public IImage ImageIcon
    {
        get => GetValue(ImageIconProperty);
        set => SetValue(ImageIconProperty, value);
    }

    public object RightContent
    {
        get => GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _contentClip = e.NameScope.Find<Border>("PART_ContentClip");
        _header = e.NameScope.Find<Border>("PART_Header");

        if (_header != null)
        {
            _header.PointerPressed += Header_PointerPressed;
            _header.PointerReleased += Header_PointerReleased;
            _header.PointerCaptureLost += Header_PointerCaptureLost;
            _header.KeyDown += Header_KeyDown;
        }

        // 与 SettingCard 一致：右侧内容区域拦截指针事件，避免点击它时触发展开/收起
        var actionContentControl = e.NameScope.Find<ContentControl>("ActionContentControl");
        if (actionContentControl != null)
        {
            actionContentControl.PointerPressed += (_, args) => { args.Handled = true; };
            actionContentControl.PointerReleased += (_, args) => { args.Handled = true; };
        }

        _templateReady = true;

        // 初始状态不播动画：默认展开时交给布局自适应，收起时固定为 0
        if (_contentClip != null)
            _contentClip.Height = IsExpanded ? double.NaN : 0;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsFontIconProperty)
            SetValue(IsNotFontIconProperty, !change.GetNewValue<bool>());
        else if (change.Property == Expander.IsExpandedProperty && _templateReady)
            AnimateContent(change.GetNewValue<bool>());
    }

    private void Header_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _isPointerPressed = true;
        e.Pointer.Capture(_header);
    }

    private void Header_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (!_isPointerPressed)
            return;

        _isPointerPressed = false;

        var pos = e.GetPosition(_header);
        if (_header == null || !new Rect(_header.Bounds.Size).Contains(pos))
            return;

        SetCurrentValue(Expander.IsExpandedProperty, !IsExpanded);
        e.Handled = true;
    }

    private void Header_PointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
    {
        _isPointerPressed = false;
    }

    private void Header_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Space or Key.Enter)
        {
            SetCurrentValue(Expander.IsExpandedProperty, !IsExpanded);
            e.Handled = true;
        }
    }

    private async void AnimateContent(bool expand)
    {
        var clip = _contentClip;
        if (clip == null)
            return;

        _animationCts?.Cancel();
        _animationCts = new CancellationTokenSource();
        var token = _animationCts.Token;

        try
        {
            if (expand)
            {
                var from = clip.Bounds.Height;

                // 临时解除固定高度，测量内容的自然高度，再回到当前可见高度开始展开
                clip.Height = double.NaN;
                clip.UpdateLayout();
                var target = clip.Bounds.Height;

                if (target <= 0)
                    return; // 尚未完成布局，保持自然高度直接展示

                // 固定起点：在动画首帧应用前不闪现完整内容
                clip.Height = Math.Clamp(from, 0, target);

                if (from < target)
                    await RunHeightAnimationAsync(clip, from, target, ExpandDuration, token);

                if (!token.IsCancellationRequested)
                    clip.Height = double.NaN; // 动画结束后交还布局自适应
            }
            else
            {
                var from = clip.Bounds.Height;
                if (from <= 0)
                {
                    clip.Height = 0;
                    return;
                }

                // 固定起点高度，动画结束后归 0
                clip.Height = from;

                await RunHeightAnimationAsync(clip, from, 0, CollapseDuration, token);

                if (!token.IsCancellationRequested)
                    clip.Height = 0;
            }
        }
        catch (OperationCanceledException)
        {
            // 动画被新的展开/收起操作接管
        }
    }

    private static async Task RunHeightAnimationAsync(
        Layoutable target, double from, double to, TimeSpan duration, CancellationToken token)
    {
        var animation = new Animation
        {
            Duration = duration,
            Easing = new SplineEasing(0.1, 0.9, 0.2, 1.0),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters = { new Setter(Layoutable.HeightProperty, from) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters = { new Setter(Layoutable.HeightProperty, to) }
                }
            }
        };

        await animation.RunAsync(target, token);
    }
}
