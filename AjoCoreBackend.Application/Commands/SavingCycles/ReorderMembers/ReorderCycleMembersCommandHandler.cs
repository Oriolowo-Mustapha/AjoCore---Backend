using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.SavingCycles.ReorderMembers
{
    public class ReorderCycleMembersCommandHandler : IRequestHandler<ReorderCycleMembersCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReorderCycleMembersCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ReorderCycleMembersCommand request, CancellationToken cancellationToken)
        {
            var cycle = await _unitOfWork.SavingCycles.GetCycleWithMembersAsync(request.SavingCycleId);
            if (cycle == null)
                throw new NotFoundException($"Saving Cycle with ID {request.SavingCycleId} not found.");

            if (cycle.Status != CycleStatus.Pending)
                throw new DomainException("Cannot reorder members after the cycle has started.");

            // Validate that all members in the request belong to the cycle
            var requestedMemberIds = request.MemberOrders.Select(m => m.MemberId).ToList();
            var validMemberIds = cycle.Members.Select(m => m.Id).ToList();

            if (requestedMemberIds.Any(id => !validMemberIds.Contains(id)))
                throw new DomainException("One or more members in the request do not belong to this saving cycle.");

            // Validate that there are no duplicate orders
            var newOrders = request.MemberOrders.Select(m => m.NewOrder).ToList();
            if (newOrders.Distinct().Count() != newOrders.Count)
                throw new DomainException("Duplicate payout orders are not allowed.");

            foreach (var memberOrder in request.MemberOrders)
            {
                var member = cycle.Members.First(m => m.Id == memberOrder.MemberId);
                member.PayoutOrder = memberOrder.NewOrder;
                _unitOfWork.SavingCycleMembers.Update(member);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
