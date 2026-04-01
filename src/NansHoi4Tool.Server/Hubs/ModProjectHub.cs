using Microsoft.AspNetCore.SignalR;
using NansHoi4Tool.Shared;
using System.Collections.Concurrent;

namespace NansHoi4Tool.Server.Hubs;

/// <summary>
/// Central SignalR hub for real-time collaboration.
/// • Host registers the session and uploads a ProjectSnapshot on connect.
/// • Guests receive the snapshot immediately so they can work without their own project file.
/// • All peers broadcast delta changes that others apply live.
/// </summary>
public class ModProjectHub : Hub
{
    private static readonly ConcurrentDictionary<string, CollaborationSession> _sessions = new();
    private static readonly ConcurrentDictionary<string, string> _connToSession = new();
    private static readonly ConcurrentDictionary<string, ProjectSnapshot> _snapshots = new();

    // ── Join / Leave ────────────────────────────────────────────────────────

    /// <summary>Called by both host (role="Owner") and guests (role="Editor").</summary>
    public async Task JoinSession(string projectId, string userId, string displayName, string role)
    {
        var sessionId = projectId;

        var session = _sessions.GetOrAdd(sessionId, _ => new CollaborationSession
        {
            SessionId = sessionId,
            ProjectId = projectId,
            HostName  = displayName,
            StartedAt = DateTime.UtcNow
        });

        var presence = new CollaboratorPresence
        {
            ConnectionId = Context.ConnectionId,
            UserId       = userId,
            DisplayName  = displayName,
            Role         = role,
            IsOnline     = true
        };

        lock (session.Collaborators)
        {
            session.Collaborators.RemoveAll(c => c.UserId == userId);
            session.Collaborators.Add(presence);
        }

        _connToSession[Context.ConnectionId] = sessionId;
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

        // Tell everyone else who just joined
        await Clients.OthersInGroup(sessionId).SendAsync("UserJoined", presence);

        // Send the caller the current session roster
        await Clients.Caller.SendAsync("SessionState", session);

        // If a snapshot already exists (host connected before this guest), deliver it
        if (role != "Owner" && _snapshots.TryGetValue(sessionId, out var snap))
            await Clients.Caller.SendAsync("ProjectSnapshot", snap);

        Console.WriteLine($"[Hub] {displayName} ({role}) joined {sessionId}");
    }

    /// <summary>Host uploads project data so guests can work without their own file.</summary>
    public async Task UploadSnapshot(ProjectSnapshot snapshot)
    {
        if (!_connToSession.TryGetValue(Context.ConnectionId, out var sessionId)) return;

        snapshot.ProjectId   = sessionId;
        snapshot.SnapshotAt  = DateTime.UtcNow;
        _snapshots[sessionId] = snapshot;

        // Push to all guests already in the session
        await Clients.OthersInGroup(sessionId).SendAsync("ProjectSnapshot", snapshot);
        Console.WriteLine($"[Hub] Snapshot uploaded for {sessionId} ({snapshot.ProjectDataJson.Length} chars)");
    }

    /// <summary>Host pushes a fresh snapshot mid-session (e.g. after saving).</summary>
    public async Task RefreshSnapshot(ProjectSnapshot snapshot)
        => await UploadSnapshot(snapshot);

    public async Task LeaveSession()
    {
        if (!_connToSession.TryRemove(Context.ConnectionId, out var sessionId)) return;

        if (_sessions.TryGetValue(sessionId, out var session))
        {
            CollaboratorPresence? user;
            lock (session.Collaborators)
            {
                user = session.Collaborators.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
                if (user != null)
                {
                    session.Collaborators.Remove(user);
                    ClearLocks(session, Context.ConnectionId);
                }

                if (session.Collaborators.Count == 0)
                {
                    _sessions.TryRemove(sessionId, out _);
                    _snapshots.TryRemove(sessionId, out _);
                    Console.WriteLine($"[Hub] Session {sessionId} closed.");
                }
            }

            if (user != null)
                await Clients.Group(sessionId).SendAsync("UserLeft", user.UserId, user.DisplayName);
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await LeaveSession();
        await base.OnDisconnectedAsync(exception);
    }

    // ── Changes ─────────────────────────────────────────────────────────────

    /// <summary>Broadcast a single delta change to all other peers.</summary>
    public async Task BroadcastChange(ProjectChangeEvent change)
    {
        if (!_connToSession.TryGetValue(Context.ConnectionId, out var sessionId)) return;
        await Clients.OthersInGroup(sessionId).SendAsync("ChangeReceived", change);
    }

    // ── Chat ────────────────────────────────────────────────────────────────

    public async Task SendChatMessage(ChatMessage message)
    {
        if (!_connToSession.TryGetValue(Context.ConnectionId, out var sessionId)) return;
        message.SentAt = DateTime.UtcNow;
        await Clients.Group(sessionId).SendAsync("ChatMessageReceived", message);
    }

    // ── Locks ────────────────────────────────────────────────────────────────

    public async Task RequestLock(string entityId, string entityType)
    {
        if (!_connToSession.TryGetValue(Context.ConnectionId, out var sessionId)) return;
        if (!_sessions.TryGetValue(sessionId, out var session)) return;

        CollaboratorPresence? holder;
        lock (session.Collaborators)
            holder = session.Collaborators.FirstOrDefault(c => c.LockedEntityId == entityId
                                                              && c.ConnectionId != Context.ConnectionId);

        if (holder != null)
        {
            await Clients.Caller.SendAsync("LockDenied", entityId, holder.DisplayName);
            return;
        }

        CollaboratorPresence? me;
        lock (session.Collaborators)
        {
            me = session.Collaborators.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            if (me != null) { me.LockedEntityId = entityId; me.CurrentEditor = entityType; }
        }

        await Clients.Group(sessionId).SendAsync("LockGranted", entityId, me?.UserId);
    }

    public async Task ReleaseLock(string entityId)
    {
        if (!_connToSession.TryGetValue(Context.ConnectionId, out var sessionId)) return;
        if (!_sessions.TryGetValue(sessionId, out var session)) return;

        lock (session.Collaborators)
        {
            var me = session.Collaborators.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            if (me != null && me.LockedEntityId == entityId)
            { me.LockedEntityId = string.Empty; me.CurrentEditor = string.Empty; }
        }

        await Clients.Group(sessionId).SendAsync("LockReleased", entityId);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static void ClearLocks(CollaborationSession session, string connectionId)
    {
        var u = session.Collaborators.FirstOrDefault(c => c.ConnectionId == connectionId);
        if (u == null) return;
        u.LockedEntityId = string.Empty;
        u.CurrentEditor  = string.Empty;
    }
}