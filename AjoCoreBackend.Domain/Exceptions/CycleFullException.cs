using System;

namespace AjoCoreBackend.Domain.Exceptions
{
    public class CycleFullException : DomainException
    {
        public Guid CycleId { get; }
        public int MaxMembers { get; }

        public CycleFullException(Guid cycleId, int maxMembers) 
            : base($"This rotational cycle has reached its maximum capacity of {maxMembers} members.")
        {
            CycleId = cycleId;
            MaxMembers = maxMembers;
        }
    }
}
