using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OnePointUI.Avalonia.Base.Enum;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls.Navigation;

public partial class NavigationFrame : UserControl
{
    private readonly int ToPx = 50;
    private ContentControl _currentFrame;

    // 存储当前和上一页的引用，用于销毁
    private object _currentPage;

    private CancellationTokenSource _hideFrameCts;
    private ContentControl _previousFrame;
    private object _previousPage;
    private bool IsOneFrame;

    public NavigationFrame()
    {
        InitializeComponent();
    }

    public NavigationFrameDirection NavigationFrameDirection { get; set; } = NavigationFrameDirection.Top;

    public async void NavigateTo(object page)
    {
        // 取消之前正在进行的任何隐藏操作
        _hideFrameCts?.Cancel();
        _hideFrameCts = new CancellationTokenSource();
        var token = _hideFrameCts.Token;

        // 销毁上一页
        DestroyPreviousPage();

        if (!IsOneFrame)
        {
            IsOneFrame = true;

            // 设置即将隐藏的Frame（Frame2）
            Frame2.Opacity = 0;
            if (NavigationFrameDirection == NavigationFrameDirection.Top)
                Frame2.Margin = new Thickness(0, ToPx, 0, -ToPx);
            else
                Frame2.Margin = new Thickness(ToPx, 0, -ToPx, 0);

            // 立即显示并重置目标Frame（Frame1）的状态
            Frame1.IsVisible = true;
            Frame1.Opacity = 1;
            Frame1.Margin = new Thickness(0);
            Frame1.Content = page;

            // 更新引用
            _previousPage = Frame2.Content;
            _previousFrame = Frame2;
            _currentPage = page;
            _currentFrame = Frame1;

            try
            {
                await Task.Delay(290, token);

                if (!token.IsCancellationRequested)
                {
                    Frame2.IsVisible = false;
                    // 清空内容
                    ClearFrameContent(Frame2);
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

            // 设置即将隐藏的Frame（Frame1）
            Frame1.Opacity = 0;
            if (NavigationFrameDirection == NavigationFrameDirection.Top)
                Frame1.Margin = new Thickness(0, ToPx, 0, -ToPx);
            else
                Frame1.Margin = new Thickness(ToPx, 0, -ToPx, 0);

            // 立即显示并重置目标Frame（Frame2）的状态
            Frame2.IsVisible = true;
            Frame2.Opacity = 1;
            Frame2.Margin = new Thickness(0);
            Frame2.Content = page;

            // 更新引用
            _previousPage = Frame1.Content;
            _previousFrame = Frame1;
            _currentPage = page;
            _currentFrame = Frame2;

            try
            {
                await Task.Delay(290, token);

                if (!token.IsCancellationRequested)
                {
                    Frame1.IsVisible = false;
                    // 清空内容
                    ClearFrameContent(Frame1);
                }
            }
            catch (TaskCanceledException)
            {
                // 忽略取消异常
            }
        }
    }

    /// <summary>
    ///     销毁上一页的内容
    /// </summary>
    private void DestroyPreviousPage()
    {
        if (_previousPage == null) return;

        try
        {
            // 如果页面实现了 IDisposable，调用 Dispose()
            if (_previousPage is IDisposable disposable) disposable.Dispose();

            // 如果页面是 Control，从视觉树中移除
            if (_previousPage is Control control)
            {
                // 清除所有绑定
                control.DataContext = null;

                // 从父容器中移除
                if (control.Parent is Panel panel)
                    panel.Children.Remove(control);
                else if (control.Parent is Decorator decorator)
                    decorator.Child = null;
                else if (control.Parent is ContentControl contentControl) contentControl.Content = null;
            }

            // 清除 Frame 的内容
            if (_previousFrame != null) ClearFrameContent(_previousFrame);

            // 清空引用，帮助垃圾回收
            _previousPage = null;
            _previousFrame = null;
        }
        catch (Exception ex)
        {
            // 记录错误但不要崩溃
            Console.WriteLine($"销毁页面时出错: {ex.Message}");
        }
    }

    /// <summary>
    ///     清除 Frame 的内容，并尝试释放资源
    /// </summary>
    private void ClearFrameContent(ContentControl frame)
    {
        if (frame.Content == null) return;

        try
        {
            var content = frame.Content;

            // 首先清空 Frame 的内容
            frame.Content = null;

            // 如果内容实现了 IDisposable，调用 Dispose()
            if (content is IDisposable disposable) disposable.Dispose();

            // 如果内容是 Control，清理资源
            if (content is Control control)
            {
                // 清除数据上下文
                control.DataContext = null;

                // 清除样式
                control.Classes.Clear();

                // 触发卸载事件（如果页面有相应的处理逻辑）
                control.RaiseEvent(new RoutedEventArgs(UnloadedEvent));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"清除 Frame 内容时出错: {ex.Message}");
        }
    }

    /// <summary>
    ///     完全销毁导航器中的所有内容
    /// </summary>
    public void DestroyAll()
    {
        try
        {
            // 取消所有正在进行中的操作
            _hideFrameCts?.Cancel();
            _hideFrameCts?.Dispose();
            _hideFrameCts = null;

            // 销毁当前页
            DestroyPage(_currentPage, _currentFrame);

            // 销毁上一页
            DestroyPage(_previousPage, _previousFrame);

            // 清除 Frame 内容
            ClearFrameContent(Frame1);
            ClearFrameContent(Frame2);

            // 重置状态
            _currentPage = null;
            _previousPage = null;
            _currentFrame = null;
            _previousFrame = null;
            IsOneFrame = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"销毁所有页面时出错: {ex.Message}");
        }
    }

    /// <summary>
    ///     销毁指定页面
    /// </summary>
    private void DestroyPage(object page, ContentControl frame)
    {
        if (page == null) return;

        try
        {
            // 如果是可释放对象
            if (page is IDisposable disposable) disposable.Dispose();

            // 清理 Frame
            if (frame != null) frame.Content = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"销毁页面时出错: {ex.Message}");
        }
    }

    /// <summary>
    ///     获取当前页面
    /// </summary>
    public object GetCurrentPage()
    {
        return _currentPage;
    }

    /// <summary>
    ///     当控件被卸载时，清理资源
    /// </summary>
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        DestroyAll();
    }

    /// <summary>
    ///     直接清空导航器（不带动画）
    /// </summary>
    public void Clear()
    {
        _hideFrameCts?.Cancel();

        Frame1.Content = null;
        Frame2.Content = null;
        Frame1.IsVisible = false;
        Frame2.IsVisible = false;

        _currentPage = null;
        _previousPage = null;
        _currentFrame = null;
        _previousFrame = null;
        IsOneFrame = false;
    }
}