using System;
using MediatR;
using AjoCoreBackend.Application.DTOs;

namespace AjoCoreBackend.Application.Queries.GetMyCycleDetails
{
    public class GetMyCycleDetailsQuery : IRequest<SavingCycleDto>
    {
        public Guid SavingCycleId { get; set; }
    }
}
