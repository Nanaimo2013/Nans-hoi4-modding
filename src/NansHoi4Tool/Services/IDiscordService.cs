namespace NansHoi4Tool.Services;

public interface IDiscordService
{
    bool IsEnabled { get; }
    void Initialize();
    void SetPresence(string state, string details, string? largeImageKey = null);
    void SetProjectPresence(string projectName, string editorPage);
    void ClearPresence();
    void Shutdown();
}
