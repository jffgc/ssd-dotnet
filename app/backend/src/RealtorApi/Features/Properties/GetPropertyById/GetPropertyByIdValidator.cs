using FluentValidation;

namespace RealtorApi.Features.Properties.GetPropertyById;

public sealed class GetPropertyByIdValidator : AbstractValidator<GetPropertyByIdRequest>
{
    public GetPropertyByIdValidator()
    {
        RuleFor(request => request.Id)
            .Must(IsCanonicalGuid)
            .WithMessage("The id must use the canonical GUID format with hyphens.")
            .OverridePropertyName("id");
    }

    private static bool IsCanonicalGuid(string? value)
    {
        return Guid.TryParseExact(value, "D", out _);
    }
}