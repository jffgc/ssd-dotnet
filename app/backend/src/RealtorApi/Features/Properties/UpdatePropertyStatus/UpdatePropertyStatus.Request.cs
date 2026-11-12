using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Features.Properties.UpdatePropertyStatus;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record UpdatePropertyStatusBody
{
    public required UpdatePropertyStatusValue Status { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter<UpdatePropertyStatusValue>))]
public enum UpdatePropertyStatusValue
{
    Available,
    Rented,
    Maintenance
}

public sealed record UpdatePropertyStatusRequest(
    string? Id,
    string? Status,
    IReadOnlyDictionary<string, string[]> BindingErrors)
{
    public static async ValueTask<UpdatePropertyStatusRequest?> BindAsync(
        HttpContext context,
        ParameterInfo parameter)
    {
        var bindingErrors = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var id = context.Request.RouteValues.TryGetValue("id", out var routeValue)
            ? routeValue?.ToString()
            : null;

        if (string.IsNullOrWhiteSpace(id))
        {
            AddError(bindingErrors, "id", "El id es obligatorio.");
        }

        string? status = null;
        if (!context.Request.HasJsonContentType())
        {
            AddError(bindingErrors, "body", "El contenido debe ser JSON.");
        }
        else
        {
            try
            {
                using var document = await JsonDocument.ParseAsync(
                    context.Request.Body,
                    cancellationToken: context.RequestAborted);

                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    AddError(bindingErrors, "body", "El body debe ser un objeto JSON.");
                }
                else
                {
                    var statusFound = false;
                    foreach (var property in document.RootElement.EnumerateObject())
                    {
                        if (!string.Equals(property.Name, "status", StringComparison.Ordinal))
                        {
                            AddError(bindingErrors, "body", "El body solo puede contener la propiedad status.");
                            continue;
                        }

                        if (statusFound)
                        {
                            AddError(bindingErrors, "status", "La propiedad status no puede repetirse.");
                            continue;
                        }

                        statusFound = true;
                        if (property.Value.ValueKind != JsonValueKind.String)
                        {
                            AddError(bindingErrors, "status", "El estado debe ser una cadena.");
                            continue;
                        }

                        status = property.Value.GetString();
                    }

                    if (!statusFound)
                    {
                        AddError(bindingErrors, "status", "El estado es obligatorio.");
                    }
                }
            }
            catch (JsonException)
            {
                AddError(bindingErrors, "body", "El body no contiene JSON válido.");
            }
        }

        return new UpdatePropertyStatusRequest(
            id,
            status,
            bindingErrors.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.ToArray(),
                StringComparer.Ordinal));
    }

    private static void AddError(IDictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var messages))
        {
            messages = [];
            errors.Add(key, messages);
        }

        messages.Add(message);
    }
}
