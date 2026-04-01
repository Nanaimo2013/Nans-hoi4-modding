using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NansHoi4Tool.Models;
using NansHoi4Tool.Services;
using System.Collections.ObjectModel;

namespace NansHoi4Tool.ViewModels;

// ── Node model ────────────────────────────────────────────────
public partial class FocusNode : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _icon = "GFX_goal_generic_political_pressure";
    [ObservableProperty] private int _cost = 70;
    [ObservableProperty] private double _x = 0;
    [ObservableProperty] private double _y = 0;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isConnectSource;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private List<string> _prerequisites = new();
    [ObservableProperty] private List<string> _mutuallyExclusive = new();
    [ObservableProperty] private string _completionReward = string.Empty;
    [ObservableProperty] private string _available = string.Empty;
    [ObservableProperty] private bool _cancelIfInvalid = true;
    [ObservableProperty] private bool _continueIfInvalid;
    [ObservableProperty] private bool _availableIfCapitulated;
}

// ── Connection model ──────────────────────────────────────────
public enum FocusConnectionType { Prerequisite, MutuallyExclusive }

public class FocusConnection
{
    public const double GridCellPx = 100;
    public const double NodeHalf   = 68;

    public string FromId { get; init; } = string.Empty;
    public string ToId   { get; init; } = string.Empty;
    public FocusConnectionType Type { get; init; }
    public double X1 { get; init; }
    public double Y1 { get; init; }
    public double X2 { get; init; }
    public double Y2 { get; init; }

    public static FocusConnection? Build(FocusNode from, FocusNode? to, FocusConnectionType type)
    {
        if (to == null) return null;
        return new FocusConnection
        {
            FromId = from.Id, ToId = to.Id, Type = type,
            X1 = from.X * GridCellPx + NodeHalf,
            Y1 = from.Y * GridCellPx + NodeHalf,
            X2 = to.X  * GridCellPx + NodeHalf,
            Y2 = to.Y  * GridCellPx + NodeHalf
        };
    }
}

// ── Per-tree tab model ────────────────────────────────────────
public partial class FocusTreeTab : ObservableObject
{
    [ObservableProperty] private string _tabId   = Guid.NewGuid().ToString("N")[..8];
    [ObservableProperty] private string _treeId  = "generic";
    [ObservableProperty] private string _countryTag = string.Empty;
    [ObservableProperty] private bool   _isGeneric  = true;
    [ObservableProperty] private bool   _continuous;
    [ObservableProperty] private FocusNode? _selectedNode;
    [ObservableProperty] private ObservableCollection<FocusNode>       _focusNodes  = new();
    [ObservableProperty] private ObservableCollection<FocusConnection> _connections = new();

    [ObservableProperty] private bool _isActive;
    [ObservableProperty] private bool _canClose = true;

    public string Label => IsGeneric ? $"Generic ({TreeId})"
                                     : string.IsNullOrEmpty(CountryTag) ? TreeId : CountryTag;

    partial void OnTreeIdChanged(string value)       => OnPropertyChanged(nameof(Label));
    partial void OnCountryTagChanged(string value)   => OnPropertyChanged(nameof(Label));
    partial void OnIsGenericChanged(bool value)      => OnPropertyChanged(nameof(Label));
    partial void OnSelectedNodeChanged(FocusNode? oldValue, FocusNode? newValue)
    {
        if (oldValue != null) oldValue.IsSelected = false;
        if (newValue != null) newValue.IsSelected = true;
    }
}

// ── ViewModel ─────────────────────────────────────────────────
public partial class FocusTreeViewModel : EditorViewModelBase
{
    [ObservableProperty] private ObservableCollection<FocusTreeTab> _trees   = new();
    [ObservableProperty] private FocusTreeTab? _activeTree;

    // Connect-mode state
    [ObservableProperty] private bool    _connectModeActive;
    [ObservableProperty] private FocusNode? _connectSource;
    [ObservableProperty] private double  _cursorX;
    [ObservableProperty] private double  _cursorY;
    [ObservableProperty] private FocusConnectionType _pendingConnectionType = FocusConnectionType.Prerequisite;

    // Convenience pass-throughs to active tree
    public ObservableCollection<FocusNode>?       FocusNodes  => ActiveTree?.FocusNodes;
    public ObservableCollection<FocusConnection>? Connections => ActiveTree?.Connections;
    public FocusNode? SelectedNode
    {
        get => ActiveTree?.SelectedNode;
        set { if (ActiveTree != null) ActiveTree.SelectedNode = value; }
    }
    public string TreeId
    {
        get => ActiveTree?.TreeId ?? string.Empty;
        set { if (ActiveTree != null) { ActiveTree.TreeId = value; MarkDirty(); } }
    }
    public string CountryTag
    {
        get => ActiveTree?.CountryTag ?? string.Empty;
        set { if (ActiveTree != null) { ActiveTree.CountryTag = value; MarkDirty(); } }
    }
    public bool IsGeneric
    {
        get => ActiveTree?.IsGeneric ?? true;
        set { if (ActiveTree != null) { ActiveTree.IsGeneric = value; MarkDirty(); } }
    }
    public bool Continuous
    {
        get => ActiveTree?.Continuous ?? false;
        set { if (ActiveTree != null) { ActiveTree.Continuous = value; MarkDirty(); } }
    }

    public FocusTreeViewModel(INotificationService notifications, ILogService log, IProjectService projects)
        : base(notifications, log, projects) { }

    protected override void OnProjectOpened(ProjectData data)
    {
        Trees.Clear();
        foreach (var dto in data.FocusTrees)
            Trees.Add(TabFromDto(dto));
        if (Trees.Count == 0)
            Trees.Add(TabFromDto(new FocusTreeData { TreeId = "generic", IsGeneric = true }));
        ActiveTree = Trees[0];
    }

    protected override void OnProjectClosed()
    {
        Trees.Clear();
        ActiveTree = null;
    }

    protected override void SyncToProjectData(ProjectData data)
    {
        data.FocusTrees = Trees.Select(TabToDto).ToList();
    }

    partial void OnActiveTreeChanged(FocusTreeTab? oldValue, FocusTreeTab? newValue)
    {
        if (oldValue != null) oldValue.IsActive = false;
        if (newValue != null) newValue.IsActive = true;
        UpdateCanClose();
        OnPropertyChanged(nameof(FocusNodes));
        OnPropertyChanged(nameof(Connections));
        OnPropertyChanged(nameof(SelectedNode));
        OnPropertyChanged(nameof(TreeId));
        OnPropertyChanged(nameof(CountryTag));
        OnPropertyChanged(nameof(IsGeneric));
        OnPropertyChanged(nameof(Continuous));
    }

    private void UpdateCanClose()
    {
        var canClose = Trees.Count > 1;
        foreach (var t in Trees) t.CanClose = canClose;
    }

    // ── Tree tab management ──────────────────────────────────
    [RelayCommand]
    private void AddTree()
    {
        var tab = new FocusTreeTab
        {
            TreeId = $"tree_{Trees.Count + 1}",
            IsGeneric = false
        };
        tab.FocusNodes.CollectionChanged += (_, _) => RebuildConnections(tab);
        Trees.Add(tab);
        ActiveTree = tab;
        UpdateCanClose();
        MarkDirty();
    }

    [RelayCommand]
    private void RemoveTree(FocusTreeTab? tab)
    {
        if (tab == null || Trees.Count <= 1) return;
        Trees.Remove(tab);
        if (ActiveTree == tab) ActiveTree = Trees.FirstOrDefault();
        UpdateCanClose();
        MarkDirty();
    }

    [RelayCommand]
    private void SelectTree(FocusTreeTab? tab)
    {
        if (tab != null) ActiveTree = tab;
    }

    // ── Focus management ──────────────────────────────────────
    [RelayCommand]
    private void AddFocus()
    {
        if (ActiveTree == null) return;
        var nodes = ActiveTree.FocusNodes;
        var id = $"focus_{nodes.Count + 1}";
        var node = new FocusNode
        {
            Id = id, Name = "New Focus",
            X = (nodes.Count % 8) * 2,
            Y = (nodes.Count / 8) * 2
        };
        nodes.Add(node);
        ActiveTree.SelectedNode = node;
        RebuildConnections(ActiveTree);
        MarkDirty();
        Notifications.Show($"Added focus '{id}'", NotificationType.Success);
    }

    [RelayCommand]
    private void DeleteFocus(FocusNode? node)
    {
        if (node == null || ActiveTree == null) return;
        ActiveTree.FocusNodes.Remove(node);
        if (ActiveTree.SelectedNode == node) ActiveTree.SelectedNode = null;
        RebuildConnections(ActiveTree);
        MarkDirty();
    }

    [RelayCommand]
    private void SelectNode(FocusNode? node)
    {
        if (ActiveTree == null) return;
        if (ConnectModeActive && node != null)
        {
            if (ConnectSource == null)
            {
                ConnectSource = node;
                node.IsConnectSource = true;
            }
            else if (ConnectSource != node)
            {
                CommitConnection(ConnectSource, node, PendingConnectionType);
                ConnectSource.IsConnectSource = false;
                ConnectSource = null;
            }
        }
        else
        {
            ActiveTree.SelectedNode = node;
        }
    }

    [RelayCommand]
    private void DuplicateFocus(FocusNode? node)
    {
        if (node == null || ActiveTree == null) return;
        var copy = new FocusNode
        {
            Id = node.Id + "_copy", Name = node.Name + " (Copy)",
            Icon = node.Icon, Cost = node.Cost,
            X = node.X + 2, Y = node.Y,
            Description = node.Description,
            CompletionReward = node.CompletionReward, Available = node.Available
        };
        ActiveTree.FocusNodes.Add(copy);
        ActiveTree.SelectedNode = copy;
        RebuildConnections(ActiveTree);
        MarkDirty();
    }

    // ── Connection management ─────────────────────────────────
    [RelayCommand]
    private void ToggleConnectMode()
    {
        ConnectModeActive = !ConnectModeActive;
        if (!ConnectModeActive) CancelConnect();
    }

    [RelayCommand]
    private void CancelConnect()
    {
        if (ConnectSource != null) ConnectSource.IsConnectSource = false;
        ConnectSource = null;
    }

    [RelayCommand]
    private void AddPrerequisite(string targetId)
    {
        if (ActiveTree?.SelectedNode == null || string.IsNullOrEmpty(targetId)) return;
        var node = ActiveTree.SelectedNode;
        if (node.Id == targetId || node.Prerequisites.Contains(targetId)) return;
        node.Prerequisites = new List<string>(node.Prerequisites) { targetId };
        RebuildConnections(ActiveTree);
        MarkDirty();
    }

    [RelayCommand]
    private void RemovePrerequisite(string targetId)
    {
        if (ActiveTree?.SelectedNode == null) return;
        var node = ActiveTree.SelectedNode;
        node.Prerequisites = node.Prerequisites.Where(p => p != targetId).ToList();
        RebuildConnections(ActiveTree);
        MarkDirty();
    }

    [RelayCommand]
    private void AddMutex(string targetId)
    {
        if (ActiveTree?.SelectedNode == null || string.IsNullOrEmpty(targetId)) return;
        var node = ActiveTree.SelectedNode;
        if (node.Id == targetId || node.MutuallyExclusive.Contains(targetId)) return;
        node.MutuallyExclusive = new List<string>(node.MutuallyExclusive) { targetId };
        RebuildConnections(ActiveTree);
        MarkDirty();
    }

    [RelayCommand]
    private void RemoveMutex(string targetId)
    {
        if (ActiveTree?.SelectedNode == null) return;
        var node = ActiveTree.SelectedNode;
        node.MutuallyExclusive = node.MutuallyExclusive.Where(p => p != targetId).ToList();
        RebuildConnections(ActiveTree);
        MarkDirty();
    }

    private void CommitConnection(FocusNode from, FocusNode to, FocusConnectionType type)
    {
        if (ActiveTree == null) return;
        if (type == FocusConnectionType.Prerequisite)
        {
            if (!from.Prerequisites.Contains(to.Id))
                from.Prerequisites = new List<string>(from.Prerequisites) { to.Id };
        }
        else
        {
            if (!from.MutuallyExclusive.Contains(to.Id))
                from.MutuallyExclusive = new List<string>(from.MutuallyExclusive) { to.Id };
        }
        RebuildConnections(ActiveTree);
        MarkDirty();
        Notifications.Show($"Connected '{from.Id}' → '{to.Id}' ({type})", NotificationType.Info);
    }

    public void RebuildConnections(FocusTreeTab? tab = null)
    {
        tab ??= ActiveTree;
        if (tab == null) return;
        tab.Connections.Clear();
        var lookup = tab.FocusNodes.ToDictionary(n => n.Id);
        foreach (var node in tab.FocusNodes)
        {
            foreach (var pid in node.Prerequisites)
            {
                var c = FocusConnection.Build(node, lookup.GetValueOrDefault(pid), FocusConnectionType.Prerequisite);
                if (c != null) tab.Connections.Add(c);
            }
            foreach (var mid in node.MutuallyExclusive)
            {
                var c = FocusConnection.Build(node, lookup.GetValueOrDefault(mid), FocusConnectionType.MutuallyExclusive);
                if (c != null) tab.Connections.Add(c);
            }
        }
    }

    public void NotifyNodeMoved()
    {
        RebuildConnections();
        MarkDirty();
    }

    // ── Helpers ───────────────────────────────────────────────
    private static FocusTreeTab TabFromDto(FocusTreeData dto)
    {
        var tab = new FocusTreeTab
        {
            TreeId = dto.TreeId, CountryTag = dto.CountryTag,
            IsGeneric = dto.IsGeneric, Continuous = dto.Continuous
        };
        foreach (var n in dto.Nodes)
            tab.FocusNodes.Add(new FocusNode
            {
                Id = n.Id, Name = n.Name, Icon = n.Icon, Cost = n.Cost,
                X = n.X, Y = n.Y, Description = n.Description,
                Prerequisites = new List<string>(n.Prerequisites),
                MutuallyExclusive = new List<string>(n.MutuallyExclusive),
                CompletionReward = n.CompletionReward, Available = n.Available,
                CancelIfInvalid = n.CancelIfInvalid, ContinueIfInvalid = n.ContinueIfInvalid,
                AvailableIfCapitulated = n.AvailableIfCapitulated
            });
        tab.FocusNodes.CollectionChanged += (_, _) => { };
        return tab;
    }

    private static FocusTreeData TabToDto(FocusTreeTab tab) => new()
    {
        TreeId = tab.TreeId, CountryTag = tab.CountryTag,
        IsGeneric = tab.IsGeneric, Continuous = tab.Continuous,
        Nodes = tab.FocusNodes.Select(n => new FocusNodeDto
        {
            Id = n.Id, Name = n.Name, Icon = n.Icon, Cost = n.Cost,
            X = n.X, Y = n.Y, Description = n.Description,
            Prerequisites = new List<string>(n.Prerequisites),
            MutuallyExclusive = new List<string>(n.MutuallyExclusive),
            CompletionReward = n.CompletionReward, Available = n.Available,
            CancelIfInvalid = n.CancelIfInvalid, ContinueIfInvalid = n.ContinueIfInvalid,
            AvailableIfCapitulated = n.AvailableIfCapitulated
        }).ToList()
    };
}
