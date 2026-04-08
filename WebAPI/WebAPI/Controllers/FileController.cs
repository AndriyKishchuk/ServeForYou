using System.Security.Claims;
using DataBase.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Factory;
using WebAPI.Models.Response;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class FileController(AplicationContext context, IFileStorageService fileStorage, IEntityFactory factory) : ControllerBase
    {
        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private int CurrentCompanyId =>
            int.Parse(User.FindFirstValue("CompanyId")!);
        private string CurrentRole =>
            User.FindFirstValue(ClaimTypes.Role)!;

      
        [HttpPost("tasks/{taskId}/files")]
        public async Task<ActionResult<FileResponse>> UploadFile(
            int taskId,
            IFormFile file,
            [FromForm] FileCategory category)
        {
            var task = await context.Tasks.FindAsync(taskId);
            if (task is null) return NotFound("Task not found.");

         
            if (!HasTaskAccess(task)) return Forbid();

            if (CurrentRole == nameof(UserRole.Manager) && category == FileCategory.Result)
                return BadRequest("Managers can only upload Attachments.");
            if (CurrentRole == nameof(UserRole.Employee) && category == FileCategory.Attachment)
                return BadRequest("Employees can only upload Results.");

            try
            {
                var (storedFileName, filePath) = await fileStorage.SaveAsync(file, CurrentCompanyId, taskId);

                var taskFile = factory.CreateTaskFile(file, taskId, CurrentUserId, storedFileName, filePath, category);

                context.TaskFiles.Add(taskFile);
                await context.SaveChangesAsync();

                return Ok(ToResponse(taskFile, User.FindFirstValue(ClaimTypes.Name) ?? "Unknown"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

  
        [HttpGet("tasks/{taskId}/files")]
        public async Task<ActionResult<IEnumerable<FileResponse>>> GetFiles(int taskId)
        {
            var task = await context.Tasks.FindAsync(taskId);
            if (task is null) return NotFound("Task not found.");

            if (!HasTaskAccess(task)) return Forbid();

            var files = await context.TaskFiles
                .Include(f => f.UploadedBy)
                .Where(f => f.TaskId == taskId)
                .OrderBy(f => f.UploadedAt)
                .ToListAsync();

            if (CurrentRole == nameof(UserRole.Admin) && task.CreatedByUserId == CurrentUserId && task.Status != Status.ReturnedToAdmin)
            {
                files = files
                    .Where(f => f.Category != FileCategory.Result)
                    .ToList();
            }

            return Ok(files.Select(f =>
                ToResponse(f, $"{f.UploadedBy.Name} {f.UploadedBy.Surname}")));
        }

        [HttpGet("files/{id}/download")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var file = await context.TaskFiles
                .Include(f => f.Task)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (file is null) return NotFound("File not found.");

            if (!HasTaskAccess(file.Task)) return Forbid();

            if (CurrentRole == nameof(UserRole.Admin)
                && file.Task.CreatedByUserId == CurrentUserId
                && file.Category == FileCategory.Result
                && file.Task.Status != Status.ReturnedToAdmin)
            {
                return Forbid();
            }

            var absolutePath = fileStorage.GetAbsolutePath(file.FilePath);
            if (!System.IO.File.Exists(absolutePath))
                return NotFound("File not found on disk.");

            var stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);
            return File(stream, file.ContentType, file.FileName);
        }

      
        [HttpDelete("files/{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var file = await context.TaskFiles
                .Include(f => f.Task)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (file is null) return NotFound();

           
            if (CurrentRole == nameof(UserRole.Manager)
                && file.Task.ManagerUserId != CurrentUserId)
                return Forbid();

            if (CurrentRole == nameof(UserRole.Admin)
                && file.Task.CreatedByUserId != CurrentUserId)
                return Forbid();

            fileStorage.Delete(file.FilePath);
            context.TaskFiles.Remove(file);
            await context.SaveChangesAsync();

            return NoContent();
        }

    
        private bool HasTaskAccess(Tasks task)
        {
            return CurrentRole switch
            {
                nameof(UserRole.Admin) => task.CreatedByUserId == CurrentUserId,
                nameof(UserRole.Manager) => task.ManagerUserId == CurrentUserId,
                nameof(UserRole.Employee) => task.AssignedToUserId == CurrentUserId,
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
