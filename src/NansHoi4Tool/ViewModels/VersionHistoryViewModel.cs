using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NansHoi4Tool.Core;
using NansHoi4Tool.Models;
using NansHoi4Tool.Services;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace NansHoi4Tool.ViewModels;

public partial class VersionSnapshot : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _label = string.Empty;
    [ObservableProperty] private string _author = string.Empty;
    [ObservableProperty] private DateTime _timestamp = DateTime.Now;
    [ObservableProperty] private string _summary = string.Empty;
    [ObservableProperty] private bool _isSelected;

    public string FormattedDate => Timestamp.ToString("MMM d, yyyy h:mm tt");

    public string? DataJson { get; set; }
}

public partial class VersionHistoryViewModel : ViewModelBase
{
    private readonly INotificationService _notifications;
    private readonly IDialogService _dialogs;
    private readonly IAppSettingsService _settings;
    private readonly IProjectService _projects;

    [ObservableProperty] private ObservableCollection<VersionSnapshot> _snapshots = new();
    [ObservableProperty] private VersionSnapshot? _selectedSnapshot;
    [ObservableProperty] private VersionSnapshot? _compareSnapshot;
    [ObservableProperty] private bool _showDiff;
    [ObservableProperty] private string _diffText = string.Empty;

    public VersionHistoryViewModel(
        INotificationService notifications,
        IDialogService dialogs,
        IAppSettingsService settings,
        IProjectService projects)
    {
        _notifications = notifications;
        _dialogs = dialogs;
        _settings = settings;
        _projects = projects;

        projects.ProjectOpened += (_, _) => Snapshots.Clear();
        projects.ProjectClosed += (_, _) => { Snapshots.Clear(); SelectedSnapshot = null; DiffText = string.Empty; };
    }

    [RelayCommand]
    private async Task CreateSnapshot()
    {
        if (!_projects.IsProjectOpen)
        {
            _notifications.ShowWarning("No project is open");
            return;
        }
        var label = await _dialogs.ShowInputAsync("Create Snapshot",
            "Enter a name for this snapshot:", $"Snapshot {Snapshots.Count + 1}");
        if (string.IsNullOrWhiteSpace(label)) return;

        var author = _settings.Current.UserName.Length > 0 ? _settings.Current.UserName : "Unknown";
        var snap = new VersionSnapshot
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            Label = label,
            Author = author,
            Timestamp = DateTime.Now,
            Summary = $"{_projects.CurrentProject?.Name ?? "Project"} — manual checkpoint",
            DataJson = JsonConvert.SerializeObject(_projects.CurrentData)
        };
        Snapshots.Insert(0, snap);
        _notifications.ShowSuccess($"Snapshot '{label}' created");
    }

    [RelayCommand]
    private async Task RestoreSnapshot(VersionSnapshot? snap)
    {
        if (snap == null || string.IsNullOrEmpty(snap.DataJson)) return;
        var ok = await _dialogs.ShowConfirmAsync("Restore Snapshot",
            $"Restore project to '{snap.Label}'? Unsaved changes will be lost.");
        if (!ok) return;

        var restored = JsonConvert.DeserializeObject<ProjectData>(snap.DataJson);
        if (restored == null) { _notifications.ShowError("Snapshot data is corrupt"); return; }

        var data = _projects.CurrentData;
        data.FocusTree = restored.FocusTree;
        data.Events = restored.Events;
        data.Ideas = restored.Ideas;
        data.Technologies = restored.Technologies;
        data.Decisions = restored.Decisions;
        data.Country = restored.Country;
        data.Localisation = restored.Localisation;
        data.Units = restored.Units;

        // Re-fire ProjectOpened so all editor VMs reload from the restored data
        _projects.ForceReload();
        _notifications.ShowSuccess($"Restored to '{snap.Label}'");
    }

    [RelayCommand]
    private void CompareWithSelected(VersionSnapshot? snap)
    {
        CompareSnapshot = snap;
        if (snap == null || SelectedSnapshot == null) { ShowDiff = false; DiffText = string.Empty; return; }

        ShowDiff = true;
        DiffText = BuildDiffSummary(SelectedSnapshot, snap);
    }

    [RelayCommand]
    private async Task DeleteSnapshot(VersionSnapshot? snap)
    {
        if (snap == null) return;
        var ok = await _dialogs.ShowConfirmAsync("Delete Snapshot", $"Delete snapshot '{snap.Label}'?");
        if (!ok) return;
        Snapshots.Remove(snap);
        if (SelectedSnapshot == snap) SelectedSnapshot = null;
    }

    private static string BuildDiffSummary(VersionSnapshot a, VersionSnapshot b)
    {
        if (string.IsNullOrEmpty(a.DataJson) || string.IsNullOrEmpty(b.DataJson))
            return "No data available for comparison.";

        var da = JsonConvert.DeserializeObject<ProjectData>(a.DataJson);
        var db = JsonConvert.DeserializeObject<ProjectData>(b.DataJson);
        if (da == null || db == null) return "Could not parse snapshot data.";

        var lines = new System.Text.StringBuilder();
        lines.AppendLine($"Comparing '{a.Label}' → '{b.Label}'");
        lines.AppendLine(new string('─', 50));

        int focusDiff = db.FocusTree.Nodes.Count - da.FocusTree.Nodes.Count;
        if (focusDiff != 0) lines.AppendLine($"Focus nodes: {(focusDiff > 0 ? "+" : "")}{focusDiff}");

        int eventDiff = db.Events.Events.Count - da.Events.Events.Count;
        if (eventDiff != 0) lines.AppendLine($"Events: {(eventDiff > 0 ? "+" : "")}{eventDiff}");

        int ideaDiff = db.Ideas.Ideas.Count - da.Ideas.Ideas.Count;
        if (ideaDiff != 0) lines.AppendLine($"Ideas: {(ideaDiff > 0 ? "+" : "")}{ideaDiff}");

        int locDiff = db.Localisation.Entries.Count - da.Localisation.Entries.Count;
        if (locDiff != 0) lines.AppendLine($"Localisation entries: {(locDiff > 0 ? "+" : "")}{locDiff}");

        if (da.Country.Tag != db.Country.Tag && !string.IsNullOrEmpty(db.Country.Tag))
            lines.AppendLine($"Country tag: {da.Country.Tag} → {db.Country.Tag}");

        if (lines.Length == 0 || lines.ToString().Split('\n').Length <= 2)
            lines.AppendLine("No structural changes detected.");

        return lines.ToString();
    }
}
