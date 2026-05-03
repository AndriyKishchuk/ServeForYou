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
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await userService.GetUsersAsync();
        }

        [HttpGet("me")]
        public async Task<ActionResult<User>> GetMe()
        {
            return await userService.GetMeAsync(User);
        }

        [HttpPut("me")]
        public async Task<ActionResult<User>> UpdateMe([FromBody] UpdateMyProfileRequest request)
        {
            return await userService.UpdateMeAsync(User, request);
        }

        [HttpPatch("me/password")]
        public async Task<ActionResult> ChangeMyPassword([FromBody] ChangePasswordRequest request)
        {
            return await userService.ChangeMyPasswordAsync(User, request);
        }

        [HttpDelete("me")]
        public async Task<ActionResult> DeleteMyAccount()
        {
            return await userService.DeleteMyAccountAsync(User);
        }

        [HttpGet("managers")]
        [AllowAnonymous]
        public async Task<ActionResult> GetManagers()
        {
            return await userService.GetManagersAsync();
        }

        [HttpGet("company")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetCompanyEmployees()
        {
            return await userService.GetCompanyEmployeesAsync(User);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            return await userService.GetUserAsync(id);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> CreateUser([FromBody] RequestRegister request)
        {
            return await userService.CreateUserAsync(request);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> UpdateUser([FromBody] User user, int id)
        {
            return await userService.UpdateUserAsync(user, id);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            return await userService.DeleteUserAsync(id);
        }
    }
}
