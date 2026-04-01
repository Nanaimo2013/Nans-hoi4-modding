using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NansHoi4Tool.Core;
using NansHoi4Tool.Services;
using NansHoi4Tool.Shared;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace NansHoi4Tool.ViewModels;

public partial class ChatMessageItem : ObservableObject
{
    [ObservableProperty] private string _displayName = string.Empty;
    [ObservableProperty] private string _text        = string.Empty;
    [ObservableProperty] private string _timestamp   = string.Empty;
    [ObservableProperty] private bool   _isOwn;
}

public partial class CollaboratorItem : ObservableObject
{
    [ObservableProperty] private string _displayName = string.Empty;
    [ObservableProperty] private string _role        = string.Empty;
    [ObservableProperty] private bool   _isHost;
}

public partial class CollaborationViewModel : ViewModelBase
{
    private readonly ICollaborationService  _collab;
    private readonly IProjectService        _projects;
    private readonly IAppSettingsService    _settings;
    private readonly INotificationService   _notifications;
    private readonly INavigationService     _navigation;

    [ObservableProperty] private bool   _isConnected;
    [ObservableProperty] private bool   _isHosting;
    [ObservableProperty] private string _connectionStatus = "Disconnected";
    [ObservableProperty] private string _sessionId        = string.Empty;
    [ObservableProperty] private string _sessionName      = string.Empty;
    [ObservableProperty] private string _hostAddress      = string.Empty;
    [ObservableProperty] private int    _hostPort         = 51420;
    [ObservableProperty] private string _chatInput        = string.Empty;
    [ObservableProperty] private string _localIp          = string.Empty;

    public ObservableCollection<CollaboratorItem> Collaborators { get; } = new();
    public ObservableCollection<ChatMessageItem>  ChatMessages  { get; } = new();

    public CollaborationViewModel(
        ICollaborationService collab,
        IProjectService       projects,
        IAppSettingsService   settings,
        INotificationService  notifications,
        INavigationService    navigation)
    {
        _collab        = collab;
        _projects      = projects;
        _settings      = settings;
        _notifications = notifications;
        _navigation    = navigation;

        LocalIp = GetLocalIp();

        _collab.ConnectionStatusChanged += (_, s) => Ui(() => ConnectionStatus = s);

        _collab.UserJoined += (_, p) => Ui(() =>
        {
            if (Collaborators.All(c => c.DisplayName != p.DisplayName))
                Collaborators.Add(new CollaboratorItem { DisplayName = p.DisplayName, Role = p.Role, IsHost = p.Role == "Owner" });
        });

        _collab.UserLeft += (_, t) => Ui(() =>
        {
            var item = Collaborators.FirstOrDefault(c => c.DisplayName == t.Name);
            if (item != null) Collaborators.Remove(item);
            AddSystemMessage($"{t.Name} left the session");
        });

        _collab.SnapshotReceived += (_, snap) => Ui(() =>
        {
            SessionName = snap.ProjectName;
            try
            {
                var data = JsonSerializer.Deserialize<Models.ProjectData>(snap.ProjectDataJson);
                if (data != null)
                {
                    _projects.LoadTemporarySnapshot(snap.ProjectName, data);
                    _navigation.NavigateTo("FocusTree");
                    _notifications.ShowFull("Project loaded", $"Working on \"{snap.ProjectName}\" (read from host)");
                }
            }
            catch (Exception ex)
            {
                _notifications.ShowError($"Failed to load project snapshot: {ex.Message}");
            }
        });

        _collab.ChangeReceived += (_, change) =>
            _projects.ApplyRemoteChange(change);

        _collab.ChatReceived += (_, msg) => Ui(() =>
        {
            var me = _settings.Current.UserName;
            ChatMessages.Add(new ChatMessageItem
            {
                DisplayName = msg.DisplayName,
                Text        = msg.Text,
                Timestamp   = msg.SentAt.ToLocalTime().ToString("HH:mm"),
                IsOwn       = msg.UserId == me || msg.DisplayName == me
            });
        });
    }

    // ── Host ────────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task HostSession()
    {
        if (!_projects.IsProjectOpen)
        {
            _notifications.ShowWarning("Open a project before hosting");
            return;
        }

        IsBusy = true;
        var projectJson = _projects.SerializeCurrentProject();
        var ok = await _collab.HostSessionAsync(
            _projects.CurrentProject!.Id,
            _settings.Current.UserName,
            _settings.Current.UserName,
            projectJson,
            _projects.CurrentProject!.Name);
        IsBusy = false;

        if (!ok) return;

        IsConnected = true;
        IsHosting   = true;
        SessionId   = _projects.CurrentProject!.Id;
        SessionName = _projects.CurrentProject!.Name;
        Collaborators.Clear();
        Collaborators.Add(new CollaboratorItem
        {
            DisplayName = _settings.Current.UserName,
            Role        = "Owner",
            IsHost      = true
        });
        AddSystemMessage($"Session started — your IP: {LocalIp}:{51420}");
    }

    // ── Guest ────────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task JoinSession()
    {
        if (string.IsNullOrWhiteSpace(HostAddress))
        {
            _notifications.ShowWarning("Enter the host's IP address");
            return;
        }

        IsBusy = true;
        var sessionId = string.IsNullOrWhiteSpace(SessionId) ? "default" : SessionId;
        var ok = await _collab.JoinSessionAsync(
            HostAddress, HostPort, sessionId,
            _settings.Current.UserName,
            _settings.Current.UserName);
        IsBusy = false;

        if (!ok) return;

        IsConnected = true;
        IsHosting   = false;
        AddSystemMessage("Waiting for project data from host…");
    }

    // ── Disconnect ────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task Disconnect()
    {
        IsBusy = true;
        await _collab.DisconnectAsync();
        IsBusy = false;
        IsConnected = false;
        IsHosting   = false;
        Collaborators.Clear();
        ChatMessages.Clear();
        ConnectionStatus = "Disconnected";
        if (!IsHosting) _projects.CloseTemporarySnapshot();
    }

    // ── Push fresh snapshot mid-session ──────────────────────────────────────

    [RelayCommand]
    private async Task PushSnapshot()
    {
        if (!IsConnected || !IsHosting || !_projects.IsProjectOpen) return;
        await _collab.UploadSnapshotAsync(
            _projects.SerializeCurrentProject(),
            _projects.CurrentProject!.Name);
        _notifications.ShowSuccess("Project snapshot pushed to all guests");
    }

    // ── Chat ─────────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task SendChat()
    {
        if (string.IsNullOrWhiteSpace(ChatInput)) return;
        var text = ChatInput.Trim();
        ChatInput = string.Empty;
        await _collab.SendChatAsync(text, _settings.Current.UserName);

        // Also show own message locally
        ChatMessages.Add(new ChatMessageItem
        {
            DisplayName = _settings.Current.UserName,
            Text        = text,
            Timestamp   = DateTime.Now.ToString("HH:mm"),
            IsOwn       = true
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void AddSystemMessage(string text)
        => ChatMessages.Add(new ChatMessageItem { DisplayName = "System", Text = text, Timestamp = DateTime.Now.ToString("HH:mm"), IsOwn = false });

    private static void Ui(Action a)
        => System.Windows.Application.Current?.Dispatcher.Invoke(a);

    private static string GetLocalIp()
    {
        try
        {
            using var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            s.Connect("8.8.8.8", 65530);
            return (s.LocalEndPoint as IPEndPoint)?.Address.ToString() ?? "unknown";
        }
        catch { return "unknown"; }
    }
}
