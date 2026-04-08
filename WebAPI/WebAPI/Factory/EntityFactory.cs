using DataBase.Users;
using WebAPI.Models.Request;

namespace WebAPI.Factory
{
    public class EntityFactory : IEntityFactory
    {
        public User CreateUser(RequestRegister request)
        {
            return new User
            {
                Name = request.Name,
                Surname = request.Surname,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                CompanyId = request.CompanyId ?? 1
            };
        }
        public Tasks CreateTask(CreateTaskRequest request, int createdByUserId)
        {
            return new Tasks
            {
                TaskName = request.TaskName,
                Description = request.TaskDescription,
                CreatedByUserId = createdByUserId,
                ManagerUserId = createdByUserId,
                AssignedToUserId = request.AssignedToUserId,
                Status = Status.New,
                CreatedAt = DateTime.UtcNow
            };
        }
        public Tasks CreateTask(CreateCustomerTaskRequest request, int createdByUserId)
        {
            return new Tasks
            {
                TaskName = request.TaskName,
                Description = request.TaskDescription,
                CreatedByUserId = createdByUserId,
                ManagerUserId = request.ManagerUserId,
                AssignedToUserId = request.ManagerUserId,
                Status = Status.New,
                CreatedAt = DateTime.UtcNow
            };

        }
        public TaskFile CreateTaskFile(IFormFile formFile, int taskId, int userId, string storedFileName, string filePath, FileCategory category)
        {
            return new TaskFile
            {
                TaskId = taskId,
                UploadedByUserId = userId,
                FileName = formFile.FileName,
                StoredFileName = storedFileName,
                FilePath = filePath,
                ContentType = formFile.ContentType,
                FileSize = formFile.Length,
                Category = category,
                UploadedAt = DateTime.UtcNow
            };
        }

        public Company CreateCompany(CompanyRequest request)
        {
            return new Company
            {
                CompanyName = request.companyName,
            };
        }
        public Adreses CreateAdress(AdressRequest request)
        {
            return new Adreses
            {
                Street = request.Street,
                City = request.City,
                CompanyId = request.CompanyId
            };
        }

    }
}
