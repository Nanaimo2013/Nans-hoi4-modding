using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NansHoi4Tool.Core;
using NansHoi4Tool.Services;

namespace NansHoi4Tool.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly IAppSettingsService _settings;
    private readonly INotificationService _notifications;
    private readonly IProjectService _projects;
    private readonly IDialogService _dialogs;
    private readonly IDiscordService _discord;
    private readonly DashboardViewModel _dashboard;

    [ObservableProperty] private string _currentPageKey = "Dashboard";
    [ObservableProperty] private bool _sidebarExpanded = true;
    [ObservableProperty] private string _windowTitle = "Nan's Hoi4 Tool";
    [ObservableProperty] private string _statusText = "Ready";
    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private bool _hasActiveProject;
    [ObservableProperty] private string _activeProjectName = string.Empty;

    public string VersionDisplay => AppVersion.Display;

    public MainWindowViewModel(
        INavigationService navigation,
        IAppSettingsService settings,
        INotificationService notifications,
        IProjectService projects,
        IDialogService dialogs,
        IDiscordService discord,
        DashboardViewModel dashboard)
    {
        _navigation = navigation;
        _settings = settings;
        _dashboard = dashboard;
        _notifications = notifications;
        _projects = projects;
        _dialogs = dialogs;
        _discord = discord;

        UserName = settings.Current.UserName.Length > 0 ? settings.Current.UserName : "Modder";
        SidebarExpanded = !settings.Current.Appearance.SidebarCollapsed;

        _navigation.Navigated += (_, key) =>
        {
            CurrentPageKey = key;
            UpdateWindowTitle(key);
            if (HasActiveProject)
                _discord.SetProjectPresence(ActiveProjectName, key);
            else
                _discord.SetPresence("Browsing", key == "Dashboard" ? "Dashboard" : key);
        };

        _projects.ProjectOpened += (_, meta) =>
        {
            HasActiveProject = true;
            ActiveProjectName = meta.Name;
            UpdateWindowTitle(CurrentPageKey);
            StatusText = $"Opened: {meta.Name}";
            _discord.SetProjectPresence(meta.Name, CurrentPageKey);
        };

        _projects.ProjectClosed += (_, _) =>
        {
            HasActiveProject = false;
            ActiveProjectName = string.Empty;
            UpdateWindowTitle(CurrentPageKey);
            _discord.SetPresence("In the launcher", "No project open");
        };

        _projects.ProjectChanged += (_, _) =>
        {
            StatusText = HasActiveProject ? $"{ActiveProjectName} — unsaved changes" : "Ready";
        };
    }

    [RelayCommand]
    private void Navigate(string pageKey) => _navigation.NavigateTo(pageKey);

    [RelayCommand]
    private void ToggleSidebar()
    {
        SidebarExpanded = !SidebarExpanded;
        _settings.Current.Appearance.SidebarCollapsed = !SidebarExpanded;
        _settings.Save();
    }

    [RelayCommand]
    private async Task SaveProject()
    {
        if (!_projects.IsProjectOpen) return;
        IsBusy = true;
        StatusText = "Saving...";
        await _projects.SaveProjectAsync();
        IsBusy = false;
        StatusText = $"{ActiveProjectName} — saved";
    }

    [RelayCommand]
    private async Task SaveProjectAs()
    {
        if (!_projects.IsProjectOpen) return;
        var path = _dialogs.SaveFileDialog(
            "HOI4 Project (*.h4proj)|*.h4proj",
            "Save Project As",
            $"{ActiveProjectName}.h4proj");
        if (string.IsNullOrEmpty(path)) return;
        IsBusy = true;
        StatusText = "Saving...";
        await _projects.SaveProjectAsAsync(path);
        IsBusy = false;
        StatusText = $"{ActiveProjectName} — saved";
    }

    [RelayCommand]
    private async Task ExportMod()
    {
        if (!_projects.IsProjectOpen) { _notifications.ShowWarning("No project open"); return; }
        var folder = _dialogs.OpenFolderDialog("Select HOI4 mod output folder");
        if (string.IsNullOrEmpty(folder)) return;
        IsBusy = true;
        await _projects.ExportToHoi4Async(folder);
        IsBusy = false;
    }

    [RelayCommand]
    private async Task NewProject()
    {
        _navigation.NavigateTo("Dashboard");
        await _dashboard.NewProjectCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task OpenProject()
    {
        _navigation.NavigateTo("Dashboard");
        await _dashboard.OpenProjectCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private void CloseProject()
    {
        _projects.CloseProject();
        _navigation.NavigateTo("Dashboard");
    }

    private void UpdateWindowTitle(string pageKey)
    {
        var suffix = HasActiveProject ? $" — {ActiveProjectName}" : string.Empty;
        WindowTitle = $"Nan's Hoi4 Tool{suffix}";
        StatusText = pageKey switch
        {
            "Dashboard" => HasActiveProject ? $"Project: {ActiveProjectName}" : "No project open",
            "FocusTree" => "Focus Tree Editor",
            "Events" => "Event Editor",
            "Ideas" => "National Spirits & Ideas",
            "Technologies" => "Technology Editor",
            "Decisions" => "Decisions Editor",
            "Country" => "Country Editor",
            "Localisation" => "Localisation Editor",
            "Units" => "Unit & Equipment Editor",
            "Settings" => "Settings",
            "VersionHistory" => "Version History",
            _ => pageKey
        };
    }
}
