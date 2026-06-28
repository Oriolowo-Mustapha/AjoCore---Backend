using AjoCoreBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjoCoreBackend.Persistence.Configurations
{
    public class SavingCycleConfiguration : IEntityTypeConfiguration<SavingCycle>
    {
        public void Configure(EntityTypeBuilder<SavingCycle> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.ContributionAmount)
                .IsRequired()
                .HasColumnType("numeric(18,2)");
            builder.Property(s => s.IndividualTargetAmount)
                .IsRequired()
                .HasColumnType("numeric(18,2)");

            builder.Property(s => s.NombaSubAccountId)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(s => s.IndividualOwnerId)
                .IsRequired(false);

           

            // Convert Enums to string
            builder.Property(s => s.CycleType)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasOne(s => s.CooperativeGroup)
                .WithMany(c => c.Cycles)
                .HasForeignKey(s => s.CooperativeGroupId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.Members)
            .WithOne(x => x.Cycle)
            .HasForeignKey(x => x.SavingCycleId)
            .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
