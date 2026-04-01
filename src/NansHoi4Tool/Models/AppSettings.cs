namespace NansHoi4Tool.Models;

public class AppSettings
{
    public string Theme { get; set; } = "Dark";
    public string AccentColor { get; set; } = "Blue";
    public string UserName { get; set; } = string.Empty;
    public string UserAvatarPath { get; set; } = string.Empty;
    public bool DiscordRichPresenceEnabled { get; set; } = true;
    public bool AutoSaveEnabled { get; set; } = true;
    public int AutoSaveIntervalSeconds { get; set; } = 60;
    public string Hoi4InstallPath { get; set; } = string.Empty;
    public List<string> RecentProjectPaths { get; set; } = new();
    public CollaborationSettings Collaboration { get; set; } = new();
    public AppearanceSettings Appearance { get; set; } = new();
}

public class CollaborationSettings
{
    public string DisplayName { get; set; } = string.Empty;
    public int ServerPort { get; set; } = 5050;
    public bool AutoStartServer { get; set; } = false;
}

public class AppearanceSettings
{
    public bool AnimationsEnabled { get; set; } = true;
    public double SidebarWidth { get; set; } = 220;
    public bool SidebarCollapsed { get; set; } = false;
    public string FontSize { get; set; } = "Medium";
}
