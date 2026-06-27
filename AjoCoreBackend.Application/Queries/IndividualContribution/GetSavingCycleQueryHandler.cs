using AjoCoreBackend.Application.DTOs;
using AjoCoreBackend.Application.DTOs.IndividualSavingCycle;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AjoCoreBackend.Application.Queries.IndividualContribution
{
    public class GetSavingCycleQueryHandler(ILogger<GetSavingCycleQueryHandler> _logger,ISavingCycleRepository _savingCycleRepository,ICurrentUserService _currentUser,IMapper _mapper) : IRequestHandler<GetAllSavingCycleQuery, List<SavingCycleDto>>
    {
        public async Task<List<SavingCycleDto>> Handle(GetAllSavingCycleQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all saving cycles for Individual: {CooperativeGroupId}", _currentUser.UserId);

            var savingCycles = await _savingCycleRepository.GetSavingCycleByIndividualId(_currentUser.UserGuid ?? Guid.Empty);

            if(savingCycles == null || savingCycles.Count == 0)
            {
                _logger.LogInformation("No saving cycles found for Individual: {CooperativeGroupId}", _currentUser.UserId);
                return new List<SavingCycleDto>();
            }

            return  _mapper.Map<List<SavingCycleDto>>(savingCycles);
        }
    }
}

         
