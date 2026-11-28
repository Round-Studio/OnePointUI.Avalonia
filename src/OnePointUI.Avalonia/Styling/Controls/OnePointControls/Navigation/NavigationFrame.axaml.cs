using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OnePointUI.Avalonia.Base.Enum;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls.Navigation;

/// <summary>
/// 定义导航的动画类型
/// </summary>
public enum NavigationAnimationType
{
    /// <summary>
    /// 默认的位移加淡入淡出动画
    /// </summary>
    Default,

    /// <summary>
    /// 明暗交替（纯透明度）动画：前一个页面完全淡出到0，再让后一个页面淡入到1
    /// </summary>
    FadeThrough
}

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

    /// <summary>
    /// 页面切换的方向
    /// </summary>
    public NavigationFrameDirection NavigationFrameDirection { get; set; } = NavigationFrameDirection.Top;

    /// <summary>
    /// 导航动画类型
    /// </summary>
    public NavigationAnimationType AnimationType { get; set; } = NavigationAnimationType.Default;

    public async void NavigateTo(object page)
    {
        // 取消之前正在进行的任何隐藏操作
        _hideFrameCts?.Cancel();
        _hideFrameCts = new CancellationTokenSource();
        var token = _hideFrameCts.Token;

        // 销毁上一页
        DestroyPreviousPage();

        // 根据不同的动画类型执行不同的逻辑
        if (AnimationType == NavigationAnimationType.FadeThrough)
        {
            await NavigateFadeThroughAsync(page, token);
        }
        else
        {
            NavigateDefault(page, token);
        }
    }

    /// <summary>
    /// 默认的位移加淡入淡出动画逻辑
    /// </summary>
    private async void NavigateDefault(object page, CancellationToken token)
    {
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
                    ClearFrameContent(Frame2);
                }
            }
            catch (TaskCanceledException)
            {
                // 任务被取消是正常现象
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
    /// 明暗交替动画类型：先将当前页面透明度设为0（跑完动画），再将下一个页面透明度设为1
    /// </summary>
    private async Task NavigateFadeThroughAsync(object page, CancellationToken token)
    {
        // 如果当前有正在显示的 Frame，先将其淡出到 0
        ContentControl activeFrame = IsOneFrame ? Frame1 : Frame2;
        ContentControl targetFrame = IsOneFrame ? Frame2 : Frame1;

        // 如果当前有内容在显示，先做淡出动画
        if (activeFrame.IsVisible && activeFrame.Content != null)
        {
            // 这里假设你的 XAML 中有写好针对 Opacity 的 Transition 动画，只需直接改 Opacity 即可
            activeFrame.Opacity = 0;
            
            try
            {
                // 等待淡出动画跑完（例如 200ms）
                await Task.Delay(200, token);
                if (token.IsCancellationRequested) return;

                activeFrame.IsVisible = false;
                ClearFrameContent(activeFrame);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }

        // 切换标志位
        IsOneFrame = !IsOneFrame;

        // 设置目标 Frame（无位移差，只做纯透明度过渡）
        targetFrame.Margin = new Thickness(0);
        targetFrame.Opacity = 0;
        targetFrame.IsVisible = true;
        targetFrame.Content = page;

        // 更新引用
        _previousPage = activeFrame.Content;
        _previousFrame = activeFrame;
        _currentPage = page;
        _currentFrame = targetFrame;

        // 稍微让出UI渲染帧或直接执行淡入
        await Task.Delay(20, token);
        if (token.IsCancellationRequested) return;

        // 淡入到 1
        targetFrame.Opacity = 1;
    }

    /// <summary>
    ///     销毁上一页的内容
    /// </summary>
    private void DestroyPreviousPage()
    {
        if (_previousPage == null) return;

        try
        {
            if (_previousPage is IDisposable disposable) disposable.Dispose();

            if (_previousPage is Control control)
            {
                control.DataContext = null;

                if (control.Parent is Panel panel)
                    panel.Children.Remove(control);
                else if (control.Parent is Decorator decorator)
                    decorator.Child = null;
                else if (control.Parent is ContentControl contentControl) contentControl.Content = null;
            }

            if (_previousFrame != null) ClearFrameContent(_previousFrame);

            _previousPage = null;
            _previousFrame = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"销毁页面时出错: {ex.Message}");
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

            frame.Content = null;
            if (content is IDisposable disposable) disposable.Dispose();

            if (content is Control control)
            {
                control.DataContext = null;
                control.Classes.Clear();
                control.RaiseEvent(new RoutedEventArgs(UnloadedEvent));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"清除 Frame 内容时出错: {ex.Message}");
        }
    }

    /// <summary>
    ///     完全销毁导航器中的所有内容
    /// </summary>
    public void DestroyAll()
    {
        try
        {
            _hideFrameCts?.Cancel();
            _hideFrameCts?.Dispose();
            _hideFrameCts = null;

            DestroyPage(_currentPage, _currentFrame);
            DestroyPage(_previousPage, _previousFrame);

            ClearFrameContent(Frame1);
            ClearFrameContent(Frame2);

            _currentPage = null;
            _previousPage = null;
            _currentFrame = null;
            _previousFrame = null;
            IsOneFrame = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"销毁所有页面时出错: {ex.Message}");
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
            if (page is IDisposable disposable) disposable.Dispose();
            if (frame != null) frame.Content = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"销毁页面时出错: {ex.Message}");
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