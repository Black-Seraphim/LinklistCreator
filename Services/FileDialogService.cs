using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkListCreator.Services
{
    public interface IFileDialogService
    {
        /// <summary>
        /// Öffnet einen Speicherdialog, um den Dateipfad vom Benutzer zu erhalten.
        /// </summary>
        /// <param name="defaultFileName">Der Standarddateiname.</param>
        /// <param name="filter">Der Filter für die Dateitypen (z. B. JSON-Dateien).</param>
        /// <returns>Der vollständige Pfad der ausgewählten Datei oder <c>null</c>, falls der Dialog abgebrochen wird.</returns>
        string? GetSaveFilePath(string defaultFileName);

        /// <summary>
        /// Öffnet einen Öffnen-Dialog, um eine Datei auszuwählen.
        /// </summary>
        /// <param name="filter">Der Filter für die Dateitypen (z. B. JSON-Dateien).</param>
        /// <returns>Der vollständige Pfad der ausgewählten Datei oder <c>null</c>, falls der Dialog abgebrochen wird.</returns>
        string? GetOpenFilePath();


    }


    public class FileDialogService : IFileDialogService
    {
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
