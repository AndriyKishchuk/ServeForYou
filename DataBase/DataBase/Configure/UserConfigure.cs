using DataBase.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataBase.Configure
{
    public class UserConfigure : IEntityTypeConfiguration<User> 
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
        }
    }
}
