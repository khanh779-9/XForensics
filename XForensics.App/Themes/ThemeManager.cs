using System;
using System.Linq;
using System.Windows;

namespace XForensics.App.Themes;

public static class ThemeManager
{
    private const string DarkThemeUri = "Themes/Dark.xaml";
    private const string LightThemeUri = "Themes/Light.xaml";

    public static bool IsDarkTheme { get; private set; } = true;

    public static event Action<bool>? ThemeChanged;

    public static void Initialize()
    {
        SetTheme(true);
    }

    public static void ToggleTheme()
    {
        SetTheme(!IsDarkTheme);
    }

    public static void SetTheme(bool isDark)
    {
        IsDarkTheme = isDark;
        var targetUri = isDark ? DarkThemeUri : LightThemeUri;

        try
        {
            var dictUri = new Uri(targetUri, UriKind.Relative);
            var newDict = new ResourceDictionary { Source = dictUri };

            var appResources = Application.Current.Resources;
            var existingThemeDict = appResources.MergedDictionaries.FirstOrDefault(d =>
                d.Source != null && (d.Source.OriginalString.Contains("Dark.xaml") || d.Source.OriginalString.Contains("Light.xaml") ||
                                     d.Source.OriginalString.Contains("DarkTheme.xaml") || d.Source.OriginalString.Contains("LightTheme.xaml")));

            if (existingThemeDict != null)
            {
                var index = appResources.MergedDictionaries.IndexOf(existingThemeDict);
                appResources.MergedDictionaries[index] = newDict;
            }
            else
            {
                appResources.MergedDictionaries.Insert(0, newDict);
            }

            ThemeChanged?.Invoke(isDark);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set theme: {ex.Message}");
        }
    }
}
