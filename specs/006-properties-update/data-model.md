# Modelo de datos: actualización de propiedades con imagen opcional

## Entidades y contratos del caso de uso

### Property

**Propósito**: entidad principal del catálogo inmobiliario y base del caso de uso de actualización.

**Campos principales**:
- `Id`: identificador único.
- `Title`: título visible de la propiedad.
- `Description`: descripción general del inmueble.
- `Address`: dirección.
- `Price`: precio de venta/alquiler.
- `Status`: estado de la propiedad, ya definido por `PropertyStatus`.
- `BedroomCount`: número de habitaciones.
- `BathroomCount`: número de baños.
- `AreaSquareMeters`: superficie.
- `ImageUrl`: ruta pública del archivo, conservada si no se envía una nueva imagen.
- `CreatedAt`: fecha de creación.
- `UpdatedAt`: fecha de última actualización.

**Reglas**:
- `ImageUrl` puede permanecer en el valor previo cuando la actualización no incluye imagen.
- Cuando exista una nueva imagen válida, `ImageUrl` debe apuntar a la ruta pública servida por la API y no a una ruta local del entorno.
- El modelo persistente debe seguir la semántica definida por las specs previas de foundation y persistencia.

### PropertyStatus

**Propósito**: reutilizar el dominio de estados ya definido por la especificación de persistencia.

**Valores válidos**:
- `Available`
- `Rented`
- `Maintenance`

### UpdatePropertyRequest

**Propósito**: entrada del caso de uso de actualización.

**Campos esperados**:
- `Id` o path parameter del endpoint.
- `Title`: cadena editable, si se envía.
- `Description`: cadena editable, si se envía.
- `Address`: cadena editable, si se envía.
- `Price`: decimal positivo, si se envía.
- `Status`: `Available`, `Rented` o `Maintenance`.
- `BedroomCount`: entero no negativo.
- `BathroomCount`: entero no negativo.
- `AreaSquareMeters`: decimal positivo.
- `Image`: archivo multipart opcional.

**Reglas**:
- El `id` debe existir previamente en la base de datos.
- `Image` es opcional; si no se incluye, no debe alterarse `ImageUrl`.
- Si se incluye, debe cumplir tipo `png`, `jpg` o `jpeg` y tamaño máximo de 5MB.

### UpdatePropertyResponse

**Propósito**: salida del caso de uso con la entidad actualizada.

**Campos esperados**:
- `Id`
- `Title`
- `Description`
- `Address`
- `Price`
- `Status`
- `BedroomCount`
- `BathroomCount`
- `AreaSquareMeters`
- `ImageUrl`
- `CreatedAt`
- `UpdatedAt`

### UploadedImageInfo

**Propósito**: representación del archivo validado y preparado para almacenamiento.

**Campos**:
- `FileName`
- `ContentType`
- `LengthBytes`
- `StoredPath`
- `PublicUrl`

## Relaciones

- `Property` reutiliza `PropertyStatus` como estado canónico del dominio.
- El archivo actualizado no es una entidad persistente separada dentro de esta feature; el almacenamiento se materializa en la ruta final y en el campo `ImageUrl` de la propiedad.

## Reglas de validación

- Los campos de negocio editables deben validarse en el validator del slice.
- El archivo, si existe, debe cumplir el tipo permitido y tamaño máximo.
- Los nombres generados para la imagen deben evitar colisiones y garantizar unicidad.
- `ImageUrl` debe persistirse como ruta pública no local y compatible con `UseStaticFiles` del backend.

## Estado de transiciones

La propiedad se encuentra en un estado de entidad válida en la base de datos; la transacción de actualización reemplaza solo los campos solicitados y mantiene el resto inalterado. Si la operación falla por archivo o validación, la entidad no debe quedar parcialmente actualizada.
