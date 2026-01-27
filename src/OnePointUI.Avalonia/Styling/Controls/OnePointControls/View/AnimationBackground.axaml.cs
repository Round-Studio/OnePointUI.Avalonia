using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OnePointUI.Avalonia.Base.Enum;
using OnePointUI.Avalonia.Styling.Effect;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls.View;

public partial class AnimationBackground : UserControl
{
    public BackgroundType BackgroundType
    {
        get => _backgroundType;
        set
        {
            _backgroundType = value;
            Update();
        }
    }

    private BackgroundType _backgroundType;
    public AnimationBackground()
    {
        InitializeComponent();
        
        Loaded += (sender, args) => Update();
    }

    public void Update()
    {
        if (_backgroundType == null)
        {
            _backgroundType = BackgroundType.Bubble;
        }
        SkiaEffect.UpdateColor();
            
        SkiaShaderRenderer.Stop();
        if (_backgroundType == BackgroundType.Bubble)
        {
            SkiaShaderRenderer.SetEffect(SkiaEffect.FromEmbeddedResource("bubble.sksl"));
            SkiaShaderRenderer.Start();
        }else if (_backgroundType == BackgroundType.Voronoi)
        {
            SkiaShaderRenderer.SetEffect(SkiaEffect.FromEmbeddedResource("voronoi.sksl"));
            SkiaShaderRenderer.Start();
        }
    }
}