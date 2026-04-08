using DataBase.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DataBase.Configure
{
    public class CompanyConfigure: IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasMany(c => c.Users)
                   .WithOne(t => t.Company)
                   .HasForeignKey(t => t.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
