namespace WebAPI.Models.Request
{
    public record ChangePasswordRequest(
        string CurrentPassword,
        string NewPassword
    );
}
