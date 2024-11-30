using Microsoft.Win32;

namespace LinkListCreator.Services
{
    public interface IFileDialogService
    {
        /// <summary>
        /// Opens a save dialog to select a file.
        /// </summary>
        /// <param name="defaultFileName">default file name</param>
        /// <returns>the full path of the selected file or <c>null</c>, if the dialog is aborted</returns>
        string? GetSaveFilePath(string defaultFileName);

        /// <summary>
        /// Opens a open dialog to select a file.
        /// </summary>
        /// <returns>the full path of the selected file or <c>null</c>, if the dialog is aborted</returns>
        string? GetOpenFilePath();
    }

    public class FileDialogService : IFileDialogService
    {
        /// <summary>
        /// Opens a save dialog to select a file.
        /// </summary>
        /// <param name="defaultFileName">default file name</param>
        /// <returns>the full path of the selected file or <c>null</c>, if the dialog is aborted</returns>
        public string? GetSaveFilePath(string defaultFileName)
        {
            string filter = "LINKLIST files (*.linklist)|*.linklist|All files (*.*)|*.*";

            var saveFileDialog = new SaveFileDialog
            {
                FileName = defaultFileName,
                Filter = filter
            };

            return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
        }

        /// <summary>
        /// Opens a open dialog to select a file.
        /// </summary>
        /// <returns>the full path of the selected file or <c>null</c>, if the dialog is aborted</returns>
        public string? GetOpenFilePath()
        {
            string filter = "LINKLIST files (*.linklist)|*.linklist|All files (*.*)|*.*";

            var openFileDialog = new OpenFileDialog
            {
                Filter = filter
            };

            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }
    }

}
