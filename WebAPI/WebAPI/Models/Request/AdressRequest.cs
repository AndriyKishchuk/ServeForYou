namespace WebAPI.Models.Request
{
    public record AdressRequest(
        string Street,
        string City,
        int CompanyId 
    );
}
