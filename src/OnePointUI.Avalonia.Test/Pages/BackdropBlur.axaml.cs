using Avalonia.Controls;
using Avalonia.Interactivity;
using OnePointUI.Avalonia.Styling.Controls.OnePointControls.Items;

namespace OnePointUI.Avalonia.Test.Pages;

public partial class BackdropBlur : UserControl
{
    private static readonly double[] RadiusCycle = { 20, 40, 60, 0 };
    private int _radiusIndex;

    public BackdropBlur()
    {
        InitializeComponent();

        // Cache 矩阵:默认实例留 null(合成器自选),另两个逐实例指定
        CacheVolatileBlur.CacheMode = new global::Avalonia.Media.VolatileBackdropEffectCache();
        CacheRetainedBlur.CacheMode = new global::Avalonia.Media.RetainedBackdropEffectCache();

        UpdateGlobalState();
    }

    private void ToggleGlobal_Click(object? sender, RoutedEventArgs e)
    {
        BlurBorder.IsEnabled = !BlurBorder.IsEnabled;
        UpdateGlobalState();
    }

    private void CycleRadius_Click(object? sender, RoutedEventArgs e)
    {
        _radiusIndex = (_radiusIndex + 1) % RadiusCycle.Length;
        BlurBorder.SetGlobal(true, RadiusCycle[_radiusIndex]);
        UpdateGlobalState();
    }

    private void UpdateGlobalState()
    {
        GlobalStateText.Text =
            $"全局: {(BlurBorder.IsEnabled ? "开" : "关")} R={BlurBorder.GlobalRadius:0.#}";
    }
}
