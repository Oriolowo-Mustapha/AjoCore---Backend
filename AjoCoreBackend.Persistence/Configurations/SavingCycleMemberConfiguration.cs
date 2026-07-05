using AjoCoreBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjoCoreBackend.Persistence.Configurations
{
    public class SavingCycleMemberConfiguration : IEntityTypeConfiguration<SavingCycleMember>
    {
        public void Configure(EntityTypeBuilder<SavingCycleMember> builder)
        {
            builder.HasKey(m => m.Id);

            // Convert Enum to string
            builder.Property(m => m.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Ensure a member has a unique payout order slot within a specific cycle
            builder.HasIndex(m => new { m.SavingCycleId, m.PayoutOrder })
                .IsUnique();

            // Configure NombaVirtualAccountId as optional
            builder.Property(m => m.NombaVirtualAccountId).IsRequired(false);

            // One-to-One mapping between Member and Virtual Account
            builder.HasOne(m => m.VirtualAccount)
                .WithOne(v => v.Member)
                .HasForeignKey<SavingCycleMember>(m => m.NombaVirtualAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-Many mapping to Cycle
            builder.HasOne(m => m.Cycle)
                .WithMany(c => c.Members)
                .HasForeignKey(m => m.SavingCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-Many mapping to Trader
            builder.HasOne(m => m.Trader)
                .WithMany(t => t.SavingCycleMembers)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
