using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls.Items;

/// <summary>
/// 毛玻璃底板控件:对身后(控件背后)的内容做 BackdropEffect 高斯模糊。
/// Radius:-1(FollowGlobal,默认)跟随全局;>=0 用实例值;0 单独关闭。
/// 关闭态一律置 null——任何非 null Effect(哪怕 Radius=0 占位)都会把子树内 backdrop 隔离在层内。
/// CacheMode:null(自动)实际走 Volatile——合成器默认(Retained)在任何中间层
/// (如 ScrollContentPresenter 的 OpacityMask 边缘渐隐 SaveLayer)内会被静默跳过不画。
/// CornerRadius 非零时按其生成圆角 Clip:12.2 的 backdrop blob 只按矩形 bounds 绘制不吃 CornerRadius,
/// 直角会突出圆角卡片;而几何 Clip 在渲染遍历中先于 backdrop 压入画布,可以裁到 blob(圆角修复)。
/// </summary>
public class BlurBorder : Border
{
    public const double FollowGlobal = -1d;

    public static readonly StyledProperty<double> RadiusProperty =
        AvaloniaProperty.Register<BlurBorder, double>(nameof(Radius), FollowGlobal);

    public static readonly StyledProperty<BackdropEffectCache?> CacheModeProperty =
        AvaloniaProperty.Register<BlurBorder, BackdropEffectCache?>(nameof(CacheMode));

    /// <summary>自动模式(CacheMode=null)的兜底:Volatile 无中间层门禁,层内可正常绘制。</summary>
    private static readonly VolatileBackdropEffectCache AutoCache = new();

    private static bool _isEnabled = true;
    private static double _globalRadius = 20d;

    /// <summary>全局开关,变更即通知全部实例刷新。</summary>
    public static bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value)
                return;
            _isEnabled = value;
            RaiseGlobalChanged();
        }
    }

    /// <summary>全局模糊半径,变更即通知全部实例刷新。</summary>
    public static double GlobalRadius
    {
        get => _globalRadius;
        set
        {
            if (Math.Abs(_globalRadius - value) < double.Epsilon)
                return;
            _globalRadius = value;
            RaiseGlobalChanged();
        }
    }

    public static void SetGlobal(bool enabled, double radius)
    {
        _isEnabled = enabled;
        _globalRadius = radius;
        RaiseGlobalChanged();
    }

    public static event EventHandler? GlobalChanged;

    private static void RaiseGlobalChanged() => GlobalChanged?.Invoke(null, EventArgs.Empty);

    public double Radius
    {
        get => GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    /// <summary>null=自动(Volatile 兜底);可显式指定 Retained(仅在无中间层时可用)。</summary>
    public BackdropEffectCache? CacheMode
    {
        get => GetValue(CacheModeProperty);
        set => SetValue(CacheModeProperty, value);
    }

    static BlurBorder()
    {
        RadiusProperty.Changed.AddClassHandler<BlurBorder>((c, _) => c.Refresh());
        CacheModeProperty.Changed.AddClassHandler<BlurBorder>((c, _) => c.Refresh());
        CornerRadiusProperty.Changed.AddClassHandler<BlurBorder>((c, _) => c.UpdateClip());
        BoundsProperty.Changed.AddClassHandler<BlurBorder>((c, _) => c.UpdateClip());
    }

    public BlurBorder()
    {
        AttachedToVisualTree += (_, _) =>
        {
            GlobalChanged += OnGlobalChanged;
            Refresh();
        };
        DetachedFromVisualTree += (_, _) =>
        {
            GlobalChanged -= OnGlobalChanged;
            BackdropEffect = null;
            BackdropEffectCache = null;
        };
    }

    private void OnGlobalChanged(object? sender, EventArgs e) => Refresh();

    private void Refresh()
    {
        var radius = IsEnabled ? (Radius < 0 ? GlobalRadius : Radius) : 0d;
        BackdropEffect = radius > 0 ? new BlurEffect { Radius = radius } : null;
        BackdropEffectCache = CacheMode ?? AutoCache;
    }

    /// <summary>同步圆角 Clip,让 backdrop blob 不再以直角突出圆角边缘。</summary>
    private void UpdateClip()
    {
        var size = Bounds.Size;
        var radius = CornerRadius;
        Clip = size.Width > 0 && size.Height > 0 && HasRoundedCorner(radius)
            ? CreateRoundedRectGeometry(size, radius)
            : null;
    }

    private static bool HasRoundedCorner(in CornerRadius radius) =>
        radius.TopLeft > 0 || radius.TopRight > 0 || radius.BottomRight > 0 || radius.BottomLeft > 0;

    /// <summary>按 Border 的绘制方式(ArcTo 弧线)构建圆角矩形,保证裁剪边缘与卡片圆角重合。</summary>
    private static StreamGeometry CreateRoundedRectGeometry(Size size, CornerRadius radius)
    {
        // 相邻圆角之和超出边长时等比收缩,避免弧线交叉
        var scale = Math.Min(
            Math.Min(
                size.Width / Math.Max(radius.TopLeft + radius.TopRight, double.Epsilon),
                size.Width / Math.Max(radius.BottomLeft + radius.BottomRight, double.Epsilon)),
            Math.Min(
                size.Height / Math.Max(radius.TopLeft + radius.BottomLeft, double.Epsilon),
                size.Height / Math.Max(radius.TopRight + radius.BottomRight, double.Epsilon)));
        if (scale > 1) scale = 1;

        var tl = radius.TopLeft * scale;
        var tr = radius.TopRight * scale;
        var br = radius.BottomRight * scale;
        var bl = radius.BottomLeft * scale;
        var w = size.Width;
        var h = size.Height;

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(tl, 0), isFilled: true);
            ctx.LineTo(new Point(w - tr, 0));
            if (tr > 0)
                ctx.ArcTo(new Point(w, tr), new Size(tr, tr), 0.0, false, SweepDirection.Clockwise);
            ctx.LineTo(new Point(w, h - br));
            if (br > 0)
                ctx.ArcTo(new Point(w - br, h), new Size(br, br), 0.0, false, SweepDirection.Clockwise);
            ctx.LineTo(new Point(bl, h));
            if (bl > 0)
                ctx.ArcTo(new Point(0, h - bl), new Size(bl, bl), 0.0, false, SweepDirection.Clockwise);
            ctx.LineTo(new Point(0, tl));
            if (tl > 0)
                ctx.ArcTo(new Point(tl, 0), new Size(tl, tl), 0.0, false, SweepDirection.Clockwise);
            ctx.EndFigure(isClosed: true);
        }
        return geometry;
    }
}
