using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using OnePointUI.Avalonia.Base.Entry;
using OnePointUI.Avalonia.Base.Enum;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls.Dialog;

public partial class DialogHost : UserControl
{
    private static DialogHost? _host;

    public static DialogButtons Show(DialogInfo info)
    { 
        IBrush GetDynamicResourceFromApp()
        {
            // Application.Current 是一个全局的入口点
            var app = Application.Current;
    
            if (app != null)
            {
                // 从应用程序的资源中查找 :cite[1]
                // 注意：这里查找的是 Application.Resources 里定义的资源
                if (app.TryFindResource("PrimaryBackgroundOverBrush", out var resourceValue))
                {
                    return resourceValue as IBrush;
                }
            }
    
            return new SolidColorBrush(Colors.Gray);
        }
        
        if (!info.IsWindow)
        {
            if (_host == null) throw new NullReferenceException("必须初始化 DialogHost");

            _host.IsVisible = true;
            _host.BackgroundGrid.Opacity = 0.8;
            _host.DialogBox.Content = new DialogBorder(info);
        }
        else
        {
            var body = new DialogContent(info)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            var window = new Window()
            {
                ExtendClientAreaToDecorationsHint = true,
                ExtendClientAreaTitleBarHeightHint = -1,
                ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = body,
                Background = GetDynamicResourceFromApp(),
                CanResize = false,
                SizeToContent = SizeToContent.WidthAndHeight,
                Title = info.Title
            };
            window.PointerPressed += (sender, args) =>
            {
                window.BeginMoveDrag(args);
            };

            window.Show();
        }
        
        return DialogButtons.CloseButton;
    }

    public static async Task Close()
    {
        _host.DialogBox.Content = null;
        _host.BackgroundGrid.Opacity = 0;
        
        await Task.Delay(400);
        
        _host.IsVisible = false;
    }
    public DialogHost()
    {
        InitializeComponent();

        _host = this;
    }
}