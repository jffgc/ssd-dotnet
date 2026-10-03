# Guía rápida de validación

## Prerrequisitos

- .NET 11 SDK instalado.
- PostgreSQL disponible localmente o configurado para la app.
- Proyecto backend en `app/backend/src/RealtorApi` y solución `app/Realtor.sln` ya presentes.
- Archivos de seed en `support/seed-data` disponibles en el repositorio.

## Validación funcional

### 1. Construcción del backend

```bash
dotnet restore app/Realtor.sln
dotnet build app/Realtor.sln
```

**Resultado esperado**: compilación correcta del backend y de la solución.

### 2. Ejecución de pruebas relevantes

```bash
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj
```

**Resultado esperado**: pruebas que validen la configuración EF, la migración, la idempotencia del seed y el manejo de assets externos.

### 3. Arranque de la aplicación

```bash
dotnet run --project app/backend/src/RealtorApi/RealtorApi.csproj
```

**Resultado esperado**:
- La API ejecuta `MigrateAsync()` antes de iniciar el host.
- El seed carga estados y propiedades automáticamente.
- El sistema no duplica registros en ejecuciones repetidas.
- Las imágenes quedan disponibles en la ruta final consumible por la API.

### 4. Verificación del contenido del seed

- Revisar los JSON de support y el manifiesto central.
- Comprobar que `ImageUrl` apunta a la ruta pública y no a una carpeta física de soporte.
- Confirmar que los estados esperados (`Available`, `Rented`, `Maintenance`) están presentes.

## Resultado de éxito

La app debe arrancar con la base de datos en un estado consistente y reproducible, usando migración automática y seed idempotente sin intervención manual.
