# Plan de Implementación: Persistencia y seeding inicial de propiedades

**Branch**: `003-properties-persistence-seeding` | **Fecha**: 2026-09-22 | **Spec**: [spec.md](spec.md)

**Entrada**: Especificación de la funcionalidad en `/specs/003-properties-persistence-seeding/spec.md`

## Resumen

La iniciativa define la base persistente del dominio inmobiliario en el backend de Realtor: entidades `PropertyStatus` y `Property`, configuración EF Core por assembly, migración EF Core única y seeding idempotente automático con los assets externos del repositorio. La solución debe respetar la arquitectura Vertical Slice existente, no introducir controllers ni lógica paralela y garantizar que `ImageUrl` se almacene como ruta pública servida por la API.

## Contexto técnico

**Idioma/Versión**: .NET 11, según `global.json` del repositorio.

**Dependencias principales**: ASP.NET Core Minimal APIs, EF Core, Npgsql, PostgreSQL, FluentValidation, ProblemDetails, xUnit.

**Persistencia**: PostgreSQL con EF Core; migraciones y seeding en la capa `Infrastructure/Persistence` del backend.

**Pruebas**: Proyecto `app/backend/tests/RealtorApiTests` con xUnit para validar configuración EF, migración, seeding idempotente y runtime de bootstrap.

**Plataforma objetivo**: Backend ASP.NET Core ejecutándose sobre .NET 11.

**Tipo de proyecto**: Web service / backend con dominio persistente inicial.

**Objetivos de rendimiento**: Arranque de la aplicación estable, migración automática sin bloqueo manual y seeding repetible con cargas de prueba pequeñas.

**Restricciones**: No se permiten controllers, no se usa `HasData`, no se invoca el seeder manualmente, no se hardcodean rutas de soporte, y se debe mantener compatibilidad con el patrón `ISlice` ya existente.

**Escala/alcance**: Capa de persistencia inicial para propiedades y estados, con assets externos integrados y base de datos operativa al iniciar la API.

## Verificación de la constitución

Se valida que la iniciativa cumple la constitución del repositorio:

- Solución única y compartida: la feature vive dentro de la solución ya existente y no crea una estructura paralela de persistencia ni endpoints de negocio.
- Spec-driven development: la funcionalidad está documentada en [specs/003-properties-persistence-seeding/spec.md](spec.md) y todo cambio posterior debe respetar ese alcance.
- Arquitectura canónica: el backend mantiene Vertical Slice Architecture y la persistencia queda bajo `Infrastructure`, sin controllers ni capas duplicadas.
- Stack obligatorio: se usa .NET 11, ASP.NET Core Minimal APIs, EF Core + Npgsql + PostgreSQL, FluentValidation y ProblemDetails.
- Calidad de dominio y entrega: la lógica de negocio no se mezcla con DbContext ni con la UI; se valida la idempotencia del seed y la integridad del esquema.

Resultado: la iniciativa cumple la constitución y no requiere justificación de una excepción ni cambios de arquitectura.

## Investigación de diseño

Se resuelven las decisiones técnicas necesarias para esta feature:

- Persistencia de `PropertyStatus` como enum con conversión a string y valores `Available`, `Rented` y `Maintenance`.
- Entidad `Property` con propiedades básicas de negocio y metadatos de auditoría (`CreatedAt`, `UpdatedAt`).
- Configuración EF Core centralizada vía `IEntityTypeConfiguration<T>` bajo `Infrastructure/Persistence/Configurations`.
- Registro de `DbSet` y `ApplyConfigurationsFromAssembly` dentro de `AppDbContext`, evitando mapping inline en `OnModelCreating`.
- Generación de una única migración funcional coherente para el cambio y revisión de `Up`, `Down` y snapshot antes de cerrar la task.
- Arranque de la aplicación con `app.MigrateAsync()` antes de `app.Run()`, seguido de `UseSeeding` y `UseAsyncSeeding` para datos iniciales idempotentes.
- Resolución de assets mediante un manifiesto central `support/seed-data/seed-manifest.json`, con copiado y sincronización de imágenes en la ruta pública servida por la API.
- `ImageUrl` almacenado como URL pública final y no como ruta física de soporte.

## Estructura del proyecto

### Documentación de esta feature

```text
specs/003-properties-persistence-seeding/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── README.md
└── checklists/
    └── requirements.md
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
│   │       │       ├── CreateProperty/
│   │       │       ├── UpdateProperty/
│   │       │       ├── GetPropertyById/
│   │       │       ├── ListProperties/
│   │       │       └── ChangePropertyStatus/
│   │       ├── Infrastructure/
│   │       │   ├── Persistence/
│   │       │   │   ├── AppDbContext.cs
│   │       │   │   ├── DatabaseSeeder.cs
│   │       │   │   ├── MigrationExtensions.cs
│   │       │   │   └── Configurations/
│   │       │   │       └── PropertyConfiguration.cs
│   │       │   ├── Migrations/
│   │       │   └── ...
│   │       ├── Program.cs
│   │       ├── RealtorApi.csproj
│   │       └── appsettings.json
│   └── tests/
│       └── RealtorApiTests/
└── frontend/
    └── src/
        └── RealtorWeb/

support/
└── seed-data/
    ├── seed-manifest.json
    ├── properties.json
    ├── status.json
    └── images/
```

**Decisión de estructura**: Se adopta la organización canónica del backend con dominio persistente y persistencia bajo `Infrastructure`, manteniendo la infraestructura existente de slices y sin introducir una jerarquía paralela de endpoints de negocio.

## Seguimiento de complejidad

No se identifican violaciones o excepciones de la constitución que requieran justificación adicional.
