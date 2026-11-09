# Contratos de la feature 008-open-api

La API pública de `RealtorApi` debe exponer un único documento OpenAPI v1, generado a partir de la implementación real del backend y publicado como un archivo estático JSON.

## Ubicación del contrato

- `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`
- Ruta HTTP estable: `GET /openapi/v1.json`

## Reglas del contrato

- Debe incluir el 100% de los endpoints del backend en runtime.
- Debe incluir `GET /health`, `GET /api/properties`, `GET /api/properties/{id}`, `POST /api/properties` y `PUT /api/properties/{id}`.
- Debe normalizar constraints de ruta como `{id:guid}` a `{id}` para la comparación y la documentación.
- Debe reutilizar tipos compartidos de `ProblemDetails` y `HttpValidationProblemDetails` en `components/schemas`.
- Debe no incluir Swagger UI, ReDoc UI ni ninguna UI interactiva en runtime.

## Validación esperada

- `npx @redocly/cli lint` con `.redocly.yaml`
- `dotnet tool run nswag` como smoke test de cliente C#
- pruebas xUnit de drift contra `EndpointDataSource`

La fuente de verdad del contrato es el backend real, no una copia manual o un documento editado a mano.
