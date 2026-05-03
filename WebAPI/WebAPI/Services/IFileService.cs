using System.Security.Claims;
using DataBase.Users;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Response;

namespace WebAPI.Services
{
    public interface IFileService
    {
        Task<ActionResult<FileResponse>> UploadFileAsync(ClaimsPrincipal userClaims, int taskId, IFormFile file, FileCategory category);
        Task<ActionResult<IEnumerable<FileResponse>>> GetFilesAsync(ClaimsPrincipal userClaims, int taskId);
        Task<IActionResult> DownloadFileAsync(ClaimsPrincipal userClaims, int id);
        Task<IActionResult> DeleteFileAsync(ClaimsPrincipal userClaims, int id);
    }
}
