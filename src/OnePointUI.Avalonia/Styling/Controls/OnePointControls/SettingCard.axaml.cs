﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls;

public class SettingCard : Button
{
    public static readonly StyledProperty<string> GlyphProperty =
        AvaloniaProperty.Register<SettingCard, string>(nameof(Glyph), "");
    
    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<SettingCard, string>(nameof(Header));
    
    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<SettingCard, string>(nameof(Description));
    
    public static readonly StyledProperty<bool> IsClickableProperty =
        AvaloniaProperty.Register<SettingCard, bool>(nameof(IsClickable));

    public static readonly StyledProperty<bool> IsFontIconProperty =
        AvaloniaProperty.Register<SettingCard, bool>(nameof(IsFontIcon), true);

    public static readonly StyledProperty<bool> IsNotFontIconProperty =
        AvaloniaProperty.Register<SettingCard, bool>(nameof(IsNotFontIcon));

    public static readonly StyledProperty<IImage> ImageIconProperty =
        AvaloniaProperty.Register<SettingCard, IImage>(nameof(ImageIcon));

    public string Glyph
    {
        get => GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }
    
    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    
    public bool IsClickable
    {
        get => GetValue(IsClickableProperty);
        set => SetValue(IsClickableProperty, value);
    }
    
    public bool IsFontIcon
    {
        get => GetValue(IsFontIconProperty);
        set
        {
            SetValue(IsFontIconProperty, value);
            // 当 IsFontIcon 改变时，自动更新 IsNotFontIcon
            SetValue(IsNotFontIconProperty, !value);
        }
    }
    
    public bool IsNotFontIcon
    {
        get => GetValue(IsNotFontIconProperty);
        set => SetValue(IsNotFontIconProperty, value);
    }
    
    public IImage ImageIcon
    {
        get => GetValue(ImageIconProperty);
        set => SetValue(ImageIconProperty, value);
    }

    // 重写属性改变通知方法
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // 当 IsFontIcon 属性改变时，自动更新 IsNotFontIcon
        if (change.Property == IsFontIconProperty)
        {
            SetValue(IsNotFontIconProperty, !(bool)change.NewValue!);
        }
    }
}