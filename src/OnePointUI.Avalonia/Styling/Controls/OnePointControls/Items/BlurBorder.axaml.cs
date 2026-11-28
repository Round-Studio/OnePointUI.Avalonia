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
}
