using FluentValidation;

namespace RealtorApi.Features.Properties.UpdatePropertyStatus;

public sealed class UpdatePropertyStatusValidator : AbstractValidator<UpdatePropertyStatusRequest>
{
    private static readonly string[] AllowedStatuses = ["Available", "Rented", "Maintenance"];

    public UpdatePropertyStatusValidator()
    {
        RuleFor(request => request.Id)
            .Must(IsCanonicalGuid)
            .WithMessage("El id debe usar el formato UUID canónico con guiones.")
            .OverridePropertyName("id");

        RuleFor(request => request.Status)
            .Must(status => status is not null && AllowedStatuses.Contains(status, StringComparer.Ordinal))
            .WithMessage("El estado debe ser Available, Rented o Maintenance.")
            .OverridePropertyName("status");

        RuleFor(request => request.BindingErrors)
            .Custom((bindingErrors, context) =>
            {
                foreach (var bindingError in bindingErrors)
                {
                    foreach (var message in bindingError.Value)
                    {
                        context.AddFailure(bindingError.Key, message);
                    }
                }
            });
    }

    private static bool IsCanonicalGuid(string? value)
    {
        return Guid.TryParseExact(value, "D", out _);
    }
}
