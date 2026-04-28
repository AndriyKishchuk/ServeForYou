using DataBase.Users;

namespace WebAPI.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
