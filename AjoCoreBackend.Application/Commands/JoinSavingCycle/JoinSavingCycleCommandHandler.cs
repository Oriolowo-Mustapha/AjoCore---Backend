using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Nomba;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.JoinSavingCycle
{
    public class JoinSavingCycleCommandHandler : IRequestHandler<JoinSavingCycleCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INombaApiClient _nombaApiClient;

        public JoinSavingCycleCommandHandler(
            IUnitOfWork unitOfWork,
            INombaApiClient nombaApiClient)
        {
            _unitOfWork = unitOfWork;
            _nombaApiClient = nombaApiClient;
        }

        public async Task<Guid> Handle(JoinSavingCycleCommand request, CancellationToken cancellationToken)
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

            if (cycle.Members.Any(m => m.UserId == request.UserId))
            {
                throw new MemberAlreadyExistsException(request.UserId, cycle.Id);
            }

            // Provision a NUBAN Virtual Account attached to the cycle's Nomba Sub-Account
            var virtualAccountResponse = await _nombaApiClient.CreateVirtualAccountAsync(new CreateVirtualAccountRequest
            {
                SubAccountId = cycle.NombaSubAccountId,
                AccountReference = $"member_{request.UserId:N}"
            });

            var virtualAccount = new NombaVirtualAccount
            {
                SubAccountId = cycle.NombaSubAccountId,
                AccountNumber = virtualAccountResponse.AccountNumber,
                BankName = virtualAccountResponse.BankName,
                AccountName = virtualAccountResponse.AccountName
            };

            var highestOrder = await _unitOfWork.SavingCycleMembers.GetHighestPayoutOrderAsync(request.SavingCycleId);

            var member = new SavingCycleMember
            {
                SavingCycleId = request.SavingCycleId,
                UserId = request.UserId,
                Status = MemberStatus.Active,
                PayoutOrder = highestOrder + 1,
                VirtualAccount = virtualAccount
            };

            await _unitOfWork.SavingCycleMembers.AddAsync(member);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return member.Id;
        }
    }
}
