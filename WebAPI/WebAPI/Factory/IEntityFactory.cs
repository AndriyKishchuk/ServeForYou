using DataBase.Users;
using WebAPI.Models.Request;

namespace WebAPI.Factory
{
    public interface IEntityFactory
    {
        User CreateUser(RequestRegister request);
        Tasks CreateTask(CreateTaskRequest request, int createdByUserId);
        Tasks CreateTask(CreateCustomerTaskRequest request, int createdByUserId);
        Company CreateCompany(CompanyRequest request);
        Adreses CreateAdress(AdressRequest request);
        TaskFile CreateTaskFile(IFormFile file, int taskId, int userId, string storedFileName, string filePath, FileCategory category);

    }
}
