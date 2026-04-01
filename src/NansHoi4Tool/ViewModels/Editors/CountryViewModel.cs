using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NansHoi4Tool.Models;
using NansHoi4Tool.Services;
using System.Collections.ObjectModel;

namespace NansHoi4Tool.ViewModels;

// ── Per-country entry (one per tab) ──────────────────────────────
public partial class CountryEntry : ObservableObject
{
    [ObservableProperty] private string _entryId = Guid.NewGuid().ToString("N")[..8];
    [ObservableProperty] private string _tag = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _adjective = string.Empty;
    [ObservableProperty] private string _flagPath = string.Empty;
    [ObservableProperty] private string _ideology = "neutrality";
    [ObservableProperty] private string _graphicalCulture = "western_european_gfx";
    [ObservableProperty] private string _graphicalCulture2d = "western_european_2d";
    [ObservableProperty] private int    _capitalState = 1;
    [ObservableProperty] private double _colorR = 0.5;
    [ObservableProperty] private double _colorG = 0.5;
    [ObservableProperty] private double _colorB = 0.5;
    [ObservableProperty] private string _startingTechnology = string.Empty;
    [ObservableProperty] private string _oobPath = string.Empty;

    [ObservableProperty] private bool _isActive;
    [ObservableProperty] private bool _canClose = true;

    public string Label => string.IsNullOrEmpty(Tag) ? "New Country" : Tag;
    partial void OnTagChanged(string value) => OnPropertyChanged(nameof(Label));
}

// ── ViewModel ─────────────────────────────────────────────────────
public partial class CountryViewModel : EditorViewModelBase
{
    private readonly IDialogService _dialogs;

    [ObservableProperty] private ObservableCollection<CountryEntry> _countries = new();
    [ObservableProperty] private CountryEntry? _activeCountry;

    public static IReadOnlyList<string> AvailableIdeologies { get; } = new[]
        { "neutrality", "democratic", "fascism", "communism" };

    public static IReadOnlyList<string> AvailableGraphicalCultures { get; } = new[]
    {
        "western_european_gfx", "eastern_european_gfx", "middle_eastern_gfx",
        "african_gfx", "asian_gfx", "southamerican_gfx", "commonwealth_gfx", "us_gfx"
    };

    public CountryViewModel(INotificationService notifications, ILogService log,
        IProjectService projects, IDialogService dialogs)
        : base(notifications, log, projects)
    {
        _dialogs = dialogs;
    }

    protected override void OnProjectOpened(ProjectData data)
    {
        Countries.Clear();
        foreach (var d in data.Countries)
            Countries.Add(EntryFromDto(d));
        ActiveCountry = Countries.FirstOrDefault();
        UpdateCanClose();
    }

    protected override void OnProjectClosed()
    {
        Countries.Clear();
        ActiveCountry = null;
    }

    protected override void SyncToProjectData(ProjectData data)
    {
        data.Countries = Countries.Select(e => new CountryData
        {
            Tag = e.Tag, Name = e.Name, Adjective = e.Adjective,
            FlagPath = e.FlagPath, Ideology = e.Ideology,
            GraphicalCulture = e.GraphicalCulture, GraphicalCulture2d = e.GraphicalCulture2d,
            CapitalState = e.CapitalState,
            ColorR = e.ColorR, ColorG = e.ColorG, ColorB = e.ColorB,
            StartingTechnology = e.StartingTechnology, OobPath = e.OobPath
        }).ToList();
    }

    partial void OnActiveCountryChanged(CountryEntry? oldValue, CountryEntry? newValue)
    {
        if (oldValue != null) oldValue.IsActive = false;
        if (newValue != null) newValue.IsActive = true;
        UpdateCanClose();
    }

    private void UpdateCanClose()
    {
        var canClose = Countries.Count > 1;
        foreach (var c in Countries) c.CanClose = canClose;
    }

    [RelayCommand]
    private void AddCountry()
    {
        var entry = new CountryEntry { Tag = string.Empty, Name = "New Country" };
        Countries.Add(entry);
        ActiveCountry = entry;
        UpdateCanClose();
        MarkDirty();
        Notifications.Show("New country added — set its TAG to get started.", NotificationType.Info);
    }

    [RelayCommand]
    private void RemoveCountry(CountryEntry? entry)
    {
        if (entry == null || Countries.Count <= 1) return;
        Countries.Remove(entry);
        if (ActiveCountry == entry) ActiveCountry = Countries.FirstOrDefault();
        UpdateCanClose();
        MarkDirty();
    }

    [RelayCommand]
    private void SelectCountry(CountryEntry? entry)
    {
        if (entry != null) ActiveCountry = entry;
    }

    [RelayCommand]
    private void BrowseFlag()
    {
        if (ActiveCountry == null) return;
        var path = _dialogs.OpenFileDialog("Image Files|*.tga;*.png;*.dds|All Files|*.*", "Select Flag Image");
        if (!string.IsNullOrEmpty(path))
        {
            ActiveCountry.FlagPath = path;
            MarkDirty();
        }
    }

    private static CountryEntry EntryFromDto(CountryData d) => new()
    {
        Tag = d.Tag, Name = d.Name, Adjective = d.Adjective,
        FlagPath = d.FlagPath, Ideology = d.Ideology,
        GraphicalCulture = d.GraphicalCulture, GraphicalCulture2d = d.GraphicalCulture2d,
        CapitalState = d.CapitalState,
        ColorR = d.ColorR, ColorG = d.ColorG, ColorB = d.ColorB,
        StartingTechnology = d.StartingTechnology, OobPath = d.OobPath
    };
}
