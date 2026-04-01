using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NansHoi4Tool.Models;
using NansHoi4Tool.Services;
using System.Collections.ObjectModel;

namespace NansHoi4Tool.ViewModels;

public partial class TechBonus : ObservableObject
{
    [ObservableProperty] private string _key = string.Empty;
    [ObservableProperty] private string _value = "0.1";
}

public partial class Technology : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private int _researchCost = 1;
    [ObservableProperty] private int _startYear = 1936;
    [ObservableProperty] private string _folder = string.Empty;
    [ObservableProperty] private double _x;
    [ObservableProperty] private double _y;
    [ObservableProperty] private List<string> _prerequisites = new();
    [ObservableProperty] private ObservableCollection<TechBonus> _bonuses = new();
    [ObservableProperty] private bool _isSelected;
}

public class TechConnection
{
    private const double GridCellPx = 100;
    private const double NodeHalf   = 64;

    public double X1 { get; init; }
    public double Y1 { get; init; }
    public double X2 { get; init; }
    public double Y2 { get; init; }

    public static TechConnection? Build(Technology from, Technology? to)
    {
        if (to == null) return null;
        return new TechConnection
        {
            X1 = from.X * GridCellPx + NodeHalf, Y1 = from.Y * GridCellPx + NodeHalf,
            X2 = to.X   * GridCellPx + NodeHalf, Y2 = to.Y   * GridCellPx + NodeHalf
        };
    }
}

public partial class TechnologiesViewModel : EditorViewModelBase
{
    [ObservableProperty] private ObservableCollection<Technology> _technologies = new();
    [ObservableProperty] private Technology? _selectedTech;
    [ObservableProperty] private string _categoryId = "infantry";
    [ObservableProperty] private ObservableCollection<TechConnection> _connections = new();

    public TechnologiesViewModel(INotificationService notifications, ILogService log, IProjectService projects)
        : base(notifications, log, projects)
    {
        Technologies.CollectionChanged += (_, _) => RebuildConnections();
    }

    public void RebuildConnections()
    {
        Connections.Clear();
        var lookup = Technologies.ToDictionary(t => t.Id);
        foreach (var tech in Technologies)
            foreach (var prereqId in tech.Prerequisites)
            {
                var conn = TechConnection.Build(tech, lookup.GetValueOrDefault(prereqId));
                if (conn != null) Connections.Add(conn);
            }
    }

    public void NotifyNodeMoved()
    {
        RebuildConnections();
        MarkDirty();
    }

    partial void OnSelectedTechChanged(Technology? oldValue, Technology? newValue)
    {
        if (oldValue != null) oldValue.IsSelected = false;
        if (newValue != null) newValue.IsSelected = true;
    }

    [RelayCommand]
    private void SelectTech(Technology? tech) => SelectedTech = tech;

    protected override void OnProjectOpened(ProjectData data)
    {
        Technologies.Clear();
        CategoryId = data.Technologies.CategoryId;
        foreach (var dto in data.Technologies.Technologies)
            Technologies.Add(new Technology
            {
                Id = dto.Id, Name = dto.Name, Description = dto.Description,
                ResearchCost = dto.ResearchCost, StartYear = dto.StartYear,
                Folder = dto.Folder, X = dto.X, Y = dto.Y,
                Prerequisites = new List<string>(dto.Prerequisites),
                Bonuses = new ObservableCollection<TechBonus>(
                    dto.Bonuses.Select(b => new TechBonus { Key = b.Key, Value = b.Value }))
            });
    }

    protected override void OnProjectClosed()
    {
        Technologies.Clear();
        Connections.Clear();
        SelectedTech = null;
        CategoryId = "infantry";
    }

    protected override void SyncToProjectData(ProjectData data)
    {
        data.Technologies.CategoryId = CategoryId;
        data.Technologies.Technologies = Technologies.Select(t => new TechnologyDto
        {
            Id = t.Id, Name = t.Name, Description = t.Description,
            ResearchCost = t.ResearchCost, StartYear = t.StartYear,
            Folder = t.Folder, X = t.X, Y = t.Y,
            Prerequisites = new List<string>(t.Prerequisites),
            Bonuses = t.Bonuses.Select(b => new ModifierDto { Key = b.Key, Value = b.Value }).ToList()
        }).ToList();
    }

    [RelayCommand]
    private void AddTechnology()
    {
        var tech = new Technology
        {
            Id = $"tech_{Technologies.Count + 1}",
            Name = "New Technology",
            X = (Technologies.Count % 6) * 2,
            Y = (Technologies.Count / 6) * 2
        };
        Technologies.Add(tech);
        SelectedTech = tech;
        MarkDirty();
    }

    [RelayCommand]
    private void DeleteTechnology(Technology? tech)
    {
        if (tech == null) return;
        Technologies.Remove(tech);
        if (SelectedTech == tech) SelectedTech = null;
        MarkDirty();
    }

    [RelayCommand]
    private void AddPrerequisite(string prereqId)
    {
        if (SelectedTech == null || string.IsNullOrWhiteSpace(prereqId)) return;
        if (SelectedTech.Id == prereqId) return;
        if (!SelectedTech.Prerequisites.Contains(prereqId))
        {
            SelectedTech.Prerequisites = new List<string>(SelectedTech.Prerequisites) { prereqId };
            RebuildConnections();
            MarkDirty();
        }
    }

    [RelayCommand]
    private void RemovePrerequisite(string prereqId)
    {
        if (SelectedTech == null) return;
        SelectedTech.Prerequisites = SelectedTech.Prerequisites.Where(p => p != prereqId).ToList();
        RebuildConnections();
        MarkDirty();
    }

    [RelayCommand]
    private void AddBonus()
    {
        if (SelectedTech == null) return;
        SelectedTech.Bonuses.Add(new TechBonus { Key = "modifier", Value = "0.1" });
        MarkDirty();
    }

    [RelayCommand]
    private void RemoveBonus(TechBonus? bonus)
    {
        if (bonus == null || SelectedTech == null) return;
        SelectedTech.Bonuses.Remove(bonus);
        MarkDirty();
    }
}
