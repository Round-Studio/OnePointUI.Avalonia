using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OnePointUI.Avalonia.Base.Entry;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls.Dialog;

public partial class DialogBorder : UserControl
{
    public DialogBorder(DialogInfo info)
    {
        InitializeComponent();

        ContentBorder.Child = new DialogContent(info);
    }
}