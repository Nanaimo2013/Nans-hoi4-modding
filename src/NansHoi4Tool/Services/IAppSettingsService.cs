using NansHoi4Tool.Models;

namespace NansHoi4Tool.Services;

public interface IAppSettingsService
{
    AppSettings Current { get; }
    void Save();
    void Load();
    void AddRecentProject(string path);
}
