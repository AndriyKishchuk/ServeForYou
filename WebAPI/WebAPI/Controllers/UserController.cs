using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataBase.Users;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebAPI.Models.Request;
using WebAPI.Factory;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController(AplicationContext context) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await context.Users.Include(u => u.Company).ToListAsync();
        }

        [HttpGet("me")]
        public async Task<ActionResult<User>> GetMe()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await context.Users.Include(u => u.Company)
                                         .FirstOrDefaultAsync(u => u.Id == userId);
            return user is null ? NotFound() : user;
        }

        [HttpPut("me")]
        public async Task<ActionResult<User>> UpdateMe([FromBody] UpdateMyProfileRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await context.Users.Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null) return NotFound();

            var normalizedEmail = request.Email.Trim();
            var emailTaken = await context.Users.AnyAsync(u => u.Id != userId && u.Email == normalizedEmail);
            if (emailTaken)
                return BadRequest("Email already in use");

            user.Name = request.Name.Trim();
            user.Surname = request.Surname.Trim();
            user.Email = normalizedEmail;

            await context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPatch("me/password")]
        public async Task<ActionResult> ChangeMyPassword([FromBody] ChangePasswordRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return BadRequest("Current password is incorrect");

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                return BadRequest("New password must be at least 6 characters long");

            user.PasswordHash = request.NewPassword;
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("me")]
        public async Task<ActionResult> DeleteMyAccount()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) return NotFound();

            var hasRelatedTasks = await context.Tasks.AnyAsync(t =>
                t.CreatedByUserId == userId ||
                t.ManagerUserId == userId ||
                t.AssignedToUserId == userId);

            if (hasRelatedTasks)
                return BadRequest("You cannot delete this account while it is linked to existing tasks.");

            var hasUploadedFiles = await context.TaskFiles.AnyAsync(f => f.UploadedByUserId == userId);
            if (hasUploadedFiles)
                return BadRequest("You cannot delete this account while uploaded files are linked to it.");

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("managers")]
        [AllowAnonymous]
        public async Task<ActionResult> GetManagers()
        {
            var managers = await context.Users
                .Include(u => u.Company)
                .Where(u => u.Role == UserRole.Manager)
                .OrderBy(u => u.Name)
                .ToListAsync();

            var ratingStats = await context.Tasks
                .Where(t => t.ManagerRating.HasValue)
                .GroupBy(t => t.ManagerUserId)
                .Select(group => new
                {
                    ManagerUserId = group.Key,
                    AverageRating = group.Average(t => t.ManagerRating!.Value),
                    RatingCount = group.Count()
                })
                .ToListAsync();

            var ratingMap = ratingStats.ToDictionary(
                item => item.ManagerUserId,
                item => new
                {
                    item.AverageRating,
                    item.RatingCount
                });

            var result = managers.Select(u => new
            {
                u.Id,
                u.Name,
                u.Surname,
                u.Email,
                CompanyName = u.Company?.CompanyName ?? "ServeForYou",
                Specialization = (u.Id % 4) switch
                {
                    0 => "Operations Management",
                    1 => "Client Service Delivery",
                    2 => "Technical Coordination",
                    _ => "Project Supervision"
                },
                Rating = ratingMap.TryGetValue(u.Id, out var rating) ? Math.Round(rating.AverageRating, 1) : 0,
                RatingCount = ratingMap.TryGetValue(u.Id, out var countRating) ? countRating.RatingCount : 0
            });

            return Ok(result);
        }

      
        [HttpGet("company")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetCompanyEmployees()
        {
            var companyId = int.Parse(User.FindFirstValue("CompanyId")!);
            var employees = await context.Users
                .Where(u => u.CompanyId == companyId && u.Role == UserRole.Employee)
                .ToListAsync();
            return Ok(employees);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await context.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.Id == id);
            return user == null ? NotFound() : user;
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> CreateUser([FromBody] RequestRegister request, [FromServices] IEntityFactory factory)
        {
            var user = factory.CreateUser(request);
            context.Add(user);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> UpdateUser([FromBody] User user, int id)
        {
            try
            {
                if(user.Id != id) return BadRequest();
                context.Update(user);
                await context.SaveChangesAsync();
                return user;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!context.Users.Any(u => u.Id == id))
                    return NotFound();
                throw;
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();

            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
