namespace NansHoi4Tool.Shared;

public class ProjectMetadata
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = string.Empty;
    public string ModId { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Hoi4Version { get; set; } = "1.14.*";
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<ContributorInfo> Contributors { get; set; } = new();
    public string ThumbnailPath { get; set; } = string.Empty;
}

public class ContributorInfo
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = "Editor";
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class CollaborationSession
{
    public string SessionId { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public List<CollaboratorPresence> Collaborators { get; set; } = new();
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}

public class CollaboratorPresence
{
    public string ConnectionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = "Editor";
    public string CurrentEditor { get; set; } = string.Empty;
    public string LockedEntityId { get; set; } = string.Empty;
    public bool IsOnline { get; set; } = true;
}

public class EntityLockRequest
{
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

public class ProjectChangeEvent
{
    public string ChangeId { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string UserId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ChangeType { get; set; } = "Update";
    public string Payload { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ChatMessage
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Full project data blob sent to clients when they first join a session.</summary>
public class ProjectSnapshot
{
    public string ProjectId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    /// <summary>JSON-serialized ProjectData. Client deserialises and loads it temporarily.</summary>
    public string ProjectDataJson { get; set; } = string.Empty;
    public DateTime SnapshotAt { get; set; } = DateTime.UtcNow;
    public string HostDisplayName { get; set; } = string.Empty;
}
