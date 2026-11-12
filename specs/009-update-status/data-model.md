# Modelo de datos: actualización del estado de una propiedad

## Entidades y contratos

### Propiedad existente

**Propósito**: Entidad persistida del catálogo sobre la que opera el caso de uso.

**Campos relevantes**:

- `Id`: identificador GUID utilizado para localizar el registro.
- `Status`: único campo que esta operación puede modificar.
- `Title`, `Description`, `Address`, `Price`, `BedroomCount`, `BathroomCount`, `AreaSquareMeters` e `ImageUrl`: permanecen exactamente como estaban antes de la operación.
- `CreatedAt` y `UpdatedAt`: permanecen sin cambios; este caso de uso no mantiene timestamps de modificación.

**Reglas**:

- Si no existe una propiedad con el id solicitado, no se persiste ningún cambio.
- Si el request es válido y la propiedad existe, se actualiza exclusivamente `Status`.
- No se agrega ni modifica una entidad, propiedad persistente, índice o relación; no se requiere migración.

### Estado de propiedad

**Tipo**: Enum de dominio existente `PropertyStatus`.

**Valores aceptados por este endpoint**:

- `Available`
- `Rented`
- `Maintenance`

La allowlist del request sigue la spec aunque el tipo de dominio evolucione en otra iniciativa; esta feature no introduce valores nuevos ni reglas de transición.

### Request de actualización de estado

**Propósito**: Representar exclusivamente el cuerpo JSON del caso de uso.

**Campo**:

- `status`: requerido; texto con uno de los valores aceptados.

**Validación**:

- El documento JSON debe ser un objeto con una sola propiedad `status`.
- Body ausente, vacío, JSON inválido, valor nulo/no textual, claves adicionales o duplicadas y estado no admitido producen `400 ValidationProblemDetails`.
- El identificador de ruta debe ser un UUID canónico con guiones; otro formato produce `400 ValidationProblemDetails`.

### Response de actualización

**Propósito**: Confirmar el identificador y el estado persistido sin exponer la entidad.

**Campos**:

- `id`: GUID de la propiedad actualizada.
- `status`: estado actualizado en su forma canónica.

El body exitoso contiene exactamente esos dos campos. El detalle completo se consulta con el endpoint existente de lectura.

## Relaciones y transiciones

- La propiedad ya está relacionada con `PropertyStatus` según el modelo de dominio existente.
- Cualquier estado admitido puede cambiarse a cualquiera de los tres valores admitidos; no se definen restricciones adicionales de transición.
- Enviar el mismo estado actual sigue siendo una operación válida y responde con el mismo `id` y `status` sin alterar los otros campos.