using Microsoft.AspNetCore.Components.Forms;
using System.IO;

namespace LinkListCreator.Services
{
    public class FileUploadService
    {
        private readonly string _baseDirectory;

        /// <summary>
        /// Sets the base directory for the file upload service.
        /// If the directory does not exist, it will be created.
        /// </summary>
        public FileUploadService()
        {
            _baseDirectory = Path.Combine(AppContext.BaseDirectory, "wwwroot", "images");

            if (!Directory.Exists(_baseDirectory)) { Directory.CreateDirectory(_baseDirectory); }
        }

        /// <summary>
        /// Saves the file to the base directory.
        /// </summary>
        /// <param name="file">file content</param>
        /// <param name="fileName">file name</param>
        /// <returns></returns>
        public async Task SaveFileAsync(IBrowserFile file, string fileName)
        {
            if (file == null) { return; }

            string filePath = Path.Combine(_baseDirectory, fileName);

            await using FileStream fileStream = new(filePath, FileMode.Create);
            await file.OpenReadStream(maxAllowedSize: 1 * 1024 * 1024) // 1 MB maximum file size
                      .CopyToAsync(fileStream);
        }
    }
}
