namespace NansHoi4Tool.Services;

public interface IDialogService
{
    Task<bool> ShowConfirmAsync(string title, string message);
    Task ShowAlertAsync(string title, string message);
    Task<string?> ShowInputAsync(string title, string prompt, string defaultValue = "");
    string? OpenFileDialog(string filter = "All Files|*.*", string title = "Open File");
    string? SaveFileDialog(string filter = "All Files|*.*", string title = "Save File", string defaultName = "");
    string? OpenFolderDialog(string title = "Select Folder");
}
