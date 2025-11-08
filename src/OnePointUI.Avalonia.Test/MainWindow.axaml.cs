using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Layout;
using OnePointUI.Avalonia.Style.Core;
using System.Collections.Generic;
using System.Linq;
using OnePointUI.Avalonia.Styling.Controls.OnePointControls.WindowFrame;

namespace OnePointUI.Avalonia.Test;

public partial class MainWindow : OnePointWindow
{
    public MainWindow()
    {
        InitializeComponent();
        
        // 初始化主题色选择器
        InitializeColorSelector();
        
        // 更新当前主题文本
        UpdateThemeText();
    }
    
    private void InitializeColorSelector()
    {
        // 清空现有按钮
        ColorPanel.Children.Clear();
        
        // 为每种颜色创建一个按钮
        foreach (var colorPair in ThemeColorSelector.Colors)
        {
            var button = new Button
            {
                Width = 40,
                Height = 40,
                // 不使用Thickness对象
                // Margin = new Thickness(5),
                Background = new SolidColorBrush(colorPair.Value),
                Tag = colorPair.Key // 存储颜色名称作为Tag
            };
            
            // 设置工具提示
            ToolTip.SetTip(button, colorPair.Key);
            
            // 添加点击事件
            button.Click += ColorButton_Click;
            
            // 添加到面板
            ColorPanel.Children.Add(button);
        }
    }
    
    private void UpdateThemeText()
    {
        var currentTheme = ThemeManager.Instance.CurrentTheme;
        CurrentThemeText.Text = currentTheme == ThemeVariant.Dark ? "深色" : "浅色";
    }
    
    private void ToggleTheme_Click(object sender, RoutedEventArgs e)
    {
        ThemeManager.Instance.SetThemeModel(ThemeManager.Instance.CurrentTheme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark);
        UpdateThemeText();
    }
    
    private void ColorButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string colorName)
        {
            ThemeColorSelector.ApplyPredefinedColor(colorName);
        }
    }
    
    private void ApplyCustomColor_Click(object sender, RoutedEventArgs e)
    {
        var hexColor = CustomColorTextBox.Text?.Trim();
        if (!string.IsNullOrEmpty(hexColor))
        {
            if (!ThemeColorSelector.ApplyCustomColor(hexColor))
            {
                // 颜色格式无效，可以在这里显示错误提示
                CustomColorTextBox.Classes.Add("error");
            }
            else
            {
                CustomColorTextBox.Classes.Remove("error");
            }
        }
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        SetBorderState(true);
    }
}