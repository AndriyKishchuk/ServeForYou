using System.Security.Claims;
using DataBase.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Factory;
using WebAPI.Models.Response;

namespace WebAPI.Services
{
    public class FileService(
        AplicationContext context,
        IFileStorageService fileStorage,
        IEntityFactory factory) : IFileService
    {
        private static int CurrentUserId(ClaimsPrincipal userClaims) =>
            int.Parse(userClaims.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private static int CurrentCompanyId(ClaimsPrincipal userClaims) =>
            int.Parse(userClaims.FindFirstValue("CompanyId")!);
        private static string CurrentRole(ClaimsPrincipal userClaims) =>
            userClaims.FindFirstValue(ClaimTypes.Role)!;

        public async Task<ActionResult<FileResponse>> UploadFileAsync(ClaimsPrincipal userClaims, int taskId, IFormFile file, FileCategory category)
        {
            var currentRole = CurrentRole(userClaims);
            var currentUserId = CurrentUserId(userClaims);
            var currentCompanyId = CurrentCompanyId(userClaims);

            var task = await context.Tasks.FindAsync(taskId);
            if (task is null) return new NotFoundObjectResult("Task not found.");

            if (!HasTaskAccess(task, currentRole, currentUserId)) return new ForbidResult();

            if (currentRole == nameof(UserRole.Manager) && category == FileCategory.Result)
                return new BadRequestObjectResult("Managers can only upload Attachments.");
            if (currentRole == nameof(UserRole.Employee) && category == FileCategory.Attachment)
                return new BadRequestObjectResult("Employees can only upload Results.");

            try
            {
                var (storedFileName, filePath) = await fileStorage.SaveAsync(file, currentCompanyId, taskId);

                var taskFile = factory.CreateTaskFile(file, taskId, currentUserId, storedFileName, filePath, category);

                context.TaskFiles.Add(taskFile);
                await context.SaveChangesAsync();

                return new OkObjectResult(ToResponse(taskFile, userClaims.FindFirstValue(ClaimTypes.Name) ?? "Unknown"));
            }
            catch (ArgumentException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public async Task<ActionResult<IEnumerable<FileResponse>>> GetFilesAsync(ClaimsPrincipal userClaims, int taskId)
        {
            var currentRole = CurrentRole(userClaims);
            var currentUserId = CurrentUserId(userClaims);

            var task = await context.Tasks.FindAsync(taskId);
            if (task is null) return new NotFoundObjectResult("Task not found.");

            if (!HasTaskAccess(task, currentRole, currentUserId)) return new ForbidResult();

            var files = await context.TaskFiles
                .Include(f => f.UploadedBy)
                .Where(f => f.TaskId == taskId)
                .OrderBy(f => f.UploadedAt)
                .ToListAsync();

            if (currentRole == nameof(UserRole.Admin) && task.CreatedByUserId == currentUserId && task.Status != Status.ReturnedToAdmin)
            {
                files = files
                    .Where(f => f.Category != FileCategory.Result)
                    .ToList();
            }

            return new OkObjectResult(files.Select(f =>
                ToResponse(f, $"{f.UploadedBy.Name} {f.UploadedBy.Surname}")));
        }

        public async Task<IActionResult> DownloadFileAsync(ClaimsPrincipal userClaims, int id)
        {
            var currentRole = CurrentRole(userClaims);
            var currentUserId = CurrentUserId(userClaims);

            var file = await context.TaskFiles
                .Include(f => f.Task)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (file is null) return new NotFoundObjectResult("File not found.");

            if (!HasTaskAccess(file.Task, currentRole, currentUserId)) return new ForbidResult();

            if (currentRole == nameof(UserRole.Admin)
                && file.Task.CreatedByUserId == currentUserId
                && file.Category == FileCategory.Result
                && file.Task.Status != Status.ReturnedToAdmin)
            {
                return new ForbidResult();
            }

            var absolutePath = fileStorage.GetAbsolutePath(file.FilePath);
            if (!System.IO.File.Exists(absolutePath))
                return new NotFoundObjectResult("File not found on disk.");

            var stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);
            return new FileStreamResult(stream, file.ContentType)
            {
                FileDownloadName = file.FileName
            };
        }

        public async Task<IActionResult> DeleteFileAsync(ClaimsPrincipal userClaims, int id)
        {
            var currentRole = CurrentRole(userClaims);
            var currentUserId = CurrentUserId(userClaims);

            var file = await context.TaskFiles
                .Include(f => f.Task)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (file is null) return new NotFoundResult();

            if (currentRole == nameof(UserRole.Manager)
                && file.Task.ManagerUserId != currentUserId)
                return new ForbidResult();

            if (currentRole == nameof(UserRole.Admin)
                && file.Task.CreatedByUserId != currentUserId)
                return new ForbidResult();

            fileStorage.Delete(file.FilePath);
            context.TaskFiles.Remove(file);
            await context.SaveChangesAsync();

            return new NoContentResult();
        }

        private static bool HasTaskAccess(Tasks task, string currentRole, int currentUserId)
        {
            return currentRole switch
            {
                nameof(UserRole.Admin) => task.CreatedByUserId == currentUserId,
                nameof(UserRole.Manager) => task.ManagerUserId == currentUserId,
                nameof(UserRole.Employee) => task.AssignedToUserId == currentUserId,
                _ => false
            };
        }

        private static FileResponse ToResponse(TaskFile f, string uploaderName) => new(
            f.Id,
            f.TaskId,
            f.FileName,
            f.ContentType,
            f.FileSize,
            f.Category,
            uploaderName,
            f.UploadedAt
        );
    }
}
