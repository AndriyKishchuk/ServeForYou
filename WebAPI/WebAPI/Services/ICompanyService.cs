using DataBase.Users;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Request;

namespace WebAPI.Services
{
    public interface ICompanyService
    {
        Task<ActionResult> GetPublicCompaniesAsync();
        Task<ActionResult<IEnumerable<Company>>> GetCompaniesAsync();
        Task<ActionResult<Company>> GetCompanyAsync(int id);
        Task<ActionResult<Company>> CreateCompanyAsync(CompanyRequest request);
        Task<ActionResult<Company>> UpdateCompanyAsync(CompanyRequest request, int id);
        Task<ActionResult> DeleteCompanyAsync(int id);
    }
}
