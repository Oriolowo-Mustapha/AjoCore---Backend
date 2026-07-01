using AjoCoreBackend.Application.DTOs.Nomba;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AjoCoreBackend.Application.Commands.IndividualContriution.CreateIndividualSavingCycle
{
    public class CreateIndividualSavingCycleHandler(IUnitOfWork _unitOfWork, ILogger<CreateIndividualSavingCycleHandler> _logger, INombaApiClient _nombaApiClient, IDateTimeProvider _dateTimeProvider, ICurrentUserService _currentUser, IEmailService _emailService, IHangfireBackGroundService _hangfireBackgroundService) : IRequestHandler<CreateIndividualSavingCycleCommand, Guid>
    {
        public async Task<Guid> Handle(CreateIndividualSavingCycleCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating a new saving cycle with name: {CycleName}", request.Name);

            var savingCyleType = Enum.Parse<CycleType>(request.CycleType, ignoreCase: true);

            // 2. Create the Saving Cycle
            var newSavingCycle = new SavingCycle
            {
                Name = request.Name,
                CycleType = savingCyleType,
                ContributionAmount = request.ContributionAmount,
                IntervalDays = request.IntervalDays,
                Status = CycleStatus.Pending,
                StartDate = _dateTimeProvider.UtcNow,
                EndDate = request.EndDate,
                CooperativeGroupId = savingCyleType == CycleType.Personal ? null : request.CooperativeGroupId,
            };

            var userId = _currentUser.UserGuid ?? throw new KeyNotFoundException($"Current user with ID {_currentUser.UserId} not found");


            var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(userId);
            if (trader == null) throw new KeyNotFoundException($"Trader with ID {userId} not found");

            // Provision a NUBAN Virtual Account attached to the cycle's Nomba Sub-Account
            var virtualAccountResponse = await _nombaApiClient.CreateVirtualAccountAsync(new CreateVirtualAccountRequest
            {
                AccountReference = $"mem_{Guid.NewGuid():N}",
                AccountName = $"{trader.FirstName} {trader.LastName} AjoCore Personal"
            });

            _logger.LogInformation("Successfully created Nomba Virtual Account with Number: {AccountNumber}", virtualAccountResponse.AccountNumber);

            var virtualAccount = new NombaVirtualAccount
            {
                AccountNumber = virtualAccountResponse.AccountNumber,
                BankName = virtualAccountResponse.BankName,
                AccountName = virtualAccountResponse.AccountName
            };

            var savingCycleMember = new SavingCycleMember
            {
                UserId = (Guid)_currentUser.UserGuid!,
                SavingCycleId = newSavingCycle.Id,
                NombaVirtualAccountId = virtualAccount.Id
            };

            newSavingCycle.Members.Add( savingCycleMember );

            await _unitOfWork.SavingCycles.AddAsync(newSavingCycle);

            await _unitOfWork.Repository<NombaVirtualAccount>().AddAsync(virtualAccount);

            await _unitOfWork.Repository<SavingCycleMember>().AddAsync(savingCycleMember);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _hangfireBackgroundService.ScheduleTask<CreateIndividualSavingCycleHandler>(x => x.UpdateCycleStatusSendNotification(newSavingCycle.Id), newSavingCycle.StartDate.AddSeconds(5));

            return newSavingCycle.Id;
        }

        private async Task UpdateCycleStatusSendNotification(Guid SavingCycleId)
        {
            var now = DateTime.UtcNow;
            var userId = _currentUser.UserGuid ?? throw new KeyNotFoundException($"Current user with ID {_currentUser.UserId} not found");

            var cycles = await _unitOfWork.Repository<SavingCycle>()
                .FindAsync(x =>
                    x.Status == CycleStatus.Pending &&
                    x.Id == SavingCycleId, c => c.Members.First(s => s.UserId == userId));
            var cycle = cycles.FirstOrDefault();

            if (cycle == null) return;

            cycle.Status = CycleStatus.Active;

            // Send emails...

            var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(cycle.Members.First(m => m.UserId == userId).UserId);
            var emailSubject = $"AjoCore: {cycle.Name} Has Started";

            var emailBody = $@"

                <h3>Hello {trader.FirstName},</h3>

                <p>We're pleased to inform you that your savings cycle, <strong>{cycle.Name}</strong>, is now active.</p>

                <p><strong>Cycle Details</strong></p>
                <ul>
                    <li><strong>Contribution Amount:</strong> ₦{cycle.ContributionAmount:N2}</li>
                    <li><strong>Contribution Frequency:</strong> Every {cycle.IntervalDays} day(s)</li>
                    <li><strong>Start Date:</strong> {cycle.StartDate:dddd, MMMM dd, yyyy}</li>
                </ul>

                <p>Your first contribution is now due. Please ensure you make your contribution using your assigned virtual account to keep your participation active and on track.</p>

                <p>Timely contributions help ensure the smooth operation of the cycle and allow all members to benefit as scheduled.</p>

                <br/>

                <p>If you have any questions or need assistance, please contact the AjoCore support team.</p>

                <p>
                Thank you for saving with us.<br/>
                <strong>AjoCore Team</strong>
                </p>";


            await _emailService.SendEmailAsync(
                trader.Email,
                emailSubject,
                emailBody);

            await _unitOfWork.SaveChangesAsync();
        }

    }
        

}
   
