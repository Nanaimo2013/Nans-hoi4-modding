using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using System.Windows;
using Microsoft.Win32;

namespace NansHoi4Tool.Services;

public class DialogService : IDialogService
{
    private MetroWindow? GetMetroWindow() =>
        Application.Current?.MainWindow as MetroWindow;

    public async Task<bool> ShowConfirmAsync(string title, string message)
    {
        var win = GetMetroWindow();
        if (win == null) return false;
        var result = await win.ShowMessageAsync(title, message,
            MessageDialogStyle.AffirmativeAndNegative,
            new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });
        return result == MessageDialogResult.Affirmative;
    }

    public async Task ShowAlertAsync(string title, string message)
    {
        var win = GetMetroWindow();
        if (win == null) return;
        await win.ShowMessageAsync(title, message);
    }

    public async Task<string?> ShowInputAsync(string title, string prompt, string defaultValue = "")
    {
        var win = GetMetroWindow();
        if (win == null) return null;
        return await win.ShowInputAsync(title, prompt,
            new MetroDialogSettings { DefaultText = defaultValue });
    }

    public string? OpenFileDialog(string filter = "All Files|*.*", string title = "Open File")
    {
        var dlg = new OpenFileDialog { Filter = filter, Title = title };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public string? SaveFileDialog(string filter = "All Files|*.*", string title = "Save File", string defaultName = "")
    {
        var dlg = new SaveFileDialog { Filter = filter, Title = title, FileName = defaultName };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public string? OpenFolderDialog(string title = "Select Folder")
    {
        var dlg = new OpenFolderDialog { Title = title };
        return dlg.ShowDialog() == true ? dlg.FolderName : null;
    }
}
