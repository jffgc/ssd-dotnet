# Especificación de la Funcionalidad: Actualización del estado de una propiedad

**Feature Branch**: `009-update-status`

**Creado**: 2026-11-11

**Estado**: Implementada

**Entrada**: Descripción del usuario: Crear un endpoint público `PATCH /api/properties/{id}/status` para actualizar únicamente el estado de una propiedad existente. Debe validar el estado, conservar sin cambios el resto de datos persistidos, usar Minimal API y Vertical Slice, responder con ProblemDetails y documentar una prueba manual en el archivo `.http` del backend. La funcionalidad no incluye otros campos, imágenes ni subida de archivos.

## Escenarios de usuario y pruebas

### Historia de usuario 1 - Actualizar el estado de una propiedad existente (Prioridad: P1)

Una persona consumidora de la API necesita cambiar el estado de una propiedad existente sin modificar su información descriptiva, comercial o su imagen. Tras la operación debe recibir el identificador y el nuevo estado.

**Por qué esta prioridad**: Es el único flujo de negocio de esta feature y permite mantener actualizado el estado operativo sin arriesgar la información restante de la propiedad.

**Prueba independiente**: Enviar un estado válido a una propiedad existente y comprobar `200 OK`, la respuesta con `id` y `status`, y que una consulta posterior de detalle confirme el nuevo estado y conserve todos los demás datos, incluido `imageUrl`.

**Escenarios de aceptación**:

1. **Dado** que existe una propiedad con estado `Available`, **cuando** se envía `PATCH /api/properties/{id}/status` con `{"status":"Rented"}`, **entonces** la API responde `200 OK` con el `id` de la propiedad y `status: "Rented"`.
2. **Dado** que una propiedad existente tiene datos descriptivos, comerciales e imagen, **cuando** se actualiza su estado, **entonces** esos datos persistidos permanecen sin cambios y solo cambia `status`.
3. **Dado** que la respuesta exitosa contiene únicamente `id` y `status`, **cuando** el cliente necesita verificar el resto de los datos, **entonces** puede consultarlos mediante el endpoint de detalle existente sin que esta operación modifique ese contrato.

### Historia de usuario 2 - Rechazar una propiedad inexistente (Prioridad: P2)

Una persona consumidora necesita saber si no se encontró el recurso solicitado para evitar interpretar una actualización no realizada como exitosa.

**Por qué esta prioridad**: Un identificador inexistente no debe crear recursos ni producir cambios y debe distinguirse de una solicitud inválida.

**Prueba independiente**: Enviar un UUID canónico que no corresponda a una propiedad y comprobar `404 Not Found` con `ProblemDetails` y sin efectos persistidos.

**Escenarios de aceptación**:

1. **Dado** que no existe una propiedad con el UUID solicitado, **cuando** se intenta actualizar su estado, **entonces** la API responde `404 Not Found` con `ProblemDetails`.
2. **Dado** que la propiedad no existe, **cuando** se procesa la solicitud, **entonces** no se crea una propiedad ni se modifica ningún registro.

### Historia de usuario 3 - Rechazar solicitudes de estado inválidas (Prioridad: P3)

Una persona consumidora necesita recibir un error de validación claro cuando omite el estado o envía un valor ajeno a los estados admitidos.

**Por qué esta prioridad**: La validación evita persistir estados desconocidos y mantiene coherentes las propiedades y los contratos públicos existentes.

**Prueba independiente**: Enviar un cuerpo vacío, sin `status`, con `status: null`, con un estado no admitido y con propiedades adicionales; comprobar `400 Bad Request` con `ProblemDetails` de validación y ausencia de cambios.

**Escenarios de aceptación**:

1. **Dado** que el cuerpo está vacío o no contiene `status`, **cuando** se envía la solicitud, **entonces** la API responde `400 Bad Request` con un error de validación asociado a `status` o al cuerpo.
2. **Dado** que `status` es `null`, no es texto o no coincide con un estado admitido, **cuando** se valida la solicitud, **entonces** la API responde `400 Bad Request` y no actualiza la propiedad.
3. **Dado** que el cuerpo incluye campos distintos de `status`, **cuando** se procesa la solicitud, **entonces** la API responde `400 Bad Request` y no aplica ningún cambio.

## Casos límite

- Un UUID canónico válido que no identifica una propiedad responde `404` con `ProblemDetails`.
- Un identificador que no use el formato UUID canónico con guiones responde `400` con `ProblemDetails`, siguiendo la convención establecida por la spec 007.
- Un cuerpo vacío, omitido, `null` o no JSON responde `400` sin persistir cambios.
- Un `status` omitido, nulo, de tipo incorrecto o ajeno al conjunto admitido responde `400` y no altera la propiedad.
- Un cuerpo con campos adicionales, incluidos `title`, `description`, `address`, `price`, `bedroomCount`, `bathroomCount`, `areaSquareMeters` o `imageUrl`, responde `400`; esos valores nunca se aplican.
- Si la actualización falla, el valor anterior de `status` y los demás datos persisten sin cambios.
- La propiedad puede tener `imageUrl` con valor o `null`; en ambos casos la actualización conserva exactamente su valor previo.
- La ruta es pública y no requiere autenticación.

## Requisitos

### Requisitos funcionales

- **FR-001**: El backend DEBE exponer `PATCH /api/properties/{id}/status` como endpoint público y sin autenticación.
- **FR-002**: `{id}` DEBE aceptar el UUID canónico con guiones establecido por la spec 007. Un formato no canónico DEBE responder `400 Bad Request` con `ProblemDetails` de validación.
- **FR-003**: El cuerpo DEBE ser JSON con un único campo requerido `status`, cuyo valor sea una cadena de estado admitida.
- **FR-004**: Los únicos valores válidos de `status` DEBEN ser `Available`, `Rented` y `Maintenance`, reutilizando los valores establecidos por la spec 003.
- **FR-005**: El cuerpo DEBE rechazarse con `400 Bad Request` si falta `status`, su valor es nulo, tiene tipo incorrecto o no pertenece al conjunto de estados válidos.
- **FR-006**: El cuerpo DEBE rechazarse con `400 Bad Request` si incluye propiedades adicionales a `status`. La implementación NO DEBE permitir actualizaciones de otros campos mediante este endpoint.
- **FR-007**: Si el identificador es válido pero no corresponde a una propiedad, el backend DEBE responder `404 Not Found` con `ProblemDetails` y no debe crear ni modificar registros.
- **FR-008**: Si la propiedad existe y el estado es válido, el backend DEBE responder `200 OK` con un objeto JSON que contenga exactamente `id` y `status` actualizados.
- **FR-009**: Una actualización exitosa DEBE cambiar únicamente el campo persistido `status`. Debe conservar intactos todos los demás datos de la propiedad, incluidos `title`, `description`, `address`, `price`, `bedroomCount`, `bathroomCount`, `areaSquareMeters` e `imageUrl`, tanto si `imageUrl` contiene una URL como si es `null`.
- **FR-010**: La validación fallida DEBE usar el formato `ProblemDetails` de validación existente y el estado HTTP 400; el recurso inexistente DEBE usar `ProblemDetails` con estado HTTP 404.
- **FR-011**: El caso de uso DEBE seguir Minimal API y Vertical Slice, organizándose dentro de una feature de estado de propiedad bajo `Features/Properties`, auto-descubierta mediante `ISlice` y atendida mediante el patrón `IHandler` del repositorio. NO DEBE introducir controllers ni lógica de negocio en `Program`.
- **FR-012**: El caso de uso DEBE definir contratos explícitos de entrada y salida, validación del request y mapping explícito por slice, sin exponer directamente la entidad persistida.
- **FR-013**: El request y las operaciones asíncronas de persistencia DEBEN propagar `CancellationToken`, siguiendo las convenciones del backend.
- **FR-014**: La feature DEBE incluir un contrato OpenAPI en `specs/009-update-status/contracts/` que documente método y ruta, el cuerpo JSON, los valores válidos de `status`, y las respuestas `200`, `400` y `404` con sus formatos.
- **FR-015**: La feature DEBE incluir una guía rápida en `specs/009-update-status/quickstart.md` con prerrequisitos y pasos reproducibles para comprobar los flujos exitoso, no encontrado e inválido.
- **FR-016**: La feature DEBE incluir las tareas de implementación en `specs/009-update-status/tasks.md`, generadas después de aprobar el plan según el flujo Spec-Driven del repositorio y trazables a estas historias y requisitos.
- **FR-017**: El archivo `app/backend/src/RealtorApi/RealtorApi.http` DEBE incluir ejemplos manuales para actualizar un estado válido, omitir o invalidar `status`, y usar un identificador inexistente; los ejemplos deben indicar cómo confirmar que los demás datos, incluido `imageUrl`, se conservaron.
- **FR-018**: La spec, el contrato, la guía rápida, las tareas y la documentación añadida para esta feature DEBEN estar escritos en español, salvo identificadores técnicos, nombres de campos, valores de enum, rutas y sintaxis de herramientas.
- **FR-019**: La feature NO DEBE añadir carga o almacenamiento de imágenes, actualizar `imageUrl`, modificar otros campos, alterar endpoints existentes ni cambiar su semántica.
- **FR-020**: El caso de uso DEBE emitir logging estructurado con el identificador de la propiedad y el resultado `updated` o `notFound`. Los logs NO DEBEN incluir el cuerpo de la solicitud ni otros datos de la propiedad.

### Contrato de request y response

Request `application/json`:

```json
{
  "status": "Rented"
}
```

Response exitosa `200 OK`:

```json
{
  "id": "9c2fcbcb-f94a-4c70-b04f-b24488b89ad1",
  "status": "Rented"
}
```

El cuerpo de respuesta está limitado a `id` y `status`, según la decisión de alcance confirmada durante la especificación. La conservación de los demás datos se verifica en la persistencia o mediante una consulta de detalle posterior.

### Entidades clave

- **Propiedad**: Registro persistido del catálogo con un identificador y datos descriptivos, comerciales, de estado e imagen. Esta feature solo modifica su estado.
- **Estado de propiedad**: Valor de estado admitido por el dominio: `Available`, `Rented` o `Maintenance`.
- **Solicitud de actualización de estado**: Objeto JSON que contiene exclusivamente el campo requerido `status`.
- **Resultado de actualización de estado**: Representación pública acotada con `id` y `status`.

## Criterios de éxito

### Resultados medibles

- **SC-001**: El 100 % de las solicitudes válidas para una propiedad existente responde `200 OK` con el identificador correcto y el estado solicitado.
- **SC-002**: En el 100 % de las actualizaciones exitosas cambia únicamente `status`; todos los demás valores persistidos, incluido `imageUrl`, coinciden con sus valores anteriores.
- **SC-003**: El 100 % de los identificadores UUID válidos sin propiedad asociada responde `404` con `ProblemDetails` y no crea ni modifica registros.
- **SC-004**: El 100 % de los cuerpos sin `status`, con un valor inválido o con propiedades adicionales responde `400` con `ProblemDetails` de validación y no persiste cambios.
- **SC-005**: Los casos de éxito, `400`, `404` y conservación de datos pueden verificarse manualmente siguiendo los ejemplos del archivo `.http` y la guía rápida.
- **SC-006**: El contrato OpenAPI describe exactamente el request solo de estado y las respuestas de esta feature, sin documentar carga de archivos ni cambios de otros campos.
- **SC-007**: El 100 % de las solicitudes que llegan al handler genera un evento estructurado con `id` y resultado `updated` o `notFound`, sin incluir el cuerpo ni otros datos de la propiedad.

## Dependencias y compatibilidad

- **001-realtor-solution-foundation**: La feature forma parte de la solución Realtor compartida y respeta su gobernanza Spec-Driven.
- **002-foundation-backend**: Reutiliza Minimal API, auto-descubrimiento de slices y manejo de errores `ProblemDetails`.
- **003-properties-persistence-seeding**: Reutiliza el modelo persistente y los valores `Available`, `Rented` y `Maintenance`; no requiere cambios de esquema.
- **004-properties-list-pagination**: No altera el contrato ni el comportamiento del listado.
- **005-properties-create**: No altera la creación de propiedades ni el tratamiento de imágenes.
- **006-properties-update**: No altera el endpoint de actualización general ni amplía su semántica.
- **007-properties-get-by-id**: Reutiliza la convención de UUID canónico, respuestas ProblemDetails y permite comprobar mediante lectura posterior la preservación del resto de campos.
- **008-open-api**: El contrato generado desde la implementación real debe reflejar este endpoint cuando la implementación quede aprobada; no se editará manualmente el documento OpenAPI generado de runtime.

## Alcance

Incluye un único caso de uso público para modificar el estado de una propiedad existente por id, validar su cuerpo, representar respuestas `200`, `400` y `404`, documentar el contrato y permitir su prueba manual.

Quedan fuera de alcance la actualización de cualquier campo distinto de `status`, las imágenes y archivos, autenticación, autorización, alta, borrado, cambios al modelo de datos, migraciones y modificaciones a la semántica de endpoints existentes.

## Suposiciones

- La entidad persistente `Property`, el identificador UUID y el campo de estado existen de acuerdo con las specs 003 y posteriores.
- Los estados permitidos son exactamente `Available`, `Rented` y `Maintenance`, sin introducir estados nuevos ni transiciones restringidas.
- El request usa JSON con el nombre de campo `status` y valores de estado con la escritura canónica definida por el dominio.
- Un UUID válido sin propiedad asociada se distingue de un identificador malformado; el segundo recibe `400` conforme a la spec 007.
- La respuesta exitosa incluye únicamente `id` y `status`; los demás datos se consultan mediante el endpoint de lectura ya existente.
- El flujo obligatorio permanece `speckit.specify -> speckit.plan -> speckit.tasks -> speckit.implement`. Por ello, las tareas detalladas se generan en la fase correspondiente y no se autoriza implementación hasta su aprobación.

## Clarificaciones

### Session 2026-11-11

- Q: ¿Qué cuerpo debe devolver el `200 OK` al actualizar el estado? → A: Solo `id` y `status`; los demás datos se conservan en persistencia y se consultan con el endpoint de detalle.
- Q: ¿Qué datos debe registrar el logging estructurado? → A: El `id` y el resultado (`updated` o `notFound`), sin incluir el cuerpo ni otros datos de la propiedad.