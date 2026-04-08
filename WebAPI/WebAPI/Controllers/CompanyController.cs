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
    public class CompanyController(AplicationContext context) : ControllerBase
    {
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult> GetPublicCompanies()
        {
            var companies = await context.Companies
                .Include(c => c.Adreses)
                .OrderBy(c => c.CompanyName)
                .Select(c => new
                {
                    c.Id,
                    c.CompanyName,
                    Address = c.Adreses == null
                        ? null
                        : new
                        {
                            c.Adreses.City,
                            c.Adreses.Street
                        }
                })
                .ToListAsync();

            return Ok(companies);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
        {
            return await context.Companies.Include(c => c.Users).Include(c => c.Adreses).ToListAsync();
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Company>> GetCompany(int id)
        {
            var company = await context.Companies.Include(c => c.Users).Include(c => c.Adreses).FirstOrDefaultAsync(c => c.Id == id);
            return company == null ? NotFound() : company;
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Company>> CreateCompany([FromBody] CompanyRequest request, [FromServices] IEntityFactory factory)
        {
            var company = factory.CreateCompany(request);
           
            context.Companies.Add(company);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, company);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Company>> UpdateCompany([FromBody] CompanyRequest request, int id)
        {
            var company = await context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            company.CompanyName = request.companyName;
            await context.SaveChangesAsync();
           
            return Ok(company); 
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCompany(int id)
        {
            var company = await context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            context.Companies.Remove(company);
            await context.SaveChangesAsync();
           
            return NoContent();
        }
    }
}
