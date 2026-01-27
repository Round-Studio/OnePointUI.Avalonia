using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using OnePointUI.Avalonia.Styling.Effect;

namespace OnePointUI.Avalonia.Style.Core
{
    public class ThemeManager
    {
        private static ThemeManager? _instance;
        private readonly Application _application;
        public static ThemeVariant CurrentThemeVariant = ThemeVariant.Dark;
        public static Color AccentColor = Colors.Orange;
        
        /// <summary>
        /// 检查ThemeManager是否已初始化
        /// </summary>
        public static bool IsInitialized => _instance != null;

        public static ThemeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException("ThemeManager has not been initialized. Call Initialize first.");
                }
                return _instance;
            }
        }

        private ThemeManager(Application application)
        {
            Console.WriteLine(@"初始化主题");
            _application = application;
            
            // 加载主题颜色资源
            LoadThemeColors();
            Console.WriteLine(@"主题初始化完毕");
        }

        public static void Initialize(Application application)
        {
            // 如果已经初始化，则不再重复初始化
            if (_instance != null) return;
            
            _instance = new ThemeManager(application);
        }

        private bool _isLoadingThemeColors = false;
        private void LoadThemeColors()
        {
            if (_isLoadingThemeColors) return;
            
            _isLoadingThemeColors = true;
            try
            {
                // 创建深色主题资源
                var darkTheme = new ResourceDictionary();
                
                // 添加深色主题资源
                darkTheme["BackgroundBrush"] = new SolidColorBrush(Color.Parse("#161616"));
                darkTheme["PrimaryForegroundBrush"] = new SolidColorBrush(Colors.White);
                darkTheme["PrimaryDisabledForegroundBrush"] = new SolidColorBrush(Color.Parse("#858585"));
                darkTheme["PrimaryDisabled2ForegroundBrush"] = new SolidColorBrush(Color.Parse("#C4C4C4"));
                
                darkTheme["PrimaryBorderBrush"] = new SolidColorBrush(Color.Parse("#454545"));
                darkTheme["PrimaryBackgroundBrush"] = new SolidColorBrush(Color.Parse("#343434"));
                darkTheme["PrimaryBackgroundOverBrush"] = new SolidColorBrush(Color.Parse("#121212"));
                
                darkTheme["AccentBorderBrush"] = new SolidColorBrush(Color.Parse("#C97612"));
                darkTheme["AccentBackgroundBrush"] = new SolidColorBrush(Color.Parse("#FFBB00"));
                darkTheme["AccentBackgroundOverBrush"] = new SolidColorBrush(Color.Parse("#6E3D0E"));
                
                // 创建浅色主题资源
                var lightTheme = new ResourceDictionary();
                
                // 添加浅色主题资源
                lightTheme["BackgroundBrush"] = new SolidColorBrush(Color.Parse("#F5F5F5"));
                lightTheme["PrimaryForegroundBrush"] = new SolidColorBrush(Colors.Black);
                lightTheme["PrimaryDisabledForegroundBrush"] = new SolidColorBrush(Color.Parse("#A0A0A0"));
                lightTheme["PrimaryDisabled2ForegroundBrush"] = new SolidColorBrush(Color.Parse("#707070"));
                
                lightTheme["PrimaryBorderBrush"] = new SolidColorBrush(Color.Parse("#DDDDDD"));
                lightTheme["PrimaryBackgroundBrush"] = new SolidColorBrush(Color.Parse("#EEEEEE"));
                lightTheme["PrimaryBackgroundOverBrush"] = new SolidColorBrush(Color.Parse("#E0E0E0"));
                
                lightTheme["AccentBorderBrush"] = new SolidColorBrush(Color.Parse("#C97612"));
                lightTheme["AccentBackgroundBrush"] = new SolidColorBrush(Color.Parse("#FFBB00"));
                lightTheme["AccentBackgroundOverBrush"] = new SolidColorBrush(Color.Parse("#FFC933"));
                
                // 将主题资源添加到应用程序资源中
                _application.Resources["DarkTheme"] = darkTheme;
                _application.Resources["LightTheme"] = lightTheme;
            }
            finally
            {
                _isLoadingThemeColors = false;
            }
        }

        /// <summary>
        /// 切换主题（深色/浅色）
        /// </summary>
        public void SetThemeModel(ThemeVariant theme)
        {
            SetTheme(theme);
        }

        /// <summary>
        /// 设置主题（深色/浅色）
        /// </summary>
        /// <param name="themeVariant">主题变体</param>
        public void SetTheme(ThemeVariant themeVariant)
        {
            CurrentThemeVariant = themeVariant;
            _application.RequestedThemeVariant = themeVariant;
            
            SkiaEffect.UpdateColor();

            // 应用主题资源
            ApplyThemeResources();
        }

        /// <summary>
        /// 设置主题色
        /// </summary>
        /// <param name="color">主题色</param>
        private bool _isUpdatingAccentColors = false;
        public void SetAccentColor(Color color)
        {
            SkiaEffect.UpdateColor();
            AccentColor = color;
            if (!_isUpdatingAccentColors)
            {
                UpdateAccentColors();
            }
        }

        private bool _isApplyingThemeResources = false;
        private void ApplyThemeResources()
        {
            if (_isApplyingThemeResources) return;
            
            _isApplyingThemeResources = true;
            try
            {
                // 获取当前主题的资源字典
                var themeDictKey = CurrentThemeVariant == ThemeVariant.Dark ? "DarkTheme" : "LightTheme";
                
                // 从应用程序资源中查找主题资源字典
                if (_application.Resources.TryGetValue(themeDictKey, out var themeDict) && themeDict is ResourceDictionary resourceDict)
                {
                    // 将主题资源应用到应用程序资源中
                    foreach (var key in resourceDict.Keys)
                    {
                        _application.Resources[key] = resourceDict[key];
                    }
                    
                    // 更新主题色
                    UpdateAccentColors();
                }
            }
            finally
            {
                _isApplyingThemeResources = false;
            }
        }

        private void UpdateAccentColors()
        {
            if (_isUpdatingAccentColors) return;
            
            _isUpdatingAccentColors = true;
            try
            {
                // 更新主题色相关的资源
                _application.Resources["AccentBackgroundBrush"] = new SolidColorBrush(AccentColor);
                
                // 创建边框色（稍暗的主题色）
                var borderColor = AccentColor.Darken(0.3);
                _application.Resources["AccentBorderBrush"] = new SolidColorBrush(borderColor);
                
                // 创建悬停色（根据当前主题决定是变亮还是变暗）
                var hoverColor = AccentColor.Darken(0.5);
                _application.Resources["AccentBackgroundOverBrush"] = new SolidColorBrush(hoverColor);
            }
            finally
            {
                _isUpdatingAccentColors = false;
            }
        }

        /// <summary>
        /// 获取当前主题变体
        /// </summary>
        public ThemeVariant CurrentTheme => CurrentThemeVariant;

        /// <summary>
        /// 获取当前主题色
        /// </summary>
        public Color CurrentAccentColor => AccentColor;
    }
}