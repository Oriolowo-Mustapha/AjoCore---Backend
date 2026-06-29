using MediatR;
using System;

namespace AjoCoreBackend.Application.Commands.IndividualContriution.LiquidateEarly
{
    public class LiquidateEarlyCommand : IRequest<bool>
    {
        public Guid SavingCycleId { get; set; }
    }
}
