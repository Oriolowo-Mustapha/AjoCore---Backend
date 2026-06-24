using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AjoCoreBackend.Persistence.Contexts
{
    public class AjoCoreDbContextFactory : IDesignTimeDbContextFactory<AjoCoreDbContext>
    {
        public AjoCoreDbContext CreateDbContext(string[] args)
        {
            // Build configuration by pointing to the API project where appsettings.json lives
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../AjoCoreBackend.API");
            
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Create options builder
            var builder = new DbContextOptionsBuilder<AjoCoreDbContext>();
            
            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("The connection string 'DefaultConnection' was not found in the AjoCoreBackend.API appsettings.");
            }
            
            // Configure DbContext with PostgreSQL
            builder.UseNpgsql(connectionString, b => b.MigrationsAssembly("AjoCoreBackend.Persistence"));

            return new AjoCoreDbContext(builder.Options);
        }
    }
}
