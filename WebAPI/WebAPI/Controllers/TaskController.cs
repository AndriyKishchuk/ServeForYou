using DataBase.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Request;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskController(ITaskService taskService) : ControllerBase
    {
        [HttpGet("company/all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Tasks>>> GetAllCompanyTasks()
        {
            return await taskService.GetAllCompanyTasksAsync(User);
        }

        [HttpGet("manager/inbox")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<IEnumerable<Tasks>>> GetManagerInbox()
        {
            return await taskService.GetManagerInboxAsync(User);
        }

        [HttpPost("customer-request")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Tasks>> CreateCustomerRequest([FromBody] CreateCustomerTaskRequest request)
        {
            return await taskService.CreateCustomerRequestAsync(User, request);
        }

        [HttpGet("assigned/{employeeId}")]
        [Authorize(Roles = "Employee")]
        public async Task<ActionResult<IEnumerable<Tasks>>> GetAssignedTasks(int employeeId)
        {
            return await taskService.GetAssignedTasksAsync(User, employeeId);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tasks>> GetTask(int id)
        {
            return await taskService.GetTaskAsync(User, id);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<Tasks>> CreateTask([FromBody] CreateTaskRequest request)
        {
            return await taskService.CreateTaskAsync(User, request);
        }

        [HttpPatch("{id}/assign")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<Tasks>> AssignTaskToEmployee(int id, [FromBody] AssignTaskRequest request)
        {
            return await taskService.AssignTaskToEmployeeAsync(User, id, request);
        }

        [HttpPatch("{id}/return-to-customer")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<Tasks>> ReturnTaskToCustomer(int id)
        {
            return await taskService.ReturnTaskToCustomerAsync(User, id);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Employee")]
        public async Task<ActionResult<Tasks>> UpdateTaskStatus(int id, [FromBody] Status newStatus)
        {
            return await taskService.UpdateTaskStatusAsync(User, id, newStatus);
        }

        [HttpPatch("{id}/complete")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Tasks>> CompleteTask(int id, [FromBody] CompleteTaskRequest request)
        {
            return await taskService.CompleteTaskAsync(User, id, request);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult> DeleteTask(int id)
        {
            return await taskService.DeleteTaskAsync(User, id);
        }
    }
}
