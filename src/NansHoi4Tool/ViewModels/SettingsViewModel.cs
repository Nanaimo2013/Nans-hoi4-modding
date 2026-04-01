using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NansHoi4Tool.Core;
using NansHoi4Tool.Services;
using System.Collections.ObjectModel;

namespace NansHoi4Tool.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IAppSettingsService _settings;
    private readonly IThemeService _theme;
    private readonly INotificationService _notifications;
    private readonly IDialogService _dialogs;

    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private string _selectedTheme = "Dark";
    [ObservableProperty] private string _selectedAccent = "Blue";
    [ObservableProperty] private bool _discordEnabled = true;
    [ObservableProperty] private bool _autoSaveEnabled = true;
    [ObservableProperty] private int _autoSaveInterval = 60;
    [ObservableProperty] private string _hoi4InstallPath = string.Empty;
    [ObservableProperty] private int _serverPort = 5050;
    [ObservableProperty] private bool _animationsEnabled = true;

    public ObservableCollection<string> AvailableThemes { get; } = new() { "Dark", "Light" };
    public IReadOnlyList<string> AvailableAccents => _theme.AvailableAccents;

    public SettingsViewModel(
        IAppSettingsService settings,
        IThemeService theme,
        INotificationService notifications,
        IDialogService dialogs)
    {
        _settings = settings;
        _theme = theme;
        _notifications = notifications;
        _dialogs = dialogs;
        LoadFromSettings();
    }

    private void LoadFromSettings()
    {
        var s = _settings.Current;
        UserName = s.UserName;
        SelectedTheme = s.Theme;
        SelectedAccent = s.AccentColor;
        DiscordEnabled = s.DiscordRichPresenceEnabled;
        AutoSaveEnabled = s.AutoSaveEnabled;
        AutoSaveInterval = s.AutoSaveIntervalSeconds;
        Hoi4InstallPath = s.Hoi4InstallPath;
        ServerPort = s.Collaboration.ServerPort;
        AnimationsEnabled = s.Appearance.AnimationsEnabled;
    }

    partial void OnSelectedThemeChanged(string value) => _theme.SetTheme(value);
    partial void OnSelectedAccentChanged(string value) => _theme.SetAccent(value);

    [RelayCommand]
    private void Save()
    {
        var s = _settings.Current;
        s.UserName = UserName;
        s.DiscordRichPresenceEnabled = DiscordEnabled;
        s.AutoSaveEnabled = AutoSaveEnabled;
        s.AutoSaveIntervalSeconds = AutoSaveInterval;
        s.Hoi4InstallPath = Hoi4InstallPath;
        s.Collaboration.ServerPort = ServerPort;
        s.Appearance.AnimationsEnabled = AnimationsEnabled;
        _settings.Save();
        _notifications.ShowSuccess("Settings saved!");
    }

    [RelayCommand]
    private void BrowseHoi4Path()
    {
        var path = _dialogs.OpenFolderDialog("Select HOI4 Install Folder");
        if (path != null) Hoi4InstallPath = path;
    }
}
