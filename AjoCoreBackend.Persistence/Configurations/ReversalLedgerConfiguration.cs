using AjoCoreBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjoCoreBackend.Persistence.Configurations
{
    public class ReversalLedgerConfiguration : IEntityTypeConfiguration<ReversalLedger>
    {
        public void Configure(EntityTypeBuilder<ReversalLedger> builder)
        {
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.ReversalTxRef)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.OriginalWebhookRequestId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.FailureReason)
                .HasMaxLength(500);

            // Configure enum conversion if needed, but usually EF handles it out of the box.
            
            builder.HasOne(x => x.Member)
                .WithMany()
                .HasForeignKey(x => x.SavingCycleMemberId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
