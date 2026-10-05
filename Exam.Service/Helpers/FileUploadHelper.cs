using Microsoft.AspNetCore.Http;

namespace Exam.Service.Helpers
{
    public static class FileUploadHelper
    {
        public static async Task<string> SaveFileAsync(IFormFile file, string subFolder = "banners")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is missing or empty.");

            var extension = Path.GetExtension(file.FileName).ToLower();
                
            var forbiddenExtensions = new[]
            {
                ".exe", ".bat", ".cmd", ".sh", ".msi",
                ".php", ".asp", ".aspx", ".cshtml", ".jsp",
                ".dll", ".vb", ".vbs", ".py", ".pl", ".jar",
                ".js", ".html", ".htm"
            };

            if (Array.Exists(forbiddenExtensions, ext => ext == extension))
                throw new InvalidOperationException("This file type is not allowed for upload.");

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subFolder);
            Directory.CreateDirectory(folderPath);

            var uniqueFileName = Guid.NewGuid().ToString() + extension;
            var fullPath = Path.Combine(folderPath, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{subFolder}/{uniqueFileName}";
        }
    }
}
