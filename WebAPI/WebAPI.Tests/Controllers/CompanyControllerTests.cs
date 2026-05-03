using DataBase.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebAPI.Controllers;
using WebAPI.Factory;
using WebAPI.Models.Request;
using WebAPI.Services;

namespace WebAPI.Tests.Controllers
{
    public class CompanyControllerTests : IDisposable
    {
        private readonly AplicationContext _context;
        private readonly CompanyController _controller;
        private readonly Mock<IEntityFactory> _factoryMock;

        public CompanyControllerTests()
        {
            var options = new DbContextOptionsBuilder<AplicationContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AplicationContext(options);
            _factoryMock = new Mock<IEntityFactory>();
            var companyService = new CompanyService(_context, _factoryMock.Object);
            _controller = new CompanyController(companyService);
        }

        [Fact]
        public async Task GetPublicCompanies_ReturnsOrderedCompanies()
        {
            var companyA = new Company { Id = 1, CompanyName = "ServeForYou" };
            var companyB = new Company { Id = 2, CompanyName = "Alpha Team" };
            var addressA = new Adreses { Id = 1, CompanyId = companyA.Id, City = "Kyiv", Street = "Main" };
            var addressB = new Adreses { Id = 2, CompanyId = companyB.Id, City = "Lviv", Street = "Green" };

            _context.Companies.AddRange(companyA, companyB);
            _context.Adreses.AddRange(addressA, addressB);
            await _context.SaveChangesAsync();

            var result = await _controller.GetPublicCompanies();

            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetCompany_WithMissingId_ReturnsNotFound()
        {
            var result = await _controller.GetCompany(777);
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateCompany_ReturnsCreatedAtAction()
        {
            var request = new CompanyRequest("ServeForYou");
            var company = new Company { Id = 5, CompanyName = "ServeForYou" };
            _factoryMock.Setup(f => f.CreateCompany(request)).Returns(company);

            var result = await _controller.CreateCompany(request);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            created.ActionName.Should().Be(nameof(CompanyController.GetCompany));
            var createdCompany = Assert.IsType<Company>(created.Value);
            createdCompany.CompanyName.Should().Be("ServeForYou");
            _context.Companies.Should().ContainSingle(c => c.CompanyName == "ServeForYou");
        }

        [Fact]
        public async Task UpdateCompany_WithExistingCompany_ReturnsOkAndUpdatesName()
        {
            var company = new Company { Id = 1, CompanyName = "Old Name" };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var result = await _controller.UpdateCompany(new CompanyRequest("New Name"), company.Id);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var updatedCompany = Assert.IsType<Company>(okResult.Value);
            updatedCompany.CompanyName.Should().Be("New Name");
        }

        [Fact]
        public async Task DeleteCompany_WithExistingCompany_ReturnsNoContent()
        {
            var company = new Company { Id = 1, CompanyName = "ServeForYou" };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteCompany(company.Id);

            result.Should().BeOfType<NoContentResult>();
            _context.Companies.Should().BeEmpty();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
