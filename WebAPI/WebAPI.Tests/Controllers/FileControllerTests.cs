using System.Security.Claims;
using DataBase.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebAPI.Controllers;
using WebAPI.Factory;
using WebAPI.Models.Response;
using WebAPI.Services;

namespace WebAPI.Tests.Controllers
{
    public class FileControllerTests : IDisposable
    {
        private readonly AplicationContext _context;
        private readonly Mock<IFileStorageService> _storageMock;
        private readonly Mock<IEntityFactory> _factoryMock;
        private readonly FileController _controller;

        public FileControllerTests()
        {
            var options = new DbContextOptionsBuilder<AplicationContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AplicationContext(options);
            _storageMock = new Mock<IFileStorageService>();
            _factoryMock = new Mock<IEntityFactory>();
            var fileService = new FileService(_context, _storageMock.Object, _factoryMock.Object);
            _controller = new FileController(fileService);
        }

        private void SetUserClaims(int userId, UserRole role, int companyId, string name = "Test User")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role.ToString()),
                new Claim("CompanyId", companyId.ToString()),
                new Claim(ClaimTypes.Name, name),
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };
        }

        private static IFormFile CreateFormFile(string name = "test.txt", string content = "hello")
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", name)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };
        }

        [Fact]
        public async Task UploadFile_WithMissingTask_ReturnsNotFound()
        {
            SetUserClaims(1, UserRole.Manager, 1);

            var result = await _controller.UploadFile(999, CreateFormFile(), FileCategory.Attachment);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFound.Value.Should().Be("Task not found.");
        }

        [Fact]
        public async Task UploadFile_ManagerCannotUploadResult_ReturnsBadRequest()
        {
            var manager = new User { Id = 1, Name = "Manager", Surname = "One", Email = "m@test.com", Role = UserRole.Manager, CompanyId = 1 };
            var task = new Tasks
            {
                Id = 1,
                TaskName = "Task",
                CreatedByUserId = 10,
                ManagerUserId = manager.Id,
                AssignedToUserId = 20,
                Status = Status.New,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(manager);
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            SetUserClaims(manager.Id, manager.Role, manager.CompanyId);

            var result = await _controller.UploadFile(task.Id, CreateFormFile(), FileCategory.Result);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequest.Value.Should().Be("Managers can only upload Attachments.");
        }

        [Fact]
        public async Task UploadFile_WithValidInput_ReturnsOk()
        {
            var manager = new User { Id = 1, Name = "Manager", Surname = "One", Email = "m@test.com", Role = UserRole.Manager, CompanyId = 1 };
            var task = new Tasks
            {
                Id = 1,
                TaskName = "Task",
                CreatedByUserId = 10,
                ManagerUserId = manager.Id,
                AssignedToUserId = 20,
                Status = Status.New,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(manager);
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            SetUserClaims(manager.Id, manager.Role, manager.CompanyId, "Manager One");

            var formFile = CreateFormFile("brief.txt");
            _storageMock.Setup(s => s.SaveAsync(formFile, 1, task.Id)).ReturnsAsync(("stored.txt", "uploads/1/1/stored.txt"));

            var taskFile = new TaskFile
            {
                Id = 5,
                TaskId = task.Id,
                UploadedByUserId = manager.Id,
                FileName = "brief.txt",
                StoredFileName = "stored.txt",
                FilePath = "uploads/1/1/stored.txt",
                ContentType = "text/plain",
                FileSize = formFile.Length,
                Category = FileCategory.Attachment,
                UploadedAt = DateTime.UtcNow
            };

            _factoryMock
                .Setup(f => f.CreateTaskFile(formFile, task.Id, manager.Id, "stored.txt", "uploads/1/1/stored.txt", FileCategory.Attachment))
                .Returns(taskFile);

            var result = await _controller.UploadFile(task.Id, formFile, FileCategory.Attachment);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.Value.Should().NotBeNull();
            _context.TaskFiles.Should().ContainSingle(f => f.Id == taskFile.Id);
        }

        [Fact]
        public async Task GetFiles_ForAdminBeforeReturnedToAdmin_HidesResults()
        {
            var admin = new User { Id = 1, Name = "Admin", Surname = "A", Email = "a@test.com", Role = UserRole.Admin, CompanyId = 1 };
            var manager = new User { Id = 2, Name = "Manager", Surname = "M", Email = "m@test.com", Role = UserRole.Manager, CompanyId = 1 };
            var employee = new User { Id = 3, Name = "Emp", Surname = "E", Email = "e@test.com", Role = UserRole.Employee, CompanyId = 1 };
            var task = new Tasks
            {
                Id = 1,
                TaskName = "Task",
                CreatedByUserId = admin.Id,
                ManagerUserId = manager.Id,
                AssignedToUserId = employee.Id,
                Status = Status.InProgress,
                CreatedAt = DateTime.UtcNow
            };
            var attachment = new TaskFile { Id = 1, TaskId = task.Id, UploadedByUserId = manager.Id, FileName = "a.txt", StoredFileName = "a.txt", FilePath = "a", ContentType = "text/plain", FileSize = 1, Category = FileCategory.Attachment, UploadedAt = DateTime.UtcNow };
            var resultFile = new TaskFile { Id = 2, TaskId = task.Id, UploadedByUserId = employee.Id, FileName = "r.txt", StoredFileName = "r.txt", FilePath = "r", ContentType = "text/plain", FileSize = 1, Category = FileCategory.Result, UploadedAt = DateTime.UtcNow };

            _context.Users.AddRange(admin, manager, employee);
            _context.Tasks.Add(task);
            _context.TaskFiles.AddRange(attachment, resultFile);
            await _context.SaveChangesAsync();

            SetUserClaims(admin.Id, admin.Role, admin.CompanyId, "Admin A");

            var result = await _controller.GetFiles(task.Id);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var files = Assert.IsAssignableFrom<IEnumerable<FileResponse>>(ok.Value);
            files.Should().ContainSingle(f => f.Category == FileCategory.Attachment);
            files.Should().NotContain(f => f.Category == FileCategory.Result);
        }

        [Fact]
        public async Task DownloadFile_ForAdminResultBeforeReturnedToAdmin_ReturnsForbid()
        {
            var admin = new User { Id = 1, Name = "Admin", Surname = "A", Email = "a@test.com", Role = UserRole.Admin, CompanyId = 1 };
            var manager = new User { Id = 2, Name = "Manager", Surname = "M", Email = "m@test.com", Role = UserRole.Manager, CompanyId = 1 };
            var employee = new User { Id = 3, Name = "Emp", Surname = "E", Email = "e@test.com", Role = UserRole.Employee, CompanyId = 1 };
            var task = new Tasks
            {
                Id = 1,
                TaskName = "Task",
                CreatedByUserId = admin.Id,
                ManagerUserId = manager.Id,
                AssignedToUserId = employee.Id,
                Status = Status.SubmittedToManager,
                CreatedAt = DateTime.UtcNow
            };
            var resultFile = new TaskFile
            {
                Id = 2,
                TaskId = task.Id,
                UploadedByUserId = employee.Id,
                FileName = "result.txt",
                StoredFileName = "result.txt",
                FilePath = "uploads/result.txt",
                ContentType = "text/plain",
                FileSize = 10,
                Category = FileCategory.Result,
                UploadedAt = DateTime.UtcNow
            };

            _context.Users.AddRange(admin, manager, employee);
            _context.Tasks.Add(task);
            _context.TaskFiles.Add(resultFile);
            await _context.SaveChangesAsync();

            SetUserClaims(admin.Id, admin.Role, admin.CompanyId, "Admin A");

            var result = await _controller.DownloadFile(resultFile.Id);

            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task DeleteFile_ForManagerWithoutOwnership_ReturnsForbid()
        {
            var manager = new User { Id = 1, Name = "Manager", Surname = "M", Email = "m@test.com", Role = UserRole.Manager, CompanyId = 1 };
            var otherManager = new User { Id = 2, Name = "Manager2", Surname = "M2", Email = "m2@test.com", Role = UserRole.Manager, CompanyId = 1 };
            var task = new Tasks { Id = 1, TaskName = "Task", CreatedByUserId = 10, ManagerUserId = otherManager.Id, AssignedToUserId = 20, Status = Status.New, CreatedAt = DateTime.UtcNow };
            var taskFile = new TaskFile { Id = 1, TaskId = task.Id, UploadedByUserId = otherManager.Id, FileName = "a.txt", StoredFileName = "a.txt", FilePath = "a", ContentType = "text/plain", FileSize = 1, Category = FileCategory.Attachment, UploadedAt = DateTime.UtcNow };
            _context.Users.AddRange(manager, otherManager);
            _context.Tasks.Add(task);
            _context.TaskFiles.Add(taskFile);
            await _context.SaveChangesAsync();

            SetUserClaims(manager.Id, manager.Role, manager.CompanyId, "Manager M");

            var result = await _controller.DeleteFile(taskFile.Id);

            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task DeleteFile_ForManagerOwner_ReturnsNoContentAndDeletesFile()
        {
            var manager = new User { Id = 1, Name = "Manager", Surname = "M", Email = "m@test.com", Role = UserRole.Manager, CompanyId = 1 };
            var task = new Tasks { Id = 1, TaskName = "Task", CreatedByUserId = 10, ManagerUserId = manager.Id, AssignedToUserId = 20, Status = Status.New, CreatedAt = DateTime.UtcNow };
            var taskFile = new TaskFile { Id = 1, TaskId = task.Id, UploadedByUserId = manager.Id, FileName = "a.txt", StoredFileName = "a.txt", FilePath = "uploads/a.txt", ContentType = "text/plain", FileSize = 1, Category = FileCategory.Attachment, UploadedAt = DateTime.UtcNow };
            _context.Users.Add(manager);
            _context.Tasks.Add(task);
            _context.TaskFiles.Add(taskFile);
            await _context.SaveChangesAsync();

            SetUserClaims(manager.Id, manager.Role, manager.CompanyId, "Manager M");

            var result = await _controller.DeleteFile(taskFile.Id);

            result.Should().BeOfType<NoContentResult>();
            _storageMock.Verify(s => s.Delete("uploads/a.txt"), Times.Once);
            _context.TaskFiles.Should().BeEmpty();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
