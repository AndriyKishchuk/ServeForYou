namespace DataBase.Users
{
    public class Tasks
    {
        public int Id { get; set; }
        public string? TaskName { get; set; }
        public string? Description { get; set; }
        public int CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }
        public int ManagerUserId { get; set; }
        public User? ManagerUser { get; set; }
        public int AssignedToUserId { get; set; }
        public User? AssignedToByUser { get; set; }
        public Status Status { get; set; }
        public int? ManagerRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TaskFile> Files { get; set; } = new();


        public Tasks() { }
    }
    public enum Status
    {
        New = 1,
        InProgress = 2,
        SubmittedToManager = 3,
        ReturnedToAdmin = 4,
        Done = 5
    }
}
