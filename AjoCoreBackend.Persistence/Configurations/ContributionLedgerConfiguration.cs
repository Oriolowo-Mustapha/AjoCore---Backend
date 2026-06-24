using AjoCoreBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjoCoreBackend.Persistence.Configurations
{
    public class ContributionLedgerConfiguration : IEntityTypeConfiguration<ContributionLedger>
    {
        public void Configure(EntityTypeBuilder<ContributionLedger> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Amount)
                .IsRequired()
                .HasColumnType("numeric(18,2)");

            // Crucial idempotency guard against duplicate Nomba Webhooks
            builder.HasIndex(c => c.NombaWebhookRequestId)
                .IsUnique();
                
            builder.Property(c => c.NombaWebhookRequestId)
                .IsRequired()
                .HasMaxLength(100);

            // Ensure immutable ledger records
            builder.HasOne(c => c.Member)
                .WithMany(m => m.Contributions)
                .HasForeignKey(c => c.SavingCycleMemberId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
