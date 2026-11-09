# Investigación: OpenAPI v1 de RealtorApi

## Decisión 1: Generar el documento desde la implementación real del backend

**Decisión**: El OpenAPI v1 se generará desde la API real de `RealtorApi`, usando la infraestructura existente de ASP.NET Core Minimal API y un proceso de build/artefacto que produzca `wwwroot/openapi/v1.json`.

**Rationale**: La spec exige que el documento sea el reflejo del runtime, no una referencia escrita a mano. La llamada a `EndpointDataSource` y los endpoints reales son la única fuente de verdad y evitan drift entre código y contrato.

**Alternativas consideradas**:
- Mantener un `openapi.json` manual: rechazado porque no reflejaría el runtime ni cumpliría la comprobación de drift.
- Generar documento desde un archivo de specs ajeno al backend: rechazado porque la feature exige que la fuente sea la implementación real.

## Decisión 2: Publicar un documento estático JSON, sin UI

**Decisión**: La app servirá solo `GET /openapi/v1.json`; no se añadirá Swagger UI ni ReDoc UI ni ningún endpoint interactivo de documentación.

**Rationale**: La spec exige una API pública mínima y determinista. La documentación debe ser una respuesta JSON estática y accesible, no una UI de navegador ni un panel interactivo.

**Alternativas consideradas**:
- Añadir Swagger UI: rechazado explícitamente por la spec.
- Añadir ReDoc: rechazado como interfaz visual dentro de la aplicación.
- Usar `@redocly/cli` o NSwag como runtime: rechazado, porque esas herramientas son CLI y no forman parte de la superficie HTTP del backend.

## Decisión 3: Normalizar rutas con constraints a la forma canónica

**Decisión**: El documento y las pruebas de drift deben tratar `{id:guid}` como equivalente a `{id}`.

**Rationale**: La spec exige esta equivalencia para evitar falsos positivos en la validación, ya que ASP.NET Core usa constraints en routing y la documentación pública suele exponerse en forma canónica.

**Alternativas consideradas**:
- Comparar literal `/{id:guid}` contra el documento generado: rechazado porque no refleja la normalización requerida.
- Ignorar la normalización: rechazado por la regla del proyecto.

## Decisión 4: No introducir cambios funcionales ni de dominio

**Decisión**: La feature será estrictamente documental y validatoria; no se cambiará la lógica de endpoints ni el modelo de dominio existente.

**Rationale**: La constitución y la spec exigen que la documentación no altere la semántica del negocio ni el comportamiento ya aprobado.

**Alternativas consideradas**:
- Añadir metadata funcional o cambios de ruta: rechazado porque cambia la superficie del sistema fuera del alcance de la feature.
- Incluir lógica de validación o errores nuevos: rechazado porque la feature solo documenta el contrato de la API actual.

## Decisión 5: Validación con Redocly y NSwag como herramientas externas de desarrollo

**Decisión**: La validación se ejecutará con `npx @redocly/cli lint` y la generación del cliente se hará con `dotnet tool run nswag` en una carpeta temporal de smoke test.

**Rationale**: El repositorio exige explícitamente herramientas externas, documentación de requisitos previos y un flujo de validación reproducible. NSwag se usará únicamente como prueba de consumibilidad; no se servirá en runtime.

**Alternativas consideradas**:
- Validar solo con el compilado y no con CLI: rechazado por la spec.
- Usar NSwag como origen del documento: rechazado porque el contrato debe generarse desde runtime y luego verificarse con NSwag.

## Decisión 6: Implementación de drift verification con runtime inventory

**Decisión**: La prueba de drift comparará el inventario de endpoints de `EndpointDataSource` contra el documento JSON usando la ruta normalizada, y además ejecutará `npx @redocly/cli lint` y `dotnet tool run nswag`.

**Rationale**: La spec exige comprobación automatizada que falle si el contrato no coincide con la API real, y requiere además validación externa del documento.

**Alternativas consideradas**:
- Chequear solo la ruta del archivo: insuficiente para detectar drift real.
- Validar solo schema: insuficiente para saber si aparecen o desaparecen endpoints reales.

## Riesgos y mitigaciones

**Riesgo**: Falta de Node 20+ o `npx` en la máquina.
- Mitigación: se documenta en el checklist previo y el script debe fallar con un mensaje claro si falta el entorno.

**Riesgo**: `.redocly.yaml` inexistente o mal configurado.
- Mitigación: la feature crea el archivo obligatorio y exige `security-defined` deshabilitada y `operation-4xx-response` en warning.

**Riesgo**: `NSwag.ConsoleCore` no restaurado.
- Mitigación: se añade `dotnet tool restore`; la verificación previa exige `dotnet tool list` para confirmar disponibilidad.

**Riesgo**: El documento no se genera o desaparece durante la build.
- Mitigación: el script de regeneración falla explícitamente si el archivo no existe o la validación no pasa.

## Resultado de la investigación

La feature se resuelve con una estrategia muy concreta: construir la API real, generar el documento OpenAPI a partir de ese runtime, publicar el JSON estático, validar con Redocly y smoke-test con NSwag, y comparar runtime vs contrato para impedir drift. No se añade ninguna UI de documentación ni se cambia el comportamiento del backend.
