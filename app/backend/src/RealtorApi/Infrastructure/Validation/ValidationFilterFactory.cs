using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace RealtorApi.Infrastructure.Validation;

public sealed class ValidationFilterFactory
{
    public IValidator<T>? CreateValidator<T>(IServiceProvider provider)
    {
        return provider.GetService<IValidator<T>>();
    }

    public static EndpointFilterDelegate Create(
        EndpointFilterFactoryContext context,
        EndpointFilterDelegate next)
    {
        var serviceChecker = context.ApplicationServices.GetRequiredService<IServiceProviderIsService>();
        var filterTypes = context.MethodInfo
            .GetParameters()
            .Select(parameter => parameter.ParameterType)
            .Distinct()
            .Where(parameterType =>
                serviceChecker.IsService(typeof(IValidator<>).MakeGenericType(parameterType)))
            .Select(parameterType => (IEndpointFilter)Activator.CreateInstance(
                typeof(ValidationFilter<>).MakeGenericType(parameterType))!)
            .ToArray();

        if (filterTypes.Length == 0)
        {
            return next;
        }

        foreach (var filter in filterTypes.Reverse())
        {
            var following = next;
            next = invocationContext => filter.InvokeAsync(invocationContext, following);
        }

        return next;
    }
}

public sealed class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<T>().FirstOrDefault();
        if (request is null)
        {
            return await next(context);
        }

        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null)
        {
            return await next(context);
        }

        var validationResult = await validator.ValidateAsync(
            new ValidationContext<T>(request),
            context.HttpContext.RequestAborted);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        return await next(context);
    }
}
