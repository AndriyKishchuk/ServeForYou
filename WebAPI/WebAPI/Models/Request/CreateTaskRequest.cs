namespace WebAPI.Models.Request
{
    public record CreateTaskRequest(
        string TaskName,
        string TaskDescription,
        int AssignedToUserId
    );
}
