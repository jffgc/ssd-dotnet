# Guía rápida de validación: actualización del estado

## Prerrequisitos

- SDK .NET 11 instalado, según `global.json`.
- PostgreSQL disponible y configurado para RealtorApi.
- La solución y el proyecto de pruebas presentes en el repositorio.
- Una propiedad existente cuyo id se pueda usar en la prueba manual.

## Validación automatizada

Desde la raíz del repositorio, ejecutar:

```powershell
dotnet build .\app\Realtor.sln
dotnet test .\app\backend\tests\RealtorApiTests\RealtorApiTests.csproj
```

**Resultado esperado**: solución compilada y pruebas unitarias del request/binder, validator, mapping y handler aprobadas. Las pruebas del handler deben demostrar que solo cambia `Status`, incluidos los valores anteriores de `ImageUrl`, `CreatedAt` y `UpdatedAt`.

## Prueba HTTP manual

1. Iniciar RealtorApi:

   ```powershell
   dotnet run --project .\app\backend\src\RealtorApi\RealtorApi.csproj
   ```

2. Abrir `app/backend/src/RealtorApi/RealtorApi.http` en VS Code y usar el `baseUrl` configurado allí.
3. Sustituir `propertyId` por el UUID canónico de una propiedad existente.
4. Ejecutar el caso válido con `{"status":"Rented"}` y esperar `200 OK` con exactamente `id` y `status`.
5. Consultar `GET /api/properties/{id}` y comparar antes/después `title`, `description`, `address`, `price`, `bedroomCount`, `bathroomCount`, `areaSquareMeters` e `imageUrl`; deben conservar sus valores previos.
6. Ejecutar los casos inválidos del archivo `.http`: status omitido, `null` o no admitido, propiedad adicional, JSON/body malformado y UUID no canónico. Cada caso debe responder `400` con `ValidationProblemDetails` y no persistir cambios.
7. Ejecutar el caso con un UUID canónico sin registro. Debe responder `404` con `ProblemDetails`.

## Verificación del contrato OpenAPI

El contrato de diseño de esta feature está en `contracts/backend-properties-status.openapi.yaml`. Para regenerar y verificar el OpenAPI v1 desde la implementación real, usar el proceso aprobado por la spec 008:

```powershell
.\support\scripts\generate-openapi-v1.ps1
```

**Resultado esperado**: el documento generado incluye `PATCH /api/properties/{id}/status` y conserva el conjunto de campos y respuestas de los demás endpoints.