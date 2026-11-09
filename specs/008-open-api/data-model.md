# Modelo de datos: OpenAPI v1 de RealtorApi

## Entidades y tipos del dominio

### Property

Representa la entidad persistida del catálogo inmobiliario ya existente en el backend.

| Campo | Tipo | Requisito | Observación |
|---|---|---|---|
| `Id` | `Guid` | Obligatorio | Identificador principal del inmueble. |
| `Title` | `string` | Obligatorio | Título público de la propiedad. |
| `Description` | `string` | Obligatorio | Descripción legible del inmueble. |
| `Address` | `string` | Obligatorio | Dirección del inmueble. |
| `Price` | `decimal` | Obligatorio | Precio del bien. |
| `Status` | `PropertyStatus` | Obligatorio | Valor de dominio: `Available`, `Rented`, `Maintenance`. |
| `BedroomCount` | `int` | Obligatorio | Cantidad de dormitorios. |
| `BathroomCount` | `int` | Obligatorio | Cantidad de baños. |
| `AreaSquareMeters` | `decimal` | Obligatorio | Superficie del inmueble. |
| `ImageUrl` | `string?` | Opcional | Ruta pública o relativa de la imagen asociada. |
| `CreatedAt` | `DateTime` | Obligatorio | Fecha de creación. |
| `UpdatedAt` | `DateTime?` | Opcional | Fecha de última actualización. |

### PropertyStatus

```text
enum PropertyStatus {
  Available,
  Rented,
  Maintenance
}
```

## Modelos de entrada/salida HTTP

La documentación OpenAPI observará la API real implementada por los endpoints Minimal API del backend, sin crear modelos duplicados para la feature. Los tipos pueden incluir los que ya existen en las features de listado, creación y actualización.

### `ProblemDetails`

Tipo estándar de ASP.NET Core para errores de servidor o no encontrado, con estructura compatible con RFC 7807.

| Campo | Tipo | Observación |
|---|---|---|
| `type` | `string?` | URL o identificador del tipo de error. |
| `title` | `string?` | Título breve del problema. |
| `status` | `int?` | Código HTTP. |
| `detail` | `string?` | Descripción textual. |
| `instance` | `string?` | Ruta de la solicitud cuando aplica. |

### `HttpValidationProblemDetails`

Tipo estándar para errores de validación con errores por campo.

| Campo | Tipo | Observación |
|---|---|---|
| `type` | `string?` | Identificador del tipo. |
| `title` | `string?` | Título del problema. |
| `status` | `int?` | Código HTTP, usualmente 400. |
| `detail` | `string?` | Descripción general. |
| `instance` | `string?` | Ruta de la solicitud. |
| `errors` | `object` | Mapa de campo a lista de mensajes de validación. |

## Esquema funcional del documento OpenAPI v1

El documento se configurará para que exprese estas rutas:

- `GET /health`
- `GET /api/properties`
- `GET /api/properties/{id}`
- `POST /api/properties`
- `PUT /api/properties/{id}`

## Reglas de contrato para esta feature

- Los tipos compartidos deben registrarse bajo `components/schemas` y reutilizarse con `$ref`.
- La ruta `GET /openapi/v1.json` debe servir el JSON generado y no el endpoint interactivo.
- Las rutas documentadas deben normalizar constraints del router para compararlas con el runtime.
- Los errores de red, validación y negocio deben representarse como `ProblemDetails` o `HttpValidationProblemDetails` de forma consistente.
- La versión del contrato debe ser `v1`, con un único documento para la API pública.

## No hay cambios en el modelo de dominio

La feature no introduce nuevas entidades de negocio ni alteraciones en `Property` ni en `PropertyStatus`. La documentación contractual se apoya en el dominio que ya existe y en la representacion actual de la API http pública.
