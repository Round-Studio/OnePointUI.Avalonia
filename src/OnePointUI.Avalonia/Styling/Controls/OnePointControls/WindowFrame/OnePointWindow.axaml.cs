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

    private void UpdateUI()
    {
        PART_MainContent.Content = _mainContent;
        TitleBlock.Text = this.Title;
    }

    private object? _mainContent { get; set; }
    private Timer _stateTimer;
    public OnePointWindow()
    {
        InitializeComponent();

        _stateTimer = new(state =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (OperatingSystem.IsWindows())
                {
                    if (WindowState == WindowState.Maximized) this.Padding = new Thickness(8);
                    else this.Padding = new Thickness(0);
                }
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
}