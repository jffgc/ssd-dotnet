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
}

public static class ValidationFilterExtensions
{
    public static IEndpointConventionBuilder AddValidationFilter<T>(this IEndpointConventionBuilder builder, IServiceProvider provider)
    {
        var validator = new ValidationFilterFactory().CreateValidator<T>(provider);

        if (validator is null)
        {
            return builder;
        }

        builder.AddEndpointFilter(new ValidationFilter<T>(validator));
        return builder;
    }
}

public sealed class ValidationFilter<T> : IEndpointFilter
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<T>().FirstOrDefault();
        if (request is null)
        {
            return await next(context);
        }

        var validationResult = await _validator.ValidateAsync(new ValidationContext<T>(request), context.HttpContext.RequestAborted);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        return await next(context);
    }
}
