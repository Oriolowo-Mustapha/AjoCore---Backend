using System;
using System.Collections.Generic;
using AjoCoreBackend.Domain.Enum;

namespace AjoCoreBackend.Domain.Entities
{
    public class Trader : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Trader;
        public string? Bvn { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }
        
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }

        // Navigation Properties
        public ICollection<CooperativeGroup> AdministeredGroups { get; set; } = new List<CooperativeGroup>();
        public ICollection<CooperativeGroupMember> GroupMemberships { get; set; } = new List<CooperativeGroupMember>();
        public ICollection<InboundTransaction> Transactions { get; set; } = new List<InboundTransaction>();
    }
}
