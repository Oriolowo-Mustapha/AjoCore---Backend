using System;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AjoCoreBackend.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            var nombaBaseUrl = configuration["Nomba:BaseUrl"] ?? "https://sandbox.api.nomba.com";

            // Register Nomba API Clients
            services.AddHttpClient<INombaTokenService, NombaTokenService>(client =>
            {
                client.BaseAddress = new Uri(nombaBaseUrl);
            });

            services.AddHttpClient<INombaApiClient, NombaApiClient>(client =>
            {
                client.BaseAddress = new Uri(nombaBaseUrl);
            });

            // Register Internal Services
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddSingleton<IBankCodeService, BankCodeService>();
            services.AddScoped<IWebhookSignatureValidator, WebhookSignatureValidator>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            // Register Background Jobs
            services.AddHostedService<BackgroundJobs.LiquidationSweepService>();

            return services;
        }
    }
}

