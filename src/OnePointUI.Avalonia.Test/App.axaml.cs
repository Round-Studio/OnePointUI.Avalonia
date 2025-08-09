using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using OnePointUI.Avalonia.Style.Core;

namespace OnePointUI.Avalonia.Test;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 首先初始化ThemeManager
        ThemeManager.Initialize(this);
        
        // 设置初始主题色
        ThemeColorSelector.ApplyPredefinedColor("橙色");
        ThemeManager.Instance.SetThemeModel(ThemeVariant.Dark);
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}