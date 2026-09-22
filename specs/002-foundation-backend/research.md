# Investigación de diseño: Base de infraestructura del backend Realtor

## Decisiones tomadas

### 1. Descubrimiento de endpoints por `ISlice`
**Decisión**: Se implementará un contrato `ISlice` con un método `AddEndpoint(IEndpointRouteBuilder app)` y un registrador por assembly scanning.

**Racional**: La constitución exige Minimal APIs y Vertical Slice Architecture, y el alcance de esta feature requiere que un nuevo endpoint no necesite tocar `Program.cs` manualmente. El assembly scanning es la opción más estable y escalable para mantener el sistema abierto a futuras features sin listas manuales.

**Alternativas consideradas**:
- Registro manual en `Program.cs`: descartado porque contradice la automatización requerida y aumenta el acoplamiento.
- Descubrimiento por nombre de clase: descartado porque es frágil y rompe la convención basada en interfaces y assembly scanning.

### 2. Auto-registro de handlers mediante `IHandler`
**Decisión**: Se creará un marker interface `IHandler` y se realizará un registro por reflection basado en el assembly del backend.

**Racional**: Permite preparar la infraestructura para futuras features sin implementar handlers concretos de negocio ahora. Mantiene el diseño limpio y se ajusta a la convención establecida por la especificación.

**Alternativas consideradas**:
- Registration service manual por feature: descartado por duplicación y acoplamiento.
- USingleton registry manual: descartado por el mismo motivo y por no cumplir la expectativa de base reutilizable.

### 3. Validación automática con FluentValidation
**Decisión**: Se usará una `ValidationFilterFactory` que detecta `IValidator<T>` y ejecuta la validación sin exigir registro individual de cada validator.

**Racional**: La especificación exige validación automática, pass-through cuando no existe validador, y respuesta con `ValidationProblemDetails` en HTTP 400. Esta es la solución estándar en ASP.NET Core con Minimal APIs y mantiene el backend extensible.

**Alternativas consideradas**:
- Validación manual en cada endpoint: descartada por duplicación y falta de consistencia.
- Filtros por tipo de request con listas estáticas: descartados por no ser auto-descubribles ni mantenibles.

### 4. Manejo centralizado de errores con `Result`
**Decisión**: Los errores esperados se representarán como `Result` y se convertirán mediante una capa central de mapping a `ProblemDetails`.

**Racional**: Garantiza una respuesta uniforme, compatible con la convención del repositorio y con la obligación de retornar `ProblemDetails` en status codes correctos.

**Alternativas consideradas**:
- Excepciones sin tratamiento centralizado: descartadas porque dificultan la trazabilidad y la consistencia del API.
- Respuestas con payloads heterogéneos: descartadas porque rompen la homogeneidad de la capa de infraestructura.

### 5. Migración de la sonda `/health`
**Decisión**: El endpoint `/health` se moverá al patrón `ISlice` y será el ejemplo operativo de auto-descubrimiento y mapeo centralizado.

**Racional**: Es un indicador mínimo de infraestructura, no una feature de negocio, y valida que la resolución reflejada y el registro centralizado funcionan sin tocar `Program.cs`.

**Alternativas consideradas**:
- Mantener el endpoint en `Program.cs`: descartado porque no cumple el objetivo de automatización.
- Crear una feature funcional para salud: descartado porque la iniciativa no debe introducir lógica de negocio.

## Resultado del análisis

No quedan bloqueos de negocio ni clarificaciones pendientes. La solución es técnicamente viable, cumple la constitución y está acotada exactamente al objetivo de infraestructura transversal del backend.
