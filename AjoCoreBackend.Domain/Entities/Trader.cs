using System;
using System.Collections.Generic;

namespace AjoCoreBackend.Domain.Entities
{
    public class Trader : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Bvn { get; set; }
        public DateTime DateOfBirth { get; set; }

        // Navigation Properties
        public ICollection<CooperativeGroup> AdministeredGroups { get; set; } = new List<CooperativeGroup>();
        public ICollection<InboundTransaction> Transactions { get; set; } = new List<InboundTransaction>();
    }
}
