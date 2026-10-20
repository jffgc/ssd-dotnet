# Plan de implementación: Listado paginado de propiedades

**Rama**: `004-properties-list-pagination` | **Fecha**: 2026-10-18 | **Spec**: [spec.md](./spec.md)

**Entrada**: Especificación de funcionalidad en `specs/004-properties-list-pagination/spec.md`

## Resumen

Agregar el caso de uso `GET /api/properties` como un slice Minimal API auto-descubierto dentro de `Features/Properties/ListProperties`. El handler consultará `AppDbContext` con una proyección `AsNoTracking`, aplicará el orden por título, paginación y conteo, y devolverá un contrato explícito con metadatos. El mapping convertirá la ruta de imagen persistida en una URL absoluta basada en el esquema y host de la solicitud. Se validarán ambos parámetros con FluentValidation y `ValidationProblemDetails`. No se requieren cambios al modelo persistente ni migraciones.

Para cumplir la ruta pública `/assets/properties/{fileName}` manteniendo el acceso estático existente, el diseño agrega un mapeo estático de esa ruta a los archivos de imágenes de propiedades ya publicados por la aplicación.

## Contexto técnico

**Lenguaje/versión**: C# con .NET 11; el repositorio fija el SDK `11.0.100-preview.7.26381.103` en `global.json`.

**Dependencias principales**: ASP.NET Core Minimal APIs, EF Core, Npgsql, FluentValidation y el patrón local `ISlice`/`IHandler`. Las referencias actuales de los proyectos API incluyen EF Core `10.0.4` y Npgsql EF Core `10.0.1`; este cambio no las actualiza.

**Almacenamiento**: PostgreSQL a través de `AppDbContext`. La entidad existente `Property` contiene todos los datos requeridos y `ImageUrl`; no se agregan ni modifican campos persistentes.

**Pruebas**: xUnit en `app/backend/tests/RealtorApiTests`. La constitución autoriza unit tests por defecto; las pruebas de integración con `WebApplicationFactory` o PostgreSQL requieren aprobación explícita en spec, plan y tasks, y no se incluyen en este alcance. Las pruebas unitarias cubren handler, mapping, validator, metadatos, logging estructurado, configuración de archivos estáticos, registro de ruta y conversión de errores de validación a 400. La prueba manual queda documentada en `RealtorApi.http`.

**Plataforma objetivo**: Servicio web ASP.NET Core del backend RealtorApi.

**Tipo de proyecto**: API web backend.

**Metas de rendimiento**: No se establece un objetivo cuantitativo en la spec. La consulta limita la proyección a los campos requeridos, usa `AsNoTracking` y devuelve como máximo 100 propiedades por página.

**Restricciones**: Sin controllers, sin cambios de esquema ni migraciones, sin cambios de paquetes, y sin ampliar el caso de uso a filtros u ordenamientos alternativos. Se conserva la ruta estática existente de imágenes; `/assets/properties` se agrega como alias requerido por el contrato. Los parámetros inválidos deben recibir HTTP 400 con detalles de validación. `page` debe poder representarse como entero `Int32` positivo y `pageSize` no debe superar 100. El flujo debe propagar `CancellationToken`. El handler registra estructuradamente página, tamaño y total, sin datos de propiedad.

**Alcance**: Un endpoint de listado, sus request/response y mapping explícitos, handler, validator, pruebas unitarias de las reglas del slice, ejemplos manuales `.http` y el mapeo estático necesario para que `imageUrl` sea servible. No incluye cambios de frontend, entidades, seeds, migraciones ni otros endpoints.

## Comprobación constitucional

| Comprobación | Estado | Evidencia/diseño |
|---|---|---|
| Spec aprobada precede a implementación | Pasa | La spec fue aprobada por el usuario el 2026-10-19; la implementación queda pendiente de completar las tareas. |
| Arquitectura backend vertical slice | Pasa | El caso de uso vive en `Features/Properties/ListProperties`; el endpoint implementa `ISlice` y el handler implementa `IHandler`. |
| Minimal API y auto-descubrimiento | Pasa | Se reutilizan `RegisterSlices`, `RegisterHandlers` y `MapSliceEndpoints`; no se agregan controllers ni registros manuales. |
| Validación y errores | Pasa con ajuste de infraestructura trazado | Existe `ValidationFilterFactory`, pero `MapSliceEndpoints` actualmente no lo aplica. T014 integra el filtro de forma centralizada y resuelve validators scoped por solicitud; las entradas inválidas reciben `ValidationProblemDetails` y 400. |
| Persistencia | Pasa | Proyección de lectura sobre la entidad existente; no cambia el esquema, los seeds ni las migraciones. |
| Logging estructurado | Pasa | FR-012 y T006 requieren registrar página, tamaño y total con plantilla estructurada, sin datos personales ni descripciones de propiedades. |
| Pruebas y trazabilidad | Pasa con alcance acotado | T015-T017 se vinculan explícitamente con US1, US2 y sus requisitos; no se autoriza infraestructura de integración por esta spec. |
| Artefactos de planificación | Pasa tras decisión explícita | El usuario autorizó enmendar la constitución. La versión 1.2.0 de la constitución permite artefactos auxiliares de planificación sin sustituir `spec.md`, `plan.md` y `tasks.md`. |

**Resultado previo a investigación**: No hay gates bloqueantes. El requisito de URL servible se resuelve agregando un alias estático de `/assets/properties` a los archivos existentes, sin modificar la persistencia.

## Investigación y decisiones de diseño

Las decisiones y alternativas se documentan en [research.md](./research.md). Las decisiones principales son:

- Reutilizar `Property.ImageUrl` existente y exponer solo su nombre de archivo en la ruta pública prescrita.
- Agregar el mapeo estático `/assets/properties` sin retirar `/images/properties`.
- Exponer el estado como texto legible en la respuesta, sin cambiar la serialización global.
- Consultar el total y la página mediante EF Core asíncrono; usar proyección directa y `AsNoTracking`.
- Rechazar `pageSize` mayor que 100 y mantener la semántica acordada para `hasPrevious`, incluso en páginas fuera de rango.
- Rechazar con 400 valores de `page` que no caben en un entero `Int32` positivo; los desplazamientos válidos que excedan el rango de `Skip` producen una página vacía con metadatos reales.
- Aplicar FluentValidation centralmente al mapear los endpoints, resolviendo el validator scoped desde los servicios de cada solicitud. La factoría existente no está actualmente conectada a `MapSliceEndpoints`.
- Registrar mediante `ILogger` un evento estructurado con `Page`, `PageSize` y `TotalItems`, sin incluir contenido de las propiedades.

## Modelo de datos

El modelo de consulta/respuesta y las reglas de validación están descritos en [data-model.md](./data-model.md). Se usa exclusivamente la entidad persistente existente `Property`; no se introducen entidades ni cambios de esquema.

## Contrato externo

El contrato HTTP de `GET /api/properties` se define en [contracts/properties-list.openapi.yaml](./contracts/properties-list.openapi.yaml). Los escenarios de ejecución manual y sus resultados esperados están en [quickstart.md](./quickstart.md).

## Estructura del proyecto

### Documentación de esta funcionalidad

```text
specs/004-properties-list-pagination/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/
    └── properties-list.openapi.yaml
```

`tasks.md` se genera en la fase de tareas; no es parte de este plan.

### Código fuente

```text
app/backend/src/RealtorApi/
├── Features/
│   └── Properties/
│       └── ListProperties/
│           ├── ListPropertiesSlice.cs
│           ├── ListPropertiesHandler.cs
│           ├── ListPropertiesMapping.cs
│           └── ListPropertiesValidator.cs
├── Domain/
│   └── Properties/
│       ├── Property.cs                         # existente, sin cambios
│       └── PropertyStatus.cs                   # existente, sin cambios
├── Infrastructure/
│   ├── Endpoints/
│   │   └── ISlice.cs                           # existente, conectar el filtro central
│   ├── Handlers/
│   │   └── IHandler.cs                         # existente, reutilizado
│   ├── Persistence/
│   │   └── AppDbContext.cs                     # existente, reutilizado
│   └── Validation/
│       └── ValidationFilterFactory.cs           # existente, extender para resolución scoped
├── Program.cs                                  # mapear alias estático requerido
└── RealtorApi.http                             # agregar solicitudes manuales

app/backend/tests/RealtorApiTests/
├── Features/
│   └── Properties/
│       └── ListProperties/
│           ├── ListPropertiesHandlerTests.cs
│           ├── ListPropertiesMappingTests.cs
│           ├── ListPropertiesSliceTests.cs
│           └── ListPropertiesValidatorTests.cs
├── InfrastructureTests.cs                    # verificar ValidationProblemDetails 400
└── PropertiesStaticFilesConfigurationTests.cs # verificar alias y directorio físico
```

**Decisión estructural**: Mantener juntas las piezas específicas del caso de uso bajo `Features/Properties/ListProperties`; completar la conexión central del filtro existente, sin introducir registries, repositorios o servicios genéricos. Las pruebas nuevas se agrupan por feature sin alterar las pruebas existentes.

## Comprobación constitucional posterior al diseño

| Comprobación | Estado | Evidencia |
|---|---|---|
| Los artefactos auxiliares complementan la spec | Pasa | No reemplazan los tres artefactos normativos; `tasks.md` queda para la siguiente fase. |
| No hay cambios persistentes no aprobados | Pasa | La entidad se lee sin alteraciones; no se generan migraciones ni seeds. |
| Límites de pruebas respetados | Pasa | El plan se limita a pruebas unitarias y verificación manual documentada. |
| Logging estructurado | Pasa | El handler registra page, pageSize y total de forma estructurada, sin datos del contenido. |
| Cobertura y trazabilidad de tareas | Pasa | Cada tarea de verificación referencia US1/US2 y los FR/SC que valida; las pruebas incluidas coinciden con la estructura documentada. |
| Validación central | Pasa | El filtro se conecta al mapeo central de slices; el validator scoped se resuelve dentro del alcance de la solicitud. |
| API externa documentada | Pasa | Contrato OpenAPI con parámetros, respuesta paginada y errores 400. |
| Ruta absoluta de imagen servible | Pasa por diseño | El mapping estático adicional corresponde al mismo directorio de imágenes existente y preserva la ruta actual. |
| Alcance funcional | Pasa | No se incluyen filtros, escrituras, cambios frontend ni funcionalidades adicionales. |

**Resultado posterior al diseño**: Gates aprobados; no hay violaciones constitucionales que requieran una excepción.

## Registro de complejidad

No hay violaciones ni complejidad adicional que justificar.
