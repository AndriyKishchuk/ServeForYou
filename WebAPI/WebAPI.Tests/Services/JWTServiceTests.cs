using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DataBase.Users;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using WebAPI.Services;

namespace WebAPI.Tests.Services
{
    public class JWTServiceTests
    {
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;

        public JWTServiceTests()
        {
            var configData = new Dictionary<string, string>
            {
               {"Jwt:SecretKey", "TaskManagerMvpSuperSecretKey2026!!XyZ" },
               {"Jwt:Issuer", "TaskManagerAPI" },
               {"Jwt:Audience", "TaskManagerClient" },
               {"Jwt:ExpiresInMinutes", "60" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData!)
                .Build();

            _jwtService = new JwtService(_configuration);
        }

        [Fact]
        public void GenerateToken_ShouldReturnValidJWT()
        {
            //Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                Role = UserRole.Admin,
                CompanyId = 1
            };

            //Act
            var token = _jwtService.GenerateToken(user);

            //Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            jwtToken.Should().NotBeNull();

            jwtToken.Issuer.Should().Be(_configuration["Jwt:Issuer"]);
            jwtToken.Audiences.Should().Contain(_configuration["Jwt:Audience"]);
        }

        [Fact]
        public void GenerateToken_ShouldContainCorrectClaims()
        {
            //Arrange
            var user = new User
            {
                Id = 2,
                Email = "manager@example.com",
                Role = UserRole.Manager,
                CompanyId = 2
            };

            //Act 
            var token = _jwtService.GenerateToken(user);

            //Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            jwtToken.Should().NotBeNull();

            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "2");
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "manager@example.com");
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Manager");
            jwtToken.Claims.Should().Contain(c => c.Type == "CompanyId" && c.Value == "2");
        }

        [Theory]
        [InlineData(UserRole.Admin)]
        [InlineData(UserRole.Manager)]
        [InlineData(UserRole.Employee)]
        public void GenerateToken_ShouldWorkAllUserRoles(UserRole role)
        {
            //Arrange
            var user = new User
            {
                Id = 3,
                Email = "test@example.com",
                Role = role,
                CompanyId = 3
            };

            //Act
            var token = _jwtService.GenerateToken(user);

            //Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);


            jwtToken.Should().NotBeNull();
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == role.ToString());
        }
    }
}








