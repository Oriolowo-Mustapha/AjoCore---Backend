using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Nomba;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.SavingCycles.ApproveMember
{
    public class ApproveCycleMemberCommandHandler : IRequestHandler<ApproveCycleMemberCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INombaApiClient _nombaApiClient;

        public ApproveCycleMemberCommandHandler(IUnitOfWork unitOfWork, INombaApiClient nombaApiClient)
        {
            _unitOfWork = unitOfWork;
            _nombaApiClient = nombaApiClient;
        }

        public async Task Handle(ApproveCycleMemberCommand request, CancellationToken cancellationToken)
        {
            var cycle = await _unitOfWork.SavingCycles.GetByIdAsync(request.SavingCycleId);
            if (cycle == null)
                throw new NotFoundException($"Saving Cycle with ID {request.SavingCycleId} not found.");

            var member = await _unitOfWork.SavingCycleMembers.GetByIdAsync(request.MemberId);
            if (member == null || member.SavingCycleId != request.SavingCycleId)
                throw new NotFoundException($"Member with ID {request.MemberId} not found in this cycle.");

            if (member.ApprovalStatus == ApprovalStatus.Approved)
                throw new DomainException("Member is already approved for this cycle.");

            var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(member.UserId);
            if (trader == null) throw new NotFoundException($"Trader with ID {member.UserId} not found.");

            // Provision a NUBAN Virtual Account attached to the cycle's Nomba Sub-Account
            var virtualAccountResponse = await _nombaApiClient.CreateVirtualAccountAsync(new CreateVirtualAccountRequest
            {
                AccountReference = $"member_{member.UserId:N}",
                AccountName = $"{trader.FirstName} {trader.LastName} AjoCore"
            });

            var virtualAccount = new NombaVirtualAccount
            {
                AccountNumber = virtualAccountResponse.AccountNumber,
                BankName = virtualAccountResponse.BankName,
                AccountName = virtualAccountResponse.AccountName
            };

            member.VirtualAccount = virtualAccount;
            member.ApprovalStatus = ApprovalStatus.Approved;
            member.Status = MemberStatus.Active;
            
            _unitOfWork.SavingCycleMembers.Update(member);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
