using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NansHoi4Tool.Models;
using NansHoi4Tool.Services;
using System.Collections.ObjectModel;

namespace NansHoi4Tool.ViewModels;

public partial class EventOption : ObservableObject
{
    [ObservableProperty] private string _name = "Option A";
    [ObservableProperty] private string _trigger = string.Empty;
    [ObservableProperty] private string _effect = string.Empty;
}

public partial class Hoi4Event : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _picture = "GFX_report_event_generic_read_write";
    [ObservableProperty] private string _type = "country_event";
    [ObservableProperty] private bool _isTriggeredOnly = true;
    [ObservableProperty] private bool _hidden;
    [ObservableProperty] private bool _fire_only_once;
    [ObservableProperty] private string _trigger = string.Empty;
    [ObservableProperty] private string _mean_time_to_happen = string.Empty;
    [ObservableProperty] private ObservableCollection<EventOption> _options = new();
}

public partial class EventsViewModel : EditorViewModelBase
{
    [ObservableProperty] private ObservableCollection<Hoi4Event> _events = new();
    [ObservableProperty] private Hoi4Event? _selectedEvent;
    [ObservableProperty] private string _namespace = "generic";
    [ObservableProperty] private string _searchText = string.Empty;

    public EventsViewModel(INotificationService notifications, ILogService log, IProjectService projects)
        : base(notifications, log, projects) { }

    protected override void OnProjectOpened(ProjectData data)
    {
        Events.Clear();
        Namespace = data.Events.Namespace;
        foreach (var dto in data.Events.Events)
        {
            var ev = new Hoi4Event
            {
                Id = dto.Id, Title = dto.Title, Description = dto.Description,
                Picture = dto.Picture, Type = dto.Type,
                IsTriggeredOnly = dto.IsTriggeredOnly, Hidden = dto.Hidden,
                Fire_only_once = dto.FireOnlyOnce, Trigger = dto.Trigger,
                Mean_time_to_happen = dto.MeanTimeToHappen
            };
            foreach (var o in dto.Options)
                ev.Options.Add(new EventOption { Name = o.Name, Trigger = o.Trigger, Effect = o.Effect });
            Events.Add(ev);
        }
    }

    protected override void OnProjectClosed()
    {
        Events.Clear(); SelectedEvent = null; Namespace = "generic";
    }

    protected override void SyncToProjectData(ProjectData data)
    {
        data.Events.Namespace = Namespace;
        data.Events.Events = Events.Select(e => new EventDto
        {
            Id = e.Id, Title = e.Title, Description = e.Description,
            Picture = e.Picture, Type = e.Type,
            IsTriggeredOnly = e.IsTriggeredOnly, Hidden = e.Hidden,
            FireOnlyOnce = e.Fire_only_once, Trigger = e.Trigger,
            MeanTimeToHappen = e.Mean_time_to_happen,
            Options = e.Options.Select(o => new EventOptionDto
                { Name = o.Name, Trigger = o.Trigger, Effect = o.Effect }).ToList()
        }).ToList();
    }

    [RelayCommand]
    private void AddEvent()
    {
        var ev = new Hoi4Event
        {
            Id = $"{Namespace}.{Events.Count + 1}",
            Title = "New Event",
            Description = "Event description here."
        };
        ev.Options.Add(new EventOption { Name = "Option A", Effect = "# effects here" });
        Events.Add(ev);
        SelectedEvent = ev;
        MarkDirty();
        Notifications.Show($"Added event '{ev.Id}'", NotificationType.Success);
    }

    [RelayCommand]
    private void DeleteEvent(Hoi4Event? ev)
    {
        if (ev == null) return;
        Events.Remove(ev);
        if (SelectedEvent == ev) SelectedEvent = null;
        MarkDirty();
    }

    [RelayCommand]
    private void SelectEvent(Hoi4Event? ev) => SelectedEvent = ev;

    [RelayCommand]
    private void AddOption()
    {
        if (SelectedEvent == null) return;
        var opt = new EventOption { Name = $"Option {(char)('A' + SelectedEvent.Options.Count)}" };
        SelectedEvent.Options.Add(opt);
        MarkDirty();
    }

    [RelayCommand]
    private void RemoveOption(EventOption? opt)
    {
        if (SelectedEvent == null || opt == null) return;
        SelectedEvent.Options.Remove(opt);
        MarkDirty();
    }
}
