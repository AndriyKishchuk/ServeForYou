using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataBase.Users;
namespace DataBase.Configure
{
    public class TaskConfigure : IEntityTypeConfiguration<Tasks>
    {
        public void Configure(EntityTypeBuilder<Tasks> builder)
        {
            builder.HasKey(u => u.Id);

            builder.HasOne(t => t.CreatedByUser)
                   .WithMany(u => u.CreatedTasks)
                   .HasForeignKey(t => t.CreatedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ManagerUser)
                   .WithMany(u => u.ManagedTasks)
                   .HasForeignKey(t => t.ManagerUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.AssignedToByUser)
                   .WithMany(u => u.AssignedTasks)
                   .HasForeignKey(t => t.AssignedToUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(t => t.CreatedAt)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

        }
    }
}
