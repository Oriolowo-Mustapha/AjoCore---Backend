using System;
using System.Linq;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AjoCoreBackend.Persistence.Repositories
{
    public class LedgerRepository : GenericRepository<ContributionLedger>, ILedgerRepository
    {
        public LedgerRepository(AjoCoreDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<bool> HasWebhookBeenProcessedAsync(string webhookRequestId)
        {
            return await _dbContext.ContributionLedgers
                .AnyAsync(l => l.NombaWebhookRequestId == webhookRequestId);
        }

        public async Task<decimal> GetTotalContributionsForMemberAsync(Guid memberId)
        {
            return await _dbContext.ContributionLedgers
                .Where(l => l.SavingCycleMemberId == memberId)
                .SumAsync(l => l.Amount);
        }
    }
}
