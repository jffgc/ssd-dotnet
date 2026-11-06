namespace RealtorApi.Features.Properties.GetPropertyById;

public static class GetPropertyByIdMapping
{
    public static GetPropertyByIdQuery ToQuery(this GetPropertyByIdRequest request)
    {
        return new GetPropertyByIdQuery(Guid.ParseExact(request.Id, "D"));
    }

    public static GetPropertyByIdResponse ToResponse(
        this GetPropertyByIdData data,
        string scheme,
        HostString host)
    {
        return new GetPropertyByIdResponse(
            data.Id,
            data.Title,
            data.Description,
            data.Address,
            data.Price,
            data.Status.ToString(),
            data.BedroomCount,
            data.BathroomCount,
            data.AreaSquareMeters,
            data.ImageUrl is null
                ? null
                : PropertyImageUrlBuilder.BuildAbsoluteUrl(data.ImageUrl, scheme, host));
    }
}