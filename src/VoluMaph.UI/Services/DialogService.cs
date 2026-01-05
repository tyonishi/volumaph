using Microsoft.Win32;

namespace VoluMaph.UI.Services
{
    public class DialogService : IDialogService
    {
        public string? ShowSaveFile(string defaultFileName, string filter)
        {
            var dialog = new SaveFileDialog
            {
                Filter = filter,
                DefaultExt = System.IO.Path.GetExtension(defaultFileName)?.TrimStart('.'),
                FileName = defaultFileName
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
