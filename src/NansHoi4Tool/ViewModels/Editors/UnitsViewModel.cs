using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NansHoi4Tool.Models;
using NansHoi4Tool.Services;
using System.Collections.ObjectModel;

namespace NansHoi4Tool.ViewModels;

public partial class UnitStat : ObservableObject
{
    [ObservableProperty] private string _key = string.Empty;
    [ObservableProperty] private string _value = "0";
}

public partial class UnitDefinition : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _abbreviation = string.Empty;
    [ObservableProperty] private string _type = "land";
    [ObservableProperty] private string _sprite = string.Empty;
    [ObservableProperty] private string _mapIconCategory = "infantry";
    [ObservableProperty] private int _priority = 600;
    [ObservableProperty] private bool _isResizable = false;
    [ObservableProperty] private ObservableCollection<UnitStat> _stats = new();
}

public partial class UnitsViewModel : EditorViewModelBase
{
    [ObservableProperty] private ObservableCollection<UnitDefinition> _units = new();
    [ObservableProperty] private UnitDefinition? _selectedUnit;
    [ObservableProperty] private string _selectedUnitType = "All";

    public static IReadOnlyList<string> UnitTypes { get; } = new[] { "All", "land", "naval", "air" };

    public UnitsViewModel(INotificationService notifications, ILogService log, IProjectService projects)
        : base(notifications, log, projects) { }

    protected override void OnProjectOpened(ProjectData data)
    {
        Units.Clear();
        foreach (var dto in data.Units.Units)
            Units.Add(new UnitDefinition
            {
                Id = dto.Id, Name = dto.Name, Abbreviation = dto.Abbreviation,
                Type = dto.Type, Sprite = dto.Sprite,
                MapIconCategory = dto.MapIconCategory, Priority = dto.Priority,
                IsResizable = dto.IsResizable,
                Stats = new ObservableCollection<UnitStat>(
                    dto.Stats.Select(s => new UnitStat { Key = s.Key, Value = s.Value }))
            });
    }

    protected override void OnProjectClosed() { Units.Clear(); SelectedUnit = null; }

    protected override void SyncToProjectData(ProjectData data)
    {
        data.Units.Units = Units.Select(u => new UnitDto
        {
            Id = u.Id, Name = u.Name, Abbreviation = u.Abbreviation,
            Type = u.Type, Sprite = u.Sprite,
            MapIconCategory = u.MapIconCategory, Priority = u.Priority,
            IsResizable = u.IsResizable,
            Stats = u.Stats.Select(s => new ModifierDto { Key = s.Key, Value = s.Value }).ToList()
        }).ToList();
    }

    [RelayCommand]
    private void AddUnit()
    {
        var unit = new UnitDefinition
        {
            Id = $"unit_{Units.Count + 1}",
            Name = "New Unit",
            Abbreviation = "NU"
        };
        Units.Add(unit);
        SelectedUnit = unit;
        MarkDirty();
    }

    [RelayCommand]
    private void DeleteUnit(UnitDefinition? unit)
    {
        if (unit == null) return;
        Units.Remove(unit);
        if (SelectedUnit == unit) SelectedUnit = null;
        MarkDirty();
    }

    [RelayCommand]
    private void AddStat()
    {
        SelectedUnit?.Stats.Add(new UnitStat());
        MarkDirty();
    }

    [RelayCommand]
    private void RemoveStat(UnitStat? stat)
    {
        SelectedUnit?.Stats.Remove(stat!);
        MarkDirty();
    }
}
