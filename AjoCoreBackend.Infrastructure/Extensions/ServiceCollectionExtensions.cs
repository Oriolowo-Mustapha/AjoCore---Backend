using System;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AjoCoreBackend.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExternalServices(this IServiceCollection services)
        {
            // Register Nomba API Clients
            services.AddHttpClient<INombaTokenService, NombaTokenService>(client =>
            {
                client.BaseAddress = new Uri("https://api.nomba.com");
            });

            services.AddHttpClient<INombaApiClient, NombaApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.nomba.com");
            });

            // Register Internal Services
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddSingleton<IBankCodeService, BankCodeService>();
            services.AddScoped<IWebhookSignatureValidator, WebhookSignatureValidator>();

            // Register Background Jobs
            services.AddHostedService<BackgroundJobs.LiquidationSweepService>();

            return services;
        }
    }
}
