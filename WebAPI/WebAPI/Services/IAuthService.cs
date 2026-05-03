using System.Security.Claims;
using DataBase.Users;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Request;
using WebAPI.Models.Response;

namespace WebAPI.Services
{
    public interface IAuthService
    {
        Task<ActionResult<AuthResponse>> LoginAsync(LoginRequests loginRequest);
        Task<ActionResult<AuthResponse>> RegisterAsync(RequestRegister request);
        Task<ActionResult<AuthResponse>> GetMeAsync(ClaimsPrincipal userClaims);
    }
}
