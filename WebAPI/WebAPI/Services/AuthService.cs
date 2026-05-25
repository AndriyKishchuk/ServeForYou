using System.Security.Claims;
using DataBase.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Factory;
using WebAPI.Models.Request;
using WebAPI.Models.Response;

namespace WebAPI.Services
{
    public class AuthService(AplicationContext context, IJwtService jwtService, IEntityFactory factory) : IAuthService
    {
        public async Task<ActionResult<AuthResponse>> LoginAsync(LoginRequests loginRequest)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

            if (user is null)
                return new UnauthorizedObjectResult("Invalid email or password");

            var token = jwtService.GenerateToken(user);
            var redirectUrl = GetRedirectUrl(user.Role);

            return new OkObjectResult(new AuthResponse(user.Id, user.Name, user.Email, user.Role,
                                       user.CompanyId, redirectUrl, token));
        }

        public async Task<ActionResult<AuthResponse>> RegisterAsync(RequestRegister request)
        {
            if (await context.Users.AnyAsync(u => u.Email == request.Email))
                return new BadRequestObjectResult("Email already in use");

            var company = await context.Companies.FirstOrDefaultAsync(i => i.CompanyName == "ServeForYou");

            if (company == null) return new BadRequestObjectResult("Invalid company ID");

            request = request with { CompanyId = company.Id };

            var user = factory.CreateUser(request);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var token = jwtService.GenerateToken(user);
            var redirectUrl = GetRedirectUrl(user.Role);

            return new OkObjectResult(new AuthResponse(user.Id, user.Name, user.Email, user.Role,
                                       user.CompanyId, redirectUrl, token));
        }

        public async Task<ActionResult<AuthResponse>> GetMeAsync(ClaimsPrincipal userClaims)
        {
            var userId = int.Parse(userClaims.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) return new NotFoundResult();

            var redirectUrl = GetRedirectUrl(user.Role);

            return new OkObjectResult(new AuthResponse(user.Id, user.Name, user.Email, user.Role,
                                       user.CompanyId, redirectUrl, string.Empty));
        }

        private static string GetRedirectUrl(UserRole role) => role switch
        {
            UserRole.Admin => "/customer/overview",
            UserRole.Manager => "/manager/dashboard",
            UserRole.Employee => "/employee/tasks",
            _ => "/login"
        };
    }
}
