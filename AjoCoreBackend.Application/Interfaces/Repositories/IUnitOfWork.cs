using System;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Domain.Entities;

namespace AjoCoreBackend.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ISavingCycleRepository SavingCycles { get; }
        ISavingCycleMemberRepository SavingCycleMembers { get; }
        ILedgerRepository Ledgers { get; }
        
        IGenericRepository<T> Repository<T>() where T : BaseEntity;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
