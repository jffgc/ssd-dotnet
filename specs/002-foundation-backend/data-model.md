# Modelo de datos de la infraestructura del backend

## Entidades de infraestructura

### ISlice
**Propósito**: Contrato que permite registrar un endpoint de Minimal API sin edición manual en `Program.cs`.

**Campos / miembros**:
- `AddEndpoint(IEndpointRouteBuilder app)`: método de extensión de registro del endpoint.

**Reglas de validación**:
- Debe ser una clase inmutable o stateless.
- Debe limitarse a la definición del endpoint y su handler asociado.
- No debe contener lógica de negocio ni persistencia.

**Relaciones**:
- Un slice puede depender de un `IHandler` o de servicios de infraestructura.
- Un slice se descubre por assembly scanning.

### IHandler
**Propósito**: Marker interface para el auto-registro de handlers en futuras features.

**Campos / miembros**:
- Ninguno. Es un contrato de marca, no de comportamiento.

**Reglas de validación**:
- Debe ser implementado por clases de infraestructura o caso de uso.
- No debe usarse para poner lógica comercial en esta fase.

**Relaciones**:
- Los handlers reales heredan este marker y quedan registrados por reflection.

### Result
**Propósito**: Representa fallos esperados y valores de retorno normalizados para la infraestructura del backend.

**Campos / miembros**:
- `bool IsSuccess`
- `T Value` o `Error` equivalente según la implementación final.
- `ProblemDetails` asociado en la conversión centralizada.

**Reglas de validación**:
- Debe representar únicamente estados esperados del sistema.
- La conversión a `ProblemDetails` debe hacerse en una capa central de errores.

**Relaciones**:
- Se usa en la capa de endpoints y validación para devolver respuestas homogéneas.

### HealthStatus
**Propósito**: Modelo mínimo de respuesta para la sonda de estado del backend.

**Campos / miembros**:
- `Status` (por ejemplo, `Healthy`)
- `Timestamp` opcional

**Reglas de validación**:
- Debe ser un payload sin lógica de negocio.
- Debe reflejar únicamente el estado operativo del sistema.

**Relaciones**:
- Se expone como respuesta del endpoint `/health` migrado a `ISlice`.

## Consideraciones de diseño

- No existen entidades de dominio ni modelos de negocio en esta feature.
- La estructura es puramente infra y de soporte para futuras features.
- Los contratos anteriores son la base de la arquitectura transversal del backend.
