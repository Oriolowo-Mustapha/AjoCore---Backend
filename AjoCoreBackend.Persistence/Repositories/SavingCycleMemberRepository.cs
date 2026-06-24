using System;
using System.Linq;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AjoCoreBackend.Persistence.Repositories
{
    public class SavingCycleMemberRepository : GenericRepository<SavingCycleMember>, ISavingCycleMemberRepository
    {
        public SavingCycleMemberRepository(AjoCoreDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<SavingCycleMember?> GetMemberWithVirtualAccountAsync(Guid memberId)
        {
            return await _dbContext.SavingCycleMembers
                .Include(m => m.VirtualAccount)
                .FirstOrDefaultAsync(m => m.Id == memberId);
        }

        public async Task<bool> DoesMemberExistInCycleAsync(Guid cycleId, Guid virtualAccountId)
        {
            return await _dbContext.SavingCycleMembers
                .AnyAsync(m => m.SavingCycleId == cycleId && m.NombaVirtualAccountId == virtualAccountId);
        }

        public async Task<int> GetHighestPayoutOrderAsync(Guid cycleId)
        {
            var exists = await _dbContext.SavingCycleMembers.AnyAsync(m => m.SavingCycleId == cycleId);
            if (!exists) return 0;
            
            return await _dbContext.SavingCycleMembers
                .Where(m => m.SavingCycleId == cycleId)
                .MaxAsync(m => m.PayoutOrder);
        }
    }
}
