using System;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Auth.UpdateBvn
{
    public class UpdateBvnCommandHandler : IRequestHandler<UpdateBvnCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateBvnCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(UpdateBvnCommand request, CancellationToken cancellationToken)
        {
            var userIdString = _currentUserService.UserId;
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated properly.");
            }

            var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(userId);
            if (trader == null)
            {
                throw new NotFoundException($"Trader not found.");
            }

            if (trader.IsKycCompleted)
            {
                throw new InvalidOperationException("KYC is already completed. BVN cannot be updated again.");
            }

            trader.Bvn = request.Bvn;
            trader.IsKycCompleted = true;

            _unitOfWork.Repository<Trader>().Update(trader);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
