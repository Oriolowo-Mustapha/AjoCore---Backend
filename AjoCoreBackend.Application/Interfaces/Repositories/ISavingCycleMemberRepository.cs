using System;
using System.Threading.Tasks;
using AjoCoreBackend.Domain.Entities;

namespace AjoCoreBackend.Application.Interfaces.Repositories
{
    public interface ISavingCycleMemberRepository : IGenericRepository<SavingCycleMember>
    {
        Task<SavingCycleMember?> GetMemberWithVirtualAccountAsync(Guid memberId);
        Task<bool> DoesMemberExistInCycleAsync(Guid cycleId, Guid virtualAccountId);
        Task<int> GetHighestPayoutOrderAsync(Guid cycleId);
    }
}
