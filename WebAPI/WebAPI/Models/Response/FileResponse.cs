using DataBase.Users;
namespace WebAPI.Models.Response
{
    public record FileResponse(
        int Id,
        int TaskId,
        string FileName,
        string ContentType,
        long FileSize,
        FileCategory Category,
        string UploadedByName,
        DateTime UploadedAt
    );
}
