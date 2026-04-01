namespace NansHoi4Tool.Services;

public record ReleaseInfo(
    string Version,
    string ReleaseNotes,
    string DownloadUrl,
    long FileSizeBytes,
    DateTime PublishedAt);

public interface IAutoUpdateService
{
    /// <summary>Check GitHub Releases for a newer version. Returns null if already up-to-date.</summary>
    Task<ReleaseInfo?> CheckForUpdateAsync(CancellationToken ct = default);

    /// <summary>Download installer to a temp file, reporting progress 0-100.</summary>
    Task<string> DownloadInstallerAsync(ReleaseInfo release, IProgress<double> progress, CancellationToken ct = default);

    /// <summary>Launch the downloaded installer and exit the app.</summary>
    void ApplyUpdate(string installerPath, bool silent = false);
}
