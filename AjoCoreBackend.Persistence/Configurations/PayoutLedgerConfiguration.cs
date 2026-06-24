using AjoCoreBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjoCoreBackend.Persistence.Configurations
{
    public class PayoutLedgerConfiguration : IEntityTypeConfiguration<PayoutLedger>
    {
        public void Configure(EntityTypeBuilder<PayoutLedger> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Amount)
                .IsRequired()
                .HasColumnType("numeric(18,2)");

            // Crucial idempotency guard against duplicate Nomba outbound payouts
            builder.HasIndex(p => p.MerchantTxRef)
                .IsUnique();

            builder.Property(p => p.MerchantTxRef)
                .IsRequired()
                .HasMaxLength(100);

            // Ensure immutable ledger records
            builder.HasOne(p => p.Member)
                .WithMany(m => m.Payouts)
                .HasForeignKey(p => p.SavingCycleMemberId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
