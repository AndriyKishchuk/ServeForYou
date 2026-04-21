using System.Security.Claims;
using DataBase.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models.Request;
using WebAPI.Models.Response;
using WebAPI.Factory;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(AplicationContext context, JwtService jwtService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequests loginRequest)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

            if (user is null /* || BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash)*/)
                return Unauthorized("Invalid email or password");

            var token = jwtService.GenerateToken(user);
            var redirectUrl = GetRedirectUrl(user.Role);

            return Ok(new AuthResponse(user.Id, user.Name, user.Email, user.Role,
                                       user.CompanyId, redirectUrl, token));
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(
            [FromBody] RequestRegister request,
            [FromServices] IEntityFactory factory)
        {
            if (await context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email already in use");

            var company = request.CompanyId.HasValue
                ? await context.Companies.FirstOrDefaultAsync(c => c.Id == request.CompanyId.Value)
                : await context.Companies.OrderBy(c => c.Id).FirstOrDefaultAsync();

            if (company is null)
            {
                company = new Company { CompanyName = "ServeForYou" };
                context.Companies.Add(company);
                await context.SaveChangesAsync();
            }

            if (!string.Equals(company.CompanyName, "ServeForYou", StringComparison.Ordinal))
            {
                company.CompanyName = "ServeForYou";
                await context.SaveChangesAsync();
            }

            request = request with { CompanyId = company.Id };

            var user = factory.CreateUser(request);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var token = jwtService.GenerateToken(user);
            var redirectUrl = GetRedirectUrl(user.Role);

            return Ok(new AuthResponse(user.Id, user.Name, user.Email, user.Role,
                                       user.CompanyId, redirectUrl, token));
        }

      
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<AuthResponse>> GetMe()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) return NotFound();

            var redirectUrl = GetRedirectUrl(user.Role);
           
            return Ok(new AuthResponse(user.Id, user.Name, user.Email, user.Role,
                                       user.CompanyId, redirectUrl, string.Empty));
        }

        private static string GetRedirectUrl(UserRole role) => role switch
        {
            UserRole.Admin    => "/customer/overview",
            UserRole.Manager  => "/manager/dashboard",
            UserRole.Employee => "/employee/tasks",  
            _                => "/login"
        };
    }
}

