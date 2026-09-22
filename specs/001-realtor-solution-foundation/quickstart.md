# Guía de validación rápida: foundation Realtor

## Requisitos previos

- Repositorio clonado localmente.
- SDK de .NET compatible con la versión indicada en [global.json](../../global.json).
- Acceso a una terminal con PowerShell o dotnet disponible en el `PATH`.

## Verificación de entorno

1. Desde la raíz del repositorio, ejecutar:
   ```powershell
   dotnet --version
   ```
2. Confirmar que la versión devuelta coincide con la declarada en [global.json](../../global.json).

## Validación de la base de la solución

1. Crear o preparar la estructura de la solución bajo `app/`.
2. Confirmar que existe la solución principal `app/Realtor.sln`.
3. Confirmar que los proyectos base están ubicados en:
   - `app/backend/src/RealtorApi/`
   - `app/backend/tests/RealtorApiTests/`
   - `app/frontend/src/RealtorWeb/`
   - `app/frontend/test/RealtorWeb/`
4. Verificar que el backend usa ASP.NET Core Minimal APIs y no controllers.
5. Verificar que el frontend usa Blazor Web App con Razor Components.
6. Verificar que `Program.cs` contiene solo configuración base, sin negocio ni features.

## Resultado esperado

- La solución compila de forma estable en la fase base.
- Los proyectos existen en las rutas correctas.
- La arquitectura canónica del repositorio se respeta.
- No existen entidades de dominio ni lógicas de negocio añadidas en esta iniciativa.

## Observación

La foundation es un primer bloque técnico y no debe ser usada como implementación funcional de la aplicación; la lógica de negocio se añadirá en iniciativas posteriores cuando exista una spec aprobada.
