# Investigación y decisiones

## Contexto

La feature consulta una propiedad existente con ASP.NET Core Minimal APIs y el slice debe mantener el contrato público de 004, la nulabilidad de imagen de 005 y los errores de recurso de 006. El repositorio fija .NET 11 preview en `global.json`; el proyecto API usa EF Core 10.0.4, Npgsql EF Core 10.0.1 y FluentValidation.AspNetCore 11.3.0. Las pruebas del backend usan xUnit 2.9.3 y EF Core InMemory. No se requieren paquetes nuevos.

## Decisiones

### Identificador de ruta

**Decisión**: Recibir el `id` como texto en un request del slice ligado a la ruta y validarlo con `Guid.TryParseExact(value, "D", ...)`. El formato `D` admite el UUID canónico con guiones en mayúsculas o minúsculas y rechaza formatos alternativos. El endpoint no usará una restricción de ruta `:guid` ni un parámetro tipado `Guid`; después de superar la validación, el mapping convertirá el valor a `Guid` para la consulta.

**Racional**: La spec requiere HTTP 400 para formatos no canónicos. Una restricción `:guid` puede impedir que la ruta coincida y convertir el caso en 404; el binding directo a `Guid` tampoco expresa la regla contractual exacta. La infraestructura ya aplica FluentValidation a parámetros de request del slice y devuelve `ValidationProblem` antes de invocar el handler.

**Alternativas consideradas**:
- Usar `{id:guid}` y un parámetro `Guid`: descartado porque no garantiza la distinción pública de formato y ausencia requerida por la spec.
- Validar solo con `Guid.TryParse`: descartado porque acepta formas distintas al UUID canónico acordado.

### Lectura y respuesta 404

**Decisión**: El handler consulta `AppDbContext.Properties` con `AsNoTracking`, filtra por `Id` y proyecta solo los datos del contrato a un resultado específico del slice. La consulta asíncrona recibe `CancellationToken`. Si no hay fila, el handler registra `notFound` y devuelve `null`; el endpoint convierte ese resultado a HTTP 404 con `ProblemDetails`, siguiendo el patrón existente de `UpdateProperty`.

**Racional**: `Id` es la clave GUID existente, por lo que la búsqueda es unitaria y no requiere conteos, paginación, repositorio nuevo ni cambios de esquema. El patrón nullable evita introducir una capa de errores paralela y coincide con el endpoint de actualización.

**Alternativas consideradas**:
- Devolver una colección vacía o HTTP 200: descartado porque una consulta unitaria sin recurso exige 404 según la spec.
- Exponer la entidad EF como respuesta: descartado por el contrato explícito y la separación entre persistencia y API.

### DTO y URL de imagen

**Decisión**: Definir contratos y respuesta explícitos dentro del nuevo slice. Mantener el mapping de respuesta en el slice de detalle y extraer la generación segura de URL de imagen, hoy privada en el mapping de listado, a un helper específico de propiedades compartido por ambos mappings. El helper conserva las comprobaciones del nombre de archivo y construye la URL absoluta con el esquema y host de la solicitud. Si `ImageUrl` persistido es `null`, el DTO incluye `imageUrl: null`.

**Racional**: Un helper compartido evita que listado y detalle diverjan en normalización o seguridad de nombres, mientras cada slice conserva su mapping explícito y su DTO público. La transformación no cambia la persistencia ni agrega una URL alternativa.

**Alternativas consideradas**:
- Copiar la normalización de imagen al nuevo mapping: descartado para evitar dos implementaciones de la misma regla.
- Hacer que el slice de detalle dependa del mapping de listado: descartado porque crea dependencia entre casos de uso.
- Introducir una estrategia de imagen por defecto: descartado porque 005 define la ausencia de imagen como `null`.

### Validación y pruebas

**Decisión**: Registrar el validator del nuevo request mediante el escaneo existente de FluentValidation. Añadir pruebas unitarias xUnit para formato de ID, mapping con/sin imagen, handler con y sin resultado y evento de logging. Mantener la comprobación HTTP manual mediante los ejemplos de `quickstart.md` y `RealtorApi.http`; no agregar pruebas de integración automatizadas ni infraestructura de test no aprobada por la constitución.

**Racional**: El filtro central de slices aplica los validators registrados y produce `ValidationProblemDetails` 400. La constitución limita las pruebas automatizadas a unit tests salvo aprobación explícita para otros tipos, mientras el quickstart permite comprobar la interacción HTTP real.

**Alternativas consideradas**:
- Añadir `WebApplicationFactory` o pruebas de integración con base de datos: descartado porque la spec no aprueba ese tipo de prueba y la constitución lo limita.
- Omitir las pruebas manuales del binding de ruta: descartado porque la distinción 400/404 depende del comportamiento HTTP observable.

### Persistencia y alcance

**Decisión**: No modificar tablas, entidad, seeds, migraciones, paquetes, frontend ni el comportamiento de otros endpoints. El ajuste compartido del generador de URL conserva la salida de 004 y se valida con sus pruebas existentes.

**Racional**: La feature es una consulta de solo lectura y reutiliza el modelo persistente y la infraestructura actuales.
