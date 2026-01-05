namespace VoluMaph.UI.Services
{
    public interface IDialogService
    {
        // Shows a save file dialog. Returns selected file path or null if cancelled.
        string? ShowSaveFile(string defaultFileName, string filter);
    }
}
