using WebAPI.Factory;
using WebAPI.Models.Request;
using DataBase.Users;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Http;

namespace WebAPI.Tests.Factory
{
    public class EntityFactoryTests
    {
        private readonly EntityFactory _factory;

        public EntityFactoryTests()
        {
            _factory = new EntityFactory();
        }

        [Fact]
        public void CreateUser_WithValidRequest_ShouldCreateUser()
        {
            //Arrange
            var request = new RequestRegister(
                Name: "John",
                Surname: "Doe",
                Email: "john@test.com",
                Password: "password123",
                Role: UserRole.Admin,
                CompanyId: 1
            );
            //Act 
            var user = _factory.CreateUser(request);
            //Assert 
            user.Should().NotBeNull();
            user.Name.Should().Be(request.Name);
            user.Surname.Should().Be(request.Surname);
            user.Email.Should().Be(request.Email);
            user.Role.Should().Be(request.Role);
            user.CompanyId.Should().Be(request.CompanyId);
            user.PasswordHash.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void CreateUser_WithNullCompanyId_ShouldUseDefaultCompanyId()
        {
            //Arrange
            var request = new RequestRegister(
               Name: "Jane",
               Surname: "Smith",
               Email: "jane@test.com",
               Password: "password123",
               Role: UserRole.Manager,
               CompanyId: null
            );
            
            //Act 
            var user = _factory.CreateUser(request);

            //Assert
            user.Should().NotBeNull();
            user.CompanyId.Should().Be(1);
        }
        [Fact]
        public void CreateTask_WithValidRequest_ShouldCreateTask()
        {
            //Arrange
            var request = new CreateTaskRequest(
                TaskName: "Test Task",
                TaskDescription: "This is a test task",
                AssignedToUserId: 1
                );
            //Act
            var task = _factory.CreateTask(request, createdByUserId: 10);
            //Assert 
            task.Should().NotBeNull();
            task.TaskName.Should().Be(request.TaskName);
            task.Description.Should().Be(request.TaskDescription);
            task.AssignedToUserId.Should().Be(request.AssignedToUserId);
            task.CreatedByUserId.Should().Be(10);
            task.ManagerUserId.Should().Be(10);
            task.Status.Should().Be(Status.New);
            task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void CreateTaskFile_WithValidInput_ShouldCreateTaskFile()
        {
            //Arrange 
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("file.pdf");
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");

            //Act
            var taskFile = _factory.CreateTaskFile(fileMock.Object, taskId: 1, userId: 10, storedFileName: "stored_file.pdf", filePath: "/uploads/1/1/stored_file.pdf", category: FileCategory.Attachment);

            //Assert 
            taskFile.Should().NotBeNull();
            taskFile.TaskId.Should().Be(1);
            taskFile.UploadedByUserId.Should().Be(10);
            taskFile.FileName.Should().Be("file.pdf");
            taskFile.StoredFileName.Should().Be("stored_file.pdf");
            taskFile.FilePath.Should().Be("/uploads/1/1/stored_file.pdf");
            taskFile.ContentType.Should().Be("application/pdf");
            taskFile.FileSize.Should().Be(1024);
            taskFile.Category.Should().Be(FileCategory.Attachment);
            taskFile.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void CreateCustomerTAsk_WithValidRequest_ShouldCreateTask()
        {
            //Arrange 
            var request = new CreateCustomerTaskRequest(
                TaskName: "Customer task",
                TaskDescription: "This is a customer task",
                ManagerUserId: 10
                );
            int createdByUserId = 5;

            //Act
            var task = _factory.CreateTask(request, createdByUserId);

            //Assert
            task.Should().NotBeNull();
            task.TaskName.Should().Be(request.TaskName);
            task.Description.Should().Be(request.TaskDescription);
            task.CreatedByUserId.Should().Be(createdByUserId);
            task.ManagerUserId.Should().Be(request.ManagerUserId);
            task.Status.Should().Be(Status.New);
        }
        [Fact]
        public void CreateCustomerTask_WithSameManagerAndCreator_ShouldCreateTask()
        {
            //Arrange 
            var request = new CreateCustomerTaskRequest(
                TaskName: "Customer task",
                TaskDescription: "This is a customer task",
                ManagerUserId: 5
                );
            int createdByUserId = 5;

            //Act
            var task = _factory.CreateTask(request, createdByUserId);

            //Assert
            task.Should().NotBeNull();
            task.TaskName.Should().Be(request.TaskName);
            task.Description.Should().Be(request.TaskDescription);
            task.CreatedByUserId.Should().Be(createdByUserId);
            task.ManagerUserId.Should().Be(request.ManagerUserId);
            task.AssignedToUserId.Should().Be(request.ManagerUserId);
            task.Status.Should().Be(Status.New);
        }

        [Fact]
        public void CreateCompany_WithValidRequeuing_ShouldCreateTask()
        {
            //Arrange
            var request = new CompanyRequest(
                companyName: "Test Company"
                );
            //Act 
            var company = _factory.CreateCompany(request);

            //Assert
            company.Should().NotBeNull();
            company.CompanyName.Should().Be(request.companyName);
        }

        [Fact]
        public void CreateAdress_WithValidRequest_ShouldCreateAddress()
        {
            //Arrange
            var request = new AdressRequest(
                Street: "123 Main St",
                City: "Anytown",
                CompanyId: 1
                );
            //Act
            var address = _factory.CreateAdress(request);
            //Assert
            address.Should().NotBeNull();
            address.Street.Should().Be(request.Street);
            address.City.Should().Be(request.City);
            address.CompanyId.Should().Be(request.CompanyId);
        }
    }
}
