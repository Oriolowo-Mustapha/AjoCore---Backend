using System;
using AjoCoreBackend.Domain.Enum;

namespace AjoCoreBackend.Domain.Entities
{
    public class CooperativeGroupMember : BaseEntity
    {
        public Guid CooperativeGroupId { get; set; }
        public Guid TraderId { get; set; }
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
        public DateTime? ApprovedAt { get; set; }

        // Navigation Properties
        public CooperativeGroup Group { get; set; } = null!;
        public Trader Trader { get; set; } = null!;
    }
}
