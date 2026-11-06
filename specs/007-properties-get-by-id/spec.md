# Especificación de la Funcionalidad: Consulta de propiedad por identificador

**Feature Branch**: `007-properties-get-by-id`

**Creado**: 2026-11-05

**Estado**: En especificación

**Entrada**: Descripción del usuario: Definir `GET /api/properties/{id}` para consultar una única propiedad, alineado con los contratos existentes de propiedades y distinguiendo identificadores malformados de GUID válidos sin registro.

## Clarifications

### Session 2026-11-05

- Q: ¿El `id` debe aceptar solo UUID canónico con guiones o cualquier formato que .NET pueda interpretar como `Guid`? → A: Solo UUID canónico con guiones; otros formatos reciben 400.

## Escenarios de usuario y pruebas

### Historia de usuario 1 - Consultar una propiedad existente (Prioridad: P1)

Una persona que navega el catálogo necesita consultar los datos completos de una propiedad concreta para revisar su descripción, dirección, precio, estado, características e imagen, sin recibir resultados ni metadatos de paginación.

**Por qué esta prioridad**: La consulta de detalle es el flujo principal y complementa el listado paginado definido en la spec 004.

**Prueba independiente**: Solicitar una propiedad existente por su GUID y comparar el cuerpo 200 con los datos conocidos, verificando los diez campos del contrato y la ausencia de metadatos de paginación.

**Escenarios de aceptación**:

1. **Dado** que existe una propiedad identificada por un GUID válido, **cuando** se consulta `GET /api/properties/{id}`, **entonces** la API responde `200 OK` con `id`, `title`, `description`, `address`, `price`, `status`, `bedroomCount`, `bathroomCount`, `areaSquareMeters` e `imageUrl`.
2. **Dado** que la propiedad tiene imagen, **cuando** se obtiene su detalle, **entonces** `imageUrl` es una URL absoluta pública con el esquema y host de la solicitud y la ruta `/assets/properties/{fileName}`.
3. **Dado** que la propiedad no tiene imagen, **cuando** se obtiene su detalle, **entonces** el campo `imageUrl` está presente con valor `null`, sin generar una imagen alternativa.
4. **Dado** que el detalle es exitoso, **cuando** se inspecciona el cuerpo, **entonces** no contiene `items`, metadatos de página ni otros campos ajenos al contrato de detalle.

### Historia de usuario 2 - Distinguir propiedad inexistente de identificador malformado (Prioridad: P2)

Una persona consumidora de la API necesita distinguir entre un identificador que no representa un GUID y un GUID válido que no corresponde a una propiedad, para corregir su solicitud o reconocer que el recurso no existe.

**Por qué esta prioridad**: La distinción proporciona errores previsibles y mantiene la compatibilidad con los contratos públicos de validación y recursos no encontrados.

**Prueba independiente**: Consultar con un identificador no parseable como GUID, con un UUID sin guiones que .NET pueda interpretar como `Guid`, y con un UUID canónico que no exista; verificar `400` con error asociado a `id` para los dos primeros y `404` con ProblemDetails para el último.

**Escenarios de aceptación**:

1. **Dado** que el segmento `id` no usa el formato UUID canónico con guiones, **cuando** se consulta el endpoint, **entonces** la API responde `400 Bad Request` con ProblemDetails, preferiblemente ValidationProblemDetails, e identifica el error en `errors.id`, incluso si .NET puede interpretarlo como `Guid`.
2. **Dado** que `id` es un GUID válido pero no hay una propiedad registrada con ese identificador, **cuando** se consulta el endpoint, **entonces** la API responde `404 Not Found` con ProblemDetails.
3. **Dado** que el identificador es inválido o no existe, **cuando** se procesa la solicitud, **entonces** no se devuelve un DTO de propiedad ni se expone información interna de persistencia.

### Historia de usuario 3 - Consumir una URL de imagen estable entre hosts y propiedades sin imagen (Prioridad: P3)

Una persona que consume la API desde distintos entornos necesita que las imágenes se puedan abrir desde el mismo host y esquema usados para la solicitud, y que una propiedad sin imagen siga siendo consultable.

**Por qué esta prioridad**: La representación consistente de imágenes evita enlaces rotos entre entornos y conserva propiedades válidas que todavía no tienen imagen.

**Prueba independiente**: Consultar una propiedad con imagen mediante solicitudes HTTP y HTTPS en hosts distintos, y otra propiedad sin imagen; verificar que la URL refleja cada solicitud y que la segunda conserva `imageUrl: null`.

**Escenarios de aceptación**:

1. **Dado** que una propiedad tiene un nombre de archivo de imagen y la solicitud actual usa un esquema y host determinados, **cuando** se devuelve el detalle, **entonces** `imageUrl` se forma como `http(s)://{host}/assets/properties/{fileName}` usando el esquema y host actuales.
2. **Dado** que la propiedad no tiene imagen persistida, **cuando** se devuelve el detalle desde cualquier host o esquema, **entonces** `imageUrl` permanece en `null`.
3. **Dado** que se genera una URL de imagen, **cuando** se inspecciona su valor, **entonces** no contiene rutas físicas ni es una URL relativa.

## Casos límite

- Un GUID válido sin registro responde `404` con ProblemDetails; no es una respuesta 200 vacía ni un error de validación.
- Un segmento que no use el formato UUID canónico con guiones responde `400` con ProblemDetails y un error asociado a `id`; no se ejecuta una búsqueda de propiedad, aunque .NET pueda interpretar el valor como `Guid`.
- Una propiedad sin imagen devuelve el campo obligatorio `imageUrl` con valor `null`; no se inventa una imagen alternativa ni se omite el campo.
- El esquema, hostname y puerto de la URL pública deben corresponder a la solicitud actual, tanto para HTTP como para HTTPS.
- Los valores de host distintos no deben cambiar los datos de propiedad ni revelar directorios locales.
- La respuesta no incluye `items`, `page`, `pageSize`, `totalItems`, `totalPages`, `hasNext` ni `hasPrevious`.

## Requisitos

### Requisitos funcionales

- **FR-001**: El backend DEBE exponer `GET /api/properties/{id}` para consultar una propiedad individual por identificador.
- **FR-002**: `{id}` DEBE usar el formato UUID canónico con guiones (`8-4-4-4-12` caracteres hexadecimales, sin llaves ni paréntesis). Cualquier otro formato DEBE responder `400 Bad Request` con ProblemDetails, preferiblemente ValidationProblemDetails con un error asociado a `id`, aunque .NET pueda interpretarlo como `Guid`.
- **FR-003**: Si `{id}` es un GUID válido y no existe una propiedad correspondiente, la API DEBE responder `404 Not Found` con ProblemDetails. Este caso no debe confundirse con un identificador malformado.
- **FR-004**: Para una propiedad existente, la API DEBE responder `200 OK` con un DTO explícito que contenga exactamente `id`, `title`, `description`, `address`, `price`, `status`, `bedroomCount`, `bathroomCount`, `areaSquareMeters` e `imageUrl`.
- **FR-005**: La respuesta 200 NO DEBE incluir metadatos de paginación ni exponer directamente una entidad de EF Core.
- **FR-006**: `imageUrl` DEBE estar presente y ser `null` cuando la propiedad no tenga imagen. No se debe generar ni seleccionar una imagen alternativa.
- **FR-007**: Cuando exista imagen, `imageUrl` DEBE ser una URL absoluta pública con el patrón `http(s)://{host}/assets/properties/{fileName}`, construida con el esquema y host de la solicitud actual y el nombre de archivo de la imagen persistida.
- **FR-008**: La URL pública NO DEBE ser relativa ni contener una ruta física o interna del servidor.
- **FR-009**: El caso de uso DEBE respetar la alineación y dependencia con las specs 001 a 006: solución única y gobernanza de 001; arquitectura y errores de 002; modelo persistente de 003; campos, proyección de lectura, mapping explícito, cancelación y URL pública de 004; posibilidad de `ImageUrl = null` de 005; y compatibilidad con el recurso y convenciones de propiedades de 006, sin alterar los comportamientos existentes.
- **FR-010**: El backend DEBE organizar el caso de uso como Vertical Slice y Minimal API, con slice, handler, mapping explícito y validator; el endpoint DEBE auto-registrarse mediante `ISlice` y el handler mediante `IHandler`, sin controllers.
- **FR-011**: La lectura DEBE usar un DTO explícito y proyectar solo los campos requeridos; el mapping no debe exponer la entidad de EF ni incorporar lógica de negocio.
- **FR-012**: El flujo de solicitud y las operaciones asíncronas de lectura DEBEN aceptar y propagar `CancellationToken`.
- **FR-013**: El contrato público DEBE documentar las respuestas 200, 400 y 404, incluida la distinción entre GUID malformado y GUID válido sin registro, en `contracts/backend-properties-get-by-id.openapi.yaml`.
- **FR-014**: El handler DEBE emitir logging estructurado para cada consulta ejecutada, registrando el identificador y el resultado de búsqueda (`found` o `notFound`), sin incluir título, dirección, descripción ni otros datos de la propiedad.

### Entidades clave

- **Propiedad**: Registro existente del catálogo con identificador GUID, datos descriptivos, precio, estado, características e imagen opcional.
- **Detalle de propiedad**: Representación pública de lectura con los diez campos establecidos por el contrato de propiedades de la spec 004 y con `imageUrl` nullable según la spec 005.

## Criterios de éxito

### Resultados medibles

- **SC-001**: El 100 % de las consultas con un GUID existente devuelve `200 OK` y los diez campos del contrato de detalle, sin metadatos de paginación.
- **SC-002**: El 100 % de los segmentos que no usan el formato UUID canónico con guiones devuelve `400` con ProblemDetails y un error identificable para `id`, incluso si .NET puede interpretarlos como `Guid`.
- **SC-003**: El 100 % de los GUID bien formados sin registro devuelve `404` con ProblemDetails.
- **SC-004**: El 100 % de las propiedades con imagen devuelve una URL absoluta con el esquema, host y puerto de la solicitud y la ruta `/assets/properties/{fileName}`; ninguna respuesta expone rutas físicas o URLs relativas.
- **SC-005**: El 100 % de las propiedades sin imagen devuelve `imageUrl: null` y mantiene el resto de los campos del contrato.
- **SC-006**: Una persona consumidora puede completar los casos manuales de propiedad existente, GUID inexistente, GUID malformado, imagen ausente y variación de esquema/host siguiendo `quickstart.md` sin depender de una respuesta paginada.
- **SC-007**: En el 100 % de las consultas que llegan al handler, el evento estructurado registra el identificador y el resultado `found` o `notFound`, sin incluir datos descriptivos de la propiedad.

## Dependencias y compatibilidad con specs previas

- **001-realtor-solution-foundation**: La funcionalidad forma parte de la solución Realtor compartida y respeta su base y gobernanza Spec-Driven.
- **002-foundation-backend**: Se integra con Minimal API, auto-registro y el contrato de validación: las validaciones fallidas usan `ValidationProblemDetails` con HTTP 400.
- **003-properties-persistence-seeding**: Reutiliza el identificador GUID, la entidad y los datos persistidos existentes; no crea ni modifica el modelo persistente.
- **004-properties-list-pagination**: Reutiliza exactamente los diez campos del elemento de propiedad, la lectura proyectada, mapping explícito, cancelación y la regla de URL absoluta desde esquema/host actual. El endpoint de detalle no reutiliza la envoltura paginada.
- **005-properties-create**: Mantiene `imageUrl` como campo presente y nullable porque una propiedad puede persistirse sin imagen; no se crea una imagen de reemplazo.
- **006-properties-update**: Mantiene compatibilidad con las propiedades y convenciones públicas existentes sin cambiar el comportamiento de actualización.

## Alcance

Incluye la consulta de una propiedad por GUID, sus respuestas de éxito y error, el contrato de campos y la composición de URL pública. No incluye listado, paginación, búsqueda, filtros, ordenamientos, altas, actualizaciones, borrado, autenticación ni cambios de esquema o datos.

## Suposiciones

- El modelo persistente `Property` y sus datos ya existen conforme a las specs 003-006; `Property.Id` es `Guid`.
- El campo persistido `ImageUrl` es `null` cuando no existe imagen; cuando tiene valor, permite obtener el nombre de archivo para la ruta pública definida por la spec 004.
- La infraestructura central existente convierte fallos de validación en `ValidationProblemDetails` HTTP 400 y errores de recurso inexistente en ProblemDetails con estado 404.
- La URL de imagen se construye a partir del esquema y host confiables de la solicitud actual según la configuración de hosting existente; el detalle no cambia la estrategia general de generación de URLs establecida por 004.
- La operación es de solo lectura y no requiere migraciones ni escrituras persistentes.
