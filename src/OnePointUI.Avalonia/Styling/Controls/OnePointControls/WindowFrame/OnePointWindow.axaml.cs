using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls.WindowFrame;

public partial class OnePointWindow : Window
{
    public object? MainContent
    {
        get
        {
            return _mainContent;
        }
        set
        {
            _mainContent = value;
            UpdateUI();
        }
    }
    public object? TitleBarContent
    {
        get
        {
            return _titleBarContent;
        }
        set
        {
            _titleBarContent = value;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        PART_MainContent.Content = _mainContent;
        TitleBlock.Text = this.Title;
        TitleContent.Content = _titleBarContent;
    }

    private object? _mainContent { get; set; }
    private object? _titleBarContent { get; set; }
    private Timer _stateTimer;
    public OnePointWindow()
    {
        InitializeComponent();

        Frame.NavigateTo("");
        _stateTimer = new(state =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (OperatingSystem.IsWindows())
                {
                    if (WindowState == WindowState.Maximized) this.Padding = new Thickness(8);
                    else this.Padding = new Thickness(0);
                }

                if (WindowState == WindowState.Maximized) MaxBtnIcon.Glyph = "\uE923";
                else MaxBtnIcon.Glyph = "\uE922";
                
                TitleBlock.Text = this.Title;
            });
        });
        _stateTimer.Change(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(100));
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        this.BeginMoveDrag(e);
    }

    private void MinBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void MaxBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState == WindowState.Maximized ?  WindowState.Normal : WindowState.Maximized;
    }

    private void CloseBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close();
        
        Environment.Exit(0);
    }

    public void CloseDraw()
    {
        SetBorderState(false);
    }

    public async void OpenDraw(object? page,string title)
    {
        BorderTitle.Text = title;
        await SetBorderState(true);

        Frame.NavigateTo(page);
    }

    private async Task SetBorderState(bool state)
    {
        if (state)
        {
            BottomBorder.Margin = new Thickness(0, this.Height, 0, -this.Height);
            await Task.Delay(100);
            BorderGrid.IsVisible = true;
            BottomBorder.Margin = new Thickness(0, 100, 0, 0);
            BorderBackground.Opacity = 0.3;
            await Task.Delay(200);
        }
        else
        {
            BottomBorder.Margin = new Thickness(0, this.Height, 0, -this.Height);
            BorderBackground.Opacity = 0;
            await Task.Delay(800);
            BorderGrid.IsVisible = false;
            Frame.NavigateTo("");
        }
    }

    private void CloseBorderBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        SetBorderState(false);
    }
}