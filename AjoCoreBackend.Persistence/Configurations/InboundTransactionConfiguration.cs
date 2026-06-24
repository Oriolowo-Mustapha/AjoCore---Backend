using AjoCoreBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjoCoreBackend.Persistence.Configurations
{
    public class InboundTransactionConfiguration : IEntityTypeConfiguration<InboundTransaction>
    {
        public void Configure(EntityTypeBuilder<InboundTransaction> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Amount).IsRequired().HasColumnType("numeric(18,2)");
            
            builder.Property(i => i.TransactionReference).IsRequired().HasMaxLength(100);
            builder.HasIndex(i => i.TransactionReference).IsUnique();

            builder.Property(i => i.PaymentMethod).HasMaxLength(50);
            builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);

            builder.HasOne(i => i.Trader)
                .WithMany(t => t.Transactions)
                .HasForeignKey(i => i.TraderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
