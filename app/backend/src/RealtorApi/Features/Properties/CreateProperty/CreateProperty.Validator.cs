using FluentValidation;

namespace RealtorApi.Features.Properties.CreateProperty;

public sealed class CreatePropertyValidator : AbstractValidator<CreatePropertyRequest>
{
    private const long MaxImageSizeInBytes = 5 * 1024 * 1024;

    public CreatePropertyValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.Price)
            .GreaterThan(0m);

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.BedroomCount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.BathroomCount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AreaSquareMeters)
            .GreaterThan(0m);

        RuleFor(x => x.Image)
            .Custom((image, context) =>
            {
                if (image is null)
                {
                    return;
                }

                if (image.Length == 0)
                {
                    context.AddFailure(nameof(CreatePropertyRequest.Image), "The uploaded file is empty.");
                    return;
                }

                if (image.Length > MaxImageSizeInBytes)
                {
                    context.AddFailure(nameof(CreatePropertyRequest.Image), "The uploaded file exceeds the maximum allowed size of 5MB.");
                    return;
                }

                var extension = Path.GetExtension(image.FileName);
                var normalizedExtension = extension.TrimStart('.').ToLowerInvariant();
                if (normalizedExtension is not ("png" or "jpg" or "jpeg"))
                {
                    context.AddFailure(nameof(CreatePropertyRequest.Image), "Only PNG, JPG and JPEG files are allowed.");
                }
            });
    }
}
