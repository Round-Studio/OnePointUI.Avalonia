using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OnePointUI.Avalonia.Styling.Controls.OnePointControls.Navigation.TabView;

namespace OnePointUI.Avalonia.Test.Pages;

public partial class TabViewDemo : UserControl
{
    private int _pageIndex = 1;

    public TabViewDemo()
    {
        InitializeComponent();
    }

    private void DemoTabView_OnAddTabClick(object? sender, RoutedEventArgs e)
    {
        _pageIndex++;
        DemoTabView.AddPage($"Page {_pageIndex}", new TextBlock
        {
            Text = $"动态页面 {_pageIndex}",
            Margin = new Thickness(20)
        });
    }
}
