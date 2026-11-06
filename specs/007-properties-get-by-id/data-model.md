# Modelo de datos: consulta de propiedad por identificador

## Alcance persistente

Este caso de uso es de solo lectura. Reutiliza la entidad persistente `Property` y no crea ni modifica tablas, columnas, relaciones, índices, configuraciones o migraciones.

## Entrada

### Identificador de propiedad

| Campo | Tipo de contrato | Regla |
|---|---|---|
| `id` | UUID canónico | El segmento debe usar el formato con guiones `8-4-4-4-12`. Otros formatos reciben HTTP 400 con error asociado a `id`, incluso si .NET puede parsearlos como `Guid`. |

Un UUID canónico que no identifica una fila existente produce HTTP 404 con ProblemDetails. La distinción entre error de formato y ausencia de registro es parte del contrato público.

## Datos existentes consultados

### `Property` (entidad persistente existente)

| Propiedad | Uso en el contrato | Transformación |
|---|---|---|
| `Id` (`Guid`) | `id` | Sin transformación. |
| `Title` (`string`) | `title` | Valor proyectado. |
| `Description` (`string`) | `description` | Valor proyectado. |
| `Address` (`string`) | `address` | Valor proyectado. |
| `Price` (`decimal`) | `price` | Valor proyectado. |
| `Status` (`PropertyStatus`) | `status` | Nombre textual del miembro del enum existente. |
| `BedroomCount` (`int`) | `bedroomCount` | Valor proyectado. |
| `BathroomCount` (`int`) | `bathroomCount` | Valor proyectado. |
| `AreaSquareMeters` (`decimal`) | `areaSquareMeters` | Valor proyectado. |
| `ImageUrl` (`string?`) | `imageUrl` | `null` si no hay imagen; si existe, se obtiene el nombre de archivo y se construye la URL pública. |

`CreatedAt` y `UpdatedAt` no forman parte del contrato.

## Datos de salida

### Detalle de propiedad

El DTO explícito contiene exactamente los diez campos establecidos por el contrato de lectura de la spec 004: `id`, `title`, `description`, `address`, `price`, `status`, `bedroomCount`, `bathroomCount`, `areaSquareMeters` e `imageUrl`. `imageUrl` es obligatorio en la respuesta, pero su valor puede ser `null`. No se serializa directamente la entidad persistente.

No se incluyen `items`, `page`, `pageSize`, `totalItems`, `totalPages`, `hasNext` ni `hasPrevious`.

## Mapping de imagen

- Si `ImageUrl` persistido es `null`, la respuesta incluye `imageUrl: null`.
- Si existe imagen, se usa su nombre de archivo para construir `http(s)://{host}/assets/properties/{fileName}`.
- El esquema y host (incluido el puerto cuando corresponda) provienen de la solicitud actual.
- La salida siempre es una URL absoluta pública; no contiene una ruta relativa ni una ruta física del servidor.
- No se genera ni selecciona una imagen alternativa.

## Estados de consulta

| Condición | HTTP | Cuerpo |
|---|---:|---|
| GUID válido y propiedad existente | 200 | DTO de detalle con los diez campos. |
| Segmento no parseable como GUID | 400 | ProblemDetails o ValidationProblemDetails, con el error asociado a `id`. |
| GUID válido sin propiedad asociada | 404 | ProblemDetails. |

La consulta no cambia datos persistentes y propaga la cancelación de la solicitud mediante `CancellationToken`.
