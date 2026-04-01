namespace NansHoi4Tool.Services;

public interface IThemeService
{
    string CurrentTheme { get; }
    string CurrentAccent { get; }
    void SetTheme(string theme);
    void SetAccent(string accent);
    IReadOnlyList<string> AvailableAccents { get; }
}
