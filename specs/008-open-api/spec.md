# Especificación de la Funcionalidad: OpenAPI v1 de RealtorApi

**Feature Branch**: `008-open-api`

**Creado**: 2026-11-08

**Estado**: Implementada

**Entrada**: Descripción del usuario: "Crea una nueva spec que se llamara 008-open-api. Generar un único documento OpenAPI versionado (v1) del contrato HTTP público de RealtorApi, publicado como openapi/v1.json y accesible por una ruta HTTP estable GET /openapi/v1.json. El documento debe generarse a partir de la implementación real del backend (Minimal APIs), nunca por edición manual."

## Escenarios de usuario y pruebas

### Historia de usuario 1 - Contrato público derivado del backend real (Prioridad: P1)

El equipo de backend necesita un contrato HTTP público y verificable para la API de Realtor que esté siempre alineado con la implementación real de Minimal APIs. El documento OpenAPI debe representarse como artefacto generado, no escrito a mano, y debe incluir todo el inventario de endpoints expuestos por el backend.

**Por qué esta prioridad**: Es la base de la interoperabilidad del sistema y garantiza que la API pública se documente en función del código real, evitando drift funcional y documentales.

**Prueba independiente**: Se puede verificar generando el documento desde el backend y comprobando que incluye el 100% de los endpoints implementados, con rutas normalizadas, request/response y componentes compartidos.

**Escenarios de aceptación**:

1. **Dado** que el backend implementa endpoints Minimal API reales, **cuando** se genera el documento OpenAPI v1, **entonces** el archivo resultante refleja exactamente el inventario corriente de rutas públicas del backend sin edición manual.
2. **Dado** que existen constraints del router ASP.NET Core como `{id:guid}`, **cuando** se compara el contrato con el runtime, **entonces** la ruta documentada se normaliza a su forma canónica sin constraint para la verificación y el documento final.
3. **Dado** que la API pública incluye errores de validación y no encontrado, **cuando** se genera el documento, **entonces** los errores siguen el formato ProblemDetails y se exponen como componentes reutilizables en `components/schemas`.
4. **Dado** que la aplicación expone un conjunto de endpoints, **cuando** se valida el contrato, **entonces** el documento incluye `GET /health`, `GET /api/properties`, `GET /api/properties/{id}`, `POST /api/properties` y `PUT /api/properties/{id}` sin omisiones.

### Historia de usuario 2 - Publicación estática y exclusión explícita de UI (Prioridad: P2)

Los consumidores y validadores necesitan un documento estático y estable, sin depender de una interfaz gráfica de exploración. La API debe publicar solo el JSON versionado, no la aplicación de Swagger UI ni ninguna alternativa visual interactiva.

**Por qué esta prioridad**: La especificación requiere una superficie HTTP pública mínima y determinista, sin introducir componentes de runtime que aumenten la exposición o complejidad del backend.

**Prueba independiente**: Se puede verificar accediendo a `GET /openapi/v1.json` y comprobando que devuelve `200 OK` con `Content-Type: application/json` y que la aplicación no expone Swagger UI, ReDoc ni interfaces interactivas.

**Escenarios de aceptación**:

1. **Dado** que la solución debe publicar una ruta HTTP estable, **cuando** se invoca `GET /openapi/v1.json`, **entonces** responde `200 OK` con contenido JSON de la especificación OpenAPI.
2. **Dado** que el documento debe publicarse como archivo estático del backend, **cuando** el servidor sirve contenido desde `wwwroot`, **entonces** se obtiene la ruta `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` como artefacto del repositorio y la ruta HTTP asociada es accesible.
3. **Dado** que la feature excluye cualquier UI interactiva, **cuando** se revisa la app en runtime, **entonces** no existe Swagger UI ni ReDoc UI ni endpoint de exploración visual dentro de la app.
4. **Dado** que herramientas como @redocly/cli o NSwag son herramientas CLI de validación y generación, **cuando** se usan durante desarrollo, **entonces** no forman parte de la superficie HTTP de runtime y no se interpretan como UI de la aplicación.

### Historia de usuario 3 - Verificación, validación y protección ante drift (Prioridad: P3)

La solución debe contar con validación automatizada y scripts reproducibles para detectar desalineaciones entre el código real y el documento. Esto incluye validación Redocly, smoke test con NSwag y pruebas xUnit de drift.

**Por qué esta prioridad**: La documentación pública no debe quedar obsoleta ni incumplir la especificación; la validación debe detectar fallos antes de proliferar drift en el contrato.

**Prueba independiente**: Se puede verificar ejecutando el script de regeneración y la prueba de drift inducido, y comprobando que ambas fallan cuando se corrompe el contrato o el endpoint real.

**Escenarios de aceptación**:

1. **Dado** que el backend y el documento deben mantenerse sincronizados, **cuando** un endpoint cambia o desaparece, **entonces** la prueba de drift detecta la discrepancia y falla.
2. **Dado** que el contrato debe validarse de forma estricta, **cuando** se ejecuta `npx @redocly/cli lint`, **entonces** la validación usa `.redocly.yaml` con las reglas apropiadas para una API pública sin autenticación.
3. **Dado** que la generación del cliente tipado es una comprobación de consumibilidad, **cuando** se ejecuta NSwag, **entonces** el cliente se genera sin correcciones manuales del contrato.
4. **Dado** que existen dependencias externas necesarias, **cuando** el equipo prepara la máquina, **entonces** la feature documenta explícitamente la verificación previa de Node.js, npx y NSwag para evitar fallos sorpresa durante la validación.

## Casos límite

- ¿Qué ocurre si el backend agrega un endpoint nuevo y el documento no lo refleja? Debe detectarse como drift y la verificación automatizada debe fallar.
- ¿Qué ocurre si el contrato usa constraints como `{id:guid}` y la especificación normaliza a `{id}`? Debe tratarse como equivalente en el documento y en las pruebas de drift.
- ¿Qué ocurre si algún error no está modelado como `ProblemDetails`? Debe rechazarse porque la especificación exige un formato consistente.
- ¿Qué ocurre si se publica Swagger UI o ReDoc UI dentro de la app? La feature debe considerarse no conforme, porque su exclusión es parte del alcance explícito.
- ¿Qué ocurre si el documento no se genera desde la implementación real? La feature debe rechazarse por violar la regla de generación a partir del código.
- ¿Qué ocurre si el archivo `v1.json` no existe o no se valida? El script reproductible debe fallar explícitamente.
- ¿Qué ocurre si falta `.redocly.yaml` o tiene configuración incorrecta? La validación de Redocly falla y la feature no está completa.
- ¿Qué ocurre si no está instalado Node.js 20+ o npx? La verificación previa debe reportar un bloqueante antes de implementar y validar.
- ¿Qué ocurre si NSwag no ha sido restaurado con `dotnet tool restore`? La feature no se puede validar totalmente y debe documentarse como precondición.

## Requisitos

### Requisitos funcionales

- **FR-001**: El backend DEBE generar un único documento OpenAPI v1 del contrato HTTP público de la API.
- **FR-002**: El documento DEBE ubicarse en `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` como artefacto versionado del repositorio.
- **FR-003**: La API DEBE publicar `GET /openapi/v1.json` como recurso estático y estable.
- **FR-004**: El contrato DEBE cubrir el 100% de los endpoints implementados en el backend, incluyendo `GET /health`, `GET /api/properties`, `GET /api/properties/{id}`, `POST /api/properties` y `PUT /api/properties/{id}`.
- **FR-005**: La normalización de rutas DEBE considerar que los constraints del router (por ejemplo `{id:guid}`) son equivalentes a su versión canónica sin constraint ({id}) en el documento y en la verificación automatizada de drift.
- **FR-006**: Cada endpoint DEBE incluir método, ruta, parámetros de ruta y query, request body cuando aplique, y responses de éxito y error.
- **FR-007**: Los errores DEBEN representarse mediante `ProblemDetails` y `HttpValidationProblemDetails`, con una representación consistente en el documento.
- **FR-008**: Los tipos compartidos (`PropertyStatus`, `ProblemDetails`, `HttpValidationProblemDetails`) DEBEN exponerse como componentes reutilizables bajo `components/schemas` y referenciados mediante `$ref`.
- **FR-009**: El documento DEBE generarse a partir de la implementación real del backend Minimal API, nunca por edición manual ni por duplicación de especificaciones previas.
- **FR-010**: La solución DEBE excluir cualquier UI de exploración interactiva, incluyendo Swagger UI y ReDoc UI, y NO DEBE incluir ningún endpoint visual o explorador web en runtime.
- **FR-011**: La feature DEBE incluir un archivo `.redocly.yaml` obligatorio en la raíz del repositorio para validación de especificaciones, con la regla `security-defined` deshabilitada y `operation-4xx-response` configurada como warning.
- **FR-012**: Debe existir el script reproducible `support/scripts/generate-openapi-v1.ps1` que regenera y valida el documento siguiendo el flujo definido: build del proyecto, Redocly lint, generación de cliente de prueba con NSwag y reporte final de éxito.
- **FR-013**: La feature DEBE incluir verificación automatizada de drift en xUnit que compare los endpoints reales de runtime contra el documento OpenAPI, aplicando la normalización de constraints.
- **FR-014**: La verificación automatizada DEBE validar el documento con `npx @redocly/cli lint` e integrar el smoke test con `dotnet tool run nswag` para confirmar que el documento es consumible sin correcciones manuales.
- **FR-015**: La feature DEBE incluir una comprobación explícita de que las herramientas externas están instaladas antes de validar el contrato, documentando Node.js 20+, `npx`, `@redocly/cli` y NSwag.
- **FR-016**: La feature DEBE limitarse a una única versión de API (`v1`), sin versionado paralelo ni duplicación de documentos.
- **FR-017**: El documento DEBE ser un artefacto público de la API. No se modificarán endpoints existentes; se autoriza implementar únicamente `GET /api/properties/{id}` porque forma parte del contrato requerido por FR-004 y no existía en el backend.

### Requisitos técnicos y herramientas externas

#### Requisitos de sistema

- **TR-001**: Node.js 20+ DEBE estar instalado y disponible en `PATH`, verificable con `node --version` y `npm --version`.
- **TR-002**: `npx` DEBE estar disponible para ejecutar `@redocly/cli` desde la línea de comandos.
- **TR-003**: El repositorio DEBE utilizar el SDK de .NET definido por `global.json`, según la gobernanza del proyecto.

#### Herramientas NPM

- **TR-004**: `@redocly/cli` DEBE ser una herramienta de validación disponible vía `npx @redocly/cli --version`.
- **TR-005**: La validación del documento OpenAPI DEBE ejecutarse con `npx @redocly/cli lint` usando la configuración de `.redocly.yaml` del repositorio.

#### Herramientas .NET locales

- **TR-006**: NSwag (específicamente `NSwag.ConsoleCore`) DEBE estar registrado en `.config/dotnet-tools.json` del repositorio.
- **TR-007**: NSwag DEBE restaurarse con `dotnet tool restore` y ejecutarse con `dotnet tool run nswag`.
- **TR-008**: NSwag se usa SOLO para generar un cliente C# tipado en pruebas de humo y validar consumibilidad del documento; NO formará parte de la app runtime ni de la superficie de servicio.
- **TR-009**: La herramienta NO se usa para servir Swagger UI ni para ninguna interfaz visual de documentación.

#### Archivo de configuración requerido

- **TR-010**: `.redocly.yaml` DEBE existir en la raíz del repositorio como artefacto obligatorio de la feature.
- **TR-011**: La configuración DEBE deshabilitar la regla `security-defined` porque la API pública no tiene autenticación configurada.
- **TR-012**: La configuración DEBE degradar `operation-4xx-response` a `warning` para evitar errores por la naturaleza pública de la API sin romper el objetivo de validación.
- **TR-013**: Sin este archivo, la validación Redocly queda incompleta y la feature no es aceptable.

#### Verificación previa de herramientas

- **TR-014**: Antes de implementar o validar, el equipo DEBE verificar:
  - `node --version` (>= 20)
  - `npm --version`
  - `npx @redocly/cli --version`
  - `dotnet tool restore`
  - `dotnet tool list` para confirmar que NSwag está disponible

### Script reproducible de regeneración

- **FR-018**: Debe existir `support/scripts/generate-openapi-v1.ps1` y ejecutarse secuencialmente en esta secuencia:
  1. `dotnet build` del proyecto `RealtorApi` para generar `v1.json` automáticamente vía `OpenApiGenerateDocuments`.
  2. Validación con `npx @redocly/cli lint` sobre `v1.json` usando `.redocly.yaml`.
  3. Generación del cliente tipado de prueba con NSwag hacia `artifacts/openapi-client-smoke/`.
  4. Impresión de reporte final con la ruta del documento y confirmación de validaciones.
- **FR-019**: El script DEBE fallar explícitamente si el documento no se genera, si la validación falla o si alguna fase crítica no finaliza correctamente.
- **FR-020**: El script DEBE estar pensado como mecanismo reproducible para desarrolladores y CI, no como tarea manual del editor.

### Verificación automatizada de drift

- **FR-021**: Debe existir una prueba xUnit de drift que compare el inventario de endpoints reales del backend con los paths documentados en `openapi/v1.json`, aplicando la normalización de constraints.
- **FR-022**: La prueba DEBE fallar cuando exista desalineación entre el runtime y el documento generado.
- **FR-023**: La prueba DEBE ejecutar validación del documento con `@redocly/cli lint` y un smoke test con NSwag para validar que el contrato sigue siendo consumible.
- **FR-024**: La verificación no puede depender de una versión manual del documento; la fuente de verdad debe ser el backend ejecutando el runtime actual.

### Exclusión explícita de UI

- **FR-025**: La solución NO DEBE incluir Swagger UI, ReDoc UI ni ningún tipo de exploración interactiva de la API en runtime.
- **FR-026**: `@redocly/cli` y `NSwag` NO son interfaces interactivas; son herramientas de línea de comandos usadas en desarrollo y validación y no forman parte de la aplicación ejecutada.
- **FR-027**: La app debe exponer únicamente el documento estático JSON y no la documentación visual en runtime.

## Requisitos no funcionales

- **NFR-001**: El documento OpenAPI DEBE generarse por pipeline automatizado desde la app real, evitando dependencia de edición manual o aligeración heredada del contrato.
- **NFR-002**: La validación DEBE ser reproducible y ejecutable desde el entorno de desarrollo con herramientas locales y la configuración del repositorio.
- **NFR-003**: El proceso de generación y validación DEBE ser determinista y fallar con mensajes claros si falta una herramienta o si el espectro del documento no coincide con runtime.
- **NFR-004**: La superficie HTTP pública DEBE ser mínima y estable; solo se publica el JSON contract y no una experiencia visual de documentación.
- **NFR-005**: La implementación DEBE conservar la arquitectura actual del backend, sin introducir controllers, UI ni complejidades ajenas a la publicación del contrato.
- **NFR-006**: El flujo de validación DEBE ser compatible con la gobernanza Spec-Driven del repositorio y con la estructura de proyectos ya existente.

## Criterios de éxito

### Resultados medibles

- **SC-001**: El 100% de los endpoints implementados, incluyendo `GET /health`, queda representado en el documento OpenAPI v1.
- **SC-002**: `GET /openapi/v1.json` responde `200 OK` con `Content-Type: application/json` en todas las validaciones funcionales.
- **SC-003**: El documento valida sin errores con `npx @redocly/cli lint` usando `.redocly.yaml` en el 100% de las ejecuciones.
- **SC-004**: La prueba de drift falla en el 100% de los casos de desalineación inducidos y no permite silencios ni inconsistencias.
- **SC-005**: Un consumidor de la API puede generar un cliente C# tipado con `dotnet tool run nswag openapi2csclient` sin correcciones manuales del contrato.
- **SC-006**: El script `generate-openapi-v1.ps1` regenera el documento y valida correctamente toda la implementación sin errores de entorno.
- **SC-007**: El archivo `.redocly.yaml` existe, es válido y contiene las reglas requeridas para una API pública sin autenticación.
- **SC-008**: El documento se publica como `openapi/v1.json` estático y no existe ninguna UI de documentación interactiva en runtime.
- **SC-009**: La integración de `@redocly/cli` y `NSwag` queda documentada como parte de la validación del desarrollo, no como superficie HTTP productiva.

## Compatibilidad con specs previas y gobernanza del repositorio

- **001-realtor-solution-foundation**: La feature respeta la base del backend y la estructura de solución ya aprobada.
- **002-foundation-backend**: La generación del documento se integra con la infraestructura Minimal API y la configuración base del servicio.
- **003-properties-persistence-seeding**: La documentación refleja el estado actual del backend sin modificar la persistencia ni los datos semilla.
- **004-properties-list-pagination**: El contrato del listado y la normalización de rutas mantienen compatibilidad con los endpoints ya definidos.
- **005-properties-create**: La representación de creación y errores sigue la semántica del backend real y la documentación de `ProblemDetails`.
- **006-properties-update**: La especificación del OpenAPI no altera el comportamiento de los endpoints ya implementados; solo representa contratualmente el código real.
- **Gobernanza Spec-Driven**: La feature queda descrita como una especificación de documentación y validación de contrato, con alcance, priorización, requisitos y verificaciones independientes.

## Suposiciones

- El desarrollador tiene Node.js 20+ instalado y disponible en `PATH`.
- `npx @redocly/cli` puede ejecutarse desde la terminal del repositorio.
- `.config/dotnet-tools.json` incluye `NSwag.ConsoleCore` y puede restaurarse con `dotnet tool restore`.
- El archivo `.redocly.yaml` existe en la raíz y define reglas de validación adecuadas para una API pública sin autenticación.
- El backend es la fuente de verdad del contrato y prevalece sobre cualquier especificación manual previa.
- La feature se limita a una única versión de API (`v1`) y no implica versionado múltiple ni otras interfaces de documentación.
- No se modifica el comportamiento de endpoints existentes. La única ampliación funcional autorizada es la consulta de lectura `GET /api/properties/{id}` requerida por FR-004.
