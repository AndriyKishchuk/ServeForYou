using Microsoft.EntityFrameworkCore;
using DataBase.Configure;

namespace DataBase.Users
{
    public class AplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Tasks> Tasks { get; set; } = null!;
        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<Adreses> Adreses { get; set; } = null!;
        public DbSet<TaskFile> TaskFiles { get; set; } = null!;

        public AplicationContext(DbContextOptions<AplicationContext> options): base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfigure());
            modelBuilder.ApplyConfiguration(new TaskConfigure());
            modelBuilder.ApplyConfiguration(new CompanyConfigure());
            modelBuilder.ApplyConfiguration(new AdressConfigure());
            modelBuilder.ApplyConfiguration(new TaskFileConfigure());
        }
    }
}
