using DataBase.Users;
namespace WebAPI.Models.Response
{
    public record AuthResponse(
        int UserId,
        string? Name,
        string? Email,
        UserRole Role,
        int CompanyId,
        string RedirectUrl,
        string Token
    );
}

