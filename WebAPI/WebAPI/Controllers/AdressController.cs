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
    public class AdressController(AplicationContext context) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Adreses>>> GetAdresess()
        {
            return await context.Adreses.Include(a => a.Company).ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Adreses>> GetAddress(int id)
        {
            var address = await context.Adreses.Include(a => a.Company).FirstOrDefaultAsync(a => a.Id == id);
            return address == null ? NotFound() : address;
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Adreses>> CreateAddress([FromBody] AdressRequest request, [FromServices] IEntityFactory factory)
        {
            var adress = factory.CreateAdress(request);
            context.Adreses.Add(adress);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAddress), new { id = adress.Id }, adress);
        }
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Adreses>> UpdateAddress([FromBody] AdressRequest request, int id)
        {
            var address = await context.Adreses.FindAsync(id);
            if (address == null) return NotFound();

            address.Street = request.Street;
            address.City = request.City;
            await context.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteAddress(int id)
        {
            var address = await context.Adreses.FindAsync(id);
            if (address == null) return NotFound();
            context.Adreses.Remove(address);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
