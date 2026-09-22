# Contratos de infraestructura del backend

## Contrato principal: ISlice

```csharp
public interface ISlice
{
    void AddEndpoint(IEndpointRouteBuilder app);
}
```

**Propósito**: permitir que cada endpoint se registre en la aplicación sin cambiar `Program.cs`.

## Contrato de handler: IHandler

```csharp
public interface IHandler { }
```

**Propósito**: marcar handlers para auto-registro por reflection.

## Contrato de error esperado: Result

```csharp
public sealed record Result(bool IsSuccess, string? ErrorCode = null, string? Message = null);
```

**Propósito**: encapsular errores esperados de la aplicación y convertirlos a `ProblemDetails` de forma centralizada.

## Contrato de validación

- Si existe `IValidator<T>`, la validación se ejecuta automáticamente.
- Si no existe, el request pasa sin error de validación.
- Si falla la validación, la respuesta es `ValidationProblemDetails` con status `400 Bad Request`.

## Contrato de salud

```json
{
  "status": "Healthy"
}
```

**Propósito**: demostrar que el end-to-end del backend funciona con el mecanismo de descubrimiento centralizado de ISlice.
