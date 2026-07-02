using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Profile;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Exceptions;
using AutoMapper;
using MediatR;

namespace AjoCoreBackend.Application.Queries.Profile.GetTraderProfile
{
    public class GetTraderProfileQueryHandler : IRequestHandler<GetTraderProfileQuery, TraderProfileDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetTraderProfileQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<TraderProfileDto> Handle(GetTraderProfileQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserGuid;
            if (!userId.HasValue)
            {
                throw new ForbiddenAccessException("User not authenticated.");
            }

            var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(userId.Value);
            if (trader == null)
            {
                throw new NotFoundException($"Trader with ID {userId.Value} not found.");
            }

            var dto = _mapper.Map<TraderProfileDto>(trader);

            // Mask BVN (only show last 4 digits)
            if (!string.IsNullOrEmpty(dto.Bvn) && dto.Bvn.Length > 4)
            {
                dto.Bvn = new string('*', dto.Bvn.Length - 4) + dto.Bvn.Substring(dto.Bvn.Length - 4);
            }

            // Mask PayoutAccountNumber (only show last 4 digits)
            if (!string.IsNullOrEmpty(dto.PayoutAccountNumber) && dto.PayoutAccountNumber.Length > 4)
            {
                dto.PayoutAccountNumber = new string('*', dto.PayoutAccountNumber.Length - 4) + dto.PayoutAccountNumber.Substring(dto.PayoutAccountNumber.Length - 4);
            }

            return dto;
        }
    }
}
