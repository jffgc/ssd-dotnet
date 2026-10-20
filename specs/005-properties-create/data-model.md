# Modelo de datos: alta de propiedades con imagen opcional

## Entidades y contratos del caso de uso

### Property

**Propósito**: Entidad principal del catálogo inmobiliario y base del caso de uso de creación.

**Campos principales**:
- `Id`: identificador único.
- `Title`: nombre visible de la propiedad.
- `Description`: descripción del inmueble.
- `Address`: dirección.
- `Price`: precio de venta/alquiler.
- `Status`: estado de la propiedad, ya definido por `PropertyStatus`.
- `BedroomCount`: número de habitaciones.
- `BathroomCount`: número de baños.
- `AreaSquareMeters`: superficie.
- `ImageUrl`: ruta pública del archivo, `null` si no se cargó.
- `CreatedAt`: fecha de creación.
- `UpdatedAt`: fecha de última actualización.

**Reglas**:
- `ImageUrl` puede ser `null` cuando la propiedad se crea sin imagen.
- Cuando exista imagen, debe apuntar a la URL pública servida por la API, no a una ruta local de desarrollo.
- Los datos persistidos deben seguir la semántica establecida por la spec previa de persistencia.

### PropertyStatus

**Propósito**: reutilizar el dominio de estados ya definido en la base del proyecto.

**Valores válidos**:
- `Available`
- `Rented`
- `Maintenance`

### CreatePropertyRequest

**Propósito**: entrada del caso de uso.

**Campos esperados**:
- `Title`: cadena obligatoria.
- `Description`: cadena obligatoria.
- `Address`: cadena obligatoria.
- `Price`: decimal positivo.
- `Status`: `Available`, `Rented` o `Maintenance`.
- `BedroomCount`: entero no negativo.
- `BathroomCount`: entero no negativo.
- `AreaSquareMeters`: decimal positivo.
- `Image`: archivo multipart opcional.

**Reglas**:
- `Image` no es obligatorio.
- Si se incluye, debe tener formato `png`, `jpg` o `jpeg` y tamaño máximo de 5MB.

### CreatePropertyResponse

**Propósito**: salida del caso de uso con la propiedad recién creada.

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
- El archivo subido no es una entidad persistente separada para esta feature; su información se materializa únicamente en la ruta y en el campo `ImageUrl` de la propiedad.

## Reglas de validación

- `Title`, `Description`, `Address`, `Price`, `Status` y `AreaSquareMeters` deben ser válidos en la entrada del caso de uso.
- El archivo si existe debe cumplir el tipo permitido y el tamaño máximo.
- `ImageUrl` debe persistirse como una ruta pública y no como una ruta de soporte del repositorio.
- Todos los parámetros del request deben ser validados por el validator del slice antes de guardar la entidad.

## Estado de transiciones

No se requiere un state machine complejo para esta feature; la entidad se crea en estado válido y el cambio de estado queda para futuras features.
