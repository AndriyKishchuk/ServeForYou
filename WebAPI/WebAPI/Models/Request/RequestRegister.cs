using DataBase.Users;

namespace WebAPI.Models.Request
{
    public record RequestRegister(
        string Name,
        string Surname,
        string Email,
        string Password,
        UserRole Role,
        int? CompanyId
    );
    
}
