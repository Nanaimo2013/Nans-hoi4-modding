using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NansHoi4Tool.Models;
using NansHoi4Tool.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Data;

namespace NansHoi4Tool.ViewModels;

public partial class LocalisationEntry : ObservableObject
{
    [ObservableProperty] private string _key = string.Empty;
    [ObservableProperty] private string _english = string.Empty;
    [ObservableProperty] private string _french = string.Empty;
    [ObservableProperty] private string _german = string.Empty;
    [ObservableProperty] private string _spanish = string.Empty;
    [ObservableProperty] private string _russian = string.Empty;
    [ObservableProperty] private string _polish = string.Empty;
    [ObservableProperty] private bool _hasIssue;
}

public partial class LocalisationViewModel : EditorViewModelBase
{
    [ObservableProperty] private ObservableCollection<LocalisationEntry> _entries = new();
    [ObservableProperty] private LocalisationEntry? _selectedEntry;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _filterLanguage = "All";

    public static IReadOnlyList<string> Languages { get; } = new[]
    {
        "All", "English", "French", "German", "Spanish", "Russian", "Polish"
    };

    private readonly IDialogService _dialogs;

    public LocalisationViewModel(
        INotificationService notifications,
        ILogService log,
        IProjectService projects,
        IDialogService dialogs)
        : base(notifications, log, projects)
    {
        _dialogs = dialogs;
        EntriesView = CollectionViewSource.GetDefaultView(Entries);
        EntriesView.Filter = FilterEntry;
    }

    public ICollectionView EntriesView { get; }

    partial void OnSearchTextChanged(string value) => EntriesView.Refresh();
    partial void OnFilterLanguageChanged(string value) => EntriesView.Refresh();

    private bool FilterEntry(object item)
    {
        if (item is not LocalisationEntry entry) return false;
        if (!string.IsNullOrEmpty(SearchText))
        {
            var q = SearchText;
            if (!entry.Key.Contains(q, StringComparison.OrdinalIgnoreCase) &&
                !entry.English.Contains(q, StringComparison.OrdinalIgnoreCase) &&
                !entry.French.Contains(q, StringComparison.OrdinalIgnoreCase) &&
                !entry.German.Contains(q, StringComparison.OrdinalIgnoreCase))
                return false;
        }
        return true;
    }

    protected override void OnProjectOpened(ProjectData data)
    {
        Entries.Clear();
        foreach (var dto in data.Localisation.Entries)
            Entries.Add(new LocalisationEntry
            {
                Key = dto.Key, English = dto.English, French = dto.French,
                German = dto.German, Spanish = dto.Spanish,
                Russian = dto.Russian, Polish = dto.Polish
            });
    }

    protected override void OnProjectClosed() { Entries.Clear(); SelectedEntry = null; }

    protected override void SyncToProjectData(ProjectData data)
    {
        data.Localisation.Entries = Entries.Select(e => new LocalisationEntryDto
        {
            Key = e.Key, English = e.English, French = e.French,
            German = e.German, Spanish = e.Spanish,
            Russian = e.Russian, Polish = e.Polish
        }).ToList();
    }

    [RelayCommand]
    private void AddEntry()
    {
        var entry = new LocalisationEntry { Key = $"new_key_{Entries.Count + 1}" };
        Entries.Add(entry);
        SelectedEntry = entry;
        MarkDirty();
    }

    [RelayCommand]
    private void DeleteEntry(LocalisationEntry? entry)
    {
        if (entry == null) return;
        Entries.Remove(entry);
        if (SelectedEntry == entry) SelectedEntry = null;
        MarkDirty();
    }

    [RelayCommand]
    private void ImportCsv()
    {
        var path = _dialogs.OpenFileDialog("CSV Files|*.csv|All Files|*.*", "Import Localisation CSV");
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            var lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length < 2) return;

            var header = ParseCsvLine(lines[0]);
            int iKey = IndexOf(header, "Key"), iEn = IndexOf(header, "English"),
                iFr = IndexOf(header, "French"), iDe = IndexOf(header, "German"),
                iEs = IndexOf(header, "Spanish"), iRu = IndexOf(header, "Russian"),
                iPl = IndexOf(header, "Polish");
            if (iKey < 0) { Notifications.ShowError("CSV missing 'Key' column"); return; }

            var imported = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                var cols = ParseCsvLine(lines[i]);
                if (cols.Length <= iKey) continue;
                var key = cols[iKey];
                if (string.IsNullOrWhiteSpace(key)) continue;

                var existing = Entries.FirstOrDefault(e => e.Key == key);
                if (existing == null) { existing = new LocalisationEntry { Key = key }; Entries.Add(existing); }

                if (iEn >= 0 && iEn < cols.Length) existing.English  = cols[iEn];
                if (iFr >= 0 && iFr < cols.Length) existing.French   = cols[iFr];
                if (iDe >= 0 && iDe < cols.Length) existing.German   = cols[iDe];
                if (iEs >= 0 && iEs < cols.Length) existing.Spanish  = cols[iEs];
                if (iRu >= 0 && iRu < cols.Length) existing.Russian  = cols[iRu];
                if (iPl >= 0 && iPl < cols.Length) existing.Polish   = cols[iPl];
                imported++;
            }
            MarkDirty();
            Notifications.ShowSuccess($"Imported {imported} entries from CSV");
        }
        catch (Exception ex) { Notifications.ShowError($"Import failed: {ex.Message}"); }
    }

    [RelayCommand]
    private void ExportCsv()
    {
        var path = _dialogs.SaveFileDialog("CSV Files|*.csv|All Files|*.*", "Export Localisation CSV", "localisation.csv");
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Key,English,French,German,Spanish,Russian,Polish");
            foreach (var e in Entries)
                sb.AppendLine($"{CsvEscape(e.Key)},{CsvEscape(e.English)}," +
                              $"{CsvEscape(e.French)},{CsvEscape(e.German)}," +
                              $"{CsvEscape(e.Spanish)},{CsvEscape(e.Russian)},{CsvEscape(e.Polish)}");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
            Notifications.ShowSuccess($"Exported {Entries.Count} entries to CSV");
        }
        catch (Exception ex) { Notifications.ShowError($"Export failed: {ex.Message}"); }
    }

    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var field  = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"') { if (inQuotes && i + 1 < line.Length && line[i + 1] == '"') { field.Append('"'); i++; } else inQuotes = !inQuotes; }
            else if (c == ',' && !inQuotes) { fields.Add(field.ToString()); field.Clear(); }
            else field.Append(c);
        }
        fields.Add(field.ToString());
        return fields.ToArray();
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private static int IndexOf(string[] headers, string name)
        => Array.FindIndex(headers, h => h.Trim().Equals(name, StringComparison.OrdinalIgnoreCase));
}
