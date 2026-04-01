using NansHoi4Tool.Shared;

namespace NansHoi4Tool.Services;

public interface ICollaborationService
{
    bool IsConnected { get; }
    bool IsHosting { get; }
    string ConnectionStatus { get; }
    CollaborationSession? CurrentSession { get; }
    IReadOnlyList<CollaboratorPresence> Collaborators { get; }

    // ── Host: open session and push project data to guests ──
    Task<bool> HostSessionAsync(string projectId, string userId, string displayName, string projectDataJson, string projectName);

    // ── Guest: connect without requiring a local project file ──
    Task<bool> JoinSessionAsync(string host, int port, string sessionId, string userId, string displayName);

    Task DisconnectAsync();
    Task UploadSnapshotAsync(string projectDataJson, string projectName);
    Task<bool> RequestLockAsync(string entityId, string entityType);
    Task ReleaseLockAsync(string entityId);
    Task SendChangeAsync(ProjectChangeEvent change);
    Task SendChatAsync(string text, string displayName);

    event EventHandler<string>?                          ConnectionStatusChanged;
    event EventHandler<CollaboratorPresence>?            UserJoined;
    event EventHandler<(string UserId, string Name)>?   UserLeft;
    event EventHandler<ProjectSnapshot>?                 SnapshotReceived;
    event EventHandler<ProjectChangeEvent>?              ChangeReceived;
    event EventHandler<ChatMessage>?                     ChatReceived;
    event EventHandler<(string entityId, string userId)>? LockGranted;
    event EventHandler<string>?                          LockDenied;
    event EventHandler<string>?                          LockReleased;
}
