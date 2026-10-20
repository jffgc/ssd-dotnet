# Plan de Implementación: Alta de propiedades con upload opcional de imagen

**Branch**: `005-properties-create` | **Fecha**: 2026-10-19 | **Spec**: [spec.md](spec.md)

**Entrada**: Especificación de la funcionalidad en `/specs/005-properties-create/spec.md`

## Resumen

La iniciativa define un nuevo caso de uso de negocio para dar de alta propiedades en el backend de Realtor mediante un slice Minimal API con creación pública, validación del archivo opcional y persistencia del `ImageUrl` final. La solución respeta la arquitectura Vertical Slice ya existente, mantendrá `imageUrl` en `null` cuando no se envíe imagen, y guardará el asset subido en `wwwroot/assets/properties` cuando el archivo sea válido. La funcionalidad debe conservar compatibilidad con el dominio, el arranque y el seeding ya definidos en las specs previas de foundation y persistencia.

## Contexto técnico

**Idioma/Versión**: .NET 11, según `global.json` del repositorio.

**Dependencias principales**: ASP.NET Core Minimal APIs, EF Core, Npgsql, PostgreSQL, FluentValidation, ProblemDetails, xUnit.

**Persistencia**: PostgreSQL con EF Core y dominio `Property`/`PropertyStatus` ya integrado en la base del backend.

**Testing**: Proyecto `app/backend/tests/RealtorApiTests` con xUnit para validar endpoint, mapeo, validación, persistencia y comportamiento del archivo.

**Plataforma objetivo**: Backend ASP.NET Core ejecutándose sobre .NET 11 y sirviendo contenido estático desde `wwwroot`.

**Tipo de proyecto**: Web service / backend con caso de uso de escritura inmobiliaria.

**Objetivos de rendimiento**: creación de propiedades con latencia aceptable, manejo de imagenes pequeñas y validación de entrada en tiempo real sin bloquear la app.

**Restricciones**: no se permiten controllers, no se agregan capas transversales paralelas, la imagen es opcional, la validación de archivo se hace en backend, y `ImageUrl` debe ser la ruta pública servida por la API.

**Escala/alcance**: caso de uso `CreateProperty` para propiedades con y sin imagen, siguiendo el dominio previo y con pruebas de integración funcionales.

## Verificación de la constitución

Se valida que la iniciativa cumple la constitución del repositorio:

- Solución única y compartida: la feature es un caso de uso más del backend existente, sin paralelismos de arquitectura ni capa de servicios funcionales desconectada.
- Spec-driven development: la funcionalidad está documentada en [specs/005-properties-create/spec.md](spec.md) y el alcance queda acotado a la creación de propiedades con upload opcional.
- Arquitectura canónica: se mantiene Vertical Slice Architecture, `ISlice` auto-registro y `IHandler` para lógica del caso de uso; no se introduce controller ni lógica de negocio en mapping.
- Stack obligatorio: se usa .NET 11, ASP.NET Core Minimal APIs, EF Core, PostgreSQL/Npgsql, FluentValidation, ProblemDetails y `wwwroot` para assets públicos.
- Calidad de dominio y entrega: la validación del upload y del `ProblemDetails` se hace en la capa del caso de uso y las pruebas verifican el comportamiento real.

Resultado: la iniciativa cumple la constitución y no requiere una excepción arquitectónica ni una justificación adicional.

## Investigación de diseño

Se resuelven las decisiones técnicas necesarias para esta feature:

- El caso de uso `CreateProperty` será un slice en `Features/Properties/CreateProperty` con `ISlice`, validación, handler y mapping explícitos.
- El endpoint público será `POST /api/properties`, aceptando datos escalares junto con un archivo opcional en multipart/form-data.
- Cuando no se envía imagen, la entidad se persiste con `ImageUrl = null`; si hay archivo y es válido, se guarda en `wwwroot/assets/properties` y la propiedad guarda la ruta pública relativa o absoluta compatible con la API.
- La validación del upload se realizará en backend con FluentValidation: tipos permitidos `png`, `jpg`, `jpeg` y límite global de 5MB.
- La resolución del nombre del archivo será determinista y segura para evitar conflictos y mantener la ruta final estable para clientes.
- El `ProblemDetails` responderá a errores de validación, archivo no soportado, tamaño demasiado grande y payload corrupto o incompleto.
- Las pruebas de la feature cubrirán creación con y sin imagen, validación del tipo, validación del tamaño y consistencia del valor persistido.

## Estructura del proyecto

### Documentación de esta feature

```text
specs/005-properties-create/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── README.md
└── tasks.md               # creado luego por /speckit.tasks
```

### Código fuente (raíz del repositorio)

```text
app/
├── Realtor.sln
├── backend/
│   ├── src/
│   │   └── RealtorApi/
│   │       ├── Domain/
│   │       │   └── Properties/
│   │       │       ├── Property.cs
│   │       │       └── PropertyStatus.cs
│   │       ├── Features/
│   │       │   └── Properties/
│   │       │       └── CreateProperty/
│   │       │           ├── CreateProperty.Endpoint.cs
│   │       │           ├── CreateProperty.Handler.cs
│   │       │           ├── CreateProperty.Mapping.cs
│   │       │           ├── CreateProperty.Request.cs
│   │       │           ├── CreateProperty.Response.cs
│   │       │           └── CreateProperty.Validator.cs
│   │       ├── Infrastructure/
│   │       │   ├── Endpoints/
│   │       │   ├── Handlers/
│   │       │   ├── Persistence/
│   │       │   └── Validation/
│   │       ├── Program.cs
│   │       └── wwwroot/assets/properties/
│   └── tests/
│       └── RealtorApiTests/
├── frontend/
└── support/
```

**Decisión de estructura**: se adopta la organización canónica del backend con el slice `CreateProperty` dentro de `Features/Properties`, manteniendo el dominio y la persistencia existentes y evitando cualquier cambio no requerido fuera del alcance de la feature.

## Complejidad y seguimiento

No se identifican violaciones de la constitución ni excepciones que deban justificarse en esta initiative.
