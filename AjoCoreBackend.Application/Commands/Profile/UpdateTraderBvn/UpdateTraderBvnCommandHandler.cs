using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Profile;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Exceptions;
using AutoMapper;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Profile.UpdateTraderBvn
{
    public class UpdateTraderBvnCommandHandler : IRequestHandler<UpdateTraderBvnCommand, TraderProfileDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public UpdateTraderBvnCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<TraderProfileDto> Handle(UpdateTraderBvnCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserGuid;
            if (!userId.HasValue) throw new ForbiddenAccessException("User not authenticated.");

            var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(userId.Value);
            if (trader == null) throw new NotFoundException($"Trader with ID {userId.Value} not found.");

            trader.Bvn = request.Bvn;
            trader.UpdatedAt = System.DateTime.UtcNow;

            _unitOfWork.Repository<Trader>().Update(trader);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<TraderProfileDto>(trader);
            if (!string.IsNullOrEmpty(dto.Bvn) && dto.Bvn.Length > 4)
                dto.Bvn = new string('*', dto.Bvn.Length - 4) + dto.Bvn.Substring(dto.Bvn.Length - 4);
            if (!string.IsNullOrEmpty(dto.PayoutAccountNumber) && dto.PayoutAccountNumber.Length > 4)
                dto.PayoutAccountNumber = new string('*', dto.PayoutAccountNumber.Length - 4) + dto.PayoutAccountNumber.Substring(dto.PayoutAccountNumber.Length - 4);

            return dto;
        }
    }
}
