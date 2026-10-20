using Microsoft.AspNetCore.Mvc;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Features.Properties.ListProperties;

/// <summary>Raw query-string values for a paginated property listing.</summary>
public sealed record ListPropertiesRequest
{
    /// <summary>The one-based page number, defaulting to 1 when omitted.</summary>
    [FromQuery(Name = "page")]
    public string? Page { get; init; }

    /// <summary>The maximum number of properties to return, defaulting to 6 when omitted.</summary>
    [FromQuery(Name = "pageSize")]
    public string? PageSize { get; init; }
}

/// <summary>Normalized pagination values used by the listing handler.</summary>
public sealed record ListPropertiesQuery(int Page, int PageSize);

/// <summary>A projected property record used to assemble the public listing response.</summary>
public sealed record ListPropertyData(
    Guid Id,
    string Title,
    string Description,
    string Address,
    decimal Price,
    PropertyStatus Status,
    int BedroomCount,
    int BathroomCount,
    decimal AreaSquareMeters,
    string ImageUrl);

/// <summary>Properties and count returned by the database query before HTTP mapping.</summary>
public sealed record ListPropertiesQueryResult(
    IReadOnlyList<ListPropertyData> Items,
    int Page,
    int PageSize,
    long TotalItems);

/// <summary>A property in the paginated public listing.</summary>
public sealed record ListPropertyResponse(
    Guid Id,
    string Title,
    string Description,
    string Address,
    decimal Price,
    string Status,
    int BedroomCount,
    int BathroomCount,
    decimal AreaSquareMeters,
    string ImageUrl);

/// <summary>A page of properties and the metadata needed to navigate the catalog.</summary>
public sealed record ListPropertiesResponse(
    IReadOnlyList<ListPropertyResponse> Items,
    int Page,
    int PageSize,
    long TotalItems,
    long TotalPages,
    bool HasNext,
    bool HasPrevious);
