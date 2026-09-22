using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace RealtorApi.Infrastructure.Handlers;

public interface IHandler
{
}

public static class HandlerRegistrationExtensions
{
    public static IServiceCollection RegisterHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IHandler).IsAssignableFrom(type))
            .Distinct();

        foreach (var type in handlerTypes)
        {
            if (services.Any(service => service.ServiceType == type))
            {
                continue;
            }

            services.AddScoped(type);
        }

        return services;
    }
}
