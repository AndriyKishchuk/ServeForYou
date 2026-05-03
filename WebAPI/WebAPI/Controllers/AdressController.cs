using DataBase.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Request;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdressController(IAddressService addressService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Adreses>>> GetAdresess()
        {
            return await addressService.GetAddressesAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Adreses>> GetAddress(int id)
        {
            return await addressService.GetAddressAsync(id);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Adreses>> CreateAddress([FromBody] AdressRequest request)
        {
            return await addressService.CreateAddressAsync(request);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Adreses>> UpdateAddress([FromBody] AdressRequest request, int id)
        {
            return await addressService.UpdateAddressAsync(request, id);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteAddress(int id)
        {
            return await addressService.DeleteAddressAsync(id);
        }
    }
}
