using DataBase.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Response;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class FileController(IFileService fileService) : ControllerBase
    {
        [HttpPost("tasks/{taskId}/files")]
        public async Task<ActionResult<FileResponse>> UploadFile(int taskId, IFormFile file, [FromForm] FileCategory category)
        {
            return await fileService.UploadFileAsync(User, taskId, file, category);
        }

        [HttpGet("tasks/{taskId}/files")]
        public async Task<ActionResult<IEnumerable<FileResponse>>> GetFiles(int taskId)
        {
            return await fileService.GetFilesAsync(User, taskId);
        }

        [HttpGet("files/{id}/download")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            return await fileService.DownloadFileAsync(User, id);
        }

        [HttpDelete("files/{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            return await fileService.DeleteFileAsync(User, id);
        }
    }
}
