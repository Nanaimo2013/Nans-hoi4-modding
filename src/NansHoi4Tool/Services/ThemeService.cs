using ControlzEx.Theming;
using NansHoi4Tool.Models;

namespace NansHoi4Tool.Services;

public class ThemeService : IThemeService
{
    private readonly IAppSettingsService _settings;

    public string CurrentTheme { get; private set; } = "Dark";
    public string CurrentAccent { get; private set; } = "Blue";

    public IReadOnlyList<string> AvailableAccents { get; } = new List<string>
    {
        "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald",
        "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta",
        "Crimson", "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna"
    };

    public ThemeService(IAppSettingsService settings)
    {
        _settings = settings;
        CurrentTheme = settings.Current.Theme;
        CurrentAccent = settings.Current.AccentColor;
    }

    public void SetTheme(string theme)
    {
        CurrentTheme = theme;
        Apply();
        _settings.Current.Theme = theme;
        _settings.Save();
    }

    public void SetAccent(string accent)
    {
        CurrentAccent = accent;
        Apply();
        _settings.Current.AccentColor = accent;
        _settings.Save();
    }

    public void Apply()
    {
        ThemeManager.Current.ChangeTheme(System.Windows.Application.Current, $"{CurrentTheme}.{CurrentAccent}");
    }
}
