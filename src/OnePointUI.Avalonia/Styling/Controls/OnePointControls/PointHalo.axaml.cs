using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls;

public partial class PointHalo : UserControl
{
    // 光辉直径 = 父容器较大边长 × 0.8
    private const double HaloScale = 0.8;

    private Visual? _observedParent;

    public PointHalo()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // 跟随父容器尺寸变化调整光辉大小
        if (Parent is Visual parent)
        {
            _observedParent = parent;
            parent.PropertyChanged += OnParentPropertyChanged;
            UpdateHaloSize(parent.Bounds);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_observedParent != null)
        {
            _observedParent.PropertyChanged -= OnParentPropertyChanged;
            _observedParent = null;
        }
    }

    private void OnParentPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == BoundsProperty && e.NewValue is Rect bounds)
            UpdateHaloSize(bounds);
    }

    private void UpdateHaloSize(Rect bounds)
    {
        var size = Math.Max(bounds.Width, bounds.Height) * HaloScale;
        if (size <= 0) return;

        MovingRing.Width = size;
        MovingRing.Height = size;
    }

    private void Canvas_PointerMoved(object sender, PointerEventArgs e)
    {
        // 获取鼠标指针相对于 Canvas 的当前位置
        var currentPoint = e.GetPosition(MainCanvas);

        // 计算圆环新的左上角位置，使其中心与鼠标指针对齐
        // 减去圆环宽度和高度的一半以实现中心对齐
        var newX = currentPoint.X - MovingRing.Width / 2;
        var newY = currentPoint.Y - MovingRing.Height / 2;

        // 使用 Canvas.SetLeft 和 SetTop 附加属性设置圆环的位置
        Canvas.SetLeft(MovingRing, newX);
        Canvas.SetTop(MovingRing, newY);
    }

    private void Control_PointerEntered(object sender, PointerEventArgs e)
    {
        // 鼠标进入时显示光晕
        MovingRing.Opacity = 0.4;
    }

    private void Control_PointerExited(object sender, PointerEventArgs e)
    {
        // 鼠标离开时隐藏光晕
        MovingRing.Opacity = 0;
    }
}
