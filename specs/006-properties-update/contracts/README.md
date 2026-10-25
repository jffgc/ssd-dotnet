# Contratos de la feature 006: actualización de propiedades

## Endpoint público

### `PUT /api/properties/{id}`

**Propósito**: actualizar una propiedad existente por identificador, permitiendo cambios de negocio y una imagen opcional.

**Request**:
- `id` en la ruta.
- Campos editables del inmueble (`title`, `description`, `address`, `price`, `status`, `bedroomCount`, `bathroomCount`, `areaSquareMeters`).
- Archivo opcional `Image` en `multipart/form-data`.

**Comportamiento**:
- Si la propiedad no existe, responde `404 Not Found`.
- Si no se incluye imagen, conserva `imageUrl` previo.
- Si se incluye una imagen válida, guarda el asset y actualiza la URL pública o relativa.
- Si la imagen es inválida (vacía, formato no permitido, contenido real inválido), responde `415 Unsupported Media Type`.
- Si la imagen excede 5MB, responde `413 Payload Too Large`.
- Si falla el almacenamiento de la imagen, la operación debe quedar atómica y responder `500` sin dejar cambios parciales.

**Response exitosa**:
- `200 OK` con la entidad actualizada en el cuerpo de la respuesta.

**Erros esperados**:
- `400` para validación de negocio inválida.
- `404` si la entidad no existe.
- `413` si la imagen supera 5MB.
- `415` si el archivo es vacío, tiene contenido real inválido o formato no permitido.
- `500` si falla el almacenamiento interno de la imagen.

## Nota de compatibilidad

Este contrato se mantiene alineado con la arquitectura Minimal API, `ISlice`, `IHandler`, mapeo explícito y los patrones de validación y errores ya usados por la feature 005 de alta de propiedades.
