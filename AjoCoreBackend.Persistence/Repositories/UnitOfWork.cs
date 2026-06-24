using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Persistence.Contexts;

namespace AjoCoreBackend.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AjoCoreDbContext _dbContext;
        private Hashtable? _repositories;

        private ISavingCycleRepository? _savingCycles;
        private ISavingCycleMemberRepository? _savingCycleMembers;
        private ILedgerRepository? _ledgers;

        public UnitOfWork(AjoCoreDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public ISavingCycleRepository SavingCycles => _savingCycles ??= new SavingCycleRepository(_dbContext);
        public ISavingCycleMemberRepository SavingCycleMembers => _savingCycleMembers ??= new SavingCycleMemberRepository(_dbContext);
        public ILedgerRepository Ledgers => _ledgers ??= new LedgerRepository(_dbContext);

        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            if (_repositories == null) _repositories = new Hashtable();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(GenericRepository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _dbContext);
                if (repositoryInstance != null)
                {
                    _repositories.Add(type, repositoryInstance);
                }
            }

            return (IGenericRepository<T>)_repositories[type]!;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
