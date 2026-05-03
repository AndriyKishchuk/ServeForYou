using DataBase.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Factory;
using WebAPI.Models.Request;

namespace WebAPI.Services
{
    public class AddressService(AplicationContext context, IEntityFactory factory) : IAddressService
    {
        public async Task<ActionResult<IEnumerable<Adreses>>> GetAddressesAsync()
        {
            return await context.Adreses.Include(a => a.Company).ToListAsync();
        }

        public async Task<ActionResult<Adreses>> GetAddressAsync(int id)
        {
            var address = await context.Adreses.Include(a => a.Company).FirstOrDefaultAsync(a => a.Id == id);
            return address == null ? new NotFoundResult() : address;
        }

        public async Task<ActionResult<Adreses>> CreateAddressAsync(AdressRequest request)
        {
            var adress = factory.CreateAdress(request);
            context.Adreses.Add(adress);
            await context.SaveChangesAsync();
            return new CreatedAtActionResult("GetAddress", "Adress", new { id = adress.Id }, adress);
        }

        public async Task<ActionResult<Adreses>> UpdateAddressAsync(AdressRequest request, int id)
        {
            var address = await context.Adreses.FindAsync(id);
            if (address == null) return new NotFoundResult();

            address.Street = request.Street;
            address.City = request.City;
            await context.SaveChangesAsync();

            return new NoContentResult();
        }

        public async Task<ActionResult> DeleteAddressAsync(int id)
        {
            var address = await context.Adreses.FindAsync(id);
            if (address == null) return new NotFoundResult();

            context.Adreses.Remove(address);
            await context.SaveChangesAsync();

            return new NoContentResult();
        }
    }
}
