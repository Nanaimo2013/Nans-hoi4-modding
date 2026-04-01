using Microsoft.AspNetCore.SignalR.Client;
using NansHoi4Tool.Shared;

namespace NansHoi4Tool.Services;

/// <summary>
/// Manages a SignalR connection for real-time LAN collaboration.
/// Host mode: spawns a local server process, connects to it, and uploads the project snapshot.
/// Guest mode: connects to the host's IP/port and receives the project snapshot automatically.
/// </summary>
public class CollaborationService : ICollaborationService, IAsyncDisposable
{
    private readonly ILogService _log;
    private readonly INotificationService _notifications;
    private HubConnection? _hub;
    private System.Diagnostics.Process? _serverProcess;

    public string ConnectionStatus { get; private set; } = "Disconnected";
    public bool IsConnected  => _hub?.State == HubConnectionState.Connected;
    public bool IsHosting    { get; private set; }
    public CollaborationSession? CurrentSession { get; private set; }
    public IReadOnlyList<CollaboratorPresence> Collaborators =>
        CurrentSession?.Collaborators ?? (IReadOnlyList<CollaboratorPresence>)Array.Empty<CollaboratorPresence>();

    public event EventHandler<string>?                           ConnectionStatusChanged;
    public event EventHandler<CollaboratorPresence>?             UserJoined;
    public event EventHandler<(string UserId, string Name)>?    UserLeft;
    public event EventHandler<ProjectSnapshot>?                  SnapshotReceived;
    public event EventHandler<ProjectChangeEvent>?               ChangeReceived;
    public event EventHandler<ChatMessage>?                      ChatReceived;
    public event EventHandler<(string entityId, string userId)>? LockGranted;
    public event EventHandler<string>?                           LockDenied;
    public event EventHandler<string>?                           LockReleased;

    public CollaborationService(ILogService log, INotificationService notifications)
    {
        _log = log;
        _notifications = notifications;
    }

    // ── Host ────────────────────────────────────────────────────────────────

    public async Task<bool> HostSessionAsync(string projectId, string userId, string displayName,
                                              string projectDataJson, string projectName)
    {
        IsHosting = true;
        SetStatus("Starting server…");

        // Launch the embedded server process on the host machine
        LaunchServerProcess(51420);

        // Give it a moment to bind
        await Task.Delay(1200);

        var ok = await ConnectAsync("localhost", 51420, projectId, userId, displayName, "Owner");
        if (!ok) return false;

        // Upload the project snapshot so late-joining guests get it
        await UploadSnapshotAsync(projectDataJson, projectName);
        return true;
    }

    // ── Guest ────────────────────────────────────────────────────────────────

    public async Task<bool> JoinSessionAsync(string host, int port, string sessionId,
                                              string userId, string displayName)
    {
        IsHosting = false;
        return await ConnectAsync(host, port, sessionId, userId, displayName, "Editor");
    }

    // ── Snapshot ─────────────────────────────────────────────────────────────

    public async Task UploadSnapshotAsync(string projectDataJson, string projectName)
    {
        if (!IsConnected || CurrentSession == null) return;
        var snap = new ProjectSnapshot
        {
            ProjectId       = CurrentSession.ProjectId,
            ProjectName     = projectName,
            ProjectDataJson = projectDataJson,
            HostDisplayName = CurrentSession.HostName,
            SnapshotAt      = DateTime.UtcNow
        };
        try { await _hub!.InvokeAsync("UploadSnapshot", snap); }
        catch (Exception ex) { _log.Warning($"UploadSnapshot failed: {ex.Message}"); }
    }

    // ── Core connect ─────────────────────────────────────────────────────────

    private async Task<bool> ConnectAsync(string host, int port, string sessionId,
                                           string userId, string displayName, string role)
    {
        try
        {
            SetStatus($"Connecting to {host}:{port}…");
            _hub = new HubConnectionBuilder()
                .WithUrl($"http://{host}:{port}/hub")
                .WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5) })
                .Build();

            RegisterHandlers();
            await _hub.StartAsync();
            SetStatus("Joining session…");
            await _hub.InvokeAsync("JoinSession", sessionId, userId, displayName, role);
            SetStatus("Connected");
            _notifications.ShowSuccess($"{(role == "Owner" ? "Hosting" : "Joined")} session on {host}:{port}");
            return true;
        }
        catch (Exception ex)
        {
            SetStatus($"Failed: {ex.Message}");
            _log.Error($"Collab connect failed: {host}:{port}", ex);
            _notifications.ShowError($"Could not connect: {ex.Message}");
            return false;
        }
    }

    private void RegisterHandlers()
    {
        if (_hub == null) return;

        _hub.On<CollaboratorPresence>("UserJoined", p =>
        {
            CurrentSession?.Collaborators.Add(p);
            UserJoined?.Invoke(this, p);
            _notifications.Show($"🟢 {p.DisplayName} joined");
        });

        _hub.On<string, string>("UserLeft", (userId, name) =>
        {
            CurrentSession?.Collaborators.RemoveAll(c => c.UserId == userId);
            UserLeft?.Invoke(this, (userId, name));
            _notifications.Show($"🔴 {name} left");
        });

        _hub.On<CollaborationSession>("SessionState", session =>
            CurrentSession = session);

        _hub.On<ProjectSnapshot>("ProjectSnapshot", snap =>
            SnapshotReceived?.Invoke(this, snap));

        _hub.On<ProjectChangeEvent>("ChangeReceived", change =>
            ChangeReceived?.Invoke(this, change));

        _hub.On<ChatMessage>("ChatMessageReceived", msg =>
            ChatReceived?.Invoke(this, msg));

        _hub.On<string, string>("LockGranted", (entityId, uid) =>
            LockGranted?.Invoke(this, (entityId, uid)));

        _hub.On<string, string>("LockDenied", (entityId, holder) =>
        {
            LockDenied?.Invoke(this, entityId);
            _notifications.ShowWarning($"'{entityId}' is locked by {holder}");
        });

        _hub.On<string>("LockReleased", entityId =>
            LockReleased?.Invoke(this, entityId));

        _hub.Reconnected += _ =>
        {
            SetStatus("Connected (reconnected)");
            _notifications.Show("Reconnected to session");
            return Task.CompletedTask;
        };

        _hub.Closed += ex =>
        {
            SetStatus("Disconnected");
            if (ex != null) _notifications.ShowWarning("Disconnected from session");
            return Task.CompletedTask;
        };
    }

    // ── Disconnect ────────────────────────────────────────────────────────────

    public async Task DisconnectAsync()
    {
        if (_hub != null)
        {
            try
            {
                await _hub.InvokeAsync("LeaveSession");
                await _hub.StopAsync();
            }
            catch (Exception ex) { _log.Warning($"Disconnect error: {ex.Message}"); }
            finally
            {
                await _hub.DisposeAsync();
                _hub = null;
            }
        }

        StopServerProcess();
        CurrentSession = null;
        IsHosting = false;
        SetStatus("Disconnected");
    }

    // ── Changes / Chat / Locks ────────────────────────────────────────────────

    public async Task SendChangeAsync(ProjectChangeEvent change)
    {
        if (!IsConnected) return;
        try { await _hub!.InvokeAsync("BroadcastChange", change); }
        catch (Exception ex) { _log.Warning($"SendChange failed: {ex.Message}"); }
    }

    public async Task SendChatAsync(string text, string displayName)
    {
        if (!IsConnected) return;
        try { await _hub!.InvokeAsync("SendChatMessage", new ChatMessage { Text = text, DisplayName = displayName }); }
        catch (Exception ex) { _log.Warning($"SendChat failed: {ex.Message}"); }
    }

    public async Task<bool> RequestLockAsync(string entityId, string entityType)
    {
        if (!IsConnected) return true;
        try { await _hub!.InvokeAsync("RequestLock", entityId, entityType); return true; }
        catch { return false; }
    }

    public async Task ReleaseLockAsync(string entityId)
    {
        if (!IsConnected) return;
        try { await _hub!.InvokeAsync("ReleaseLock", entityId); }
        catch { }
    }

    // ── Server process ────────────────────────────────────────────────────────

    private void LaunchServerProcess(int port)
    {
        try
        {
            var exeDir  = AppDomain.CurrentDomain.BaseDirectory;
            var exePath = System.IO.Path.Combine(exeDir, "NansHoi4Tool.Server.exe");
            if (!System.IO.File.Exists(exePath))
            {
                _log.Warning($"Server exe not found at {exePath}; trying dotnet run fallback");
                return;
            }

            _serverProcess = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName  = exePath,
                    Arguments = port.ToString(),
                    UseShellExecute       = false,
                    CreateNoWindow        = true,
                    RedirectStandardOutput = false
                }
            };
            _serverProcess.Start();
            _log.Info($"[Collab] Server started on port {port}");
        }
        catch (Exception ex)
        {
            _log.Warning($"Could not start server process: {ex.Message}");
        }
    }

    private void StopServerProcess()
    {
        try { _serverProcess?.Kill(entireProcessTree: true); }
        catch { }
        finally { _serverProcess?.Dispose(); _serverProcess = null; }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private void SetStatus(string status)
    {
        ConnectionStatus = status;
        ConnectionStatusChanged?.Invoke(this, status);
        _log.Info($"[Collab] {status}");
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub != null) { await _hub.DisposeAsync(); _hub = null; }
        StopServerProcess();
    }
}