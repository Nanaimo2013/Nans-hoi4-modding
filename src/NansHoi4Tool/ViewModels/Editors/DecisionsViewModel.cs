using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NansHoi4Tool.Models;
using NansHoi4Tool.Services;
using System.Collections.ObjectModel;

namespace NansHoi4Tool.ViewModels;

public partial class Decision : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _icon = "GFX_decision_generic_political_discourse";
    [ObservableProperty] private int _costPoliticalPower = 50;
    [ObservableProperty] private int _daysReDo = 0;
    [ObservableProperty] private int _daysRemove = 0;
    [ObservableProperty] private string _available = string.Empty;
    [ObservableProperty] private string _visible = string.Empty;
    [ObservableProperty] private string _effect = string.Empty;
    [ObservableProperty] private string _removeEffect = string.Empty;
}

public partial class DecisionCategory : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _icon = string.Empty;
    [ObservableProperty] private ObservableCollection<Decision> _decisions = new();
}

public partial class DecisionsViewModel : EditorViewModelBase
{
    [ObservableProperty] private ObservableCollection<DecisionCategory> _categories = new();
    [ObservableProperty] private DecisionCategory? _selectedCategory;
    [ObservableProperty] private Decision? _selectedDecision;

    public DecisionsViewModel(INotificationService notifications, ILogService log, IProjectService projects)
        : base(notifications, log, projects) { }

    protected override void OnProjectOpened(ProjectData data)
    {
        Categories.Clear();
        foreach (var dto in data.Decisions.Categories)
        {
            var cat = new DecisionCategory { Id = dto.Id, Name = dto.Name, Icon = dto.Icon };
            foreach (var d in dto.Decisions)
                cat.Decisions.Add(new Decision
                {
                    Id = d.Id, Name = d.Name, Description = d.Description,
                    Icon = d.Icon, CostPoliticalPower = d.CostPoliticalPower,
                    DaysReDo = d.DaysReDo, DaysRemove = d.DaysRemove,
                    Available = d.Available, Visible = d.Visible,
                    Effect = d.Effect, RemoveEffect = d.RemoveEffect
                });
            Categories.Add(cat);
        }
    }

    protected override void OnProjectClosed() { Categories.Clear(); SelectedCategory = null; SelectedDecision = null; }

    protected override void SyncToProjectData(ProjectData data)
    {
        data.Decisions.Categories = Categories.Select(c => new DecisionCategoryDto
        {
            Id = c.Id, Name = c.Name, Icon = c.Icon,
            Decisions = c.Decisions.Select(d => new DecisionDto
            {
                Id = d.Id, Name = d.Name, Description = d.Description,
                Icon = d.Icon, CostPoliticalPower = d.CostPoliticalPower,
                DaysReDo = d.DaysReDo, DaysRemove = d.DaysRemove,
                Available = d.Available, Visible = d.Visible,
                Effect = d.Effect, RemoveEffect = d.RemoveEffect
            }).ToList()
        }).ToList();
    }

    [RelayCommand]
    private void AddCategory()
    {
        var cat = new DecisionCategory { Id = $"cat_{Categories.Count + 1}", Name = "New Category" };
        Categories.Add(cat);
        SelectedCategory = cat;
        MarkDirty();
    }

    [RelayCommand]
    private void DeleteCategory(DecisionCategory? cat)
    {
        if (cat == null) return;
        Categories.Remove(cat);
        if (SelectedCategory == cat) { SelectedCategory = null; SelectedDecision = null; }
        MarkDirty();
    }

    [RelayCommand]
    private void AddDecision()
    {
        if (SelectedCategory == null) return;
        var d = new Decision { Id = $"decision_{SelectedCategory.Decisions.Count + 1}", Name = "New Decision" };
        SelectedCategory.Decisions.Add(d);
        SelectedDecision = d;
        MarkDirty();
    }

    [RelayCommand]
    private void DeleteDecision(Decision? d)
    {
        if (d == null || SelectedCategory == null) return;
        SelectedCategory.Decisions.Remove(d);
        if (SelectedDecision == d) SelectedDecision = null;
        MarkDirty();
    }
}
