# Plan de implementación: Actualización del estado de una propiedad

**Rama**: `009-update-status` | **Fecha**: 2026-11-11 | **Spec**: [spec.md](spec.md)

**Entrada**: Especificación de la funcionalidad en `specs/009-update-status/spec.md`.

## Resumen

Agregar `PATCH /api/properties/{id}/status` como slice independiente bajo `Features/Properties/UpdatePropertyStatus`. El request admite únicamente `status`; el handler busca la propiedad por GUID, asigna solo `Property.Status`, persiste el cambio y emite logging estructurado con el identificador y el resultado. La respuesta exitosa contiene únicamente `id` y `status`; los errores esperados mantienen `400` con `ValidationProblemDetails` y `404` con `ProblemDetails`. No se modifica el modelo persistente ni ningún endpoint existente.

Para mantener el formato `400 ValidationProblemDetails` también en errores de lectura del body, el request del slice normalizará los errores de binding localmente antes de que el filtro automático de FluentValidation lo valide. Esto evita cambiar la configuración global de Minimal APIs.

## Contexto técnico

**Lenguaje/versión**: C# con SDK .NET 11 fijado en `global.json`.

**Dependencias principales**: ASP.NET Core Minimal APIs, EF Core, Npgsql, PostgreSQL, FluentValidation, System.Text.Json y `ILogger<T>`.

**Almacenamiento**: PostgreSQL existente mediante `AppDbContext`; sin cambio de esquema ni migración.

**Pruebas**: xUnit para request binding, validators, mapping y handler; validación manual del pipeline HTTP desde `RealtorApi.http`. No se planifican pruebas de integración con `WebApplicationFactory` ni Testcontainers.

**Plataforma destino**: API web backend ASP.NET Core.

**Tipo de proyecto**: Servicio web dentro de la solución Realtor existente.

**Objetivo de rendimiento**: La spec no define un SLO cuantitativo. El caso de uso realiza una búsqueda por clave primaria y una escritura de una propiedad.

**Restricciones**: El único campo persistido que cambia es `Status`, incluidos `CreatedAt`, `UpdatedAt` e `ImageUrl`, que deben mantenerse intactos. Solo se admiten `Available`, `Rented` y `Maintenance`. Respuesta de éxito exacta con `id` y `status`; no autenticación, archivos ni otros campos. No se cambia la semántica de endpoints existentes.

**Escala/alcance**: Una propiedad por solicitud; sin operaciones por lote, nuevas transiciones de dominio ni cambios de datos o esquema.

## Revisión de constitución

**Puerta inicial**: Aprobada antes de la investigación.

- La funcionalidad permanece dentro de la solución compartida y de la feature `Properties`.
- Se utiliza Vertical Slice con Minimal API, `ISlice` e `IHandler`; no se agregan controllers, proyectos ni registros manuales en `Program.cs`.
- Se reutilizan .NET 11, EF Core, Npgsql, PostgreSQL, FluentValidation, ProblemDetails y logging estructurado.
- No se modifica la persistencia ni se genera migración.
- Las pruebas automatizadas se limitan a unit tests; los casos HTTP completos se verifican manualmente mediante `.http`, conforme a la constitución.
- Los documentos de la feature se redactan en español.

**Puerta posterior al diseño**: Aprobada. El diseño mantiene las mismas restricciones, especifica los contratos y no introduce desviaciones constitucionales.

## Decisiones de diseño

1. El segmento `id` se enlaza como texto y se valida con `Guid.TryParseExact(..., "D")`, siguiendo `GetPropertyById`; solo después de validar se convierte a `Guid`. No se usa un constraint `:guid`, que impediría que el validator responda `400` para identificadores no canónicos.
2. El DTO público `UpdatePropertyStatusBody` representa exclusivamente el body JSON y contiene solo el campo `Status`. Se mantiene separado del objeto local de binding, que reúne el identificador de ruta y los errores de lectura del request. El binder local detecta body vacío, JSON malformado, media type no JSON, tipo incorrecto, claves desconocidas y duplicados, y los presenta al validator como errores de entrada. El endpoint declara `.Accepts<UpdatePropertyStatusBody>("application/json")` para que el contrato generado incluya el body y su schema. El filtro existente produce `ValidationProblemDetails` HTTP 400 sin modificar las opciones globales ni `Program.cs`.
3. El handler consulta la entidad rastreada y modifica únicamente `Status`; no reutiliza `UpdatePropertyMapping`, que altera más campos e `UpdatedAt`. La ausencia se representa con resultado nullable y el endpoint la traduce a `ProblemDetails` 404, siguiendo el slice de detalle existente.
4. El response mapping crea un DTO con `Id` y el nombre canónico de `Status`; la entidad EF no se expone.
5. El handler registra `PropertyId` y `Outcome` (`updated` o `notFound`) sin datos del body o de la propiedad. Todas las operaciones de persistencia propagan `CancellationToken`.
6. Se agregan unit tests de binder/request, validator, mapping y handler. La prueba manual en `.http` valida el status HTTP, el cuerpo de respuesta y la preservación del detalle y de `imageUrl` mediante una consulta posterior.
7. El YAML bajo `contracts/` documenta el contrato de diseño de la feature. La implementación declara metadata de request y response para que el OpenAPI v1 runtime incluya el body JSON, el enum y las respuestas desde el backend según la spec 008; no se edita manualmente.

## Estructura del proyecto

### Documentación de la feature

```text
specs/009-update-status/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── backend-properties-status.openapi.yaml
└── tasks.md (se genera en la fase speckit.tasks)
```

### Código fuente y pruebas

```text
app/backend/src/RealtorApi/Features/Properties/UpdatePropertyStatus/
├── UpdatePropertyStatus.Endpoint.cs
├── UpdatePropertyStatus.Handler.cs
├── UpdatePropertyStatus.Mapping.cs
├── UpdatePropertyStatus.Request.cs
├── UpdatePropertyStatus.Response.cs
└── UpdatePropertyStatus.Validator.cs

app/backend/tests/RealtorApiTests/Features/Properties/UpdatePropertyStatus/
├── UpdatePropertyStatusHandlerTests.cs
├── UpdatePropertyStatusMappingTests.cs
├── UpdatePropertyStatusRequestTests.cs
└── UpdatePropertyStatusValidatorTests.cs

app/backend/src/RealtorApi/RealtorApi.http
```

**Decisión de estructura**: El caso de uso y sus contratos permanecen juntos en un slice bajo `Features/Properties`; las pruebas se agrupan por feature. El archivo `.http` existente recibe escenarios manuales. No se agregan capas, proyectos ni archivos de infraestructura compartida.
