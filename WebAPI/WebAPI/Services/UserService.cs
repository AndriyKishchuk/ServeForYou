using System.Security.Claims;
using DataBase.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Factory;
using WebAPI.Models.Request;

namespace WebAPI.Services
{
    public class UserService : IUserService
    {
        private readonly AplicationContext _context;
        private readonly IRedisCacheService _redisCache;
        private readonly IEntityFactory _factory;
        public UserService(AplicationContext context, IRedisCacheService redisCache, IEntityFactory factory)
        {
            _context = context;
            _redisCache = redisCache;
            _factory = factory;
        }

        private static int CurrentUserId(ClaimsPrincipal userClaims) => int.Parse(userClaims.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private static string UserIdCacheKey(int userId) => $"User:{userId}";
        private static string UserMeCacheKey(int userId) => $"User:{userId}:Me";
        private static string ManagersCacheKey() => "Users:Managers";


        public async Task<ActionResult<User>> GetMeAsync(ClaimsPrincipal userClaims)
        {
            var userId = CurrentUserId(userClaims);
            var cacheKey = UserMeCacheKey(userId);

            var cachedUser = await _redisCache.GetAsync<User>(cacheKey);
            if (cachedUser is not null) return new OkObjectResult(cachedUser);

            var user = await _context.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null) return new NotFoundResult();

            await _redisCache.SetAsync(cacheKey, user, TimeSpan.FromMinutes(30));
            await _redisCache.SetAsync(UserIdCacheKey(userId), user.Id, TimeSpan.FromHours(1));

            return new OkObjectResult(user);
        }

        public async Task<ActionResult<User>> UpdateMeAsync(ClaimsPrincipal userClaims, UpdateMyProfileRequest request)
        {
            var userId = CurrentUserId(userClaims);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) return new NotFoundResult();

            var emailExists = request.Email.Trim();
            var emaiInUser = await _context.Users.AnyAsync(u => u.Email == emailExists && u.Id != userId);
            if(emaiInUser) return new BadRequestObjectResult("Email is already in use");

            user.Name = request.Name;
            user.Surname = request.Surname;
            user.Email = request.Email;
            
            await _context.SaveChangesAsync();

            await _redisCache.RemoveAsync(UserMeCacheKey(userId));
            await _redisCache.RemoveAsync(UserIdCacheKey(userId));
            await _redisCache.RemoveAsync(ManagersCacheKey());

            return new OkObjectResult(user);
        }

        public async Task<ActionResult> ChangeMyPasswordAsync(ClaimsPrincipal userClaims, ChangePasswordRequest request)
        {
            var userId = int.Parse(userClaims.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null) return new NotFoundResult();

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return new BadRequestObjectResult("Current password is incorrect");

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                return new BadRequestObjectResult("New password must be at least 6 characters long");

            user.PasswordHash = request.NewPassword;
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }

        public async Task<ActionResult> DeleteMyAccountAsync(ClaimsPrincipal userClaims)
        {
            var userId = CurrentUserId(userClaims);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) return new NotFoundResult();

            var hasRelatedTasks = await _context.Tasks.AnyAsync(t =>
                t.CreatedByUserId == userId ||
                t.ManagerUserId == userId ||
                t.AssignedToUserId == userId);

            if (hasRelatedTasks)
                return new BadRequestObjectResult("You cannot delete this account while it is linked to existing tasks.");

            var hasUploadedFiles = await _context.TaskFiles.AnyAsync(f => f.UploadedByUserId == userId);
            if (hasUploadedFiles)
                return new BadRequestObjectResult("You cannot delete this account while uploaded files are linked to it.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await _redisCache.RemoveAsync(UserMeCacheKey(userId));
            await _redisCache.RemoveAsync(UserIdCacheKey(userId));
            await _redisCache.RemoveAsync(ManagersCacheKey());

            return new NoContentResult();
        }

        public async Task<ActionResult> GetManagersAsync()
        {
            var cachedManagers = await _redisCache.GetAsync<List<ManagerListDTO>>(ManagersCacheKey());
            if (cachedManagers is not null) return new OkObjectResult(cachedManagers);

            var managers = await _context.Users
                .Include(u => u.Company)
                .Where(u => u.Role == UserRole.Manager)
                .OrderBy(u => u.Name)
                .ToListAsync();

            var ratingStats = await _context.Tasks
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

            await _redisCache.SetAsync(ManagersCacheKey(), result.ToList(), TimeSpan.FromMinutes(30));

            return new OkObjectResult(result);
        }

        public async Task<ActionResult<IEnumerable<User>>> GetCompanyEmployeesAsync(ClaimsPrincipal userClaims)
        {
            var companyId = int.Parse(userClaims.FindFirstValue("CompanyId")!);
            var employees = await _context.Users
                .Where(u => u.CompanyId == companyId && u.Role == UserRole.Employee)
                .ToListAsync();
            return new OkObjectResult(employees);
        }

        public async Task<ActionResult<User>> GetUserAsync(int id)
        {
            var userId = UserIdCacheKey(id);

            var cachedUser = await _redisCache.GetAsync<User>(userId);
            if (cachedUser is not null) return new OkObjectResult(cachedUser);

            var user = await _context.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.Id == id);
            if (user is null) return new NotFoundResult();

            await _redisCache.SetAsync(userId, user, TimeSpan.FromMinutes(30));

            return new OkObjectResult(user);
        }

        public async Task<ActionResult<User>> CreateUserAsync(RequestRegister request)
        {
            var user = _factory.CreateUser(request);
            _context.Add(user);
            await _context.SaveChangesAsync();
            await _redisCache.SetAsync(UserIdCacheKey(user.Id), user.Id, TimeSpan.FromHours(1));

            return new OkObjectResult(user);
        }

        public async Task<ActionResult<User>> UpdateUserAsync(User user, int id)
        {
            try
            {
                if (user.Id != id) return new BadRequestResult();
                _context.Update(user);
                await _context.SaveChangesAsync();
                await _redisCache.SetAsync(UserIdCacheKey(id), user.Id, TimeSpan.FromHours(1));
                return new OkObjectResult(user);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(u => u.Id == id))
                    return new NotFoundResult();
                throw;
            }
        }

        public async Task<ActionResult> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return new NotFoundResult();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            await _redisCache.RemoveAsync(UserIdCacheKey(id));
            return new NoContentResult();
        }

        public async Task<ActionResult<IEnumerable<User>>> GetUsersAsync()
        {
            return await _context.Users.Include(u => u.Company).ToListAsync();
        }
    }
}
