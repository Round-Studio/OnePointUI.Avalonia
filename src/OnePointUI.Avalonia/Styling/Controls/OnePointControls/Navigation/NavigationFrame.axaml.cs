using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls.Navigation;

public partial class NavigationFrame : UserControl
{
    private bool IsOneFrame = false;
    public NavigationFrame()
    {
        InitializeComponent();
    }

    // 在类级别声明CancellationTokenSource变量
    private CancellationTokenSource _hideFrameCts;

    public async void NavigateTo(object Page)
    {
        // 取消之前正在进行的任何隐藏操作
        _hideFrameCts?.Cancel();
        _hideFrameCts = new CancellationTokenSource();
        var token = _hideFrameCts.Token;

        if (!IsOneFrame)
        {
            IsOneFrame = true;

            // 立即将即将隐藏的Frame设置为半透明和偏移
            Frame2.Opacity = 0;
            Frame2.Margin = new Thickness(0, 20, 0, -20);

            // 立即显示并重置目标Frame的状态
            Frame1.IsVisible = true;
            Frame1.Opacity = 1;
            Frame1.Margin = new Thickness(0);
            Frame1.Content = Page;

            try
            {
                // 等待400毫秒，但如果token被取消，这里会抛出异常
                await Task.Delay(290, token);
            
                // 只有当token未被取消时，才执行隐藏操作
                if (!token.IsCancellationRequested)
                {
                    Frame2.IsVisible = false;
                    Frame2.Content = null;
                }
            }
            catch (TaskCanceledException)
            {
                // 任务被取消是正常现象，这里捕获异常即可，无需处理
            }
        }
        else
        {
            IsOneFrame = false;

            Frame1.Opacity = 0;
            Frame1.Margin = new Thickness(0, 20, 0, -20);

            Frame2.IsVisible = true;
            Frame2.Opacity = 1;
            Frame2.Margin = new Thickness(0);
            Frame2.Content = Page;

            try
            {
                await Task.Delay(290, token);
                if (!token.IsCancellationRequested)
                {
                    Frame1.IsVisible = false;
                    Frame1.Content = null;
                }
            }
            catch (TaskCanceledException)
            {
                // 忽略取消异常
            }
        }
    }
}