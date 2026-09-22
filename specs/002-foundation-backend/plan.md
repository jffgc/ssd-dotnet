# Plan de Implementación: Base de infraestructura del backend Realtor

**Branch**: `002-foundation-backend` | **Fecha**: 2026-09-21 | **Spec**: [spec.md](spec.md)

**Entrada**: Especificación de la funcionalidad en `/specs/002-foundation-backend/spec.md`

## Resumen

La iniciativa define la base interna del backend de Realtor, con una infraestructura transversal para descubrimiento de endpoints, registro automático de handlers, validación de requests y manejo consistente de errores. No incluye entidades de dominio, persistencia ni features de producto; su propósito es preparar la aplicación para crecer bajo Vertical Slice Architecture y mantener el código alineado con la constitución del repositorio.

## Contexto técnico

**Idioma/Versión**: .NET 11, según `global.json` del repositorio.

**Dependencias principales**: ASP.NET Core Minimal APIs, FluentValidation, ProblemDetails, Microsoft.Extensions.Logging, xUnit para pruebas.

**Persistencia**: N/A en esta iniciativa. No se crean DbContext, migraciones ni conexiones a bases de datos.

**Pruebas**: Proyecto `app/backend/tests/RealtorApiTests` con xUnit para validar los contratos de infraestructura y la sonda /health.

**Plataforma objetivo**: Backend ASP.NET Core ejecutándose en .NET 11.

**Tipo de proyecto**: Web service / backend foundation.

**Objetivos de rendimiento**: No se definen metas de negocio ni picos altos; la prioridad es estabilidad estructural, compilación y validación de infraestructura.

**Restricciones**: No puede existir lógica de negocio, no se permiten controllers, no se crean entidades de dominio ni features del producto, y no se debe tocar la versión de la plataforma.

**Escala/alcance**: Infraestructura del backend para features futuras, con enfoque en auto-descubrimiento, validación y errores estandarizados.

## Verificación de la constitución

Se valida que la iniciativa cumple los principios del repositorio:

- Solución única y compartida: la base del backend vive dentro de la solución ya existente en `app/` y no se recrea la solución.
- Spec-driven development: la funcionalidad está documentada en `specs/002-foundation-backend/spec.md` y la implementación posterior debe respetar ese alcance.
- Arquitectura canónica: backend con Minimal APIs y Vertical Slice Architecture; no se usan controllers ni capas técnicas globales para orquestar las features.
- Stack obligatorio: .NET 11, ASP.NET Core Minimal APIs y FluentValidation; no se introduce tecnología alternativa ni se modifica `global.json`.
- Calidad de dominio y entrega: no se crean entidades de negocio ni lógica operativa en esta iniciativa.

Resultado: la iniciativa cumple con la constitución y no requiere complejidad adicional ni justificación de desviaciones.

## Investigación de diseño

Se resuelven las decisiones de infraestructura necesarias para esta feature:

- Uso de `ISlice` con assembly scanning para descubrimiento centralizado.
- Uso de `IHandler` marker para auto-registro de handlers en futuras features.
- Uso de `ValidationFilterFactory` para validación automática basada en `IValidator<T>`.
- Conversión de `Result` a `ProblemDetails` para respuestas homogéneas.
- Migración del endpoint `/health` al patrón ISlice para demostrar el funcionamiento real del mecanismo centralizado.

## Estructura del proyecto

### Documentación de esta feature

```text
specs/002-foundation-backend/
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
│   │       ├── Program.cs
│   │       ├── Infrastructure/
│   │       │   ├── Endpoints/
│   │       │   ├── Validation/
│   │       │   ├── Handlers/
│   │       │   └── Errors/
│   │       └── Features/
│   │           └── Health/
│   └── tests/
│       └── RealtorApiTests/
└── frontend/
    └── src/
        └── RealtorWeb/
```

**Decisión de estructura**: Se adopta la organización canónica del backend con un bloque `Infrastructure` para concerns transversales y un slice mínimo para `/health`, sin introducir reglas de negocio ni entidades de dominio.

## Seguimiento de complejidad

No se identifican violaciones o excepciones de la constitución que requieran justificación adicional.
