namespace WebAPI.Models.Request
{
    public record CreateCustomerTaskRequest(
        string TaskName,
        string TaskDescription,
        int ManagerUserId
    );
}
