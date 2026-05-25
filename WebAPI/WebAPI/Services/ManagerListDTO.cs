namespace WebAPI.Services
{
    public record ManagerListDTO(
        int Id,
        string? Name,
        string? Surname,
        string? Email,
        string? CompanyName,
        string? Role,
        string? Specialization,
        double Rating,
        int RatingCount
    );
   
}
