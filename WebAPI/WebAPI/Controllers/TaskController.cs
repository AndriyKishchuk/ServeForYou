using System.Security.Claims;
using DataBase.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models.Request;
using WebAPI.Factory;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskController(AplicationContext context, IEntityFactory factory) : ControllerBase
    {
        // Допоміжні методи для читання claims із JWT токена
        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private int CurrentCompanyId =>
            int.Parse(User.FindFirstValue("CompanyId")!);
        private string CurrentRole =>
            User.FindFirstValue(ClaimTypes.Role)!;

        [HttpGet("company/all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Tasks>>> GetAllCompanyTasks()
        {
            return await context.Tasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.ManagerUser)
                .Include(t => t.AssignedToByUser)
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.CreatedByUserId == CurrentUserId)
                .ToListAsync();
        }

        [HttpGet("manager/inbox")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<IEnumerable<Tasks>>> GetManagerInbox()
        {
            return await context.Tasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.ManagerUser)
                .Include(t => t.AssignedToByUser)
                .Where(t => t.ManagerUserId == CurrentUserId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        [HttpPost("customer-request")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Tasks>> CreateCustomerRequest([FromBody] CreateCustomerTaskRequest request)
        {
            var manager = await context.Users.FirstOrDefaultAsync(u => u.Id == request.ManagerUserId);
            if (manager is null)
                return BadRequest("Manager not found");

            if (manager.CompanyId != CurrentCompanyId)
                return BadRequest("Manager must belong to the same company");

            if (manager.Role != UserRole.Manager)
                return BadRequest("Selected user is not a manager");

            var task = factory.CreateTask(request, CurrentUserId);            
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        } 
        [HttpGet("assigned/{employeeId}")]
        [Authorize(Roles = "Employee")]
        public async Task<ActionResult<IEnumerable<Tasks>>> GetAssignedTasks(int employeeId)
        {
           
            if (CurrentRole == "Employee" && employeeId != CurrentUserId)
                return Forbid();

            return await context.Tasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.ManagerUser)
                .Where(t => t.AssignedToUserId == employeeId)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tasks>> GetTask(int id)
        {
            var task = await context.Tasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.ManagerUser)
                .Include(t => t.AssignedToByUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            if (CurrentRole == "Employee" && task.AssignedToUserId != CurrentUserId)
                return Forbid();

          
            if (CurrentRole == "Manager" && task.ManagerUserId != CurrentUserId)
                return Forbid();

            if (CurrentRole == "Admin" && task.CreatedByUserId != CurrentUserId)
                return Forbid();

            return Ok(task);
        }

   
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<Tasks>> CreateTask(
            [FromBody] CreateTaskRequest request,
            [FromServices] IEntityFactory factory)
        {
            
            var assignedUser = await context.Users.FindAsync(request.AssignedToUserId);
            if (assignedUser == null)
                return BadRequest("Assigned user not found");

         
            if (assignedUser.CompanyId != CurrentCompanyId)
                return BadRequest("Cannot assign task to a user from a different company");

            if (assignedUser.Role != UserRole.Employee)
                return BadRequest("Tasks can only be assigned to Employees");

            var task = factory.CreateTask(request, CurrentUserId);
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpPatch("{id}/assign")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<Tasks>> AssignTaskToEmployee(int id, [FromBody] AssignTaskRequest request)
        {
            var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();

            if (task.ManagerUserId != CurrentUserId)
                return Forbid();

            if (task.Status == Status.ReturnedToAdmin)
                return BadRequest("Task already returned to customer");

            var employee = await context.Users.FirstOrDefaultAsync(u => u.Id == request.EmployeeUserId);
            if (employee == null)
                return BadRequest("Employee not found");

            if (employee.Role != UserRole.Employee)
                return BadRequest("Task can only be reassigned to an employee");

            if (employee.CompanyId != CurrentCompanyId)
                return BadRequest("Employee must belong to the same company");

            task.AssignedToUserId = employee.Id;
            await context.SaveChangesAsync();

            return Ok(task);
        }

        [HttpPatch("{id}/return-to-customer")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<Tasks>> ReturnTaskToCustomer(int id)
        {
            var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();

            if (task.ManagerUserId != CurrentUserId)
                return Forbid();

            if (task.Status != Status.SubmittedToManager)
                return BadRequest("Task can be returned to customer only after employee submission");

            task.Status = Status.ReturnedToAdmin;
            await context.SaveChangesAsync();

            return Ok(task);
        }

      
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Employee")]
        public async Task<ActionResult<Tasks>> UpdateTaskStatus(int id, [FromBody] Status newStatus)
        {
            var task = await context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

           
            if (task.AssignedToUserId != CurrentUserId)
                return Forbid();

            var validTransition =
                (task.Status == Status.New && newStatus == Status.InProgress) ||
                (task.Status == Status.InProgress && newStatus == Status.SubmittedToManager);

            if (!validTransition)
                return BadRequest("Invalid status transition for employee");

            task.Status = newStatus;
            await context.SaveChangesAsync();

            return Ok(task);
        }

        [HttpPatch("{id}/complete")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Tasks>> CompleteTask(int id, [FromBody] CompleteTaskRequest request)
        {
            var task = await context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            if (task.CreatedByUserId != CurrentUserId) return Forbid();

            if (task.Status != Status.ReturnedToAdmin)
            {
                return BadRequest("Only tasks returned to customer can be marked as done");
            }

            if (request.ManagerRating < 1 || request.ManagerRating > 5)
                return BadRequest("Manager rating must be between 1 and 5");

            task.ManagerRating = request.ManagerRating;
            task.Status = Status.Done;
            await context.SaveChangesAsync();

            return Ok(task);
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult> DeleteTask(int id)
        {
            var task = await context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            if (CurrentRole == "Manager" && task.ManagerUserId != CurrentUserId)
                return Forbid();

            if (CurrentRole == "Admin" && task.CreatedByUserId != CurrentUserId)
                return Forbid();

            context.Tasks.Remove(task);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
