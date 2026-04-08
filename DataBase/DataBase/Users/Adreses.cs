using System.ComponentModel.DataAnnotations;


namespace DataBase.Users
{
    public class Adreses
    {
        public int Id { get; set; }
        [RegularExpression(@"^[a-zA-Zа-яА-Я]+$")]
        public string? City { get; set; }
        [Required]
        public string? Street { get; set; }
        public Company? Company { get; set; }
        public int CompanyId { get; set; }

        public Adreses() { }

    }
}
