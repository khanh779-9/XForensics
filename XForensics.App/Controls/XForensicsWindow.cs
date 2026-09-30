using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using XForensics.App.Themes;

namespace XForensics.App.Controls;

/// <summary>
/// Modern seamless chromeless Window based on native WindowChrome and VietType custom window patterns.
/// Integrates a dedicated sleek 38px TitleBar with icon, title, subtitle, theme toggle and caption buttons.
/// </summary>
public class XForensicsWindow : Window
{
    private static readonly Geometry MaximizeGeometry = Geometry.Parse("M0.5,0.5 L9.5,0.5 L9.5,9.5 L0.5,9.5 Z");
    private static readonly Geometry RestoreGeometry = Geometry.Parse("M2.5,0.5 L9.5,0.5 L9.5,7.5 L7.5,7.5 M0.5,2.5 L7.5,2.5 L7.5,9.5 L0.5,9.5 Z");

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(XForensicsWindow), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TitleBarContentProperty =
        DependencyProperty.Register(nameof(TitleBarContent), typeof(object), typeof(XForensicsWindow), new PropertyMetadata(null));

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public object? TitleBarContent
    {
        get => GetValue(TitleBarContentProperty);
        set => SetValue(TitleBarContentProperty, value);
    }

    static XForensicsWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(XForensicsWindow),
            new FrameworkPropertyMetadata(typeof(XForensicsWindow)));
    }

    public XForensicsWindow()
    {
        SetResourceReference(StyleProperty, typeof(XForensicsWindow));
        ThemeManager.ThemeChanged += OnGlobalThemeChanged;
    }

    private void OnGlobalThemeChanged(bool isDark)
    {
        UpdateThemeToggleButton(isDark);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_ThemeToggleButton") is Button themeBtn)
        {
            themeBtn.Click += (_, _) => ThemeManager.ToggleTheme();
            UpdateThemeToggleButton(ThemeManager.IsDarkTheme);
        }

        if (GetTemplateChild("PART_MinimizeButton") is Button minBtn)
        {
            if (ResizeMode == ResizeMode.NoResize)
            {
                minBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                minBtn.Visibility = Visibility.Visible;
                minBtn.Click += (_, _) => WindowState = WindowState.Minimized;
            }
        }

        if (GetTemplateChild("PART_MaximizeButton") is Button maxBtn)
        {
            if (ResizeMode == ResizeMode.NoResize || ResizeMode == ResizeMode.CanMinimize)
            {
                maxBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                maxBtn.Visibility = Visibility.Visible;
                maxBtn.Click += (_, _) => ToggleMaximize();
            }
        }

        if (GetTemplateChild("PART_CloseButton") is Button closeBtn)
        {
            closeBtn.Click += (_, _) => OnCloseButtonClick();
        }

        UpdateMaximizeButtonIcon();
    }

    private void UpdateThemeToggleButton(bool isDark)
    {
        if (GetTemplateChild("PART_ThemeIcon") is Path themeIcon &&
            GetTemplateChild("PART_ThemeToggleButton") is Button themeBtn)
        {
            if (isDark)
            {
                themeIcon.Data = TryFindResource("IconSun") as Geometry;
                themeBtn.ToolTip = "Switch to Light Theme";
            }
            else
            {
                themeIcon.Data = TryFindResource("IconMoon") as Geometry;
                themeBtn.ToolTip = "Switch to Dark Theme";
            }
        }
    }

    public void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        UpdateMaximizeButtonIcon();
    }

    public void UpdateMaximizeButtonIcon()
    {
        if (GetTemplateChild("PART_MaximizeIcon") is Path maxIcon &&
            GetTemplateChild("PART_MaximizeButton") is Button maxBtn)
        {
            if (WindowState == WindowState.Maximized)
            {
                maxIcon.Data = RestoreGeometry;
                maxBtn.ToolTip = "Restore";
            }
            else
            {
                maxIcon.Data = MaximizeGeometry;
                maxBtn.ToolTip = "Maximize";
            }
        }
    }

    protected virtual void OnCloseButtonClick() => Close();
}
