using NansHoi4Tool.Models;
using Newtonsoft.Json;

namespace NansHoi4Tool.Services;

public class AppSettingsService : IAppSettingsService
{
    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NansHoi4Tool");
    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    public AppSettings Current { get; private set; } = new();

    public AppSettingsService()
    {
        Load();
    }

    public void Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                Current = JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch
        {
            Current = new AppSettings();
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonConvert.SerializeObject(Current, Formatting.Indented);
            File.WriteAllText(SettingsPath, json);
        }
        catch { }
    }

    public void AddRecentProject(string path)
    {
        Current.RecentProjectPaths.Remove(path);
        Current.RecentProjectPaths.Insert(0, path);
        if (Current.RecentProjectPaths.Count > 10)
            Current.RecentProjectPaths.RemoveAt(10);
        Save();
    }
}
