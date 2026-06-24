using AjoCoreBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjoCoreBackend.Persistence.Configurations
{
    public class TraderConfiguration : IEntityTypeConfiguration<Trader>
    {
        public void Configure(EntityTypeBuilder<Trader> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.FirstName).IsRequired().HasMaxLength(50);
            builder.Property(t => t.LastName).IsRequired().HasMaxLength(50);
            
            builder.Property(t => t.Email).IsRequired().HasMaxLength(100);
            builder.HasIndex(t => t.Email).IsUnique();

            builder.Property(t => t.PhoneNumber).IsRequired().HasMaxLength(20);
            builder.Property(t => t.Bvn).HasMaxLength(11);
        }
    }
}
