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
    public class CompanyController(ICompanyService companyService) : ControllerBase
    {
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult> GetPublicCompanies()
        {
            return await companyService.GetPublicCompaniesAsync();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
        {
            return await companyService.GetCompaniesAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Company>> GetCompany(int id)
        {
            return await companyService.GetCompanyAsync(id);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Company>> CreateCompany([FromBody] CompanyRequest request)
        {
            return await companyService.CreateCompanyAsync(request);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Company>> UpdateCompany([FromBody] CompanyRequest request, int id)
        {
            return await companyService.UpdateCompanyAsync(request, id);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCompany(int id)
        {
            return await companyService.DeleteCompanyAsync(id);
        }
    }
}
