using DataBase.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DataBase.Configure
{
    public class AdressConfigure:IEntityTypeConfiguration<Adreses>
    {
        public void Configure(EntityTypeBuilder<Adreses> builder)
        {
            builder.HasKey(a => a.Id);

            builder.HasOne(a => a.Company)
                   .WithOne(c => c.Adreses)
                   .HasForeignKey<Adreses>(a => a.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
