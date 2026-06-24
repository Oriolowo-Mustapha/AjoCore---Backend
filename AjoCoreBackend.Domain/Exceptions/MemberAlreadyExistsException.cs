using System;

namespace AjoCoreBackend.Domain.Exceptions
{
    public class MemberAlreadyExistsException : DomainException
    {
        public Guid NombaVirtualAccountId { get; }
        public Guid CycleId { get; }

        public MemberAlreadyExistsException(Guid nombaVirtualAccountId, Guid cycleId) 
            : base("This member is already actively participating in this cycle.")
        {
            NombaVirtualAccountId = nombaVirtualAccountId;
            CycleId = cycleId;
        }
    }
}
