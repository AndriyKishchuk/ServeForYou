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
    public class AdressControllerTests : IDisposable
    {
        private readonly AplicationContext _context;
        private readonly AdressController _controller;
        private readonly Mock<IEntityFactory> _factoryMock;

        public AdressControllerTests()
        {
            var options = new DbContextOptionsBuilder<AplicationContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AplicationContext(options);
            _factoryMock = new Mock<IEntityFactory>();
            var addressService = new AddressService(_context, _factoryMock.Object);
            _controller = new AdressController(addressService);
        }

        [Fact]
        public async Task GetAddress_WithMissingId_ReturnsNotFound()
        {
            var result = await _controller.GetAddress(100);
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateAddress_ReturnsCreatedAtAction()
        {
            var company = new Company { Id = 1, CompanyName = "ServeForYou" };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var request = new AdressRequest("Main", "Kyiv", company.Id);
            var address = new Adreses
            {
                Id = 1,
                Street = "Main",
                City = "Kyiv",
                CompanyId = company.Id
            };

            _factoryMock.Setup(f => f.CreateAdress(request)).Returns(address);

            var result = await _controller.CreateAddress(request);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            created.ActionName.Should().Be(nameof(AdressController.GetAddress));
            var createdAddress = Assert.IsType<Adreses>(created.Value);
            createdAddress.City.Should().Be("Kyiv");
            _context.Adreses.Should().ContainSingle(a => a.CompanyId == company.Id);
        }

        [Fact]
        public async Task UpdateAddress_WithExistingAddress_ReturnsNoContent()
        {
            var address = new Adreses { Id = 1, Street = "Old", City = "OldCity", CompanyId = 1 };
            _context.Adreses.Add(address);
            await _context.SaveChangesAsync();

            var result = await _controller.UpdateAddress(new AdressRequest("New", "Kyiv", 1), address.Id);

            result.Result.Should().BeOfType<NoContentResult>();
            var updated = await _context.Adreses.FindAsync(address.Id);
            updated!.Street.Should().Be("New");
            updated.City.Should().Be("Kyiv");
        }

        [Fact]
        public async Task DeleteAddress_WithExistingAddress_ReturnsNoContent()
        {
            var address = new Adreses { Id = 1, Street = "Main", City = "Kyiv", CompanyId = 1 };
            _context.Adreses.Add(address);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteAddress(address.Id);

            result.Should().BeOfType<NoContentResult>();
            _context.Adreses.Should().BeEmpty();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
