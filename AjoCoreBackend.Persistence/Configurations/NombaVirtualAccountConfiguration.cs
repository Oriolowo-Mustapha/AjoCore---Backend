using AjoCoreBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjoCoreBackend.Persistence.Configurations
{
    public class NombaVirtualAccountConfiguration : IEntityTypeConfiguration<NombaVirtualAccount>
    {
        public void Configure(EntityTypeBuilder<NombaVirtualAccount> builder)
        {
            builder.HasKey(n => n.Id);

            builder.Property(n => n.AccountNumber)
                .HasMaxLength(20);

            builder.Property(n => n.BankName)
                .HasMaxLength(100);

            builder.Property(n => n.AccountName)
                .HasMaxLength(150);
        }
    }
}
