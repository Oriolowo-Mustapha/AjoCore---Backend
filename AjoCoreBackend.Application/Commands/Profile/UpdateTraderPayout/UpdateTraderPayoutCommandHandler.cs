using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Profile;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Exceptions;
using AutoMapper;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Profile.UpdateTraderPayout
{
    public class UpdateTraderPayoutCommandHandler : IRequestHandler<UpdateTraderPayoutCommand, TraderProfileDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public UpdateTraderPayoutCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<TraderProfileDto> Handle(UpdateTraderPayoutCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserGuid;
            if (!userId.HasValue) throw new ForbiddenAccessException("User not authenticated.");

            var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(userId.Value);
            if (trader == null) throw new NotFoundException($"Trader with ID {userId.Value} not found.");

            trader.PayoutAccountNumber = request.PayoutAccountNumber;
            trader.PayoutBankName = request.PayoutBankName;
            trader.PayoutAccountName = request.PayoutAccountName;
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
