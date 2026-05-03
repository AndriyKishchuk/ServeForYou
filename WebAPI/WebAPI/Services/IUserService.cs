using System.Security.Claims;
using DataBase.Users;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Request;

namespace WebAPI.Services
{
    public interface IUserService
    {
        Task<ActionResult<IEnumerable<User>>> GetUsersAsync();
        Task<ActionResult<User>> GetMeAsync(ClaimsPrincipal userClaims);
        Task<ActionResult<User>> UpdateMeAsync(ClaimsPrincipal userClaims, UpdateMyProfileRequest request);
        Task<ActionResult> ChangeMyPasswordAsync(ClaimsPrincipal userClaims, ChangePasswordRequest request);
        Task<ActionResult> DeleteMyAccountAsync(ClaimsPrincipal userClaims);
        Task<ActionResult> GetManagersAsync();
        Task<ActionResult<IEnumerable<User>>> GetCompanyEmployeesAsync(ClaimsPrincipal userClaims);
        Task<ActionResult<User>> GetUserAsync(int id);
        Task<ActionResult<User>> CreateUserAsync(RequestRegister request);
        Task<ActionResult<User>> UpdateUserAsync(User user, int id);
        Task<ActionResult> DeleteUserAsync(int id);
    }
}
