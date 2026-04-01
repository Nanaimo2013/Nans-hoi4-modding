using DiscordRPC;
using DiscordRPC.Logging;

namespace NansHoi4Tool.Services;

public class DiscordService : IDiscordService, IDisposable
{
    private const string ClientId = "1234567890123456789";

    private readonly IAppSettingsService _settings;
    private readonly ILogService _log;
    private DiscordRpcClient? _client;

    public bool IsEnabled => _settings.Current.DiscordRichPresenceEnabled && _client?.IsInitialized == true;

    public DiscordService(IAppSettingsService settings, ILogService log)
    {
        _settings = settings;
        _log = log;
    }

    public void Initialize()
    {
        if (!_settings.Current.DiscordRichPresenceEnabled) return;
        try
        {
            _client = new DiscordRpcClient(ClientId)
            {
                Logger = new NullLogger()
            };
            _client.OnReady += (_, e) => _log.Info($"Discord RPC connected: {e.User.Username}");
            _client.OnError += (_, e) => _log.Warning($"Discord RPC error: {e.Message}");
            _client.Initialize();
            SetPresence("In the launcher", "Nan's Hoi4 Tool");
        }
        catch (Exception ex)
        {
            _log.Warning($"Discord RPC init failed (Discord may not be running): {ex.Message}");
        }
    }

    public void SetPresence(string state, string details, string? largeImageKey = null)
    {
        if (_client == null || !_client.IsInitialized) return;
        try
        {
            _client.SetPresence(new RichPresence
            {
                Details = details,
                State = state,
                Assets = new Assets
                {
                    LargeImageKey = largeImageKey ?? "nans_hoi4_tool",
                    LargeImageText = "Nan's Hoi4 Tool",
                    SmallImageKey = "hoi4_icon",
                    SmallImageText = "Hearts of Iron IV"
                },
                Timestamps = new Timestamps { Start = DateTime.UtcNow }
            });
        }
        catch (Exception ex)
        {
            _log.Warning($"Discord RPC SetPresence failed: {ex.Message}");
        }
    }

    public void SetProjectPresence(string projectName, string editorPage)
    {
        var pageLabel = editorPage switch
        {
            "FocusTree" => "Focus Tree Editor",
            "Events" => "Event Editor",
            "Ideas" => "National Spirits",
            "Technologies" => "Technology Editor",
            "Decisions" => "Decisions Editor",
            "Country" => "Country Editor",
            "Localisation" => "Localisation",
            "Units" => "Unit Editor",
            "VersionHistory" => "Version History",
            _ => "Modding"
        };
        SetPresence($"Editing {pageLabel}", $"Project: {projectName}");
    }

    public void ClearPresence() => _client?.ClearPresence();

    public void Shutdown()
    {
        _client?.ClearPresence();
        _client?.Dispose();
        _client = null;
    }

    public void Dispose() => Shutdown();
}
