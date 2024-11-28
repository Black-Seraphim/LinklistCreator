using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkListCreator.Services
{
    public class FileUploadService
    {
        private readonly string _baseDirectory;
        public FileUploadService()
        {
            // Setzt das Basisverzeichnis auf wwwroot/Images
            _baseDirectory = Path.Combine(AppContext.BaseDirectory, "wwwroot", "images");

            // Erstellt den Ordner, falls er nicht existiert
            if (!Directory.Exists(_baseDirectory))
            {
                Directory.CreateDirectory(_baseDirectory);
            }
        }

        public async Task SaveFileAsync(IBrowserFile file, string fileName)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            var filePath = Path.Combine(_baseDirectory, fileName);

            // Datei speichern
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.OpenReadStream(maxAllowedSize: 1 * 1024 * 1024) // 1 MB maximale Größe
                .CopyToAsync(fileStream);
        }
    }
}
