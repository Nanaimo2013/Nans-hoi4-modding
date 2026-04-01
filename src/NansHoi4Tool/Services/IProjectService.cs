using NansHoi4Tool.Models;
using NansHoi4Tool.Shared;

namespace NansHoi4Tool.Services;

public interface IProjectService
{
    ProjectMetadata? CurrentProject { get; }
    ProjectData CurrentData { get; }
    bool IsProjectOpen { get; }
    string? CurrentProjectPath { get; }

    Task<bool> NewProjectAsync(string name, string modId, string author, string projectFolderPath);
    Task<bool> OpenProjectAsync(string path);
    Task<bool> SaveProjectAsync();
    Task<bool> SaveProjectAsAsync(string path);
    void CloseProject();
    void ForceReload();
    Task<bool> ExportToHoi4Async(string outputFolder);
    Task<bool> ImportFromHoi4Async(string modFolder);

    // ── Collaboration snapshot helpers ──
    /// <summary>Load a project received from a collaboration host (no file on disk).</summary>
    void LoadTemporarySnapshot(string projectName, ProjectData data);
    /// <summary>Unload the temporary snapshot when the guest disconnects.</summary>
    void CloseTemporarySnapshot();
    /// <summary>Serialize the current ProjectData to JSON for sending as a snapshot.</summary>
    string SerializeCurrentProject();
    /// <summary>Apply a delta change received from a remote peer.</summary>
    void ApplyRemoteChange(NansHoi4Tool.Shared.ProjectChangeEvent change);

    event EventHandler<ProjectMetadata>? ProjectOpened;
    event EventHandler? ProjectClosed;
    event EventHandler? ProjectChanged;
}
