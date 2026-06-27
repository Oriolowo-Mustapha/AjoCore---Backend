using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.SavingCycles.RejectMember
{
    public class RejectCycleMemberCommandHandler : IRequestHandler<RejectCycleMemberCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RejectCycleMemberCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(RejectCycleMemberCommand request, CancellationToken cancellationToken)
        {
            var cycle = await _unitOfWork.SavingCycles.GetByIdAsync(request.SavingCycleId);
            if (cycle == null)
                throw new NotFoundException($"Saving Cycle with ID {request.SavingCycleId} not found.");

            var member = await _unitOfWork.SavingCycleMembers.GetByIdAsync(request.MemberId);
            if (member == null || member.SavingCycleId != request.SavingCycleId)
                throw new NotFoundException($"Member with ID {request.MemberId} not found in this cycle.");

            if (member.ApprovalStatus == ApprovalStatus.Rejected)
                throw new DomainException("Member is already rejected for this cycle.");

            member.ApprovalStatus = ApprovalStatus.Rejected;
            
            _unitOfWork.SavingCycleMembers.Update(member);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
