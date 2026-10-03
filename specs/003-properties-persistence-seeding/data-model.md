# Modelo de datos: propiedades y estados

## Entidades persistentes

### PropertyStatus

**Propósito**: Representa el estado operativo de una propiedad.

**Valores permitidos**:
- `Available`
- `Rented`
- `Maintenance`

**Persistencia**:
- Se mapeará con conversión EF Core a `string` en la base de datos.
- Debe mantenerse en una tabla o representación equivalente del dominio, con un valor textual estable y legible.

### Property

**Propósito**: Entidad principal del catálogo inmobiliario.

**Campos mínimos**:
- `Id` : identificador único de la propiedad.
- `Title` : título visible de la propiedad.
- `Description` : descripción del inmueble.
- `Address` : dirección física.
- `Price` : valor monetario de la propiedad.
- `Status` : estado de la propiedad, representado por `PropertyStatus` y persistido como string.
- `BedroomCount` : número de habitaciones.
- `BathroomCount` : número de baños.
- `AreaSquareMeters` : superficie en metros cuadrados.
- `ImageUrl` : ruta pública final servida por la API.
- `CreatedAt` : fecha de creación.
- `UpdatedAt` : fecha de actualización, nullable si aplica.

**Reglas**:
- `Title` y `Address` deben ser obligatorios y con longitud acotada según la evolución del schema.
- `Price` debe ser decimal con precisión suficiente para evitar truncamiento monetario.
- `AreaSquareMeters` debe ser decimal con precisión apropiada para superficies.
- `Status` debe ser validado y persistido como valor canónico del enum.
- `ImageUrl` debe apuntar a la ruta pública final del sitio/servicio y no a la ubicación física del volumen de soporte.

## Relaciones

- `Property` tiene un `Status` asociado con un conjunto de valores finitos representados por `PropertyStatus`.
- No se requieren relaciones complejas adicionales para esta feature; la entidad forma la base del catálogo y se ampliará con posteriores features.

## Reglas de validación

- Debe existir al menos un valor de estado para cada propiedad.
- Los campos críticos (`Title`, `Address`, `Price`, `Status`) deben estar completos.
- `ImageUrl` debe ser una URL válida o un path público compatible con el despliegue de la API.
- Los timestamps deben ser generados por la app y no por el cliente en un modelo de negocio no validado.

## Estado de transiciones

No se requiere un state machine complejo para esta feature; el estado de una propiedad es un valor de negocio que puede cambiar con futuras features de actualización o mantenimiento.
