using DataBase.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using WebAPI.Controllers;
using WebAPI.Factory;
using WebAPI.Models.Request;
using WebAPI.Models.Response;
using WebAPI.Services;

namespace WebAPI.Tests.Controllers
{
    public class AuthControllerTests : IDisposable
    {
        private readonly AuthController _controller;
        private readonly AplicationContext _context;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IEntityFactory> _factoryMock;

        public AuthControllerTests()
        {
            var options = new DbContextOptionsBuilder<AplicationContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AplicationContext(options);
            _factoryMock = new Mock<IEntityFactory>();
            _jwtServiceMock = new Mock<IJwtService>();
            _controller = new AuthController(_context, _jwtServiceMock.Object);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnAuthResponse()
        {
            //Arrange
            var user = new User
            {
                Id = 1,
                Name = "John",
                Email = "john@test.com",
                PasswordHash = "hashedpassword",
                Role = UserRole.Admin,
                CompanyId = 1
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _jwtServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("mocked_jwt_token");

            var loginRequest = new LoginRequests("john@test.com", "password");

            //Act 
            var result = await _controller.Login(loginRequest);

            //Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            response.Token.Should().Be("mocked_jwt_token");
            response.UserId.Should().Be(1);
            response.Email.Should().Be("john@test.com");
            response.RedirectUrl.Should().Be("/customer/overview");
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            //Arrange
            var loginRequest = new LoginRequests("invalid@test.com", "password");

            //Act
            var result = await _controller.Login(loginRequest);

            //Assert 
            var unauthorizedResult = Assert.IsType<Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult>(result.Result);
            unauthorizedResult.Should().NotBeNull();
            unauthorizedResult.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Register_WithValidRequest_ShouldCreateUserAndReturnAuthResponse()
        {
            //Arrange
            var company = new Company { Id = 1, CompanyName = "ServeForYou" };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var registerRequest = new RequestRegister(
               Name: "Jane",
               Surname: "Smith",
               Email: "jane@smith.com",
               Password: "password",
               Role: UserRole.Admin,
               CompanyId: 1
            );

            var expectedUser = new User
            {
                Id = 1,
                Name = "Jane",
                Surname = "Smith",
                Email = "jane@smith.com",
                CompanyId = 1,
                Role = UserRole.Admin
            };

            _factoryMock.Setup(f => f.CreateUser(It.IsAny<RequestRegister>())).Returns(expectedUser);
            _jwtServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("mocked_jwt_token");

            //Act 
            var result = await _controller.Register(registerRequest, _factoryMock.Object);

            //Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            response.Should().NotBeNull();
            response.Token.Should().Be("mocked_jwt_token");
            response.UserId.Should().Be(1);
            response.Email.Should().Be("jane@smith.com");
            response.RedirectUrl.Should().Be("/customer/overview");
            response.CompanyId.Should().Be(1);

            _context.Companies.Should().ContainSingle();

            var createdUser = _context.Users.First(u => u.Email == "jane@smith.com");
            createdUser.Should().NotBeNull();
        }

        [Fact]
        public async Task Register_WithNonExistingCompany_ShouldReturnBadRequest()
        {
            //Arrange
            var registerRequest = new RequestRegister(
              Name: "Jane",
              Surname: "Smith",
              Email: "jane@smith.com",
              Password: "password",
              Role: UserRole.Admin,
              CompanyId: 1
            );

            var expectedUser = new User
            {
                Id = 1,
                Name = "Jane",
                Surname = "Smith",
                Email = "jane@smith.com",
                CompanyId = 1,
                Role = UserRole.Admin
            };

            _factoryMock.Setup(f => f.CreateUser(It.IsAny<RequestRegister>())).Returns(expectedUser);
            _jwtServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("mocked_jwt_token");

            //Act
            var result = await _controller.Register(registerRequest, _factoryMock.Object);

            //Assert
            var badRequestResult = Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result.Result);
            badRequestResult.Value.Should().Be("Invalid company ID");
        }

        [Fact]
        public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
        {
            //Arrange
            var exitingUser = new User
            {
                Id = 1,
                Email = "existing@test.com",
                CompanyId = 1
            };

            _context.Users.Add(exitingUser);
            await _context.SaveChangesAsync();

            var registerRequest = new RequestRegister(
                Name: "New",
                Surname: "User",
                Email: "existing@test.com",
                Password: "password",
                Role: UserRole.Admin,
                CompanyId: 1
            );

            //Act
            var result = await _controller.Register(registerRequest, _factoryMock.Object);

            //Assert
            var badRequestResult = Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result.Result);
            badRequestResult.Value.Should().Be("Email already in use");
        }

        [Fact]
        public async Task GetMe_WithExistingUser_ShouldReturnAuthResponse()
        {
            //Arrange
            var user = new User
            {
                Id = 42,
                Name = "Olena",
                Email = "olena@test.com",
                Role = UserRole.Manager,
                CompanyId = 1,
                PasswordHash = "hashedpassword",
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[] { new Claim(ClaimTypes.NameIdentifier, "42") },
                            authenticationType: "Test"))
                }
            };

            //Act
            var result = await _controller.GetMe();

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            response.UserId.Should().Be(42);
            response.Email.Should().Be("olena@test.com");
            response.Role.Should().Be(UserRole.Manager);
            response.RedirectUrl.Should().Be("/manager/dashboard");
            response.Token.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMe_WithMissingUser_ShouldReturnNotFound()
        {
            //Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[] { new Claim(ClaimTypes.NameIdentifier, "999") },
                            authenticationType: "Test"))
                }
            };

            //Act
            var result = await _controller.GetMe();

            //Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Register_WithNullCompanyId_ShouldUseServeForYouFallbackCompany()
        {
            //Arrange
            var company = new Company { Id = 15, CompanyName = "ServeForYou" };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var registerRequest = new RequestRegister(
                Name: "Jane",
                Surname: "Smith",
                Email: "jane@smith.com",
                Password: "password",
                Role: UserRole.Admin,
                CompanyId: null
            );

            var expectedUser = new User
            {
                Id = 1,
                Name = "Jane",
                Surname = "Smith",
                Email = "jane@smith.com",
                CompanyId = company.Id,
                Role = UserRole.Admin
            };

            _factoryMock
                .Setup(f => f.CreateUser(It.Is<RequestRegister>(r => r.CompanyId == company.Id)))
                .Returns(expectedUser);
            _jwtServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("mocked_jwt_token");

            //Act
            var result = await _controller.Register(registerRequest, _factoryMock.Object);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            response.CompanyId.Should().Be(company.Id);

            _factoryMock.Verify(
                f => f.CreateUser(It.Is<RequestRegister>(r => r.CompanyId == company.Id)),
                Times.Once);
        }

        [Fact]
        public async Task Register_WithMismatchedCompanyId_ShouldReturnBadRequest()
        {
            //Arrange
            var company = new Company { Id = 2, CompanyName = "ServeForYou" };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var registerRequest = new RequestRegister(
                Name: "Jane",
                Surname: "Smith",
                Email: "jane@smith.com",
                Password: "password",
                Role: UserRole.Admin,
                CompanyId: 1
            );

            //Act
            var result = await _controller.Register(registerRequest, _factoryMock.Object);

            //Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequestResult.Value.Should().Be("Invalid company ID");
        }

        [Fact]
        public async Task Register_WhenCompanyNameNotServeForYou_ShouldRenameCompany()
        {
            //Arrange
            var company = new Company { Id = 1, CompanyName = "Apple" };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var registerRequest = new RequestRegister(
                Name: "Jane",
                Surname: "Smith",
                Email: "jane@smith.com",
                Password: "password",
                Role: UserRole.Admin,
                CompanyId: 1
            );

            var expectedUser = new User
            {
                Id = 1,
                Name = "Jane",
                Surname = "Smith",
                Email = "jane@smith.com",
                CompanyId = 1,
                Role = UserRole.Admin
            };

            _factoryMock.Setup(f => f.CreateUser(It.IsAny<RequestRegister>())).Returns(expectedUser);
            _jwtServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("mocked_jwt_token");

            //Act
            var result = await _controller.Register(registerRequest, _factoryMock.Object);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.Value.Should().BeOfType<AuthResponse>();

            var updatedCompany = await _context.Companies.FirstAsync(c => c.Id == 1);
            updatedCompany.CompanyName.Should().Be("ServeForYou");
        }
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
