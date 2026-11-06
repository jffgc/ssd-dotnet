namespace RealtorApi.Features.Properties.ListProperties;

public static class ListPropertiesMapping
{
    public static ListPropertiesQuery ToQuery(this ListPropertiesRequest request)
    {
        var page = request.Page is null ? 1 : int.Parse(request.Page, System.Globalization.CultureInfo.InvariantCulture);
        var pageSize = request.PageSize is null ? 6 : int.Parse(request.PageSize, System.Globalization.CultureInfo.InvariantCulture);

        return new ListPropertiesQuery(page, pageSize);
    }

    public static ListPropertiesResponse ToResponse(
        this ListPropertiesQueryResult result,
        string scheme,
        HostString host)
    {
        var items = result.Items
            .Select(item => new ListPropertyResponse(
                item.Id,
                item.Title,
                item.Description,
                item.Address,
                item.Price,
                item.Status.ToString(),
                item.BedroomCount,
                item.BathroomCount,
                item.AreaSquareMeters,
                PropertyImageUrlBuilder.BuildAbsoluteUrl(item.ImageUrl, scheme, host)))
            .ToList();
        var totalPages = result.TotalItems == 0
            ? 0
            : result.TotalItems / result.PageSize + (result.TotalItems % result.PageSize == 0 ? 0 : 1);

        return new ListPropertiesResponse(
            items,
            result.Page,
            result.PageSize,
            result.TotalItems,
            totalPages,
            result.Page < totalPages,
            result.Page > 1 && result.TotalItems > 0);
    }

}
