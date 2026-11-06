---
name: 007-properties-get-by-id
agent: speckit.specify
---

/speckit.specify

Crear la especificación 007-properties-get-by-id.

Objetivo:
Definir e implementar un endpoint público de consulta por id que devuelva una única propiedad:
GET /api/properties/{id}

Contexto y alineación obligatoria:
- Basarse explícitamente en la spec 004-properties-list-pagination y mantener los mismos principios de contrato de respuesta.
- Reutilizar el mismo criterio de exposición de imagen al cliente:
  - `imageUrl` debe ser URL absoluta y pública.
  - Construcción con esquema y host de la request actual.
  - Patrón: http(s)://{host}/assets/properties/{fileName}
  - No devolver rutas físicas internas ni URLs relativas.
- Mantener arquitectura Vertical Slice + Minimal APIs existente (slice + handler + mapping + validator), auto-registro por ISlice/IHandler y sin controllers.
- Mantener cancelación con CancellationToken y contrato público explícito sin exponer entidades EF directamente.
- Mantener gobernanza Spec-Driven del repositorio y dependencias con specs previas (foundation, persistencia, listado, alta, actualización).

Resultado funcional esperado:
- Cuando el id existe, devolver una respuesta 200 con los campos públicos de propiedad usados en 004:
  - id, title, description, address, price, status, bedroomCount, bathroomCount, areaSquareMeters, imageUrl
- Cuando el id no existe, devolver 404 con ProblemDetails.
- Definir comportamiento ante id inválido según contrato público del proyecto (y documentarlo explícitamente en spec/contract).

Requisitos de especificación (formato Speckit):
- Historias de usuario con prioridad P1/P2/P3 y pruebas independientes.
- Escenarios de aceptación claros.
- Casos límite (id inexistente, id inválido, propiedad sin imagen, host/esquema variable por entorno).
- Requisitos funcionales testables (FR-xxx).
- Criterios de éxito medibles (SC-xxx).
- Suposiciones explícitas.
- Contrato OpenAPI de la operación en contracts/backend-properties-get-by-id.openapi.yaml.
- Data model y quickstart de validación manual.

Criterios mínimos de response:
- `imageUrl` siempre consumible por cliente externo cuando exista imagen.
- Contrato consistente con 004 para campos de propiedad.
- Sin metadatos de paginación en esta operación por ser consulta unitaria.