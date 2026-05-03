using DataBase.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Factory;
using WebAPI.Models.Request;

namespace WebAPI.Services
{
    public class CompanyService(AplicationContext context, IEntityFactory factory) : ICompanyService
    {
        public async Task<ActionResult> GetPublicCompaniesAsync()
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

            return new OkObjectResult(companies);
        }

        public async Task<ActionResult<IEnumerable<Company>>> GetCompaniesAsync()
        {
            return await context.Companies.Include(c => c.Users).Include(c => c.Adreses).ToListAsync();
        }

        public async Task<ActionResult<Company>> GetCompanyAsync(int id)
        {
            var company = await context.Companies.Include(c => c.Users).Include(c => c.Adreses).FirstOrDefaultAsync(c => c.Id == id);
            return company == null ? new NotFoundResult() : company;
        }

        public async Task<ActionResult<Company>> CreateCompanyAsync(CompanyRequest request)
        {
            var company = factory.CreateCompany(request);
            context.Companies.Add(company);
            await context.SaveChangesAsync();
            return new CreatedAtActionResult("GetCompany", "Company", new { id = company.Id }, company);
        }

        public async Task<ActionResult<Company>> UpdateCompanyAsync(CompanyRequest request, int id)
        {
            var company = await context.Companies.FindAsync(id);
            if (company == null) return new NotFoundResult();

            company.CompanyName = request.companyName;
            await context.SaveChangesAsync();

            return new OkObjectResult(company);
        }

        public async Task<ActionResult> DeleteCompanyAsync(int id)
        {
            var company = await context.Companies.FindAsync(id);
            if (company == null) return new NotFoundResult();

            context.Companies.Remove(company);
            await context.SaveChangesAsync();
            return new NoContentResult();
        }
    }
}
