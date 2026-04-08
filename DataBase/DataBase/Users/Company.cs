using System.ComponentModel.DataAnnotations;

namespace DataBase.Users
{
    public class Company
    {
        public int Id { get; set; }
        [Required]
        public string? CompanyName { get; set; }
        public List<User> Users { get; set; } = new List<User>();
        public Adreses? Adreses { get; set; }

        public Company() { }
            
    }
}
