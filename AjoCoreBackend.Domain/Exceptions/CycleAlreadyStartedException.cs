using System;

namespace AjoCoreBackend.Domain.Exceptions
{
    public class CycleAlreadyStartedException : DomainException
    {
        public Guid CycleId { get; }

        public CycleAlreadyStartedException(Guid cycleId) 
            : base("This rotational cycle has already begun and cannot accept new members or modifications.")
        {
            CycleId = cycleId;
        }
    }
}
