using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OnePointUI.Avalonia.Base.Entry;
using OnePointUI.Avalonia.Base.Enum;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls.Dialog;

public partial class DialogHost : UserControl
{
    private static DialogHost? _host;

    public static DialogButtons Show(DialogInfo info)
    {
        if (_host == null) throw new NullReferenceException("必须初始化 DialogHost");

        _host.IsVisible = true;
        _host.BackgroundGrid.Opacity = 0.3;
        _host.DialogBox.Content = new DialogContent(info);
        
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