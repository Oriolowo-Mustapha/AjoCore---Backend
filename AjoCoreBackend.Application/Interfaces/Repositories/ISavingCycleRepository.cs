using System;
using System.Threading.Tasks;
using AjoCoreBackend.Domain.Entities;

namespace AjoCoreBackend.Application.Interfaces.Repositories
{
    public interface ISavingCycleRepository : IGenericRepository<SavingCycle>
    {
        Task<SavingCycle?> GetCycleWithMembersAsync(Guid cycleId);
        Task<ICollection<SavingCycle>> GetSavingCycleByIndividualId(Guid UserId);
        Task<bool> IsSubAccountLinkedAsync(string subAccountId);
    }
}
