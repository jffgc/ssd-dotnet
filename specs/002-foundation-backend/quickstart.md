# Guía de validación rápida: infraestructura del backend

## Requisitos previos

- SDK .NET 11 configurado por `global.json`.
- Repositorio clonado y restaurado.
- Proyecto base de la solución ya presente en `app/Realtor.sln`.

## Validación de compilación

1. Abrir la terminal en la raíz del repositorio.
2. Ejecutar:

```bash
dotnet restore app/Realtor.sln
```

3. Ejecutar:

```bash
dotnet build app/Realtor.sln
```

**Resultado esperado**: la solución compila sin errores y sin introducir entidades ni features de negocio.

## Validación de infra con pruebas unitarias

Ejecutar:

```bash
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj
```

**Resultado esperado**:
- `RegisterSlices` detecta y registra slices sin duplicados.
- `ValidationFilterFactory` valida cuando existe `IValidator<T>` y hace pass-through cuando no existe.
- Las validaciones fallidas producen `ValidationProblemDetails` con HTTP 400.
- El mapping de `Result` a `ProblemDetails` produce status y payload esperados.
- `/health` responde correctamente como endpoint ISlice.

## Validación funcional mínima de ejecución

1. Ejecutar la API:

```bash
dotnet run --project app/backend/src/RealtorApi
```

2. Hacer una petición al endpoint de salud:

```bash
curl http://localhost:<puerto>/health
```

**Resultado esperado**: respuesta operativa de salud con estructura válida y sin necesidad de registro manual en `Program.cs`.

## Criterio de cierre

La feature queda validada cuando la solución compila, las pruebas de infraestructura pasan y el endpoint `/health` funciona como ejemplo operativo del mecanismo centralizado de descubrimiento.
