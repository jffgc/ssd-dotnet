# Contratos de la feature: alta de propiedades

## Endpoint

### POST /api/properties

**Descripción**: crea una propiedad con datos básicos y un archivo opcional de imagen.

**Content-Type**: `multipart/form-data`

**Campos esperados**:
- `Title` (string, requerido)
- `Description` (string, requerido)
- `Address` (string, requerido)
- `Price` (number, requerido)
- `Status` (enum string, requerido: `Available`, `Rented`, `Maintenance`)
- `BedroomCount` (integer, requerido)
- `BathroomCount` (integer, requerido)
- `AreaSquareMeters` (number, requerido)
- `Image` (file, opcional)

**Respuesta exitosa 201 Created**:

```json
{
  "id": "7f2d3a2d-f1a9-4fc8-9d60-fc69c3c8526c",
  "title": "Casa de prueba",
  "description": "Propiedad de ejemplo",
  "address": "Calle Falsa 123",
  "price": 250000,
  "status": "Available",
  "bedroomCount": 3,
  "bathroomCount": 2,
  "areaSquareMeters": 120,
  "imageUrl": null,
  "createdAt": "2026-10-19T12:00:00Z",
  "updatedAt": null
}
```

**Respuesta con imagen**:

```json
{
  "id": "7f2d3a2d-f1a9-4fc8-9d60-fc69c3c8526c",
  "title": "Casa con imagen",
  "description": "Propiedad con foto",
  "address": "Avenida Siempre Viva 742",
  "price": 340000,
  "status": "Available",
  "bedroomCount": 4,
  "bathroomCount": 3,
  "areaSquareMeters": 180,
  "imageUrl": "/assets/properties/7f2d3a2d-f1a9-4fc8-9d60-fc69c3c8526c.jpg",
  "createdAt": "2026-10-19T12:00:00Z",
  "updatedAt": null
}
```

## Errores esperados

### 400 Bad Request / ProblemDetails

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "The uploaded file type is not supported. Only PNG, JPG and JPEG are allowed.",
  "errors": {
    "Image": ["Only PNG, JPG and JPEG files are allowed."]
  }
}
```

### 413 Payload Too Large / ProblemDetails

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.14",
  "title": "Payload Too Large",
  "status": 413,
  "detail": "The uploaded file exceeds the maximum allowed size of 5MB."
}
```

## Reglas contractuales

- `imageUrl` puede ser `null`.
- `Image` es opcional.
- Cuando se adjunta, solo se aceptan `png`, `jpg` y `jpeg`.
- El tamaño máximo permitido es `5MB`.
- La ruta final debe ser pública y servida por `wwwroot`.
- Los errores deben devolver `ProblemDetails` y no respuestas de texto libre.
