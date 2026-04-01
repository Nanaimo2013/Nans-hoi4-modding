using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NansHoi4Tool.Core;
using NansHoi4Tool.Services;
using System.Collections.ObjectModel;

namespace NansHoi4Tool.ViewModels;

public partial class RecentProjectItem : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _path = string.Empty;
    [ObservableProperty] private string _lastEdited = string.Empty;
    [ObservableProperty] private string _modType = string.Empty;
}

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IAppSettingsService _settings;
    private readonly INavigationService _navigation;
    private readonly INotificationService _notifications;
    private readonly IProjectService _projects;
    private readonly IDialogService _dialogs;

    [ObservableProperty] private string _greeting = string.Empty;
    [ObservableProperty] private ObservableCollection<RecentProjectItem> _recentProjects = new();

    public DashboardViewModel(
        IAppSettingsService settings,
        INavigationService navigation,
        INotificationService notifications,
        IProjectService projects,
        IDialogService dialogs)
    {
        _settings = settings;
        _navigation = navigation;
        _notifications = notifications;
        _projects = projects;
        _dialogs = dialogs;

        var hour = DateTime.Now.Hour;
        var name = settings.Current.UserName.Length > 0 ? settings.Current.UserName : "Modder";
        Greeting = hour switch
        {
            < 12 => $"Good morning, {name}!",
            < 17 => $"Good afternoon, {name}!",
            _ => $"Good evening, {name}!"
        };

        LoadRecentProjects();
    }

    private void LoadRecentProjects()
    {
        RecentProjects.Clear();
        foreach (var path in _settings.Current.RecentProjectPaths)
        {
            if (!File.Exists(path)) continue;
            var info = new FileInfo(path);
            RecentProjects.Add(new RecentProjectItem
            {
                Name = Path.GetFileNameWithoutExtension(path),
                Path = path,
                LastEdited = info.LastWriteTime.ToString("MMM d, yyyy h:mm tt"),
                ModType = "HOI4 Mod"
            });
        }
    }

    [RelayCommand]
    private async Task NewProject()
    {
        var name = await _dialogs.ShowInputAsync("New Project", "Enter project name:", "My Mod");
        if (string.IsNullOrWhiteSpace(name)) return;

        var modId = await _dialogs.ShowInputAsync("Mod ID", "Enter mod ID (no spaces, lowercase):",
            name.ToLower().Replace(" ", "_"));
        if (string.IsNullOrWhiteSpace(modId)) return;

        // Default to Documents\NansHoi4Tool\Projects but let user pick
        var defaultFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "NansHoi4Tool", "Projects");
        var folder = _dialogs.OpenFolderDialog($"Choose where to save '{name}' (a subfolder will be created)");
        if (string.IsNullOrEmpty(folder))
            folder = defaultFolder;

        var author = _settings.Current.UserName.Length > 0 ? _settings.Current.UserName : "Modder";
        IsBusy = true;
        var ok = await _projects.NewProjectAsync(name, modId, author, folder);
        IsBusy = false;
        if (ok)
        {
            _notifications.ShowSuccess($"Project '{name}' created and saved!");
            LoadRecentProjects();
            _navigation.NavigateTo("FocusTree");
        }
    }

    [RelayCommand]
    private async Task OpenProject()
    {
        var path = _dialogs.OpenFileDialog("HOI4 Project (*.h4proj)|*.h4proj|All Files|*.*", "Open Project");
        if (string.IsNullOrEmpty(path)) return;
        IsBusy = true;
        var ok = await _projects.OpenProjectAsync(path);
        IsBusy = false;
        if (ok)
        {
            LoadRecentProjects();
            _navigation.NavigateTo("Dashboard");
        }
    }

    [RelayCommand]
    private async Task ImportMod()
    {
        var folder = _dialogs.OpenFolderDialog("Select existing HOI4 mod folder to import");
        if (string.IsNullOrEmpty(folder)) return;
        IsBusy = true;
        var ok = await _projects.ImportFromHoi4Async(folder);
        IsBusy = false;
        if (ok) _navigation.NavigateTo("Country");
    }

    [RelayCommand]
    private async Task OpenRecentProject(string path)
    {
        IsBusy = true;
        var ok = await _projects.OpenProjectAsync(path);
        IsBusy = false;
        if (!ok)
        {
            _settings.Current.RecentProjectPaths.Remove(path);
            _settings.Save();
            LoadRecentProjects();
        }
        else
        {
            _navigation.NavigateTo("FocusTree");
        }
    }

    [RelayCommand]
    private void PinProject(string path)
    {
        var paths = _settings.Current.RecentProjectPaths;
        if (paths.Contains(path))
        {
            paths.Remove(path);
            paths.Insert(0, path);
            _settings.Save();
            LoadRecentProjects();
            _notifications.Show("Project pinned to top.", NotificationType.Info);
        }
    }
}
