using System;

namespace AjoCoreBackend.Domain.Exceptions
{
    public class InsufficientContributionException : DomainException
    {
        public decimal ExpectedAmount { get; }
        public decimal ActualAmount { get; }

        public InsufficientContributionException(decimal expectedAmount, decimal actualAmount) 
            : base($"The contribution amount of {actualAmount} is less than the required cycle amount of {expectedAmount}.")
        {
            ExpectedAmount = expectedAmount;
            ActualAmount = actualAmount;
        }
    }
}
