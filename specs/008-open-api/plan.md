# Plan de implementación: OpenAPI v1 de RealtorApi

**Rama**: `008-open-api` | **Fecha**: 2026-11-08 | **Spec**: [spec.md](./spec.md)

**Entrada**: Especificación de funcionalidad en `specs/008-open-api/spec.md`

## Resumen

Generar un contrato OpenAPI estático y versionado para la API pública de RealtorApi a partir del backend real y de sus endpoints Minimal API. La solución no añadirá Swagger UI ni ReDoc; solo publicará un JSON estático en `GET /openapi/v1.json`, y validará el resultado con Redocly y NSwag. La fuente de verdad seguirá siendo la implementación real del backend, con una verificación de drift basada en `EndpointDataSource` y una normalización de rutas que convierta constraints ASP.NET (`{id:guid}`) a su forma canónica (`{id}`).

## Contexto técnico

**Lenguaje/versión**: C# con .NET 11; el SDK se define por `global.json` y no cambia en este feature.

**Dependencias principales**: ASP.NET Core Minimal APIs, `Microsoft.AspNetCore.OpenApi`, EF Core, Npgsql, FluentValidation, `ProblemDetails`, `EndpointDataSource`, `@redocly/cli` y `NSwag.ConsoleCore`.

**Almacenamiento**: La API usa `AppDbContext` y la entidad persistente `Property` ya existente; la feature no modifica esquema ni persistencia. La salida documental no introduce modelo de dominio nuevo.

**Pruebas**: xUnit en `app/backend/tests/RealtorApiTests`. La verificación automatizada debe comprobar: (1) inventario de endpoints del runtime, (2) validación OpenAPI con Redocly, (3) generación de cliente C# tipado con NSwag. La regla general del repositorio exige pruebas reales y no solo assertions de mocks.

**Plataforma objetivo**: servicio web backend ejecutado por `RealtorApi`.

**Tipo de proyecto**: API web backend con generación de artefactos OpenAPI embebidos y publicación estática de JSON.

**Metas de rendimiento**: la validación no añadirá latencia de runtime significativa; el objetivo es determinismo y reproducibilidad, no un comportamiento funcional nuevo.

**Restricciones**: No se permite incluir Swagger UI, ReDoc UI ni cualquier interfaz gráfica interactiva. La documentación se limita al JSON estático versionado. La API debe publicarse con `wwwroot/openapi/v1.json`; la validación debe ejecutarse vía CLI externa y no como servicio web. La ruta normalizada debe tratar `{id:guid}` como `{id}` tanto para doc como para drift. No se alterarán endpoints existentes; se añade únicamente el GET de lectura por id requerido por FR-004 y autorizado para esta feature.

**Alcance**: generación, validación y prueba de drift; definición del contrato público v1; publicación estática; control de dependencias externas y script de regeneración. Incluye la consulta de lectura por id que falta para completar el inventario contractual. No incluye UX ni cambios en los demás endpoints.

## Comprobación constitucional

| Comprobación | Estado | Evidencia/diseño |
|---|---|---|
| Spec aprobada precede a implementación | Pasa | La spec es la fuente de verdad de esta feature y define el alcance y los requisitos técnicos obligatorios. |
| Arquitectura backend vertical slice | Pasa | La feature no rompe la arquitectura minimal API por slice; solo documenta y valida la API real. |
| Minimal API y auto-descubrimiento | Pasa | Los endpoints reales se toman del runtime, no de una copia manual. |
| Validación y manejo de errores | Pasa | `ProblemDetails` y `HttpValidationProblemDetails` se documentan como componentes reutilizables. |
| Persistencia | Pasa | No se modifica `AppDbContext`, ni entidades ni migraciones. |
| Logging estructurado | Pasa | No cambia el código de negocio, pero la validación de drift puede registrar un resumen estructurado del inventario de endpoints. |
| Pruebas y trazabilidad | Pasa | La feature exige pruebas xUnit de drift, Redocly `lint` y cliente NSwag como smoke test. |
| Artefactos de planificación | Pasa | El plan complementa la spec sin reemplazar los artefactos normativos. |

**Resultado previo a investigación**: no hay violaciones constitucionales. La única toma de decisión relevante es la generación y validación del documento desde runtime, con UI explícitamente excluida.

## Investigación y decisiones de diseño

Las decisiones principales se documentan en [research.md](./research.md). Se resuelven estas preguntas:

- Generar el documento v1 desde el backend real mediante `OpenApi` del pipeline ASP.NET Core y un artefacto estático `wwwroot/openapi/v1.json`.
- Publicar solo JSON y no incluir Swagger UI ni ReDoc UI, siguiendo la exclusión explícita de la spec.
- Normalizar las rutas `/{id:guid}` a `/{id}` para la comparación del runtime con el documento y para evitar falsos negativos de drift.
- Validar con `@redocly/cli lint` y `.redocly.yaml`, desactivando `security-defined` y bajando `operation-4xx-response` a warning.
- Usar NSwag solo como smoke test de consumibilidad del contrato y no como endpoint de runtime.
- Evitar incluir una UI de documentación en producción y mantener el alcance limitado a la documentación contractual.

## Modelo de datos

El esquema de datos de la API y los componentes compartidos quedan definidos en [data-model.md](./data-model.md). La entidad central es `Property`, con `PropertyStatus` como enum, y la documentación refuerza los tipos `ProblemDetails` y `HttpValidationProblemDetails` como schemas reutilizables. No se crean entidades nuevas del dominio para esta feature.

## Contrato externo

El contrato de la API y la documentación de validación se registran en [contracts/README.md](./contracts/README.md). La intención es que el artefacto resultante sea el JSON generado en `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`, con los endpoints listados y validables automáticamente.

## Estructura del proyecto

### Documentación de esta funcionalidad

```text
specs/008-open-api/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── README.md
└── tasks.md               # pendiente de la fase de tareas; no creado en este plan
```

### Código fuente

```text
app/backend/src/RealtorApi/
├── Program.cs                                 # configuración base de servicios y archivos estáticos
├── Domain/
│   └── Properties/
│       ├── Property.cs                       # entidad existente
│       └── PropertyStatus.cs                 # enum existente
├── Features/
│   ├── Health/
│   │   └── HealthSlice.cs                   # GET /health
│   └── Properties/
│       ├── GetPropertyById/
│       │   ├── GetPropertyById.Endpoint.cs    # GET /api/properties/{id:guid}
│       │   ├── GetPropertyById.Handler.cs
│       │   └── GetPropertyById.Response.cs
│       ├── ListProperties/
│       │   └── ListPropertiesSlice.cs        # GET /api/properties
│       ├── CreateProperty/
│       │   └── CreateProperty.Endpoint.cs    # POST /api/properties
│       └── UpdateProperty/
│           └── UpdateProperty.Endpoint.cs    # PUT /api/properties/{id:guid}
├── wwwroot/
│   └── openapi/
│       └── v1.json                           # artefacto generado y servido por la app
├── .redocly.yaml                             # configuración de validación
├── .config/dotnet-tools.json                  # registro local de NSwag
├── support/scripts/
│   └── generate-openapi-v1.ps1               # script reproducible (nuevo)
├── appsettings*.json
└── RealtorApi.csproj                         # ajuste para OpenAPI generation y generación del artefacto

app/backend/tests/RealtorApiTests/
├── GetPropertyByIdTests.cs                   # pruebas del caso de lectura por id
├── OpenApiContractTests.cs                   # drift, schemas y Redocly
├── OpenApiSmokeTests.cs                      # ruta HTTP, ausencia de UI y NSwag
└── ...
```

**Decisión estructural**: Mantener la API como backend minimal API real y generar el documento a partir de ese runtime. No introducir controladores ni UI. La documentación se añadirá como artefacto de build/validación; el único endpoint funcional nuevo es el GET por id aprobado, sin alterar los endpoints existentes.

## Comprobación constitucional posterior al diseño

| Comprobación | Estado | Evidencia |
|---|---|---|
| Los artefactos auxiliares complementan la spec | Pasa | `research.md`, `data-model.md`, `quickstart.md` y `contracts/README.md` sirven de apoyo a la spec y al plan. |
| No hay cambios persistentes no aprobados | Pasa | No se cambia la base de datos ni el dominio. |
| Límites de pruebas respetados | Pasa | Las pruebas se limitan a validación de contrato y drift real. |
| API pública documentada y sin UI | Pasa | La propuesta documenta solo JSON y explícitamente excluye Swagger/ReDoc. |
| Herramientas externas documentadas | Pasa | El plan exige Node 20+, `npx`, Redocly y NSwag con verificación previa. |
| Cobertura y trazabilidad | Pasa | Los requisitos FR/TR/SC de la spec quedan cubiertos por la estrategia de generación y validación. |
| Alcance funcional | Pasa | No se altera la funcionalidad existente ni el comportamiento del negocio. |

**Resultado posterior al diseño**: gates aprobados; no hay incompatibilidades con la constitución ni necesidad de excepción.

## Registro de complejidad

La complejidad es moderada por la necesidad de combinar generación de OpenAPI, validación externa y pruebas de drift. No hay violaciones a la constitución ni salidas no justificadas, porque la feature está acotada a documentación y validación contractual con un único documento v1.
