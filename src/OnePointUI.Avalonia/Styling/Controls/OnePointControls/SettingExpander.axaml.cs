using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

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
    private int _animationGeneration;
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

    private void AnimateContent(bool expand)
    {
        var clip = _contentClip;
        if (clip == null)
            return;

        StopHeightAnimation();

        if (expand)
        {
            var from = clip.Bounds.Height;

            // 临时解除固定高度，测量内容的自然高度
            clip.Height = double.NaN;
            clip.UpdateLayout();
            var target = clip.Bounds.Height;

            if (target <= 0)
                return; // 尚未完成布局，保持自然高度直接展示

            StartHeightAnimation(clip, Math.Clamp(from, 0, target), target, ExpandDuration,
                () => clip.Height = double.NaN); // 结束后交还布局自适应
        }
        else
        {
            var from = clip.Bounds.Height;
            if (from <= 0)
            {
                clip.Height = 0;
                return;
            }

            StartHeightAnimation(clip, from, 0, CollapseDuration, () => clip.Height = 0);
        }
    }

    private void StopHeightAnimation()
    {
        // 使进行中的帧回调失效（RequestAnimationFrame 无法取消，靠代际号丢弃旧帧）
        _animationGeneration++;
    }

    private void StartHeightAnimation(Layoutable target, double from, double to, TimeSpan duration, Action finished)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel == null)
        {
            // 不在可视树上（如正在关闭），直接落到终态
            target.Height = to;
            finished();
            return;
        }

        var generation = ++_animationGeneration;
        var easing = new SplineEasing(0.1, 0.9, 0.2, 1.0);
        var stopwatch = Stopwatch.StartNew();

        // 起点立即生效：动画首帧前不闪现完整内容
        target.Height = from;

        void Frame(TimeSpan _)
        {
            if (generation != _animationGeneration)
                return; // 已被新的动画接管，不再写入

            if (this.VisualRoot == null)
            {
                _animationGeneration++;
                target.Height = to;
                finished();
                return;
            }

            var progress = stopwatch.Elapsed.TotalMilliseconds / duration.TotalMilliseconds;

            if (progress >= 1)
            {
                // 最后一帧同步写入精确终值，随后不再有任何回调，避免终值回跳闪烁
                target.Height = to;
                finished();
                return;
            }

            target.Height = from + (to - from) * easing.Ease(progress);
            topLevel.RequestAnimationFrame(Frame);
        }

        topLevel.RequestAnimationFrame(Frame);
    }
}
