using Microsoft.AspNetCore.Mvc;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Features.Properties.GetPropertyById;

/// <summary>Route values for a property detail request.</summary>
public sealed record GetPropertyByIdRequest
{
    /// <summary>The canonical GUID identifier of the property.</summary>
    [FromRoute(Name = "id")]
    public string Id { get; init; } = string.Empty;
}

/// <summary>Normalized identifier used to query one property.</summary>
public sealed record GetPropertyByIdQuery(Guid Id);

/// <summary>Projected property fields used to build the public detail response.</summary>
public sealed record GetPropertyByIdData(
    Guid Id,
    string Title,
    string Description,
    string Address,
    decimal Price,
    PropertyStatus Status,
    int BedroomCount,
    int BathroomCount,
    decimal AreaSquareMeters,
    string? ImageUrl);

/// <summary>Public response containing the details of one property.</summary>
public sealed record GetPropertyByIdResponse(
    Guid Id,
    string Title,
    string Description,
    string Address,
    decimal Price,
    string Status,
    int BedroomCount,
    int BathroomCount,
    decimal AreaSquareMeters,
    string? ImageUrl);