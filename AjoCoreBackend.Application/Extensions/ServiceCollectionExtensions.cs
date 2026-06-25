using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AjoCoreBackend.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Register MediatR handlers from this assembly
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(Behaviors.ValidationBehavior<,>));
            });

            // Register all FluentValidation validators from this assembly
            services.AddValidatorsFromAssembly(assembly);

            // Register AutoMapper profiles from this assembly
            services.AddAutoMapper(cfg => cfg.AddMaps(assembly));

            return services;
        }
    }
}
