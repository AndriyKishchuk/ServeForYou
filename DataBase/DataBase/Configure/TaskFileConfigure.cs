using DataBase.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataBase.Configure
{
    public class TaskFileConfigure : IEntityTypeConfiguration<TaskFile>
    {
        public void Configure(EntityTypeBuilder<TaskFile> builder)
        {
            builder.HasKey(f => f.Id);

            builder.Property(f => f.FileName).IsRequired();
            builder.Property(f => f.StoredFileName).IsRequired();
            builder.Property(f => f.FilePath).IsRequired();
            builder.Property(f => f.ContentType).IsRequired();

            builder.Property(f => f.UploadedAt)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne(f => f.Task)
                   .WithMany(t => t.Files)
                   .HasForeignKey(f => f.TaskId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.UploadedBy)
                   .WithMany()
                   .HasForeignKey(f => f.UploadedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
