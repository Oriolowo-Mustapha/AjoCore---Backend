using System;

namespace AjoCoreBackend.Domain.Exceptions
{
    public class InvalidPayoutOrderException : DomainException
    {
        public int RequestedOrder { get; }

        public InvalidPayoutOrderException(int requestedOrder) 
            : base($"The requested payout slot #{requestedOrder} is either already taken or out of bounds for this cycle.")
        {
            RequestedOrder = requestedOrder;
        }
    }
}
