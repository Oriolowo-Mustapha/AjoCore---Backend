using System;
using System.Threading.Tasks;
using AjoCoreBackend.Domain.Entities;

namespace AjoCoreBackend.Application.Interfaces.Repositories
{
    public interface ILedgerRepository : IGenericRepository<ContributionLedger>
    {
        Task<bool> HasWebhookBeenProcessedAsync(string webhookRequestId);
        Task<decimal> GetTotalContributionsForMemberAsync(Guid memberId);
    }
}
