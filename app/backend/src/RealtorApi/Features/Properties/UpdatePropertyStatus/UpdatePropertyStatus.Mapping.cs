using RealtorApi.Domain.Properties;

namespace RealtorApi.Features.Properties.UpdatePropertyStatus;

public static class UpdatePropertyStatusMapping
{
    public static PropertyStatus ToPropertyStatus(this string status)
    {
        return status switch
        {
            "Available" => PropertyStatus.Available,
            "Rented" => PropertyStatus.Rented,
            "Maintenance" => PropertyStatus.Maintenance,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported property status.")
        };
    }

    public static UpdatePropertyStatusResponse ToUpdatePropertyStatusResponse(this Property property)
    {
        return new UpdatePropertyStatusResponse(property.Id, property.Status.ToString());
    }
}
