
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AjoCoreBackend.Infrastructure.BackgroundJobs
{
    public class SavingReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SavingReminderService> _logger;

        public SavingReminderService(IServiceProvider serviceProvider, ILogger<SavingReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Saving Reminder Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing Saving Reminder Service.");
                }

                // Run every 24 hours
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }


        private async Task ProcessRemindersAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            var now = dateTimeProvider.UtcNow;

            // Fetch active cycles
            var activeCycles = await unitOfWork.SavingCycles.FindAsync(c => c.Status == CycleStatus.Active);

            foreach (var cycle in activeCycles)
            {
                var cycleWithMembers = await unitOfWork.SavingCycles.GetCycleWithMembersAsync(cycle.Id);
                if (cycleWithMembers == null) continue;

                // Calculate the current interval
                var daysSinceStart = (now - cycle.StartDate).TotalDays;
                var currentInterval = (int)(daysSinceStart / cycle.IntervalDays) + 1;
                
                // Calculate next payment due date
                var nextDueDate = cycle.StartDate.AddDays(currentInterval * cycle.IntervalDays);
                var daysUntilDue = (nextDueDate - now).TotalDays;

                // If the next payment is due in exactly 1 day (or close to it)
                if (daysUntilDue > 0 && daysUntilDue <= 2) 
                {
                    foreach (var member in cycleWithMembers.Members.Where(m => m.Status == MemberStatus.Active))
                    {
                        // We need the trader's email. Since SavingCycleMember links to UserId (Trader), we fetch the trader.
                        var trader = await unitOfWork.Repository<Trader>().GetByIdAsync(member.UserId);
                        
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

                            _ = emailService.SendEmailAsync(trader.Email, emailSubject, emailBody);
                        }
                    }
                }
            }
        }
    }
}
