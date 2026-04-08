namespace WebAPI.Services
{
    public interface IFileStorageService
    {
        Task<(string storedFileName, string filePath)> SaveAsync(
            IFormFile file, int companyId, int taskId);
        void Delete(string filePath);
        string GetAbsolutePath(string filePath);

    }
}