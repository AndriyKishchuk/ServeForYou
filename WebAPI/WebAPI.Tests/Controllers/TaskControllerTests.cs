using Moq;
using WebAPI.Controllers;
using WebAPI.Factory;
using DataBase.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Request;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WebAPI.Services;

namespace WebAPI.Tests.Controllers
{
    public class TaskControllerTests : IDisposable
    {
        private readonly AplicationContext _context;
        private readonly TaskController _controller;
        private readonly Mock<IEntityFactory> _mockFactory;

        public TaskControllerTests()
        {
            var options = new DbContextOptionsBuilder<AplicationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());

            _context = new AplicationContext(options.Options);
            _mockFactory = new Mock<IEntityFactory>();
            var taskService = new TaskService(_context, _mockFactory.Object);
            _controller = new TaskController(taskService);
        }

        private void SetUser(ControllerBase controller, User user)
        {
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("CompanyId", user.CompanyId.ToString())
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(userClaims, "TestAuthType"))
                }
            };
        }


        [Fact]
        public async Task Admin_Can_Create_Task_to_Manager()
        {
            //Arrange 
            var admin = new User { Id = 1, Role = UserRole.Admin, CompanyId = 1 };
            var manager = new User { Id = 2, Role = UserRole.Manager, CompanyId = 1 };
            _context.Users.AddRange(admin, manager);
            await _context.SaveChangesAsync();

            SetUser(_controller, admin);

            var request = new CreateCustomerTaskRequest(
                TaskName: "Test Task",
                TaskDescription: "Test Description",
                ManagerUserId: manager.Id
            );

            var createdTask = new Tasks
            {
                Id = 1,
                TaskName = request.TaskName,
                Description = request.TaskDescription,
                CreatedByUserId = admin.Id
            };
            _mockFactory.Setup(f => f.CreateTask(request, admin.Id))
                        .Returns(createdTask);
            //Act
            var result = await _controller.CreateCustomerRequest(request);

            //Assert
            var okResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedTask = Assert.IsType<Tasks>(okResult.Value);
            Assert.Equal(createdTask.Id, returnedTask.Id);
            Assert.Equal(createdTask.TaskName, returnedTask.TaskName);
            Assert.Equal(createdTask.Description, returnedTask.Description);
            Assert.Equal(createdTask.CreatedByUserId, returnedTask.CreatedByUserId);
        }

        [Fact]
        public async Task Manager_Sees_Only_His_Tasks()
        {
            //Arrange 
            var manager = new User { Id = 2, Role = UserRole.Manager, CompanyId = 1 };
            _context.Users.Add(manager);

            var task1 = new Tasks { Id = 1, TaskName = "Task 1", ManagerUserId = manager.Id, CreatedAt = DateTime.UtcNow };
            var task2 = new Tasks { Id = 2, TaskName = "Task 2", ManagerUserId = manager.Id, CreatedAt = DateTime.UtcNow };
            _context.Tasks.AddRange(task1, task2);

            await _context.SaveChangesAsync();

            SetUser(_controller, manager);
            //Act
            var result = await _controller.GetManagerInbox();

            //Assert
            var okResult = Assert.IsAssignableFrom<IEnumerable<Tasks>>(result.Value);
            Assert.All(okResult, t => Assert.Equal(manager.Id, t.ManagerUserId));
        }

        [Fact]
        public async Task Employee_Sees_Only_His_Tasks()
        {
            //Arrange
            var employee = new User { Id = 3, Role = UserRole.Employee, CompanyId = 1 };
            _context.Users.Add(employee);

            var task1 = new Tasks { Id = 1, TaskName = "Task 1", AssignedToUserId = employee.Id, CreatedAt = DateTime.UtcNow };
            var task2 = new Tasks { Id = 2, TaskName = "Task 2", AssignedToUserId = employee.Id, CreatedAt = DateTime.UtcNow };
            _context.Tasks.AddRange(task1, task2);

            await _context.SaveChangesAsync();

            SetUser(_controller, employee);

            //Act
            var result = await _controller.GetAssignedTasks(employee.Id);

            //Assert
            var okResult = Assert.IsAssignableFrom<IEnumerable<Tasks>>(result.Value);
            Assert.All(okResult, t => Assert.Equal(employee.Id, t.AssignedToUserId));
        }

        [Fact]
        public async Task Employee_Cannot_Get_Other_Employees_Tasks()
        {
            //Arrange
            var employee = new User { Id = 3, Role = UserRole.Employee, CompanyId = 1 };
            var task = new Tasks { Id = 1, TaskName = "Task 1", AssignedToUserId = 4, CreatedAt = DateTime.UtcNow };
            _context.Users.Add(employee);
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            SetUser(_controller, employee);

            //Act
            var result = await _controller.GetTask(task.Id);

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Employee_Can_Change_Status_Only_Allowed_Way()
        {
            //Arrange
            var employee = new User { Id = 3, Role = UserRole.Employee, CompanyId = 1 };
            var task = new Tasks { Id = 1, TaskName = "Task 1", AssignedToUserId = employee.Id, Status = Status.New, CreatedAt = DateTime.UtcNow };
            _context.Users.Add(employee);
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            SetUser(_controller, employee);

            //Act
            var result = await _controller.UpdateTaskStatus(task.Id, Status.InProgress);

            //Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task Manager_Can_Return_To_Admin_Only_New_Tasks()
        {
            //Arrange
            var manager = new User { Id = 2, Role = UserRole.Manager, CompanyId = 1 };
            var task = new Tasks { Id = 1, TaskName = "Task 1", ManagerUserId = manager.Id, Status = Status.New, CreatedAt = DateTime.UtcNow };
            _context.Users.Add(manager);
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            SetUser(_controller, manager);
            //Act
            var result = await _controller.ReturnTaskToCustomer(task.Id);
            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Admin_Can_Complete_Only_If_Returned_To_Customer()
        {
            //Arrange
            var admin = new User { Id = 1, Role = UserRole.Admin, CompanyId = 1 };
            var task = new Tasks { Id = 1, TaskName = "Task 1", CreatedByUserId = admin.Id, Status = Status.ReturnedToAdmin, CreatedAt = DateTime.UtcNow };
            _context.Users.Add(admin);
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            SetUser(_controller, admin);

            var request = new CompleteTaskRequest(ManagerRating: 5);

            //Act
            var result = await _controller.CompleteTask(task.Id, request);

            //Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetTask_Returns_NotFound_If_Task_Does_Not_Exist()
        {
            //Arrange
            var employee = new User { Id = 3, Role = UserRole.Employee, CompanyId = 1 };
            _context.Users.Add(employee);
            await _context.SaveChangesAsync();
            SetUser(_controller, employee);

            //Act
            var result = await _controller.GetTask(999);

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
           
        }

        [Fact]
        public async Task DeleteTask_For_Admin_Returns_Ok()
        {
            //Arrange
            var admin = new User { Id = 1, Role = UserRole.Admin, CompanyId = 1 };
            var task = new Tasks { Id = 1, TaskName = "Task 1", CreatedByUserId = admin.Id, CreatedAt = DateTime.UtcNow };
            _context.Users.Add(admin);
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            SetUser(_controller, admin);

            //Act
            var result = await _controller.DeleteTask(task.Id);

            //Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTask_For_Manager_Returns_Ok()
        {
            //Arrange
            var manager = new User { Id = 2, Role = UserRole.Manager, CompanyId = 1 };
            var task = new Tasks { Id = 1, TaskName = "Task 1", ManagerUserId = manager.Id, CreatedAt = DateTime.UtcNow };
            _context.Users.Add(manager);
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            SetUser(_controller, manager);

            //Act
            var result = await _controller.DeleteTask(task.Id);

            //Assert
            Assert.IsType<NoContentResult>(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
