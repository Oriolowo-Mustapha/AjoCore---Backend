using AjoCoreBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AjoCoreBackend.Persistence.Contexts
{
    public class AjoCoreDbContext : DbContext
    {
        public AjoCoreDbContext(DbContextOptions<AjoCoreDbContext> options) : base(options)
        {
        }

        public DbSet<SavingCycle> SavingCycles { get; set; } = null!;
        public DbSet<NombaVirtualAccount> NombaVirtualAccounts { get; set; } = null!;
        public DbSet<SavingCycleMember> SavingCycleMembers { get; set; } = null!;
        public DbSet<ContributionLedger> ContributionLedgers { get; set; } = null!;
        public DbSet<PayoutLedger> PayoutLedgers { get; set; } = null!;
        public DbSet<Trader> Traders { get; set; } = null!;
        public DbSet<CooperativeAdmin> CooperativeAdmins { get; set; } = null!;
        public DbSet<CooperativeGroup> CooperativeGroups { get; set; } = null!;
        public DbSet<CooperativeGroupMember> CooperativeGroupMembers { get; set; } = null!;
        public DbSet<InboundTransaction> InboundTransactions { get; set; } = null!;
        public DbSet<RotationalSlot> RotationalSlots { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AjoCoreDbContext).Assembly);
        }
    }
}
