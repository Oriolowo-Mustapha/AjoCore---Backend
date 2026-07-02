using System;

namespace AjoCoreBackend.Application.DTOs.Profile
{
    public class TraderProfileDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Bvn { get; set; } // Masked
        public bool IsKycCompleted { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PayoutAccountNumber { get; set; } // Masked
        public string? PayoutBankName { get; set; }
        public string? PayoutAccountName { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
