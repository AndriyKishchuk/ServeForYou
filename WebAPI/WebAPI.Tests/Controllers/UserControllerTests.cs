using System.Security.Claims;
using DataBase.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebAPI.Controllers;
using WebAPI.Factory;
using WebAPI.Models.Request;

namespace WebAPI.Tests.Controllers
{
    public class UserControllerTests : IDisposable
    {
        private readonly AplicationContext _context;
        private readonly UserController _controller;
        private readonly Mock<IEntityFactory> _factoryMock;

        public UserControllerTests()
        {
            var options = new DbContextOptionsBuilder<AplicationContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AplicationContext(options);
            _factoryMock = new Mock<IEntityFactory>();
            _controller = new UserController(_context);
        }

        private void SetUserClaims(int userId, UserRole role, int companyId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role.ToString()),
                new Claim("CompanyId", companyId.ToString()),
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };
        }

        [Fact]
        public async Task GetMe_WithExistingUser_ReturnsUser()
        {
            var company = new Company { Id = 1, CompanyName = "ServeForYou" };
            var user = new User { Id = 10, Name = "Ivan", Email = "ivan@test.com", Role = UserRole.Admin, CompanyId = 1 };
            _context.Companies.Add(company);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            SetUserClaims(user.Id, user.Role, user.CompanyId);

            var result = await _controller.GetMe();

            var returnedUser = Assert.IsType<User>(result.Value);
            returnedUser.Email.Should().Be("ivan@test.com");
        }

        [Fact]
        public async Task UpdateMe_WithTakenEmail_ReturnsBadRequest()
        {
            var company = new Company { Id = 1, CompanyName = "ServeForYou" };
            var current = new User { Id = 1, Name = "A", Surname = "B", Email = "a@test.com", Role = UserRole.Admin, CompanyId = 1 };
            var other = new User { Id = 2, Name = "C", Surname = "D", Email = "taken@test.com", Role = UserRole.Manager, CompanyId = 1 };
            _context.Companies.Add(company);
            _context.Users.AddRange(current, other);
            await _context.SaveChangesAsync();

            SetUserClaims(current.Id, current.Role, current.CompanyId);

            var result = await _controller.UpdateMe(new UpdateMyProfileRequest("A", "B", "taken@test.com"));

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequest.Value.Should().Be("Email already in use");
        }

        [Fact]
        public async Task ChangeMyPassword_WithWrongCurrentPassword_ReturnsBadRequest()
        {
            var user = new User
            {
                Id = 1,
                Email = "user@test.com",
                Role = UserRole.Employee,
                CompanyId = 1,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password")
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            SetUserClaims(user.Id, user.Role, user.CompanyId);

            var result = await _controller.ChangeMyPassword(new ChangePasswordRequest("wrong-password", "new-password"));

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("Current password is incorrect");
        }

        [Fact]
        public async Task DeleteMyAccount_WithLinkedTasks_ReturnsBadRequest()
        {
            var user = new User { Id = 5, Name = "N", Email = "n@test.com", Role = UserRole.Admin, CompanyId = 1, PasswordHash = "pass" };
            var manager = new User { Id = 6, Name = "M", Email = "m@test.com", Role = UserRole.Manager, CompanyId = 1, PasswordHash = "pass" };
            var employee = new User { Id = 7, Name = "E", Email = "e@test.com", Role = UserRole.Employee, CompanyId = 1, PasswordHash = "pass" };
            var task = new Tasks
            {
                Id = 100,
                TaskName = "Linked",
                Description = "desc",
                CreatedByUserId = user.Id,
                ManagerUserId = manager.Id,
                AssignedToUserId = employee.Id,
                Status = Status.New,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.AddRange(user, manager, employee);
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            SetUserClaims(user.Id, user.Role, user.CompanyId);

            var result = await _controller.DeleteMyAccount();

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("You cannot delete this account while it is linked to existing tasks.");
        }

        [Fact]
        public async Task DeleteMyAccount_WithoutLinks_ReturnsNoContent()
        {
            var user = new User { Id = 11, Name = "N", Email = "n@test.com", Role = UserRole.Employee, CompanyId = 1, PasswordHash = "pass" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            SetUserClaims(user.Id, user.Role, user.CompanyId);

            var result = await _controller.DeleteMyAccount();

            result.Should().BeOfType<NoContentResult>();
            _context.Users.Should().NotContain(u => u.Id == user.Id);
        }

        [Fact]
        public async Task GetManagers_ReturnsRatingStats()
        {
            var company = new Company { Id = 1, CompanyName = "ServeForYou" };
            var manager = new User { Id = 3, Name = "Marta", Surname = "K", Email = "m@test.com", Role = UserRole.Manager, CompanyId = 1 };
            var admin = new User { Id = 4, Name = "Admin", Surname = "A", Email = "a@test.com", Role = UserRole.Admin, CompanyId = 1 };
            var employee = new User { Id = 5, Name = "Emp", Surname = "E", Email = "e@test.com", Role = UserRole.Employee, CompanyId = 1 };
            var task1 = new Tasks { Id = 1, TaskName = "T1", CreatedByUserId = admin.Id, ManagerUserId = manager.Id, AssignedToUserId = employee.Id, Status = Status.Done, ManagerRating = 5, CreatedAt = DateTime.UtcNow };
            var task2 = new Tasks { Id = 2, TaskName = "T2", CreatedByUserId = admin.Id, ManagerUserId = manager.Id, AssignedToUserId = employee.Id, Status = Status.Done, ManagerRating = 3, CreatedAt = DateTime.UtcNow };

            _context.Companies.Add(company);
            _context.Users.AddRange(manager, admin, employee);
            _context.Tasks.AddRange(task1, task2);
            await _context.SaveChangesAsync();

            var result = await _controller.GetManagers();

            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsAssignableFrom<IEnumerable<object>>(ok.Value);
            var managerRow = payload.Single();
            var rowType = managerRow.GetType();

            rowType.GetProperty("Name")!.GetValue(managerRow)!.Should().Be("Marta");
            rowType.GetProperty("Rating")!.GetValue(managerRow)!.Should().Be(4d);
            rowType.GetProperty("RatingCount")!.GetValue(managerRow)!.Should().Be(2);
        }

        [Fact]
        public async Task GetCompanyEmployees_ReturnsOnlyEmployeesFromClaimCompany()
        {
            var manager = new User { Id = 1, Name = "Man", Email = "man@test.com", Role = UserRole.Manager, CompanyId = 1 };
            var employee1 = new User { Id = 2, Name = "E1", Email = "e1@test.com", Role = UserRole.Employee, CompanyId = 1 };
            var employee2 = new User { Id = 3, Name = "E2", Email = "e2@test.com", Role = UserRole.Employee, CompanyId = 2 };
            _context.Users.AddRange(manager, employee1, employee2);
            await _context.SaveChangesAsync();

            SetUserClaims(manager.Id, manager.Role, manager.CompanyId);

            var result = await _controller.GetCompanyEmployees();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var users = Assert.IsAssignableFrom<IEnumerable<User>>(ok.Value);
            users.Should().ContainSingle(u => u.Id == employee1.Id);
            users.Should().NotContain(u => u.Id == employee2.Id);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
