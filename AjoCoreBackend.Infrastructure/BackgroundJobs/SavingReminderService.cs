using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using Microsoft.Extensions.Logging;

namespace AjoCoreBackend.Infrastructure.BackgroundJobs
{
    public class SavingReminderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<SavingReminderService> _logger;

        public SavingReminderService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IDateTimeProvider dateTimeProvider,
            ILogger<SavingReminderService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task ProcessRemindersAsync()
        {
            _logger.LogInformation("Hangfire Job: Saving Reminder Service is executing.");
            try
            {
                var now = _dateTimeProvider.UtcNow;

                // Fetch active cycles
                var activeCycles = await _unitOfWork.SavingCycles.FindAsync(c => c.Status == CycleStatus.Active);

                foreach (var cycle in activeCycles)
                {
                    var cycleWithMembers = await _unitOfWork.SavingCycles.GetCycleWithMembersAsync(cycle.Id);
                    if (cycleWithMembers == null) continue;

                    // Calculate the current interval
                    var daysSinceStart = (now - cycle.StartDate.Value).TotalDays;
                    var currentInterval = (int)(daysSinceStart / cycle.IntervalDays) + 1;
                    
                    // Calculate next payment due date
                    var nextDueDate = cycle.StartDate.Value.AddDays(currentInterval * cycle.IntervalDays);
                    var daysUntilDue = (nextDueDate - now).TotalDays;

                    // If the next payment is due in exactly 1 day (or close to it)
                    if (daysUntilDue > 0 && daysUntilDue <= 2) 
                    {
                        foreach (var member in cycleWithMembers.Members.Where(m => m.Status == MemberStatus.Active))
                        {
                            var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(member.UserId);
                            
                            if (trader != null && !string.IsNullOrEmpty(trader.Email))
                            {
                                var emailSubject = $"AjoCore Reminder: Contribution Due for {cycle.Name}";
                                var emailBody = $@"
                                    <h3>Hello {trader.FirstName},</h3>
                                    <p>This is a quick reminder that your next contribution of <strong>₦{cycle.ContributionAmount:N2}</strong> for the cycle <strong>{cycle.Name}</strong> is due in {Math.Ceiling(daysUntilDue)} day(s) on {nextDueDate.ToShortDateString()}.</p>
                                    <p>Please ensure you transfer the funds to your dedicated Nomba virtual account to avoid defaulting.</p>
                                    <br/>
                                    <p>Thank you,<br/>AjoCore Team</p>
                                ";

                                _ = _emailService.SendEmailAsync(trader.Email, emailSubject, emailBody);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing Hangfire Saving Reminder Service.");
                throw; // Rethrow so Hangfire knows the job failed and can retry
            }
        }
    }
}
