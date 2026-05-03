using System.Security.Claims;
using DataBase.Users;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Request;

namespace WebAPI.Services
{
    public interface ITaskService
    {
        Task<ActionResult<IEnumerable<Tasks>>> GetAllCompanyTasksAsync(ClaimsPrincipal user);
        Task<ActionResult<IEnumerable<Tasks>>> GetManagerInboxAsync(ClaimsPrincipal user);
        Task<ActionResult<Tasks>> CreateCustomerRequestAsync(ClaimsPrincipal user, CreateCustomerTaskRequest request);
        Task<ActionResult<IEnumerable<Tasks>>> GetAssignedTasksAsync(ClaimsPrincipal user, int employeeId);
        Task<ActionResult<Tasks>> GetTaskAsync(ClaimsPrincipal user, int id);
        Task<ActionResult<Tasks>> CreateTaskAsync(ClaimsPrincipal user, CreateTaskRequest request);
        Task<ActionResult<Tasks>> AssignTaskToEmployeeAsync(ClaimsPrincipal user, int id, AssignTaskRequest request);
        Task<ActionResult<Tasks>> ReturnTaskToCustomerAsync(ClaimsPrincipal user, int id);
        Task<ActionResult<Tasks>> UpdateTaskStatusAsync(ClaimsPrincipal user, int id, Status newStatus);
        Task<ActionResult<Tasks>> CompleteTaskAsync(ClaimsPrincipal user, int id, CompleteTaskRequest request);
        Task<ActionResult> DeleteTaskAsync(ClaimsPrincipal user, int id);
    }
}
