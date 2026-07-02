using System;
using System.Collections.Generic;

namespace AjoCoreBackend.Application.DTOs.Profile
{
    public class CooperativeAdminProfileDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public List<AdministeredGroupDto> AdministeredGroups { get; set; } = new List<AdministeredGroupDto>();
    }

    public class AdministeredGroupDto
    {
        public Guid GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
    }
}
