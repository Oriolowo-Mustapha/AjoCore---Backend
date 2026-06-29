using System;

namespace AjoCoreBackend.Domain.Entities
{
    public class NombaVirtualAccount : BaseEntity
    {
        public string? AccountNumber { get; set; }
        public string? BankName { get; set; }
        public string? AccountName { get; set; }
        
        // Navigation Properties
        public SavingCycleMember? Member { get; set; }

    }
}
