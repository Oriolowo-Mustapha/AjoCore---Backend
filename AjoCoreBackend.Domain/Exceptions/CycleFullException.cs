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
    public class UserBankDetailNotFoundExcepion : DomainException
    {
        public Guid TraderId { get; }
        public string TraderName { get; }

        public UserBankDetailNotFoundExcepion(Guid traderId, string traderName)
            : base($"This user's personal account details is not found {traderName}")
        {
            TraderId = traderId;
            TraderName = traderName;
        }
    }
}
