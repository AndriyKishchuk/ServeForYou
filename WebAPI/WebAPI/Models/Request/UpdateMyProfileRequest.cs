namespace WebAPI.Models.Request
{
    public record UpdateMyProfileRequest(
        string Name,
        string Surname,
        string Email
    );
}
