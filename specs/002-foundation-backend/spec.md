# Especificación de la Funcionalidad: Base de infraestructura del backend Realtor

**Feature Branch**: `002-foundation-backend`

**Creado**: 2026-09-21

**Estado**: Borrador

**Entrada**: Descripción del usuario: "Crear la especificación 002-foundation-backend. Objetivo: establecer la estructura base interna del backend RealtorApi y los cross-cutting concerns transversales, sin crear entidades de dominio ni features funcionales de producto. Esta iniciativa parte de la base ya creada en la spec 001 y NO debe recrear la solución ni los proyectos."

## Escenarios de Usuario y Pruebas

### Historia de usuario 1 - Infraestructura base del backend (Prioridad: P1)

El equipo necesita una estructura interna clara y reutilizable para el backend de Realtor, preparada para futuras features sin introducir lógica de negocio ni casos de uso concretos. La base debe permitir descubrir y mapear endpoints, registrar handlers y validar entradas de forma centralizada y consistente.

**Por qué esta prioridad**: Esta infraestructura es la base de todo el desarrollo posterior del backend y determina si el proyecto puede crecer manteniendo la arquitectura canónica del repositorio.

**Prueba independiente**: Se puede validar compilando el proyecto y ejecutando sus pruebas de infraestructura, con la confirmación de que el backend queda preparado para nuevas features sin tocar Program.cs para cada endpoint ni registrar dependencias manualmente.

**Escenarios de aceptación**:

1. **Dado** que el backend ya existe como proyecto .NET con la solución base, **cuando** un desarrollador añade una nueva unidad de endpoint, **entonces** el sistema la descubre automáticamente a través de un mecanismo centralizado sin requerir modificaciones manuales en Program.cs.
2. **Dado** que la estructura del backend sigue vertical slice architecture, **cuando** se revisa la organización del proyecto, **entonces** los slices y la infraestructura compartida quedan separados de las features de producto y no se crean controllers ni soluciones paralelas.
3. **Dado** que la aplicación necesita validación y manejo de errores, **cuando** se expone un endpoint con y sin validador, **entonces** el sistema aplica validación automática cuando existe un validator y responde con un error estructurado cuando falla la validación.

---

### Historia de usuario 2 - Registro automático y manejo de errores reutilizable (Prioridad: P1)

Los equipos de desarrollo necesitan un patrón único para registrar handlers y manejar fallos previstos de forma consistente. Esto reduce la duplicación de código, hace predecible la validación y permite que futuras features hereden capacidad operativa sin crear infraestructura ad hoc.

**Por qué esta prioridad**: El registro automático y la conversión de errores son capacidades transversalmente críticas para todas las features del backend; si no están bien fundamentadas, cada caso de uso añade complejidad innecesaria.

**Prueba independiente**: Se puede verificar con pruebas unitarias que validan el descubrimiento de handlers, la detección de validadores y la conversión de Result a ProblemDetails con sus status codes esperados.

**Escenarios de aceptación**:

1. **Dado** que existe una clasificación de handlers por marker interface, **cuando** se escanea el assembly del backend, **entonces** cada handler elegible queda registrado sin listados ni referencias manuales.
2. **Dado** que el sistema usa FluentValidation en la capa de entrada, **cuando** un request no cumple la validación, **entonces** devuelve un ValidationProblemDetails con HTTP 400.
3. **Dado** que existe un fallo previsto representado como Result, **cuando** se convierte al formato de respuesta de la API, **entonces** se retorna un ProblemDetails con el payload y el status code correctos.

---

### Historia de usuario 3 - Sonda de infraestructura y salud del sistema (Prioridad: P2)

El equipo necesita una prueba end-to-end mínima que demuestre que el descubrimiento y mapeo de endpoints funciona correctamente en el backend sin depender de una feature de negocio. La sonda /health debe servir como indicador operativo de la infraestructura.

**Por qué esta prioridad**: Permite verificar que la infraestructura centralizada realmente funciona en ejecución, no solo en pruebas unitarias aisladas.

**Prueba independiente**: Se puede ejecutar un request a /health y validar que responde con el estado esperado, confirmando que el endpoint fue migrado al patrón ISlice y descubierto sin registro manual.

**Escenarios de aceptación**:

1. **Dado** que el endpoint /health es infraestructura, **cuando** se despliega la aplicación, **entonces** la ruta responde correctamente y no introduce entidad de negocio ni base de datos.
2. **Dado** que el sistema usa discovery centralizado, **cuando** se revisa la configuración del programa, **entonces** Program.cs no contiene un mapeo de endpoints individual ni dependencias manuales para esa sonda.

---

### Casos límite

- ¿Qué ocurre si un desarrollador intenta añadir una feature de negocio en esta iniciativa? Debe rechazarse porque esta especificación limita el alcance a infraestructura transversal de backend.
- ¿Qué ocurre si un endpoint se registra manualmente en Program.cs? La solución no cumple la expectativa de auto-descubrimiento y centralización.
- ¿Qué ocurre si se crea una entidad de dominio, AppDbContext o migración como parte de la base? No es válido para esta iniciativa; la infraestructura no incluye persistencia ni lógica de negocio.

## Requisitos

### Requisitos funcionales

- **FR-001**: El backend DEBE organizarse internamente conforme a Vertical Slice Architecture, con una estructura base preparada para futuras features sin introducir casos de uso de producto.
- **FR-002**: El backend DEBE contar con una infraestructura de descubrimiento y mapeo de endpoints Minimal API basada en el contrato ISlice y en assembly scanning.
- **FR-003**: El sistema DEBE incluir un registro centralizado de slices mediante `RegisterSlices` por assembly, sin listados manuales ni descubrimiento por nombre de clase.
- **FR-004**: El sistema DEBE incluir un mapeador centralizado `MapSliceEndpoints` que orquesta la adición de endpoints sin requerir modificaciones en Program.cs para cada ruta nueva.
- **FR-005**: La app DEBE soportar auto-registro de handlers de casos de uso mediante la interfaz marker `IHandler` y assembly scanning, sin implementar handlers concretos de negocio en esta fase.
- **FR-006**: La validación automática de Minimal APIs DEBE registrarse mediante assembly scanning de validadores y una `ValidationFilterFactory` basada en la detección de `IValidator<T>`.
- **FR-007**: Si un validator existe para un tipo de request, el sistema DEBE ejecutar la validación automáticamente; si no existe, DEBE hacer pass-through sin error.
- **FR-008**: Los errores de validación DEBEN devolverse como `ValidationProblemDetails` con HTTP 400.
- **FR-009**: El sistema DEBE manejar errores esperados con `Result` y convertirlos centralmente a `ProblemDetails` con el status code y payload adecuados.
- **FR-010**: La aplicación DEBE incluir logging estructurado mediante `ILogger` como parte de la infraestructura compartida disponible para futuras features.
- **FR-011**: El endpoint `/health` DEBE migrarse al patrón ISlice y dejar de estar registrado manualmente en Program.cs; debe servir como sonda operativa de infraestructura.
- **FR-012**: `Program.cs` DEBE limitarse a configurar servicios, middleware, registrar la infraestructura anterior y mapear endpoints mediante el mecanismo centralizado.
- **FR-013**: El backend NO DEBE crear entidades de dominio, AppDbContext, migraciones, seeders, conexiones a base de datos ni casos de uso de negocio.
- **FR-014**: El backend NO DEBE usar controllers ni lógicas paralelas de integración para endpoints ni validación.
- **FR-015**: Las clases de prueba utilizadas para validar la infraestructura DEBEN vivir en `app/backend/tests/RealtorApiTests`, nunca en el proyecto de producción.
- **FR-016**: Las pruebas requeridas DEBEN verificar el descubrimiento de `ISlice`, la detección y ejecución de `IValidator<T>`, la conversión de `Result` a `ProblemDetails` y la respuesta del endpoint /health.

### Entidades clave

- **ISlice**: Contrato base para cada endpoint del backend. Define la capacidad de registrar la ruta sin intervención manual en Program.cs.
- **IHandler**: Marker de infraestructura para registrar handlers generados por assembly scanning en futuras features, sin incluir handlers concretos de negocio en esta fase.
- **Result**: Modelo de transporte para fallos esperados y respuestas de negocio futuras, convertido a ProblemDetails por la capa de infraestructura.
- **HealthStatus**: Estado mínimo de la sonda operativa del sistema que confirma que la infraestructura centralizada funciona correctamente.

## Criterios de éxito

### Resultados medibles

- **SC-001**: El backend compila con la infraestructura transversal registrada y la solución no requiere modificar Program.cs para añadir nuevos endpoints.
- **SC-002**: El endpoint `/health` responde correctamente como endpoint ISlice y funciona como comprobación de infraestructura del backend.
- **SC-003**: Un request validado por un `IValidator<T>` recibe validación automática; si no existe validador, la llamada se ejecuta sin error de validación.
- **SC-004**: Las pruebas unitarias de infraestructura pasan y validan el comportamiento real de discovery, validation y ProblemDetails.
- **SC-005**: No existen entidades de dominio, persistencia ni features de negocio implementadas en el backend de esta iniciativa.

## Suposiciones

- La estructura base del backend ya está creada en la solución principal y no se debe recrear la solución ni los proyectos existentes.
- La carpeta `app/backend/src/RealtorApi` es la ubicación canónica para la infraestructura base del backend durante esta iniciativa.
- El stack del repositorio permanece definido por la constitución y por `global.json`; no se modifica la versión de la plataforma ni la solución principal.
- Futuras features de negocio heredarán esta infraestructura y no necesitan crear registries ni validación manuales.
- La infra de salud es una sonda operativa, no una feature de producto ni una entidad de negocio.
