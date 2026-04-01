using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NansHoi4Tool.Models;
using NansHoi4Tool.Services;
using System.Collections.ObjectModel;

namespace NansHoi4Tool.ViewModels;

public partial class IdeaModifier : ObservableObject
{
    [ObservableProperty] private string _key = string.Empty;
    [ObservableProperty] private string _value = "0.1";
}

public partial class NationalIdea : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _picture = "GFX_idea_generic_political_advisor";
    [ObservableProperty] private string _slot = "political_advisor";
    [ObservableProperty] private string _ideaToken = string.Empty;
    [ObservableProperty] private bool _allowedCivilWar = true;
    [ObservableProperty] private string _available = string.Empty;
    [ObservableProperty] private ObservableCollection<IdeaModifier> _modifiers = new();
}

public partial class IdeasViewModel : EditorViewModelBase
{
    [ObservableProperty] private ObservableCollection<NationalIdea> _ideas = new();
    [ObservableProperty] private NationalIdea? _selectedIdea;

    public static IReadOnlyList<string> AvailableSlots { get; } = new[]
    {
        "political_advisor", "army_chief", "navy_chief", "air_chief",
        "high_command", "tank_manufacturer", "naval_manufacturer",
        "aircraft_manufacturer", "industrial_concern", "theorist",
        "country", "hidden_ideas"
    };

    public IdeasViewModel(INotificationService notifications, ILogService log, IProjectService projects)
        : base(notifications, log, projects) { }

    protected override void OnProjectOpened(ProjectData data)
    {
        Ideas.Clear();
        foreach (var dto in data.Ideas.Ideas)
        {
            var idea = new NationalIdea
            {
                Id = dto.Id, Name = dto.Name, Description = dto.Description,
                Picture = dto.Picture, Slot = dto.Slot, IdeaToken = dto.IdeaToken,
                AllowedCivilWar = dto.AllowedCivilWar, Available = dto.Available
            };
            foreach (var m in dto.Modifiers)
                idea.Modifiers.Add(new IdeaModifier { Key = m.Key, Value = m.Value });
            Ideas.Add(idea);
        }
    }

    protected override void OnProjectClosed() { Ideas.Clear(); SelectedIdea = null; }

    protected override void SyncToProjectData(ProjectData data)
    {
        data.Ideas.Ideas = Ideas.Select(i => new IdeaDto
        {
            Id = i.Id, Name = i.Name, Description = i.Description,
            Picture = i.Picture, Slot = i.Slot, IdeaToken = i.IdeaToken,
            AllowedCivilWar = i.AllowedCivilWar, Available = i.Available,
            Modifiers = i.Modifiers.Select(m => new ModifierDto { Key = m.Key, Value = m.Value }).ToList()
        }).ToList();
    }

    [RelayCommand]
    private void SelectIdea(NationalIdea? idea) => SelectedIdea = idea;

    [RelayCommand]
    private void AddIdea()
    {
        var idea = new NationalIdea
        {
            Id = $"idea_{Ideas.Count + 1}",
            Name = "New Idea",
            IdeaToken = $"idea_{Ideas.Count + 1}"
        };
        Ideas.Add(idea);
        SelectedIdea = idea;
        MarkDirty();
    }

    [RelayCommand]
    private void DeleteIdea(NationalIdea? idea)
    {
        if (idea == null) return;
        Ideas.Remove(idea);
        if (SelectedIdea == idea) SelectedIdea = null;
        MarkDirty();
    }

    [RelayCommand]
    private void AddModifier()
    {
        SelectedIdea?.Modifiers.Add(new IdeaModifier());
        MarkDirty();
    }

    [RelayCommand]
    private void RemoveModifier(IdeaModifier? mod)
    {
        SelectedIdea?.Modifiers.Remove(mod!);
        MarkDirty();
    }
}
