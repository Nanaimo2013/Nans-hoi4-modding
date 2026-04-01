using NansHoi4Tool.Models;
using NansHoi4Tool.Shared;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace NansHoi4Tool.Services;

public class ProjectService : IProjectService
{
    private readonly IAppSettingsService _settings;
    private readonly ILogService _log;
    private readonly INotificationService _notifications;
    private readonly IHoi4ScriptExporter _exporter;

    public ProjectMetadata? CurrentProject { get; private set; }
    public ProjectData CurrentData { get; private set; } = new();
    public bool IsProjectOpen => CurrentProject != null;
    public string? CurrentProjectPath { get; private set; }

    public event EventHandler<ProjectMetadata>? ProjectOpened;
    public event EventHandler? ProjectClosed;
    public event EventHandler? ProjectChanged;

    public ProjectService(
        IAppSettingsService settings,
        ILogService log,
        INotificationService notifications,
        IHoi4ScriptExporter exporter)
    {
        _settings = settings;
        _log = log;
        _notifications = notifications;
        _exporter = exporter;
    }

    public async Task<bool> NewProjectAsync(string name, string modId, string author, string projectFolderPath)
    {
        try
        {
            // Create the folder structure on disk
            var projectDir = Path.Combine(projectFolderPath, name);
            Directory.CreateDirectory(projectDir);
            Directory.CreateDirectory(Path.Combine(projectDir, "countries"));
            Directory.CreateDirectory(Path.Combine(projectDir, "focustrees"));
            Directory.CreateDirectory(Path.Combine(projectDir, "tech"));
            Directory.CreateDirectory(Path.Combine(projectDir, "events"));
            Directory.CreateDirectory(Path.Combine(projectDir, "decisions"));
            Directory.CreateDirectory(Path.Combine(projectDir, "ideas"));
            Directory.CreateDirectory(Path.Combine(projectDir, "history"));
            Directory.CreateDirectory(Path.Combine(projectDir, "exports"));

            CurrentProject = new ProjectMetadata
            {
                Name = name,
                ModId = modId,
                Author = author,
                Contributors = new List<ContributorInfo>
                {
                    new() { UserId = author, DisplayName = author, Role = "Owner" }
                }
            };
            CurrentData = new ProjectData();

            var projectFilePath = Path.Combine(projectDir, $"{modId}.h4proj");
            if (!await WriteProjectFileAsync(projectFilePath))
                return false;

            CurrentProjectPath = projectFilePath;
            _settings.AddRecentProject(projectFilePath);
            ProjectOpened?.Invoke(this, CurrentProject);
            _log.Info($"New project created: {name} at {projectFilePath}");
            return true;
        }
        catch (Exception ex)
        {
            _log.Error("Failed to create project", ex);
            _notifications.ShowError($"Failed to create project: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> OpenProjectAsync(string path)
    {
        try
        {
            if (!File.Exists(path)) return false;
            using var zip = ZipFile.OpenRead(path);
            var projectEntry = zip.GetEntry("project.json");
            if (projectEntry == null) return false;

            using var stream = projectEntry.Open();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            CurrentProject = JsonConvert.DeserializeObject<ProjectMetadata>(json);
            if (CurrentProject == null) return false;

            var dataEntry = zip.GetEntry("data.json");
            if (dataEntry != null)
            {
                using var ds = dataEntry.Open();
                using var dr = new StreamReader(ds);
                var dataJson = await dr.ReadToEndAsync();
                CurrentData = JsonConvert.DeserializeObject<ProjectData>(dataJson) ?? new ProjectData();
            }
            else
            {
                CurrentData = new ProjectData();
            }

            CurrentProjectPath = path;
            _settings.AddRecentProject(path);
            ProjectOpened?.Invoke(this, CurrentProject);
            _log.Info($"Opened project: {CurrentProject.Name}");
            return true;
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to open project: {path}", ex);
            _notifications.ShowError($"Failed to open project: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SaveProjectAsync()
    {
        if (CurrentProjectPath == null)
            return await SaveProjectAsAsync(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "NansHoi4Tool", $"{CurrentProject?.Name ?? "project"}.h4proj"));
        return await WriteProjectFileAsync(CurrentProjectPath);
    }

    public async Task<bool> SaveProjectAsAsync(string path)
    {
        if (await WriteProjectFileAsync(path))
        {
            CurrentProjectPath = path;
            _settings.AddRecentProject(path);
            return true;
        }
        return false;
    }

    private async Task<bool> WriteProjectFileAsync(string path)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            if (CurrentProject != null)
                CurrentProject.UpdatedAt = DateTime.UtcNow;

            if (File.Exists(path)) File.Delete(path);
            using var zip = ZipFile.Open(path, ZipArchiveMode.Create);

            var projectEntry = zip.CreateEntry("project.json");
            await using (var stream = projectEntry.Open())
            await using (var writer = new StreamWriter(stream))
                await writer.WriteAsync(JsonConvert.SerializeObject(CurrentProject, Formatting.Indented));

            var dataEntry = zip.CreateEntry("data.json");
            await using (var ds = dataEntry.Open())
            await using (var dw = new StreamWriter(ds))
                await dw.WriteAsync(JsonConvert.SerializeObject(CurrentData, Formatting.Indented));

            zip.CreateEntry("mod/");
            zip.CreateEntry("assets/");
            zip.CreateEntry("history/");

            _log.Info($"Saved project to: {path}");
            ProjectChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to save project: {path}", ex);
            _notifications.ShowError($"Save failed: {ex.Message}");
            return false;
        }
    }

    public void CloseProject()
    {
        CurrentProject = null;
        CurrentData = new ProjectData();
        CurrentProjectPath = null;
        ProjectClosed?.Invoke(this, EventArgs.Empty);
        _log.Info("Project closed");
    }

    public void ForceReload()
    {
        if (CurrentProject == null) return;
        ProjectOpened?.Invoke(this, CurrentProject);
        _log.Info("Project force-reloaded");
    }

    public async Task<bool> ExportToHoi4Async(string outputFolder)
    {
        if (CurrentProject == null) return false;
        try
        {
            var modFolder = Path.Combine(outputFolder, CurrentProject.ModId);
            Directory.CreateDirectory(modFolder);

            var descriptor = $"name=\"{CurrentProject.Name}\"\n" +
                             $"path=\"mod/{CurrentProject.ModId}\"\n" +
                             $"supported_version=\"{CurrentProject.Hoi4Version}\"\n" +
                             $"version=\"{CurrentProject.Version}\"\n";
            await File.WriteAllTextAsync(Path.Combine(outputFolder, $"{CurrentProject.ModId}.mod"), descriptor);
            await File.WriteAllTextAsync(Path.Combine(modFolder, "descriptor.mod"), descriptor);

            // Focus tree
            if (CurrentData.FocusTree.Nodes.Count > 0)
            {
                var ftDir = Path.Combine(modFolder, "common", "national_focus");
                Directory.CreateDirectory(ftDir);
                await File.WriteAllTextAsync(
                    Path.Combine(ftDir, $"{CurrentData.FocusTree.TreeId}.txt"),
                    _exporter.ExportFocusTree(CurrentData.FocusTree));
            }

            // Events
            if (CurrentData.Events.Events.Count > 0)
            {
                var evDir = Path.Combine(modFolder, "events");
                Directory.CreateDirectory(evDir);
                await File.WriteAllTextAsync(
                    Path.Combine(evDir, $"{CurrentData.Events.Namespace}.txt"),
                    _exporter.ExportEvents(CurrentData.Events));
            }

            // Ideas
            if (CurrentData.Ideas.Ideas.Count > 0)
            {
                var idDir = Path.Combine(modFolder, "common", "ideas");
                Directory.CreateDirectory(idDir);
                await File.WriteAllTextAsync(
                    Path.Combine(idDir, $"{CurrentProject.ModId}_ideas.txt"),
                    _exporter.ExportIdeas(CurrentData.Ideas));
            }

            // Decisions
            if (CurrentData.Decisions.Categories.Count > 0)
            {
                var dcDir = Path.Combine(modFolder, "common", "decisions");
                Directory.CreateDirectory(dcDir);
                await File.WriteAllTextAsync(
                    Path.Combine(dcDir, $"{CurrentProject.ModId}_decisions.txt"),
                    _exporter.ExportDecisions(CurrentData.Decisions));
            }

            // Technologies
            if (CurrentData.Technologies.Technologies.Count > 0)
            {
                var techDir = Path.Combine(modFolder, "common", "technologies");
                Directory.CreateDirectory(techDir);
                await File.WriteAllTextAsync(
                    Path.Combine(techDir, $"{CurrentProject.ModId}_technologies.txt"),
                    _exporter.ExportTechnologies(CurrentData.Technologies));
            }

            // Localisation
            if (CurrentData.Localisation.Entries.Count > 0)
            {
                var locDir = Path.Combine(modFolder, "localisation");
                Directory.CreateDirectory(locDir);
                foreach (var lang in new[] { "english", "french", "german", "spanish", "russian", "polish" })
                    await File.WriteAllTextAsync(
                        Path.Combine(locDir, $"{CurrentProject.ModId}_l_{lang}.yml"),
                        _exporter.ExportLocalisation(CurrentData.Localisation, lang),
                        System.Text.Encoding.UTF8);
            }

            _notifications.ShowSuccess($"Mod exported to: {modFolder}");
            _log.Info($"Exported mod to: {modFolder}");
            return true;
        }
        catch (Exception ex)
        {
            _log.Error("Export failed", ex);
            _notifications.ShowError($"Export failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ImportFromHoi4Async(string modFolder)
    {
        try
        {
            var descriptorPath = Directory.GetFiles(modFolder, "descriptor.mod", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (descriptorPath == null) return false;

            // ── metadata ─────────────────────────────────────────────────────
            var descText = await File.ReadAllTextAsync(descriptorPath);
            var descNode = ClausewitzParser.Parse(descText);
            var meta = new ProjectMetadata
            {
                Name        = descNode.GetString("name", Path.GetFileName(modFolder)),
                Version     = descNode.GetString("version", "1.0"),
                Hoi4Version = descNode.GetString("supported_version", "1.*.*"),
                ModId       = Path.GetFileName(modFolder)
            };

            var data = new ProjectData();

            // ── focus trees ───────────────────────────────────────────────────
            var ftDir = Path.Combine(modFolder, "common", "national_focus");
            if (Directory.Exists(ftDir))
            {
                foreach (var file in Directory.EnumerateFiles(ftDir, "*.txt"))
                {
                    var text = await File.ReadAllTextAsync(file);
                    var root = ClausewitzParser.Parse(text);
                    var ft   = root.Block("focus_tree");
                    if (ft == null) continue;
                    data.FocusTree.TreeId     = ft.GetString("id", Path.GetFileNameWithoutExtension(file));
                    data.FocusTree.CountryTag = ft.Block("country")?.Block("modifier")?.GetString("tag") ?? string.Empty;
                    data.FocusTree.Continuous = ft.GetBool("is_default");
                    foreach (var f in ft.All("focus"))
                    {
                        var prereqIds = f.All("prerequisite")
                            .SelectMany(p => p.All("focus")).Select(x => x.Key).ToList();
                        var mutexIds = f.All("mutually_exclusive")
                            .SelectMany(m => m.All("focus")).Select(x => x.Key).ToList();
                        data.FocusTree.Nodes.Add(new FocusNodeDto
                        {
                            Id = f.GetString("id"), Name = f.GetString("id"),
                            Icon = f.GetString("icon", "GFX_goal_generic_political_pressure"),
                            Cost = f.GetInt("cost", 70),
                            X = f.GetDouble("x"), Y = f.GetDouble("y"),
                            Prerequisites = prereqIds, MutuallyExclusive = mutexIds,
                            CancelIfInvalid = f.GetBool("cancel_if_invalid", true),
                            ContinueIfInvalid = f.GetBool("continue_if_invalid"),
                            AvailableIfCapitulated = f.GetBool("available_if_capitulated")
                        });
                    }
                    break;
                }
            }

            // ── events ────────────────────────────────────────────────────────
            var evDir = Path.Combine(modFolder, "events");
            if (Directory.Exists(evDir))
            {
                foreach (var file in Directory.EnumerateFiles(evDir, "*.txt"))
                {
                    var text = await File.ReadAllTextAsync(file);
                    var root = ClausewitzParser.Parse(text);
                    data.Events.Namespace = root.GetString("add_namespace", data.Events.Namespace);
                    foreach (var evType in new[] { "country_event", "news_event", "state_event", "unit_leader_event" })
                        foreach (var ev in root.All(evType))
                        {
                            var opts = ev.All("option").Select(o => new EventOptionDto
                            {
                                Name   = o.GetString("name"),
                                Effect = o.Block("ai_chance") == null ? string.Empty : string.Empty
                            }).ToList();
                            data.Events.Events.Add(new EventDto
                            {
                                Id = ev.GetString("id"), Type = evType,
                                Picture = ev.GetString("picture", "GFX_report_event_generic_read_write"),
                                IsTriggeredOnly = ev.GetBool("is_triggered_only"),
                                Hidden = ev.GetBool("hidden"), FireOnlyOnce = ev.GetBool("fire_only_once"),
                                Options = opts
                            });
                        }
                }
            }

            // ── ideas ─────────────────────────────────────────────────────────
            var idDir = Path.Combine(modFolder, "common", "ideas");
            if (Directory.Exists(idDir))
            {
                foreach (var file in Directory.EnumerateFiles(idDir, "*.txt"))
                {
                    var text = await File.ReadAllTextAsync(file);
                    var root = ClausewitzParser.Parse(text);
                    var ideasBlock = root.Block("ideas");
                    if (ideasBlock == null) continue;
                    foreach (var slot in ideasBlock.Children.Where(c => c.IsBlock))
                        foreach (var idea in slot.Children.Where(c => c.IsBlock))
                        {
                            var mods = idea.Block("modifier")?.Children
                                .Where(c => c.Value != null)
                                .Select(c => new ModifierDto { Key = c.Key, Value = c.Value! })
                                .ToList() ?? new();
                            data.Ideas.Ideas.Add(new IdeaDto
                            {
                                Id = idea.Key, Name = idea.Key, Slot = slot.Key,
                                Picture = idea.GetString("picture", "GFX_idea_generic_political_advisor"),
                                AllowedCivilWar = idea.Block("allowed_civil_war") != null,
                                Modifiers = mods
                            });
                        }
                }
            }

            // ── decisions ─────────────────────────────────────────────────────
            var dcDir = Path.Combine(modFolder, "common", "decisions");
            if (Directory.Exists(dcDir))
            {
                foreach (var file in Directory.EnumerateFiles(dcDir, "*.txt"))
                {
                    var text = await File.ReadAllTextAsync(file);
                    var root = ClausewitzParser.Parse(text);
                    foreach (var cat in root.Children.Where(c => c.IsBlock))
                    {
                        var catDto = new DecisionCategoryDto { Id = cat.Key, Name = cat.Key };
                        foreach (var dec in cat.Children.Where(c => c.IsBlock))
                            catDto.Decisions.Add(new DecisionDto
                            {
                                Id = dec.Key, Name = dec.Key,
                                Icon = dec.GetString("icon", "GFX_decision_generic_political_discourse"),
                                CostPoliticalPower = dec.GetInt("cost", 50),
                                DaysRemove = dec.GetInt("days_remove"), DaysReDo = dec.GetInt("days_re_do")
                            });
                        if (catDto.Decisions.Count > 0)
                            data.Decisions.Categories.Add(catDto);
                    }
                }
            }

            // ── technologies ──────────────────────────────────────────────────
            var techImportDir = Path.Combine(modFolder, "common", "technologies");
            if (Directory.Exists(techImportDir))
            {
                foreach (var file in Directory.EnumerateFiles(techImportDir, "*.txt"))
                {
                    var text = await File.ReadAllTextAsync(file);
                    var root = ClausewitzParser.Parse(text);
                    var techBlock = root.Block("technologies");
                    if (techBlock == null) continue;
                    data.Technologies.CategoryId = Path.GetFileNameWithoutExtension(file);
                    foreach (var tech in techBlock.Children.Where(c => c.IsBlock))
                    {
                        var folderBlock = tech.Block("folder");
                        double tx = 0, ty = 0;
                        var posBlock = folderBlock?.Block("position");
                        if (posBlock != null) { tx = posBlock.GetDouble("x"); ty = posBlock.GetDouble("y"); }
                        var prereqs = tech.All("dependencies")
                            .SelectMany(d => d.Children.Where(c => !c.IsBlock).Select(c => c.Key))
                            .ToList();
                        var bonuses = tech.Block("categories")?.Children
                            .Where(c => c.Value != null)
                            .Select(c => new ModifierDto { Key = c.Key, Value = c.Value! })
                            .ToList() ?? new();
                        data.Technologies.Technologies.Add(new TechnologyDto
                        {
                            Id = tech.Key, Name = tech.Key,
                            ResearchCost = tech.GetInt("research_cost", 1),
                            StartYear = tech.GetInt("start_year", 1936),
                            Folder = folderBlock?.GetString("name") ?? string.Empty,
                            X = tx, Y = ty,
                            Prerequisites = prereqs, Bonuses = bonuses
                        });
                    }
                }
            }

            // ── localisation ──────────────────────────────────────────────────
            var locDir = Path.Combine(modFolder, "localisation");
            if (Directory.Exists(locDir))
            {
                var entryMap = new Dictionary<string, LocalisationEntryDto>(StringComparer.OrdinalIgnoreCase);
                foreach (var file in Directory.EnumerateFiles(locDir, "*_l_*.yml", SearchOption.AllDirectories))
                {
                    var lang = Regex.Match(file, @"_l_(\w+)\.yml").Groups[1].Value.ToLower();
                    var text = await File.ReadAllTextAsync(file, System.Text.Encoding.UTF8);
                    foreach (var (key, value) in ClausewitzParser.ParseYml(text))
                    {
                        if (!entryMap.TryGetValue(key, out var entry))
                        {
                            entry = new LocalisationEntryDto { Key = key };
                            entryMap[key] = entry;
                        }
                        switch (lang)
                        {
                            case "english": entry.English  = value; break;
                            case "french":  entry.French   = value; break;
                            case "german":  entry.German   = value; break;
                            case "spanish": entry.Spanish  = value; break;
                            case "russian": entry.Russian  = value; break;
                            case "polish":  entry.Polish   = value; break;
                        }
                    }
                }
                data.Localisation.Entries = entryMap.Values.ToList();
            }

            CurrentProject     = meta;
            CurrentData        = data;
            CurrentProjectPath = null;
            ProjectOpened?.Invoke(this, meta);
            _log.Info($"Imported mod: {meta.Name} — focuses:{data.FocusTree.Nodes.Count} events:{data.Events.Events.Count} ideas:{data.Ideas.Ideas.Count} loc:{data.Localisation.Entries.Count}");
            _notifications.ShowSuccess($"Imported mod: {meta.Name}");
            return true;
        }
        catch (Exception ex)
        {
            _log.Error("Import failed", ex);
            _notifications.ShowError($"Import failed: {ex.Message}");
            return false;
        }
    }

    // ── Collaboration snapshot helpers ──────────────────────────────────────

    public void LoadTemporarySnapshot(string projectName, ProjectData data)
    {
        CurrentProject = new ProjectMetadata
        {
            Id   = System.Guid.NewGuid().ToString("N")[..8],
            Name = projectName
        };
        CurrentData        = data;
        CurrentProjectPath = null;
        ProjectOpened?.Invoke(this, CurrentProject);
        _log.Info($"[Collab] Loaded temporary snapshot: {projectName}");
    }

    public void CloseTemporarySnapshot()
    {
        if (CurrentProjectPath != null) return; // not a temp snapshot
        CurrentProject = null;
        CurrentData    = new ProjectData();
        ProjectClosed?.Invoke(this, EventArgs.Empty);
        _log.Info("[Collab] Temporary snapshot closed");
    }

    public string SerializeCurrentProject()
    {
        try { return System.Text.Json.JsonSerializer.Serialize(CurrentData); }
        catch (Exception ex) { _log.Warning($"SerializeCurrentProject failed: {ex.Message}"); return "{}"; }
    }

    public void ApplyRemoteChange(NansHoi4Tool.Shared.ProjectChangeEvent change)
    {
        // Simple no-op for now; full CRDT/patch implementation can be added per entity type
        _log.Info($"[Collab] Remote change: {change.EntityType}/{change.EntityId} ({change.ChangeType})");
        ProjectChanged?.Invoke(this, EventArgs.Empty);
    }
}
