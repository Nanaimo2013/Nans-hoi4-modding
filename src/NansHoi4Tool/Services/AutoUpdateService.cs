using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;

namespace NansHoi4Tool.Services;

public class AutoUpdateService : IAutoUpdateService
{
    private const string RepoOwner  = "Nanaimo2013";
    private const string RepoName   = "Nans-hoi4-modding";
    private const string ApiUrl     = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";

    private readonly ILogService  _log;
    private readonly HttpClient   _http;

    public AutoUpdateService(ILogService log)
    {
        _log  = log;
        _http = new HttpClient();
        _http.DefaultRequestHeaders.UserAgent.ParseAdd($"NansHoi4Tool/{CurrentVersion}");
    }

    private static string CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";

    public async Task<ReleaseInfo?> CheckForUpdateAsync(CancellationToken ct = default)
    {
        try
        {
            var release = await _http.GetFromJsonAsync<GitHubRelease>(ApiUrl, ct);
            if (release is null) return null;

            var latest = release.TagName?.TrimStart('v') ?? "0.0.0";
            if (!IsNewer(latest, CurrentVersion)) return null;

            var asset = release.Assets?.FirstOrDefault(a =>
                a.Name?.EndsWith("-Setup.exe", StringComparison.OrdinalIgnoreCase) == true);

            if (asset is null) return null;

            return new ReleaseInfo(
                Version:      latest,
                ReleaseNotes: release.Body ?? string.Empty,
                DownloadUrl:  asset.BrowserDownloadUrl ?? string.Empty,
                FileSizeBytes: asset.Size,
                PublishedAt:  release.PublishedAt);
        }
        catch (Exception ex)
        {
            _log.Warning($"[Update] Check failed: {ex.Message}");
            return null;
        }
    }

    public async Task<string> DownloadInstallerAsync(ReleaseInfo release, IProgress<double> progress, CancellationToken ct = default)
    {
        var dest = Path.Combine(Path.GetTempPath(), $"NansHoi4Tool-{release.Version}-Setup.exe");
        _log.Info($"[Update] Downloading {release.DownloadUrl} → {dest}");

        using var response = await _http.GetAsync(release.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var total  = response.Content.Headers.ContentLength ?? release.FileSizeBytes;
        var buffer = new byte[81920];
        long downloaded = 0;

        await using var src  = await response.Content.ReadAsStreamAsync(ct);
        await using var file = File.Create(dest);

        int read;
        while ((read = await src.ReadAsync(buffer, ct)) > 0)
        {
            await file.WriteAsync(buffer.AsMemory(0, read), ct);
            downloaded += read;
            if (total > 0) progress.Report(downloaded * 100.0 / total);
        }

        _log.Info("[Update] Download complete.");
        return dest;
    }

    public void ApplyUpdate(string installerPath, bool silent = false)
    {
        var args = silent ? "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART" : "/SILENT";
        Process.Start(new ProcessStartInfo(installerPath, args) { UseShellExecute = true });
        System.Windows.Application.Current?.Dispatcher.Invoke(
            () => System.Windows.Application.Current.Shutdown());
    }

    private static bool IsNewer(string latest, string current)
    {
        if (Version.TryParse(latest, out var l) && Version.TryParse(current, out var c))
            return l > c;
        return false;
    }

    // ── GitHub API models ────────────────────────────────────────────────────
    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]    public string?              TagName      { get; set; }
        [JsonPropertyName("body")]        public string?              Body         { get; set; }
        [JsonPropertyName("published_at")]public DateTime             PublishedAt  { get; set; }
        [JsonPropertyName("assets")]      public List<GitHubAsset>?   Assets       { get; set; }
    }

    private sealed class GitHubAsset
    {
        [JsonPropertyName("name")]                  public string? Name                { get; set; }
        [JsonPropertyName("browser_download_url")]  public string? BrowserDownloadUrl  { get; set; }
        [JsonPropertyName("size")]                  public long    Size                { get; set; }
    }
}
