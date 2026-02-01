using Avalonia.Controls;
using Avalonia.Interactivity;
using OnePointUI.Avalonia.Base.Entry;
using OnePointUI.Avalonia.Styling.Controls.OnePointControls.Dialog;

namespace OnePointUI.Avalonia.Test.Pages;

public partial class BasicControls : UserControl
{
    public BasicControls()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        DialogHost.Show(new DialogInfo
        {
            Content = "Dialog Content",
            Title = "Dialog Title",
            CloseButtonText = "Close"
        });
    }
}