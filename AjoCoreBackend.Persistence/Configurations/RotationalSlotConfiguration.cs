using AjoCoreBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjoCoreBackend.Persistence.Configurations
{
    public class RotationalSlotConfiguration : IEntityTypeConfiguration<RotationalSlot>
    {
        public void Configure(EntityTypeBuilder<RotationalSlot> builder)
        {
            builder.HasKey(r => r.Id);

            // A cycle can only have one specific slot number
            builder.HasIndex(r => new { r.SavingCycleId, r.SlotNumber }).IsUnique();

            builder.HasOne(r => r.Cycle)
                .WithMany() 
                .HasForeignKey(r => r.SavingCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.AssignedMember)
                .WithMany()
                .HasForeignKey(r => r.AssignedMemberId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
