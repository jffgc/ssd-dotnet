# Investigación de diseño: persistencia y seeding de propiedades

## Decisiones

### Decisión 1: Persistencia con EF Core y PostgreSQL
- La base de datos del sistema permanecerá en PostgreSQL con EF Core usando Npgsql.
- Se usará un modelo de dominio mínimo para `PropertyStatus` y `Property` y el esquema se gestionará con migraciones versionadas.
- El valor del estado se almacenará como texto en la base de datos mediante conversión `enum -> string` para facilitar lectura y compatibilidad con consultas.

**Racional**: La constitución fija EF Core + PostgreSQL como stack obligatorio del backend y la feature exige persistencia estable para catálogo de propiedades.

**Alternativas consideradas**:
- Persistencia con `HasData` únicamente: descartada porque la especificación prohibe este mecanismo para la feature.
- Persistencia con valores numéricos del enum: descartada porque el requisito exige string en base de datos.
- Generación de múltiples migraciones por entidad: descartada porque la spec exige una sola migración coherente para el cambio funcional.

### Decisión 2: Patrones de arranque y seeding
- El arranque de la API ejecutará `app.MigrateAsync()` antes de `app.Run()`.
- El `DbContext` configurará `UseSeeding` y `UseAsyncSeeding`.
- `DatabaseSeeder` implementará versión síncrona y asíncrona con lógica idempotente basada en comprobación de existencia por clave natural o identificador.

**Racional**: Esto cumple la secuencia de bootstrap definida por la documentación de persistencia y evita ejecuciones manuales del seeder.

**Alternativas consideradas**:
- Invocar el seeder en `MigrationExtensions` o desde código manual: descartada por la especificación.
- Ejecutar seed solo si hay migraciones pendientes: descartada porque la feature exige que se dispare incluso sin cambios pendientes.

### Decisión 3: Assets externos y manifiesto central
- Los archivos de datos y las imágenes se integrarán al proyecto API mediante `RealtorApi.csproj`.
- El seed resolverá rutas usando `support/seed-data/seed-manifest.json` y el contenido del manifest para copiar cada recurso a la ruta final consumible.
- El campo `ImageUrl` guardará la URL pública servida por la API y no rutas físicas de `support`.

**Racional**: La feature exige que los assets estén disponibles en runtime y que el seeding sea reproducible y no dependa de hardcoded paths dispersos.

**Alternativas consideradas**:
- Guardar paths físicos en el JSON del seed: descartado porque rompe la regla de `ImageUrl` pública.
- Resolver rutas con valores localizados por clase o servicio disperso: descartado porque la especificación exige un manifest central único.

## Requisitos de diseño derivados

- Las configuraciones EF Core deben vivir en `Infrastructure/Persistence/Configurations` y usarse con `ApplyConfigurationsFromAssembly`.
- El `AppDbContext` debe registar los `DbSet` mínimos necesarios para `Property` y `PropertyStatus`.
- El seed debe cargar los estados base y luego las propiedades, manteniendo el orden para evitar referencias inválidas.
- Las pruebas deben verificar el comportamiento real del seed, el schema generado y la valor de `ImageUrl` final.

## Riesgos y mitigación

- Falta de manifest: se valida desde el arranque y se falla de forma clara si no existe un asset requerido.
- Duplicación de registros: la lógica del seed compara por clave natural o identificación para garantizar idempotencia.
- Imagen pública inconsistente: el valor persistido debe derivarse de la ruta final definida por el manifest y no de la ubicación original.
