# Plan de implementación: Consulta de propiedad por identificador

**Rama**: `007-properties-get-by-id` | **Fecha**: 2026-11-05 | **Spec**: [spec.md](./spec.md)

**Entrada**: Especificación en `specs/007-properties-get-by-id/spec.md`

## Resumen

Agregar `GET /api/properties/{id}` como un slice Minimal API auto-descubierto en `Features/Properties/GetPropertyById`. El endpoint recibirá el identificador de ruta como texto para que el validator aplique el formato UUID canónico con guiones y devuelva 400 para otros formatos. El handler consultará `AppDbContext` sin tracking, proyectará los campos públicos a un contrato explícito y devolverá un resultado nulo cuando no exista la propiedad; el endpoint convertirá ese resultado a 404 con ProblemDetails. El mapping mantendrá `imageUrl` en `null` cuando no exista imagen y, si existe, formará la URL absoluta con el esquema y host de la solicitud.

Para mantener idéntica la generación segura de URL de imágenes entre el listado 004 y el detalle, se extraerá el helper privado actual de `ListPropertiesMapping` a un helper específico del módulo de propiedades compartido por ambos mappings. No se modificarán datos persistentes ni la ruta estática existente.

## Contexto técnico

**Lenguaje/versión**: C# con .NET 11; `global.json` fija SDK `11.0.100-preview.7.26381.103`.

**Dependencias principales**: ASP.NET Core Minimal APIs, EF Core 10.0.4, Npgsql EF Core 10.0.1, FluentValidation.AspNetCore 11.3.0 y los contratos locales `ISlice`/`IHandler`. No se agregan ni actualizan paquetes.

**Almacenamiento**: PostgreSQL mediante `AppDbContext`. Se reutiliza `Property.Id` (`Guid`) y sus campos actuales; no hay migraciones.

**Pruebas**: xUnit 2.9.3 y EF Core InMemory. Las pruebas automatizadas serán unitarias conforme a la constitución: validator, handler, mapping y logging. La verificación del binding HTTP, códigos 400/404 y respuestas públicas se documenta para ejecución manual en `quickstart.md` y en `RealtorApi.http`; no se agrega infraestructura de integración sin aprobación explícita.

**Plataforma objetivo**: Servicio web ASP.NET Core del backend RealtorApi.

**Tipo de proyecto**: API web backend dentro de la solución Realtor compartida.

**Metas de rendimiento**: La spec no fija un umbral cuantitativo. La operación hace una búsqueda unitaria por clave primaria, sin conteos ni paginación; no se introduce un objetivo de latencia no aprobado.

**Restricciones**: Sin controllers, cambios de esquema, migraciones, dependencias nuevas, autenticación nueva ni cambios de frontend. Se exige UUID canónico `D` y HTTP 400 para otras formas; GUID canónico sin registro devuelve 404. La respuesta contiene solo los diez campos públicos, sin metadatos de paginación. Se propaga `CancellationToken`, se registra estructuradamente `id` y `found`/`notFound`, y `imageUrl` debe ser `null` o una URL absoluta pública con el esquema/host actuales. La aplicación ya mapea `/assets/properties` y no configura forwarded headers; esta feature no altera esa configuración.

**Escala/alcance**: Una propiedad por solicitud; el tamaño del catálogo y la carga concurrente no están especificados. No se añaden capacidades de búsqueda, filtros ni operaciones de escritura.

## Comprobación constitucional previa a investigación

| Comprobación | Estado | Evidencia/diseño |
|---|---|---|
| Spec-Driven y límites de fase | Pasa para planificación | La spec 007 existe y fue aclarada; esta fase no implementa código. La implementación queda condicionada a la aprobación de spec y a tareas aprobadas. |
| Arquitectura canónica | Pasa | Slice por caso de uso con Minimal API, `ISlice`, `IHandler`, mapping y validator; no se introducen controllers ni registros manuales. |
| Stack obligatorio | Pasa | Se reutilizan .NET 11, EF Core, Npgsql, PostgreSQL, FluentValidation y ProblemDetails del repositorio. |
| Validación y errores | Pasa | El request conserva el id como texto y su validator existente en el flujo de slices responde 400; el GUID no encontrado se mapea a ProblemDetails 404. |
| Contratos independientes del dominio | Pasa | Se proyecta a records del slice y la respuesta no serializa entidades EF. |
| Pruebas y evidencia | Pasa con alcance acotado | Se planifican unit tests y pasos HTTP manuales. No se autorizan pruebas de integración automatizadas. |
| Persistencia y alcance | Pasa | Consulta de solo lectura; sin cambios de entidad, esquema, migración, seed ni frontend. |

**Resultado previo a investigación**: No hay gates que bloqueen el diseño. La spec mantiene el estado documental que tiene actualmente; este plan no autoriza por sí solo la implementación.

## Investigación y decisiones de diseño

Las decisiones, su racional y las alternativas consideradas están en [research.md](./research.md). Las principales son:

- Recibir `{id}` como texto y validarlo con `Guid.TryParseExact(id, "D", ...)`; no usar `:guid`, ya que la regla pública debe producir 400 y no un 404 por ruta no coincidente.
- Consultar por clave con `AsNoTracking` y proyección directa; propagar el token de cancelación a la operación EF asíncrona.
- Devolver `null` desde el handler para ausencia y construir `ProblemDetails` 404 en el endpoint, siguiendo el patrón existente de `UpdateProperty`.
- Extraer la construcción segura de imagen a un helper del módulo `Features/Properties`, evitando dependencia del detalle hacia el slice de listado.
- Mantener pruebas automatizadas unitarias y comprobar el comportamiento HTTP end-to-end manualmente con los ejemplos existentes.

## Modelo de datos

El caso de uso reutiliza el modelo persistente existente y no agrega entidades ni columnas. La entrada, la proyección y la respuesta se describen en [data-model.md](./data-model.md). `imageUrl` es un campo presente nullable; los metadatos de paginación de 004 no pertenecen a la respuesta de detalle.

## Contrato externo y validación manual

El contrato de `GET /api/properties/{id}` está en [contracts/backend-properties-get-by-id.openapi.yaml](./contracts/backend-properties-get-by-id.openapi.yaml). Incluye el patrón UUID canónico y las respuestas 200, 400 y 404. Los escenarios manuales para existente, no encontrado, formato no canónico, ausencia de imagen y esquema/host variables están en [quickstart.md](./quickstart.md); se reflejarán también en `app/backend/src/RealtorApi/RealtorApi.http` conforme a la convención del listado.

## Estructura del proyecto

### Documentación de esta funcionalidad

```text
specs/007-properties-get-by-id/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── backend-properties-get-by-id.openapi.yaml
└── tasks.md  # Se genera en la fase /speckit.tasks
```

### Código fuente y pruebas previstos

```text
app/backend/src/RealtorApi/
├── Features/Properties/
│   ├── GetPropertyById/
│   │   ├── GetPropertyByIdContracts.cs
│   │   ├── GetPropertyByIdSlice.cs
│   │   ├── GetPropertyByIdHandler.cs
│   │   ├── GetPropertyByIdMapping.cs
│   │   └── GetPropertyByIdValidator.cs
│   ├── PropertyImageUrlBuilder.cs  # Helper compartido nuevo
│   └── ListProperties/
│       └── ListPropertiesMapping.cs  # Delegará la URL al helper conservando su contrato
└── RealtorApi.http  # Ejemplos manuales del detalle

app/backend/tests/RealtorApiTests/
└── Features/Properties/
    ├── PropertyImageUrlBuilderTests.cs
    └── GetPropertyById/
        ├── GetPropertyByIdValidatorTests.cs
        ├── GetPropertyByIdHandlerTests.cs
        └── GetPropertyByIdMappingTests.cs
```

**Decisión estructural**: Mantener el endpoint, handler, request/response, mapping y validator en un slice autónomo. El helper compartido contiene únicamente la conversión segura de la ruta de imagen a URL pública; ambos mappings mantienen sus DTO y contratos. El descubrimiento existente registra automáticamente los slices, handlers y validators.

## Comprobación constitucional posterior al diseño

| Comprobación | Estado | Evidencia |
|---|---|---|
| Contrato de la spec preservado | Pasa | La salida tiene los diez campos requeridos, `imageUrl` nullable, respuestas diferenciadas 400/404 y ninguna paginación. |
| Arquitectura Vertical Slice | Pasa | Las piezas del caso de uso quedan en `GetPropertyById`; helper compartido limitado a URL de imagen del módulo de propiedades. |
| Validación | Pasa | Request de ruta textual validado antes del handler; la documentación y el OpenAPI especifican UUID canónico. |
| Persistencia y cancelación | Pasa | Lectura proyectada, sin tracking, sin mutaciones y con `CancellationToken`. |
| Logging | Pasa | El handler registra el GUID y `found`/`notFound`, sin datos descriptivos. |
| Pruebas | Pasa | Unit tests y validación HTTP manual; no se agregan pruebas de integración no aprobadas. |
| Compatibilidad con 004 | Pasa | El helper compartido conserva las reglas de nombre seguro y URL absoluta del listado; sus pruebas existentes deben seguir pasando. |
| Trazabilidad | Pasa | Validator, handler, mapping, logging y escenarios manuales cubren los FR/SC de la spec 007. |

**Resultado posterior al diseño**: Gates aprobados; no se requieren excepciones constitucionales. La implementación requiere aprobación explícita de la spec y la fase de tareas.

## Registro de complejidad

No hay violaciones constitucionales. El helper compartido está justificado porque el listado y el detalle deben aplicar la misma normalización segura de imagen y hoy esa lógica existe como método privado del listado.
