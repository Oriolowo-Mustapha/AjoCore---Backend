using AjoCoreBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjoCoreBackend.Persistence.Configurations
{
    public class CooperativeGroupMemberConfiguration : IEntityTypeConfiguration<CooperativeGroupMember>
    {
        public void Configure(EntityTypeBuilder<CooperativeGroupMember> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.CooperativeGroupId, x.TraderId }).IsUnique();
            builder.HasOne(x => x.Group).WithMany(g => g.Members).HasForeignKey(x => x.CooperativeGroupId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Trader).WithMany(t => t.GroupMemberships).HasForeignKey(x => x.TraderId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
