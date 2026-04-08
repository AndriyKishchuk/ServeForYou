using System.ComponentModel.DataAnnotations;

namespace DataBase.Users
{
    public class User
    {
        public int Id { get; set; }
        [RegularExpression(@"^[a-zA-Zа-яА-Я]+$")]
        public string? Name { get; set; }
        [RegularExpression(@"^[a-zA-Zа-яА-Я]+$")]
        public string? Surname { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }

        public UserRole Role { get; set; }

        public int CompanyId { get; set; }
        public Company? Company { get; set; }

        public List<Tasks> CreatedTasks { get; set; } = new List<Tasks>();
        public List<Tasks> AssignedTasks { get; set; } = new List<Tasks>();
        public List<Tasks> ManagedTasks { get; set; } = new List<Tasks>();

        public User() { }
    }

    public enum UserRole
    {
        Admin = 1,
        Manager = 2,
        Employee = 3
    }
}
