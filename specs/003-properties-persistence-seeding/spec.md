# Especificación de la Funcionalidad: Persistencia y seeding inicial de propiedades

**Feature Branch**: `003-properties-persistence-seeding`

**Creado**: 2026-09-22

**Estado**: En implementación

**Entrada**: Descripción del usuario: "Crear la especificación 003-properties-persistence-seeding. Objetivo: Definir e implementar el modelo persistente inicial de propiedades y estados de propiedad, incluyendo migración EF Core, seeding idempotente automático y manejo de archivos externos para que queden consumibles por el proceso de inserción y por futuras consultas de propiedades con imagen. Contexto obligatorio: Respetar estrictamente las reglas de persistencia definidas en database.instructions.md. Seguir los skills oficiales de entidad/configuración, migraciones y seeding del repositorio. Mantener la arquitectura backend y el patrón de Vertical Slice. Mantener compatibilidad funcional con el patrón de slice existente, por ejemplo HealthSlice.cs, evitando cambios no requeridos por la spec. Estructura obligatoria: app/backend/src/RealtorApi/Domain/Properties/Property.cs, PropertyStatus.cs; Features/Properties/...; Infrastructure/Persistence/...; Program.cs; etc. Reglas de organización: todo endpoint de negocio debe vivir dentro de Features; persistencia y migraciones bajo Infrastructure. Fuentes externas de seed y assets: properties.json, properties-statuses.json, properties, support/seed-data/seed-manifest.json. Alcance funcional incluido: entidades persistentes PropertyStatus y Property, estado como string, configuración por IEntityTypeConfiguration, DbSet, migración única, UseSeeding/UseAsyncSeeding, MigrateAsync antes de Run, archivos JSON e imágenes en proyecto, seed desde manifest no hardcoded, ImageUrl pública final, no controllers, no HasData, no MigrateAsync condicionado, no dotnet ef database update como criterio de cierre. Criterios de éxito: modelo persistente compilando, migración generada y revisada, seeding operativo, assets consumibles en runtime y ImageUrl final servido por la API.”

## Escenarios de Usuario y Pruebas

### Historia de usuario 1 - Persistencia del catálogo inicial de propiedades (Prioridad: P1)

El equipo necesita un modelo persistente estable para las propiedades dentro del backend de Realtor, con estados de disponibilidad y metadatos básicos para soportar inserciones futuras y consultas de listado/detalle de propiedades con imagen. La persistencia debe seguir la arquitectura del backend y permitir la evolución de nuevas features sin duplicar lógica ni romper la estructura vertical slice.

**Por qué esta prioridad**: Es la base funcional del dominio inmobiliario y define la capa de datos que consumirá la inserción inicial y cualquier consulta posterior sobre propiedades, por lo que debe estar correctamente modelada y validada antes de sumar lógica adicional.

**Prueba independiente**: Se puede verificar compilando el backend, ejecutando las pruebas de configuración EF y validando que las entidades, conversiones y migraciones quedan sincronizadas con el esquema esperado.

**Escenarios de aceptación**:

1. **Dado** que el backend define el dominio persistente de propiedades, **cuando** la aplicación arranca con migración habilitada, **entonces** las entidades PropertyStatus y Property quedan creadas con el esquema requerido y sin duplicar registros.
2. **Dado** que una propiedad requiere campos básicos y de estado, **cuando** se persiste un registro, **entonces** la información queda almacenada con los campos obligatorios, tipos apropiados y el estado representado como texto para la base de datos.
3. **Dado** que el sistema necesita soportar futuras consultas con imágenes, **cuando** el backend devuelve la propiedad, **entonces** la URL de imagen apunta a la ruta pública servida por la API y no a un path físico de soporte.

---

### Historia de usuario 2 - Seeding automático y material externo integrados (Prioridad: P1)

Los datos iniciales y los activos visuales deben cargarse de forma idempotente al iniciar la aplicación, sin depender de ejecuciones manuales ni rutas dispersas en el código. El backend debe consumir un manifiesto central y una estructura de archivos que permita replicar el seed en distintos entornos y publicar correctamente el contenido necesario para la API.

**Por qué esta prioridad**: El arranque debe dejar la base de datos en un estado útil y reproducible, y la entrega del material externo debe ser consistente entre desarrollo, build y publicación.

**Prueba independiente**: Se puede validar ejecutando la aplicación en arranque con la base de datos vacía e inspeccionando que el seed se ejecuta una sola vez y que los JSON, el manifiesto y las imágenes quedan disponibles en runtime.

**Escenarios de aceptación**:

1. **Dado** que la aplicación tiene `UseSeeding` y `UseAsyncSeeding` configurados, **cuando** se inicia la aplicación, **entonces** la base de datos aplica migraciones y ejecuta el seeding idempotente sin duplicados.
2. **Dado** que existen archivos de seed en support y un manifiesto central, **cuando** la aplicación se despliega o ejecuta, **entonces** los datos y las imágenes quedan accesibles para el runtime según la ruta declarada en el manifiesto.
3. **Dado** que la seeding debe resolverse por configuración y no por rutas hardcodeadas, **cuando** se revisa el flujo de bootstrap, **entonces** el sistema localiza el origen y destino de cada asset mediante el manifiesto central en lugar de valores dispersos en código.

---

### Historia de usuario 3 - Validación de migración y consistencia de arranque (Prioridad: P2)

El equipo de backend necesita asegurar que el cambio funcional de propiedades se materializa como un único cambio coherente de esquema y que el arranque de la API no depende de ejecuciones manuales fuera del flujo estándar. Esto reduce errores en entornos nuevos y facilita la revisión del cambio funcional antes de cerrar la historia.

**Por qué esta prioridad**: Las migraciones y el arranque son el punto de control para la integridad del sistema, por lo que deben validarse tras confirmar la entidad y el seed, aunque la funcionalidad de negocio pueda desarrollarse después.

**Prueba independiente**: Se puede verificar revisando el snapshot, el `Up` y el `Down` de la migración generada y ejecutando la app con base de datos sin previo cargado para comprobar que el flujo de boot aplica migraciones y seed.

**Escenarios de aceptación**:

1. **Dado** que el cambio funcional incluye entidad, configuración y migración, **cuando** se revisa la migración generada, **entonces** contiene únicamente el esquema coherente del cambio de propiedades y no múltiples migraciones fragmentadas.
2. **Dado** que la app debe iniciar siempre con migración automática, **cuando** se levanta la aplicación, **entonces** ejecuta `MigrateAsync()` antes de `Run` y sin requerir una validación manual de la base de datos.
3. **Dado** que el arranque debe ser idempotente, **cuando** se reinicia la aplicación, **entonces** el seed no duplica estados ni propiedades y mantiene la configuración esperada del sistema.

---

### Casos límite

- ¿Qué ocurre si falta algún estado de propiedad en el seed? El sistema debe inicializarlo con los valores de negocio previstos y evitar dejar la base de datos en un estado incompleto.
- ¿Qué ocurre si el archivo de seed o el manifiesto no está disponible en runtime? La aplicación debe fallar de forma clara y reproducible, porque la inicialización depende de los assets declarados.
- ¿Qué ocurre si la ruta de imagen se guarda como path físico del soporte? La solución no es válida porque la URL de imagen debe apuntar a la ruta pública servida por la API.
- ¿Qué ocurre si se ejecuta el seed más de una vez? Debe ser idempotente y no duplicar filas ni recrear estados ya existentes.

## Requisitos

### Requisitos funcionales

- **FR-001**: El backend DEBE definir las entidades persistentes iniciales de dominio dentro de la estructura `Domain/Properties` con `PropertyStatus` y `Property` como piezas de dominio del catálogo inmobiliario.
- **FR-002**: `PropertyStatus` DEBE incluir los valores de negocio `Available`, `Rented` y `Maintenance` y representar el estado de la propiedad en una forma persistible y legible por la base de datos.
- **FR-003**: `Property` DEBE almacenar al menos los campos `Id`, `Title`, `Description`, `Address`, `Price`, `Status`, `BedroomCount`, `BathroomCount`, `AreaSquareMeters`, `ImageUrl`, `CreatedAt` y `UpdatedAt` con semántica y tipos apropiados para la persistencia.
- **FR-004**: El atributo `Status` de `Property` DEBE almacenarse como texto en la base de datos mediante conversión EF Core de enum a string, sin persistir el valor enum en formato numérico.
- **FR-005**: El modelo de persistencia DEBE configurarse mediante clases `IEntityTypeConfiguration<T>` dentro de `Infrastructure/Persistence/Configurations`, sin mapear entidades inline en `OnModelCreating`.
- **FR-006**: `AppDbContext` DEBE registrar los `DbSet` necesarios y aplicar la configuración del assembly usando `ApplyConfigurationsFromAssembly` para mantener la estructura centralizada y reutilizable.
- **FR-007**: El cambio funcional de propiedades DEBE materializarse en una sola migración EF Core coherente, con revisión de `Up`, `Down` y snapshot antes de cerrar la tarea.
- **FR-008**: La aplicación DEBE ejecutar `app.MigrateAsync()` antes de `app.Run()` en el arranque, y no debe condicionar la migración a la existencia de cambios pendientes en un chequeo manual.
- **FR-009**: El sistema DEBE configurar `UseSeeding` y `UseAsyncSeeding` en `DbContext` para poblar datos iniciales y garantizar que el flujo de bootstrap se ejecute aunque no haya migraciones pendientes.
- **FR-010**: `DatabaseSeeder` DEBE implementar una versión sincrónica y otra asíncrona equivalentes, y el seeding DEBE ser idempotente para evitar duplicados y reasignaciones accidentales de datos base.
- **FR-011**: Los archivos externos de seed e imágenes DEBEN integrarse en `RealtorApi.csproj` para que queden disponibles en build y publish, con la resolución de rutas basada en `support/seed-data/seed-manifest.json` y no en cadenas hardcodeadas.
- **FR-012**: La API DEBE exponer y mantener una ruta pública final para cada imagen de propiedad, y `ImageUrl` DEBE guardar esa ruta pública servida por la API, no la ubicación física de `support`.
- **FR-013**: El flujo de seeding DEBE consumir JSON y manifest central para cargar datos de propiedades y estados, manteniendo el mismo origen de verdad para insertados iniciales y futuras consultas de imagen.
- **FR-014**: El backend NO DEBE usar controllers, NO DEBE usar `HasData` para este escenario, NO DEBE invocar manualmente el seeder desde `MigrationExtensions` ni como fallback, y NO DEBE registrar la lógica de negocio fuera de la estructura de Features cuando exista un caso de uso real.
- **FR-015**: La aplicación DEBE mantener compatibilidad funcional con el patrón existente de slices, evitando cambios no requeridos en la infraestructura o en `HealthSlice` durante esta spec.
- **FR-016**: Las pruebas del cambio funcional DEBEN validar la configuración EF, la conversión enum-string, la migración esperada, la idempotencia del seed, la lectura del JSON y del manifiesto, y la sincronización de imágenes con la URL pública final.

### Entidades clave

- **PropertyStatus**: Representa el estado operativo de la propiedad dentro del dominio, con valores estándar de disponibilidad y mantenimiento. Su persistencia usa un mapeo de string para estabilidad textual en base de datos.
- **Property**: Representa la entidad principal del catálogo inmobiliario, con los atributos mínimos necesarios para registrar oferta, ubicación, precio, superficie, estado, imagen y tiempos de auditoría.
- **SeedManifest**: Documento central que describe la relación entre los archivos de datos externos, la ubicación de origen y la ruta final pública o de destino de cada asset de seed.

## Criterios de éxito

### Resultados medibles

- **SC-001**: El backend compila con el modelo persistente de `Property` y `PropertyStatus` implementado en la estructura de dominio y persistencia requerida.
- **SC-002**: Una migración única y coherente cubre el cambio funcional de persistencia y se revisa con `Up`, `Down` y snapshot antes de cerrar la tarea.
- **SC-003**: La aplicación inicia con migración automática y ejecuta seeding idempotente sin duplicados ni ejecuciones manuales del seeder.
- **SC-004**: Los archivos de seed y las imágenes quedan incorporados al proyecto real y están disponibles en runtime para el proceso de bootstrap y para futuras consultas de propiedades con imagen.
- **SC-005**: El valor `ImageUrl` persistido apunta a la ruta pública servida por la API y no a un path físico de `support`, permitiendo consumo directo por la capa de consulta futura.
- **SC-006**: La suite de pruebas asociada al perímetro de persistencia valida la configuración EF, la migración, la idempotencia del seeding y la consistencia del manifest de assets.

## Suposiciones

- La estructura base de la solución existente ya está creada y no debe duplicarse ni reconstruirse en esta especificación.
- Los archivos fuente de datos (`properties.json`, `properties-statuses.json`, `properties`, `seed-manifest.json`) ya existen o se entregan como parte del contenido de soporte del repositorio y se integran en la API durante la implementación.
- El arranque de la aplicación es el punto correcto para ejecutar migraciones y seed, con el patrón de EF Core definido por la documentación del repositorio.
- La ruta pública final de la imagen se resuelve al publicar o ejecutar la API, pero el valor persistido debe representarla de forma estable para futuras consultas y no depender de ubicaciones físicas de desarrollo.
- Las futuras features de negocio reutilizarán el dominio y la infraestructura de persistencia aquí definidos, manteniendo la arquitectura Vertical Slice y sin crear una capa paralela de endpoints de negocio.
