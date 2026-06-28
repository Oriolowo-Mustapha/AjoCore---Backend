using System;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AjoCoreBackend.Persistence.Repositories
{
    public class SavingCycleRepository : GenericRepository<SavingCycle>, ISavingCycleRepository
    {
        public SavingCycleRepository(AjoCoreDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<SavingCycle?> GetCycleWithMembersAsync(Guid cycleId)
        {
            return await _dbContext.SavingCycles
                .Include(c => c.Members)
                .Include(c => c.IndividualOwner)
                .FirstOrDefaultAsync(c => c.Id == cycleId);
        }

        public async Task<ICollection<SavingCycle>> GetSavingCycleByIndividualId(Guid UserId)
        {
            return await _dbContext.SavingCycles
                .Where(c => c.IndividualOwnerId == UserId)
                .ToListAsync();
        }

        public async Task<bool> IsSubAccountLinkedAsync(string subAccountId)
        {
            return await _dbContext.SavingCycles
                .AnyAsync(c => c.NombaSubAccountId == subAccountId);
        }
    }
}
