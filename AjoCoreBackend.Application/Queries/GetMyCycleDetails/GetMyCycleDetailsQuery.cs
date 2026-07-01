using AjoCoreBackend.Application.DTOs;
using MediatR;
using System;

namespace AjoCoreBackend.Application.Queries.GetMyCycleDetails
{
    public class GetMyCycleDetailsQuery : IRequest<MyCycleDetailsDto>
    {
        public Guid SavingCycleId { get; set; }
        public string ExpectedCycleType { get; set; } = string.Empty;
    }
}
