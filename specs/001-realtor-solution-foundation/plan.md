# Plan de Implementación: Base de la solución Realtor

**Branch**: `001-realtor-solution-foundation` | **Fecha**: 2026-09-21 | **Spec**: [spec.md](spec.md)

**Entrada**: Especificación de la funcionalidad en `/specs/001-realtor-solution-foundation/spec.md`

## Resumen

La iniciativa foundation define la estructura mínima necesaria para la solución Realtor, con un backend en ASP.NET Core Minimal APIs, un frontend en Blazor Web App con Razor Components, proyectos de pruebas y una configuración inicial de `Program.cs` sin lógica de negocio ni features. La solución debe alinearse exactamente con la constitución del repositorio y con la versión de .NET declarada en `global.json`.

## Contexto técnico

**Idioma/Versión**: .NET 11.0.100-preview.7.26381.103, según `global.json`

**Dependencias principales**: ASP.NET Core, Blazor Web App, EF Core, Npgsql, PostgreSQL, Refit, FluentValidation, ProblemDetails, CSS propio centralizado, Lucide Icons

**Persistencia**: PostgreSQL mediante EF Core y Npgsql, aunque la fase foundation no incluye modelos ni migraciones funcionales

**Pruebas**: Proyectos de pruebas configurados para validar compilación y estructura base; sin pruebas de negocio aún en esta iniciativa

**Plataforma objetivo**: Aplicación web fullstack con backend y frontend bajo una misma solución

**Tipo de proyecto**: Web application / solución fullstack

**Objetivos de rendimiento**: No se establecen metas de negocio ni carga; la prioridad es el arranque estructural y la compilación correcta

**Restricciones**: No se permiten controllers, la lógica de negocio queda fuera del alcance, y el stack se basa en la constitución y en `global.json`

**Escala/alcance**: Infraestructura inicial de solución, sin features ni dominio operativo

## Verificación de la constitución

Se valida que la iniciativa cumple con los principios del repositorio:

- Solución única y compartida: la base se crea dentro de `app/` bajo una sola solución.
- Spec-driven development: se documenta la iniciativa en `specs/001-realtor-solution-foundation/`.
- Arquitectura canónica: backend con Minimal APIs, frontend con Blazor Web App, prohibición de controllers.
- Stack obligatorio: .NET 11 según global.json; no se modifica ninguna versión ni configuración global.
- Calidad de dominio: no se crean entidades ni reglas de negocio en la foundation.

La constitución no presenta conflictos con esta iniciativa y el alcance es compatible con el estado base del repositorio.

## Estructura del proyecto

### Documentación de esta feature

```text
specs/001-realtor-solution-foundation/
├── spec.md
├── plan.md
└── tasks.md
```

### Código fuente (raíz del repositorio)

```text
app/
├── Realtor.sln
├── backend/
│   ├── src/
│   │   └── RealtorApi/
│   └── tests/
│       └── RealtorApiTests/
├── frontend/
│   ├── src/
│   │   └── RealtorWeb/
│   └── test/
│       └── RealtorWeb/
└── global.json (no se modifica en esta iniciativa)
```

**Decisión de estructura**: Se adopta la disposición canónica del repositorio con una solución compartida y proyectos separados por backend, frontend y pruebas, manteniendo la base sin features ni dominio.

## Seguimiento de complejidad

No se identifican desviaciones ni excepciones de la constitución que requieran justificación adicional.
