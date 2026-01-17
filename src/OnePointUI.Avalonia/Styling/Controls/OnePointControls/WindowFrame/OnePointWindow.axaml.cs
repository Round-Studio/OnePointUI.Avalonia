using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using OnePointUI.Avalonia.Styling.Controls.OnePointControls.Dialog;
using OnePointUI.Avalonia.Styling.Controls.OnePointControls.Notice.Info;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls.WindowFrame;

public partial class OnePointWindow : Window
{
    public bool IsMainWindow
    {
        get => _isMainWindow;
        set 
        {
            _isMainWindow = value;
            UpdateUI();
        }
    }
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
    public object? TitleBarContentContent
    {
        get
        {
            return _titleBarControlContent;
        }
        set
        {
            _titleBarControlContent = value;
            UpdateUI();
        }
    }

    public bool IsMaxBtn
    {
        get => _isMaxBtn;
        set
        {
            _isMaxBtn = value;
            UpdateUI();
        }
    }

    public bool IsMinBtn
    {
        get => _isMinBtn;
        set
        {
            _isMinBtn = value;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        PART_MainContent.Content = _mainContent;
        TitleBlock.Text = this.Title;
        TitleContent.Content = _titleBarContent;
        TitleBarContentBarContent.Content = _titleBarControlContent;
        
        MaxBtn.IsVisible = _isMaxBtn;
        MinBtn.IsVisible = _isMaxBtn;

        if (IsMainWindow)
        {
            DialogHost.SetHost(this.DialogHost);
        }
    }

    public NoticePanel Notice => this.NoticePanel;
    private object? _mainContent { get; set; }
    private object? _titleBarContent { get; set; }
    private object? _titleBarControlContent { get; set; }
    private bool _isMainWindow { get; set; } = false;
    private bool _isMinBtn { get; set; } = true;
    private bool _isMaxBtn { get; set; } = true;
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
        BottomBorder.Margin = new Thickness(DrawMarginLR, 0, DrawMarginLR, 0);
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
        
        if(IsMainWindow) Environment.Exit(0);
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

    public int DrawMarginLR = 10;
    private async Task SetBorderState(bool state)
    {
        if (state)
        {
            BottomBorder.Margin = new Thickness(DrawMarginLR, this.Height, DrawMarginLR, -this.Height);
            await Task.Delay(100);
            BorderGrid.IsVisible = true;
            BottomBorder.Margin = new Thickness(DrawMarginLR, 100, DrawMarginLR, 0);
            BorderBackground.Opacity = 0.3;
            await Task.Delay(200);
        }
        else
        {
            BottomBorder.Margin = new Thickness(DrawMarginLR, this.Height, DrawMarginLR, -this.Height);
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