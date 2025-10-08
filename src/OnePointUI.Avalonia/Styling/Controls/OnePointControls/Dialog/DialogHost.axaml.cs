using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using OnePointUI.Avalonia.Base.Entry;
using OnePointUI.Avalonia.Base.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls.Dialog;

public partial class DialogHost : UserControl
{
    private static DialogHost? _host;
    private static readonly Queue<DialogInfo> _dialogQueue = new Queue<DialogInfo>();
    private static bool _isShowingDialog = false;

    public static DialogButtons Show(DialogInfo info)
    { 
        // 如果当前正在显示对话框，将新对话框加入队列并返回默认值
        if (_isShowingDialog)
        {
            _dialogQueue.Enqueue(info);
            return DialogButtons.CloseButton;
        }

        _isShowingDialog = true;
        
        IBrush GetDynamicResourceFromApp()
        {
            var app = Application.Current;
    
            if (app != null)
            {
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

            // 为窗口对话框添加关闭事件处理
            window.Closed += (sender, args) =>
            {
                _isShowingDialog = false;
                // 检查队列中是否有下一个对话框
                if (_dialogQueue.Count > 0)
                {
                    var nextInfo = _dialogQueue.Dequeue();
                    Show(nextInfo);
                }
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
        _isShowingDialog = false;
        
        // 检查队列中是否有下一个对话框
        if (_dialogQueue.Count > 0)
        {
            var nextInfo = _dialogQueue.Dequeue();
            Show(nextInfo);
        }
    }

    public DialogHost()
    {
        InitializeComponent();
        _host = this;
    }
}