using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace RealtorApi.Infrastructure.Endpoints;

public interface ISlice
{
    void AddEndpoint(IEndpointRouteBuilder app);
}

public static class SliceRegistrationExtensions
{
    public static IServiceCollection RegisterSlices(this IServiceCollection services, Assembly assembly)
    {
        var sliceTypes = assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(ISlice).IsAssignableFrom(type))
            .Distinct()
            .ToList();

        foreach (var type in sliceTypes)
        {
            if (services.Any(service => service.ServiceType == typeof(ISlice) && service.ImplementationType == type))
            {
                continue;
            }

            services.AddSingleton(typeof(ISlice), type);
        }

        return services;
    }

    public static IEndpointRouteBuilder MapSliceEndpoints(this IEndpointRouteBuilder app)
    {
        var slices = app.ServiceProvider.GetServices<ISlice>();

        foreach (var slice in slices)
        {
            slice.AddEndpoint(app);
        }

        return app;
    }
}
