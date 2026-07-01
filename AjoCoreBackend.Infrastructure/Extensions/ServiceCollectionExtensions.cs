using System;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AjoCoreBackend.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            var nombaBaseUrl = configuration["Nomba:BaseUrl"] ?? "https://sandbox.api.nomba.com";

            // Register the Logging Handler
            services.AddTransient<LoggingDelegatingHandler>();

            // Register Nomba API Clients
            services.AddHttpClient<INombaTokenService, NombaTokenService>(client =>
            {
                client.BaseAddress = new Uri(nombaBaseUrl);
            })
            .AddHttpMessageHandler<LoggingDelegatingHandler>();

            services.AddHttpClient<INombaApiClient, NombaApiClient>(client =>
            {
                client.BaseAddress = new Uri(nombaBaseUrl);
                var accountId = configuration["Nomba:AccountId"];
                if (!string.IsNullOrEmpty(accountId))
                {
                    client.DefaultRequestHeaders.Add("accountId", accountId);
                }
            })
            .AddHttpMessageHandler<LoggingDelegatingHandler>();

            // Register Internal Services
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddSingleton<IBankCodeService, BankCodeService>();
            services.AddScoped<IWebhookSignatureValidator, WebhookSignatureValidator>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IHangfireBackGroundService, HangfireBackgroundService>();
            // Configure Email Settings
            services.Configure<AjoCoreBackend.Application.DTOs.Email.EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddScoped<IEmailService, SmtpEmailService>();

            // Register Background Jobs as Transient for Hangfire
            services.AddTransient<BackgroundJobs.LiquidationSweepService>();
            services.AddTransient<BackgroundJobs.SavingReminderService>();
            services.AddTransient<IReversalProcessingService, ReversalProcessingService>();

            services.AddHangfire(config =>
            {
                config.UsePostgreSqlStorage(options => 
                {
                    options.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
                });
            });

            services.AddHangfireServer();

            return services;
        }
    }
}

