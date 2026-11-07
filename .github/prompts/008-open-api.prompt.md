---
name: 008-open-api
agent: speckit.specify
---

/speckit.specify

Crea una nueva spec que se llamara 008-open-api.

Generar un único documento OpenAPI versionado (v1) del contrato HTTP público de RealtorApi, publicado como openapi/v1.json y accesible por una ruta HTTP estable GET /openapi/v1.json. El documento debe generarse a partir de la implementación real del backend (Minimal APIs), nunca por edición manual.

## Endpoints en alcance

El documento debe cubrir el 100% de los endpoints implementados, sin excepciones:
- GET /health
- GET /api/properties
- GET /api/properties/{id}
- POST /api/properties
- PUT /api/properties/{id}

Regla de normalización: las constraints del router ASP.NET Core (por ejemplo {id:guid}) se consideran equivalentes a su forma canónica sin constraint ({id}) tanto en el documento como en la verificación automatizada de drift.

## Contenido del contrato

Cada endpoint debe incluir método, ruta, parámetros de ruta y query, request body cuando aplique, responses de éxito y de error. El formato de error ProblemDetails debe representarse de forma consistente. Los tipos compartidos (PropertyStatus, ProblemDetails, HttpValidationProblemDetails) deben exponerse en components/schemas como componentes reutilizables referenciados mediante $ref, evitando duplicación inline.

## Exclusión explícita de UI

La solución NO debe incluir Swagger UI, ReDoc UI, ni ningún endpoint de exploración interactiva o interfaz visual de documentación. Solo se publica el documento estático JSON.

Aclaración importante que debe quedar documentada en la spec: las herramientas @redocly/cli y NSwag NO son interfaces interactivas; son herramientas CLI offline usadas exclusivamente durante desarrollo y validación. No forman parte de la superficie de la aplicación en runtime. Swagger UI es lo único que se excluye como característica de la app.

## Requisitos técnicos y herramientas externas (SECCIÓN OBLIGATORIA A INCLUIR EN LA SPEC)

La spec debe incluir una sección dedicada de "Requisitos técnicos y herramientas externas" que documente explícitamente todo lo necesario en la máquina de desarrollo ANTES de implementar y validar, para evitar fallos sorpresa en fase de validación:

### Requisitos de sistema
- Node.js 20+ instalado y disponible en PATH (verificable con node --version y npm --version).
- npx disponible para ejecutar @redocly/cli.

### Herramientas NPM
- @redocly/cli: validador de especificaciones OpenAPI. Debe poder ejecutarse vía npx @redocly/cli --version.

### Herramientas .NET locales
- NSwag (NSwag.ConsoleCore) registrado en dotnet-tools.json del repositorio, restaurable con dotnet tool restore y ejecutable con dotnet tool run nswag. NSwag se usa SOLO para generar un cliente C# tipado de prueba (smoke test) que verifica que el documento es consumible sin correcciones manuales; no se usa para servir Swagger ni para ninguna UI.

### Archivo de configuración requerido
- .redocly.yaml en la raíz del repositorio. Es un artefacto obligatorio de la feature. Redocly es estricto por defecto y rechaza APIs públicas sin esquema de seguridad. La configuración debe deshabilitar la regla security-defined (API pública sin autenticación) y degradar operation-4xx-response a warning. Sin este archivo, la validación Redocly falla con errores.

### Verificación previa de herramientas
La spec debe incluir un checklist de verificación previa a implementación:
- node --version (>= 20)
- npm --version
- npx @redocly/cli --version
- dotnet tool restore + dotnet tool list (NSwag disponible)

## Script reproducible de regeneración (TAREA/ARTEFACTO OBLIGATORIO)

Debe existir un script en support/scripts/generate-openapi-v1.ps1 que regenere y valide el documento de forma reproducible, ejecutando en secuencia:
1. dotnet build del proyecto RealtorApi (genera v1.json automáticamente vía OpenApiGenerateDocuments).
2. Validación con npx @redocly/cli lint sobre v1.json usando .redocly.yaml.
3. Generación de cliente tipado de prueba con NSwag hacia artifacts/openapi-client-smoke/.
4. Reporte de éxito con la ruta del documento y confirmación de validaciones.

El script debe fallar explícitamente si el documento no se genera o si alguna validación falla.

## Verificación automatizada de drift

Debe existir una verificación automatizada (pruebas xUnit) que falle cuando exista desalineación entre los endpoints reales del backend y el documento openapi/v1.json. Las pruebas deben:
- Comparar el inventario de endpoints en runtime (EndpointDataSource) contra los paths documentados, aplicando la normalización de constraints.
- Validar el documento con @redocly/cli lint (invocando la CLI externa).
- Generar un cliente tipado real con NSwag como smoke test.

Estas pruebas requieren que las herramientas externas estén instaladas; ese requisito debe estar documentado en la spec.

## Requisitos funcionales

- Generar un único documento OpenAPI con versión de API v1.
- Ubicar el documento en app/backend/src/RealtorApi/wwwroot/openapi/v1.json como archivo versionado del repositorio.
- Exponerlo mediante GET /openapi/v1.json servido como archivo estático.
- Describir el 100% de endpoints implementados incluyendo GET /health.
- Incluir método, ruta, parámetros, request body cuando aplique, y responses de éxito/error por endpoint.
- Representar ProblemDetails de forma consistente.
- Exponer tipos compartidos en components/schemas.
- Generar el documento desde la implementación real, no por edición manual.
- Disponer de verificación automatizada de drift que falle ante desalineación.
- Excluir cualquier UI de exploración interactiva.
- Incluir .redocly.yaml como artefacto de configuración de validación.
- Incluir support/scripts/generate-openapi-v1.ps1 como script reproducible.

## Criterios de éxito medibles

- 100% de endpoints implementados representados en v1.json, incluyendo GET /health.
- GET /openapi/v1.json responde 200 OK con Content-Type application/json en el 100% de validaciones funcionales.
- El documento se valida sin errores con npx @redocly/cli lint (usando .redocly.yaml) en el 100% de ejecuciones.
- La verificación de drift falla en el 100% de pruebas de drift inducido.
- Un consumidor genera un cliente tipado con dotnet tool run nswag openapi2csclient sin correcciones manuales del contrato.
- El script generate-openapi-v1.ps1 regenera y valida el documento sin errores.
- El archivo .redocly.yaml existe, es válido y contiene las reglas apropiadas para API pública.

## Assumptions

- El desarrollador tiene Node.js 20+ y puede ejecutar npx @redocly/cli.
- dotnet-tools.json incluye NSwag.ConsoleCore restaurable con dotnet tool restore.
- .redocly.yaml existe en la raíz del repositorio.
- El archivo estático wwwroot/openapi/v1.json es servible por la configuración de archivos estáticos existente.
- La implementación real del backend prevalece como fuente de verdad sobre contratos de specs previas.
- La feature se limita a una única versión de contrato (v1), sin versionado múltiple.
- No se modifica el comportamiento funcional de endpoints existentes; solo su representación contractual y su verificación de consistencia.