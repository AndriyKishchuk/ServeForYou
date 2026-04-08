namespace WebAPI.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;

        
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/gif", "image/webp",
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/zip", "application/x-zip-compressed",
            "text/plain"
        };

        private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB

        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<(string storedFileName, string filePath)> SaveAsync(IFormFile file, int companyId, int taskId)
        {
           
            if (file.Length == 0)
                throw new ArgumentException("File is empty.");
            if (file.Length > MaxFileSizeBytes)
                throw new ArgumentException($"File exceeds maximum size of 20 MB.");

           
            if (!AllowedContentTypes.Contains(file.ContentType))
                throw new ArgumentException($"File type '{file.ContentType}' is not allowed.");

           
            var relativeDirPath = Path.Combine("uploads", companyId.ToString(), taskId.ToString());
            var absoluteDirPath = Path.Combine(_env.WebRootPath, relativeDirPath);

            Directory.CreateDirectory(absoluteDirPath);

            
            var extension = Path.GetExtension(file.FileName);
            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var relativeFilePath = Path.Combine(relativeDirPath, storedFileName).Replace('\\', '/');
            var absoluteFilePath = Path.Combine(absoluteDirPath, storedFileName);

            using var stream = new FileStream(absoluteFilePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return (storedFileName, relativeFilePath);
        }

        public void Delete(string filePath)
        {
            var absolutePath = GetAbsolutePath(filePath);
            if (File.Exists(absolutePath))
                File.Delete(absolutePath);
        }

        public string GetAbsolutePath(string filePath)
            => Path.Combine(_env.WebRootPath, filePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
