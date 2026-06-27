using AjoCoreBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjoCoreBackend.Persistence.Configurations
{
    public class CooperativeAdminConfiguration : IEntityTypeConfiguration<CooperativeAdmin>
    {
        public void Configure(EntityTypeBuilder<CooperativeAdmin> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Email).IsUnique();
            builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
            builder.Property(x => x.PasswordHash).IsRequired();
            builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Role).IsRequired();
        }
    }
}
