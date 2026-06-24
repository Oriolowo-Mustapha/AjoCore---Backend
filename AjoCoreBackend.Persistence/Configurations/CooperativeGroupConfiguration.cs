using AjoCoreBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjoCoreBackend.Persistence.Configurations
{
    public class CooperativeGroupConfiguration : IEntityTypeConfiguration<CooperativeGroup>
    {
        public void Configure(EntityTypeBuilder<CooperativeGroup> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
            builder.Property(c => c.Description).HasMaxLength(500);

            builder.HasOne(c => c.AdminTrader)
                .WithMany(t => t.AdministeredGroups)
                .HasForeignKey(c => c.AdminTraderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
