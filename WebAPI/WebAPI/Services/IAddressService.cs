using DataBase.Users;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Request;

namespace WebAPI.Services
{
    public interface IAddressService
    {
        Task<ActionResult<IEnumerable<Adreses>>> GetAddressesAsync();
        Task<ActionResult<Adreses>> GetAddressAsync(int id);
        Task<ActionResult<Adreses>> CreateAddressAsync(AdressRequest request);
        Task<ActionResult<Adreses>> UpdateAddressAsync(AdressRequest request, int id);
        Task<ActionResult> DeleteAddressAsync(int id);
    }
}
