using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.StartSavingCycle
{
    public class StartSavingCycleCommandHandler : IRequestHandler<StartSavingCycleCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public StartSavingCycleCommandHandler(
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<bool> Handle(StartSavingCycleCommand request, CancellationToken cancellationToken)
        {
            var cycle = await _unitOfWork.SavingCycles.GetCycleWithMembersAsync(request.SavingCycleId);

            if (cycle == null)
            {
                throw new NotFoundException($"Saving Cycle with ID {request.SavingCycleId} was not found.");
            }

            if (cycle.Status != CycleStatus.Pending)
            {
                throw new CycleAlreadyStartedException(cycle.Id);
            }

            if (cycle.Members.Count < 2)
            {
                throw new DomainException("Cannot start a cycle with less than 2 members.");
            }

            cycle.Status = CycleStatus.Active;
            cycle.StartDate = _dateTimeProvider.UtcNow;

            if (cycle.CycleType == CycleType.Rosca)
            {
                var slotsRepo = _unitOfWork.Repository<RotationalSlot>();
                
                // Members are assigned slots based on their PayoutOrder
                var orderedMembers = cycle.Members.OrderBy(m => m.PayoutOrder).ToList();
                
                for (int i = 0; i < orderedMembers.Count; i++)
                {
                    var member = orderedMembers[i];
                    
                    var slot = new RotationalSlot
                    {
                        SavingCycleId = cycle.Id,
                        SlotNumber = i + 1,
                        IsAssigned = true,
                        AssignedMemberId = member.Id,
                        // Estimated date assumes intervals run continuously from StartDate
                        EstimatedPayoutDate = cycle.StartDate.AddDays(cycle.IntervalDays * (i + 1))
                    };
                    
                    await slotsRepo.AddAsync(slot);
                }
                
                // Set cycle EndDate based on last slot
                cycle.EndDate = cycle.StartDate.AddDays(cycle.IntervalDays * cycle.Members.Count);
            }

            _unitOfWork.SavingCycles.Update(cycle);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
