using System.Globalization;
using FluentValidation;

namespace RealtorApi.Features.Properties.ListProperties;

public sealed class ListPropertiesValidator : AbstractValidator<ListPropertiesRequest>
{
    public ListPropertiesValidator()
    {
        RuleFor(request => request.Page)
            .Must(IsPositiveInt32OrOmitted)
            .WithMessage("Page must be a positive 32-bit integer.")
            .OverridePropertyName("page");

        RuleFor(request => request.PageSize)
            .Must(IsValidPageSizeOrOmitted)
            .WithMessage("Page size must be a positive integer no greater than 100.")
            .OverridePropertyName("pageSize");
    }

    private static bool IsPositiveInt32OrOmitted(string? value)
    {
        return value is null
            || (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                && parsed > 0);
    }

    private static bool IsValidPageSizeOrOmitted(string? value)
    {
        return value is null
            || (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                && parsed is > 0 and <= 100);
    }
}
