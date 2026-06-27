using System;
using System.Collections.Generic;
using AjoCoreBackend.Domain.Enum;

namespace AjoCoreBackend.Domain.Entities
{
    public class CooperativeAdmin : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.CooperativeAdmin;

        // Navigation Properties
        public ICollection<CooperativeGroup> AdministeredGroups { get; set; } = new List<CooperativeGroup>();
    }
}
