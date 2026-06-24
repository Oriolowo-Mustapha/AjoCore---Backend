using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Persistence.Contexts;
using AjoCoreBackend.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AjoCoreBackend.Persistence.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<AjoCoreDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(AjoCoreDbContext).Assembly.FullName)));

            // Register Generic Repository and Unit of Work
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Bounded Repositories
            services.AddScoped<ISavingCycleRepository, SavingCycleRepository>();
            services.AddScoped<ISavingCycleMemberRepository, SavingCycleMemberRepository>();
            services.AddScoped<ILedgerRepository, LedgerRepository>();

            return services;
        }
    }
}
