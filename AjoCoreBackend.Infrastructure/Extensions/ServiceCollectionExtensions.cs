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
            // Register Nomba API Client
            // Note: In production, configure BaseAddress and Polly retry policies here
            services.AddHttpClient<INombaApiClient, NombaApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.nomba.com");
            });

            // Register Internal Services
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<IWebhookSignatureValidator, WebhookSignatureValidator>();

            return services;
        }
    }
}
