using System;

namespace AjoCoreBackend.Domain.Exceptions
{
    public class MemberAlreadyExistsException : DomainException
    {
        public Guid UserId { get; }
        public Guid CycleId { get; }

        public MemberAlreadyExistsException(Guid userId, Guid cycleId) 
            : base("This member is already actively participating in this cycle.")
        {
            UserId = userId;
            CycleId = cycleId;
        }
    }
}
