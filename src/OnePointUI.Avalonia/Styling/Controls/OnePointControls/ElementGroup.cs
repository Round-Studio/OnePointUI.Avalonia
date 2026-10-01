using Avalonia;
using AvaloniaOrientation = Avalonia.Layout.Orientation;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Reactive;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls;

/// <summary>
/// DM: 该组件为大肥鱼生成
/// 一组自动为首尾子元素设置圆角的 ItemsControl。
/// Horizontal：第一个元素左侧圆角、最后一个元素右侧圆角，中间元素平角。
/// Vertical  ：第一个元素顶部圆角、最后一个元素底部圆角，中间元素平角。
/// </summary>
public class ElementGroup : ItemsControl
{
    private readonly Dictionary<Control, IDisposable> _visibilitySubscriptions = new();
    private Panel? _subscribedPanel;

    private static readonly StyledProperty<AvaloniaOrientation> OrientationProperty =
        AvaloniaProperty.Register<ElementGroup, AvaloniaOrientation>(nameof(Orientation));

    public AvaloniaOrientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    static ElementGroup()
    {
        OrientationProperty.Changed.AddClassHandler<ElementGroup>(
            (x, _) => x.UpdateOrientation());
    }

    public ElementGroup()
    {
        UpdateOrientation();
    }

    // ------------------------------------------------------------------
    // 方向切换：重建 ItemsPanel
    // ------------------------------------------------------------------
    private void UpdateOrientation()
    {
        ItemsPanel = Orientation == AvaloniaOrientation.Horizontal
            ? new FuncTemplate<Panel>(() => new StackPanel { Orientation = AvaloniaOrientation.Horizontal })
            : new FuncTemplate<Panel>(() => new StackPanel { Orientation = AvaloniaOrientation.Vertical });

        UpdateElements();
    }

    // ------------------------------------------------------------------
    // 生命周期
    // ------------------------------------------------------------------
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        SubscribeToPanel();
        UpdateVisibilityHandlers();
        UpdateElements();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeToPanel();
        UpdateVisibilityHandlers();
        UpdateElements();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeFromPanel();
        ClearVisibilityHandlers();
        base.OnDetachedFromVisualTree(e);
    }

    // ------------------------------------------------------------------
    // ItemsPanelRoot 子元素集合变化：重新评估
    // ------------------------------------------------------------------
    private void SubscribeToPanel()
    {
        if (ReferenceEquals(_subscribedPanel, ItemsPanelRoot))
            return;

        UnsubscribeFromPanel();

        if (ItemsPanelRoot is Panel panel)
        {
            _subscribedPanel = panel;
            panel.Children.CollectionChanged += OnPanelChildrenChanged;
        }
    }

    private void UnsubscribeFromPanel()
    {
        if (_subscribedPanel != null)
        {
            _subscribedPanel.Children.CollectionChanged -= OnPanelChildrenChanged;
            _subscribedPanel = null;
        }
    }

    private void OnPanelChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateVisibilityHandlers();
        UpdateElements();
    }

    // ------------------------------------------------------------------
    // 可见性订阅（Avalonia 用 IsVisible）
    // ------------------------------------------------------------------
    private void ClearVisibilityHandlers()
    {
        foreach (var sub in _visibilitySubscriptions.Values)
            sub.Dispose();
        _visibilitySubscriptions.Clear();
    }

    private void UpdateVisibilityHandlers()
    {
        ClearVisibilityHandlers();

        if (ItemsPanelRoot is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                if (child is Control control)
                {
                    var sub = control
                        .GetObservable(Visual.IsVisibleProperty)
                        .Subscribe(new AnonymousObserver<bool>(_ => UpdateElements()));
                    _visibilitySubscriptions[control] = sub;
                }
            }
        }
    }

    // ------------------------------------------------------------------
    // 核心：为可见子元素设置圆角 / 边框
    // ------------------------------------------------------------------
    private void UpdateElements()
    {
        if (ItemsPanelRoot is not Panel panel)
            return;

        var visibleChildren = panel.Children
            .OfType<Control>()
            .Where(c => c.IsVisible)
            .ToList();

        int totalItems = visibleChildren.Count;
        if (totalItems == 0)
            return;

        // 只有一个元素时四角全圆
        if (totalItems == 1)
        {
            SetCornerRadius(visibleChildren[0], new CornerRadius(4));
            SetBorderThickness(visibleChildren[0], new Thickness(1));
            return;
        }

        for (int i = 0; i < totalItems; i++)
        {
            if (Orientation == AvaloniaOrientation.Horizontal)
                SetHorizontalItem(i, totalItems, visibleChildren[i]);
            else
                SetVerticalItem(i, totalItems, visibleChildren[i]);
        }
    }

    private static void SetVerticalItem(int index, int totalItems, Control element)
    {
        if (index == 0)
        {
            SetCornerRadius(element, new CornerRadius(4, 4, 0, 0)); // 左上、右上
            SetBorderThickness(element, new Thickness(1, 1, 1, 0));
        }
        else if (index == totalItems - 1)
        {
            SetCornerRadius(element, new CornerRadius(0, 0, 4, 4)); // 右下、左下
            SetBorderThickness(element, new Thickness(1, 1, 1, 1));
        }
        else
        {
            SetCornerRadius(element, new CornerRadius(0));
            SetBorderThickness(element, new Thickness(1, 1, 1, 0));
        }
    }

    private static void SetHorizontalItem(int index, int totalItems, Control element)
    {
        if (index == 0)
        {
            SetCornerRadius(element, new CornerRadius(4, 0, 0, 4)); // 左上、左下
            SetBorderThickness(element, new Thickness(1, 1, 0, 1));
        }
        else if (index == totalItems - 1)
        {
            SetCornerRadius(element, new CornerRadius(0, 4, 4, 0)); // 右上、右下
            SetBorderThickness(element, new Thickness(1, 1, 1, 1));
        }
        else
        {
            SetCornerRadius(element, new CornerRadius(0));
            SetBorderThickness(element, new Thickness(1, 1, 0, 1));
        }
    }

    // ------------------------------------------------------------------
    // 兼容不同控件类型：
    //   - TemplatedControl (Button / TextBox / ToggleButton 等，Avalonia 11 起有 CornerRadius)
    //   - Border
    //   - ContentPresenter（ItemsControl 的默认容器），需要穿透到 Content
    // ------------------------------------------------------------------
    private static void SetCornerRadius(Control control, CornerRadius radius)
    {
        switch (control)
        {
            case TemplatedControl tc:
                tc.CornerRadius = radius;
                break;
            case Border border:
                border.CornerRadius = radius;
                break;
            case ContentPresenter cp when cp.Content is Control inner:
                SetCornerRadius(inner, radius);
                break;
        }
    }

    private static void SetBorderThickness(Control control, Thickness thickness)
    {
        switch (control)
        {
            case TemplatedControl tc:
                tc.BorderThickness = thickness;
                break;
            case Border border:
                border.BorderThickness = thickness;
                break;
            case ContentPresenter cp when cp.Content is Control inner:
                SetBorderThickness(inner, thickness);
                break;
        }
    }
}