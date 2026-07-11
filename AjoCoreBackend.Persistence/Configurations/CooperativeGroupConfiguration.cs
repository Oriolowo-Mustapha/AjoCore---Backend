using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjoCoreBackend.Persistence.Configurations
{
    public class CooperativeGroupConfiguration : IEntityTypeConfiguration<CooperativeGroup>
    {
        public void Configure(EntityTypeBuilder<CooperativeGroup> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);

            builder.Property(x => x.Status).IsRequired().HasDefaultValue(GroupStatus.Active);

            builder.HasOne(x => x.CooperativeAdmin)
                .WithMany(t => t.AdministeredGroups)
                .HasForeignKey(x => x.CooperativeAdminId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
