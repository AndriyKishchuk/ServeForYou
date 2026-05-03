using System.Security.Claims;
using DataBase.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Factory;
using WebAPI.Models.Request;

namespace WebAPI.Services
{
    public class TaskService(AplicationContext context, IEntityFactory factory) : ITaskService
    {
        private static int CurrentUserId(ClaimsPrincipal user) =>
            int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private static int CurrentCompanyId(ClaimsPrincipal user) =>
            int.Parse(user.FindFirstValue("CompanyId")!);

        private static string CurrentRole(ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.Role)!;

        public async Task<ActionResult<IEnumerable<Tasks>>> GetAllCompanyTasksAsync(ClaimsPrincipal user)
        {
            var currentUserId = CurrentUserId(user);
            var tasks = await context.Tasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.ManagerUser)
                .Include(t => t.AssignedToByUser)
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.CreatedByUserId == currentUserId)
                .ToListAsync();

            return tasks;
        }

        public async Task<ActionResult<IEnumerable<Tasks>>> GetManagerInboxAsync(ClaimsPrincipal user)
        {
            var currentUserId = CurrentUserId(user);
            var tasks = await context.Tasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.ManagerUser)
                .Include(t => t.AssignedToByUser)
                .Where(t => t.ManagerUserId == currentUserId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return tasks;
        }

        public async Task<ActionResult<Tasks>> CreateCustomerRequestAsync(ClaimsPrincipal user, CreateCustomerTaskRequest request)
        {
            var currentCompanyId = CurrentCompanyId(user);
            var currentUserId = CurrentUserId(user);

            var manager = await context.Users.FirstOrDefaultAsync(u => u.Id == request.ManagerUserId);
            if (manager is null)
                return new BadRequestObjectResult("Manager not found");

            if (manager.CompanyId != currentCompanyId)
                return new BadRequestObjectResult("Manager must belong to the same company");

            if (manager.Role != UserRole.Manager)
                return new BadRequestObjectResult("Selected user is not a manager");

            var task = factory.CreateTask(request, currentUserId);
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            return new CreatedAtActionResult("GetTask", "Task", new { id = task.Id }, task);
        }

        public async Task<ActionResult<IEnumerable<Tasks>>> GetAssignedTasksAsync(ClaimsPrincipal user, int employeeId)
        {
            var currentUserId = CurrentUserId(user);
            var currentRole = CurrentRole(user);

            if (currentRole == "Employee" && employeeId != currentUserId)
                return new ForbidResult();

            var tasks = await context.Tasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.ManagerUser)
                .Where(t => t.AssignedToUserId == employeeId)
                .ToListAsync();

            return tasks;
        }

        public async Task<ActionResult<Tasks>> GetTaskAsync(ClaimsPrincipal user, int id)
        {
            var currentUserId = CurrentUserId(user);
            var currentRole = CurrentRole(user);

            var task = await context.Tasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.ManagerUser)
                .Include(t => t.AssignedToByUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return new NotFoundResult();

            if (currentRole == "Employee" && task.AssignedToUserId != currentUserId)
                return new ForbidResult();

            if (currentRole == "Manager" && task.ManagerUserId != currentUserId)
                return new ForbidResult();

            if (currentRole == "Admin" && task.CreatedByUserId != currentUserId)
                return new ForbidResult();

            return new OkObjectResult(task);
        }

        public async Task<ActionResult<Tasks>> CreateTaskAsync(ClaimsPrincipal user, CreateTaskRequest request)
        {
            var currentCompanyId = CurrentCompanyId(user);
            var currentUserId = CurrentUserId(user);

            var assignedUser = await context.Users.FindAsync(request.AssignedToUserId);
            if (assignedUser == null)
                return new BadRequestObjectResult("Assigned user not found");

            if (assignedUser.CompanyId != currentCompanyId)
                return new BadRequestObjectResult("Cannot assign task to a user from a different company");

            if (assignedUser.Role != UserRole.Employee)
                return new BadRequestObjectResult("Tasks can only be assigned to Employees");

            var task = factory.CreateTask(request, currentUserId);
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            return new CreatedAtActionResult("GetTask", "Task", new { id = task.Id }, task);
        }

        public async Task<ActionResult<Tasks>> AssignTaskToEmployeeAsync(ClaimsPrincipal user, int id, AssignTaskRequest request)
        {
            var currentUserId = CurrentUserId(user);
            var currentCompanyId = CurrentCompanyId(user);

            var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return new NotFoundResult();

            if (task.ManagerUserId != currentUserId)
                return new ForbidResult();

            if (task.Status == Status.ReturnedToAdmin)
                return new BadRequestObjectResult("Task already returned to customer");

            var employee = await context.Users.FirstOrDefaultAsync(u => u.Id == request.EmployeeUserId);
            if (employee == null)
                return new BadRequestObjectResult("Employee not found");

            if (employee.Role != UserRole.Employee)
                return new BadRequestObjectResult("Task can only be reassigned to an employee");

            if (employee.CompanyId != currentCompanyId)
                return new BadRequestObjectResult("Employee must belong to the same company");

            task.AssignedToUserId = employee.Id;
            await context.SaveChangesAsync();

            return new OkObjectResult(task);
        }

        public async Task<ActionResult<Tasks>> ReturnTaskToCustomerAsync(ClaimsPrincipal user, int id)
        {
            var currentUserId = CurrentUserId(user);
            var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return new NotFoundResult();

            if (task.ManagerUserId != currentUserId)
                return new ForbidResult();

            if (task.Status != Status.SubmittedToManager)
                return new BadRequestObjectResult("Task can be returned to customer only after employee submission");

            task.Status = Status.ReturnedToAdmin;
            await context.SaveChangesAsync();

            return new OkObjectResult(task);
        }

        public async Task<ActionResult<Tasks>> UpdateTaskStatusAsync(ClaimsPrincipal user, int id, Status newStatus)
        {
            var currentUserId = CurrentUserId(user);
            var task = await context.Tasks.FindAsync(id);
            if (task == null) return new NotFoundResult();

            if (task.AssignedToUserId != currentUserId)
                return new ForbidResult();

            var validTransition =
                (task.Status == Status.New && newStatus == Status.InProgress) ||
                (task.Status == Status.InProgress && newStatus == Status.SubmittedToManager);

            if (!validTransition)
                return new BadRequestObjectResult("Invalid status transition for employee");

            task.Status = newStatus;
            await context.SaveChangesAsync();

            return new OkObjectResult(task);
        }

        public async Task<ActionResult<Tasks>> CompleteTaskAsync(ClaimsPrincipal user, int id, CompleteTaskRequest request)
        {
            var currentUserId = CurrentUserId(user);
            var task = await context.Tasks.FindAsync(id);
            if (task == null) return new NotFoundResult();

            if (task.CreatedByUserId != currentUserId) return new ForbidResult();

            if (task.Status != Status.ReturnedToAdmin)
                return new BadRequestObjectResult("Only tasks returned to customer can be marked as done");

            if (request.ManagerRating < 1 || request.ManagerRating > 5)
                return new BadRequestObjectResult("Manager rating must be between 1 and 5");

            task.ManagerRating = request.ManagerRating;
            task.Status = Status.Done;
            await context.SaveChangesAsync();

            return new OkObjectResult(task);
        }

        public async Task<ActionResult> DeleteTaskAsync(ClaimsPrincipal user, int id)
        {
            var currentUserId = CurrentUserId(user);
            var currentRole = CurrentRole(user);
            var task = await context.Tasks.FindAsync(id);
            if (task == null) return new NotFoundResult();

            if (currentRole == "Manager" && task.ManagerUserId != currentUserId)
                return new ForbidResult();

            if (currentRole == "Admin" && task.CreatedByUserId != currentUserId)
                return new ForbidResult();

            context.Tasks.Remove(task);
            await context.SaveChangesAsync();

            return new NoContentResult();
        }
    }
}
