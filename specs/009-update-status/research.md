# Investigación técnica: actualización del estado de una propiedad

## Decisiones

### Arquitectura del caso de uso

**Decisión**: Crear un slice `UpdatePropertyStatus` en `Features/Properties`, auto-descubierto mediante `ISlice`, con un handler `IHandler`, DTOs explícitos, mapping local y validator FluentValidation.

**Justificación**: Es el patrón requerido por la constitución y las instrucciones del backend. Los slices existentes mantienen endpoints delgados, registro automático y acceso a `AppDbContext` desde el handler cuando corresponde.

**Alternativas consideradas**: Agregar la operación al slice `UpdateProperty` existente, crear un controller o registrar el endpoint en `Program.cs`. Se descartan porque mezclan casos de uso o contradicen la arquitectura aprobada.

### Identificador de ruta

**Decisión**: Enlazar `{id}` como texto, validar el UUID canónico con `Guid.TryParseExact(value, "D", out _)` y convertirlo después de la validación.

**Justificación**: `GetPropertyByIdValidator` ya establece este comportamiento. Un constraint de ruta `:guid` no dejaría que el validator distinga un GUID no canónico con una respuesta 400.

**Alternativas consideradas**: Enlazar directamente a `Guid` o usar `{id:guid}`. Ambas pueden impedir que la feature devuelva el 400 de validación definido en la spec.

### Binding y validación del body

**Decisión**: Mantener un request específico del slice y un binding local que convierta errores de forma del body en datos de entrada validables. FluentValidation seguirá siendo el mecanismo que crea el `ValidationProblemDetails` 400.

**Justificación**: La documentación oficial de Minimal APIs indica que los errores de lectura del body no ejecutan endpoint filters; el JSON malformado produce 400 y un content type no JSON produce 415. Por ello, enlazar directamente un DTO convencional no garantiza el `400 ValidationProblemDetails` requerido para todos los cuerpos inválidos. El binding local permite cumplir la spec sin cambiar el comportamiento global de otros endpoints.

**Alternativas consideradas**: Usar el binding JSON convencional más `[JsonUnmappedMemberHandling(Disallow)]`, que rechaza miembros desconocidos, pero deja fallos de deserialización fuera del filtro automático; configurar `ThrowOnBadRequest`, `AddProblemDetails` o status code pages globalmente, que podría cambiar las respuestas de endpoints existentes; ignorar propiedades adicionales, lo que debilita el contrato de body único.

### Persistencia y respuesta

**Decisión**: El handler consulta la propiedad rastreada, asigna únicamente `Property.Status`, guarda con `SaveChangesAsync(cancellationToken)` y devuelve un resultado nullable. El endpoint mapea la ausencia a 404 ProblemDetails y el éxito a un DTO que contiene solo `id` y `status`.

**Justificación**: La consulta por clave primaria es suficiente para una mutación de una sola entidad. El slice `GetPropertyById` ya usa resultado nullable para representar ausencia. El mapping del `PUT` general no es reutilizable porque cambia campos adicionales e `UpdatedAt`, mientras que FR-009 exige preservar todo salvo `Status`.

**Alternativas consideradas**: Reutilizar `UpdatePropertyMapping`, añadir un repository o crear una nueva abstracción `Result<T>`. Se descartan: el mapping amplía el alcance, el repository solo envolvería EF Core y el patrón cercano ya expresa el not found con nullable. No se requiere transacción adicional ni migración.

### Logging y cancelación

**Decisión**: Registrar `PropertyId` y `Outcome` estructurados con los valores exactos `updated` y `notFound`; no registrar el body ni los datos descriptivos. Propagar el token de cancelación hasta EF Core.

**Justificación**: Cumple FR-020 y SC-007 y sigue el patrón de `GetPropertyByIdHandler`, adaptando los valores de resultado a la decisión aprobada para esta feature.

**Alternativas consideradas**: Registrar el body o el estado anterior/nuevo. Se descartan por minimizar exposición de datos y respetar la decisión de clarificación.

### Estrategia de pruebas

**Decisión**: Usar unit tests xUnit para parseo/binding del request, validator, mapping, mutación del handler, preservación de los demás campos, logging y cancelación. Completar la prueba del pipeline HTTP manualmente desde `RealtorApi.http` y `quickstart.md`.

**Justificación**: La constitución solo autoriza unit tests salvo que la spec apruebe explícitamente otras categorías. La spec exige prueba manual `.http`, pero no autoriza `WebApplicationFactory` ni Testcontainers. El binding y la validación se pueden probar por unidad; el archivo `.http` verifica los códigos HTTP reales.

**Alternativas consideradas**: Agregar pruebas de integración con `WebApplicationFactory` o PostgreSQL Testcontainers. No se incluyen porque la autorización explícita necesaria no está en la spec.

### Contrato OpenAPI

**Decisión**: Crear `contracts/backend-properties-status.openapi.yaml` como contrato de diseño en español. La implementación declara metadata para que el OpenAPI v1 runtime se genere desde el backend según la spec 008.

**Justificación**: La spec 009 requiere un contrato dentro de su carpeta, mientras que la spec 008 establece que el documento publicado del runtime se genera desde Minimal APIs y no se edita a mano.

**Alternativas consideradas**: Editar `wwwroot/openapi/v1.json` directamente. Se descarta porque introduce drift y contradice la spec 008.

## Fuentes consultadas

- `specs/003-properties-persistence-seeding/spec.md`: valores válidos del dominio `Available`, `Rented` y `Maintenance`.
- `specs/007-properties-get-by-id/`: patrón de validación de UUID canónico y ProblemDetails para not found.
- `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/`: handler, validator, logging y endpoint de referencia.
- `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/`: confirma que el mapping general cambia más campos e `UpdatedAt`, por lo que no se reutiliza.
- `app/backend/src/RealtorApi/Infrastructure/Validation/ValidationFilterFactory.cs`: el filtro automático valida parámetros ya enlazados y produce ValidationProblemDetails.
- `.github/instructions/backend.instructions.md` y `.github/instructions/database.instructions.md`: arquitectura, validación, logging, cancelación y ausencia de cambios persistentes.
- Microsoft Learn, [Parameter binding in Minimal API applications](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding?view=aspnetcore-11.0): errores de binding del body, content type y ejecución de filters.
- Microsoft Learn, [Handle errors in ASP.NET Core APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/handle-errors?view=aspnetcore-11.0): ProblemDetails y manejo de errores en Minimal APIs.