# Especificación de la Funcionalidad: Actualización de propiedades con upload opcional de imagen

**Feature Branch**: `006-properties-update`

**Creado**: 2026-10-25

**Estado**: En especificación

**Entrada**: Descripción del usuario: "Crear la especificación 006-properties-update para definir un nuevo caso de uso de actualización de propiedades mediante un slice Minimal API en backend. La especificación debe cubrir un endpoint público de actualización por id (por ejemplo PUT /api/properties/{id}), que reciba los datos editables de la propiedad y permita de forma opcional el upload de imagen, manteniendo la misma lógica implementada en 005-properties-create para manejo de imágenes. Reglas funcionales obligatorias: antes de actualizar, buscar la propiedad por id; si no existe, responder 404 sin cambios; si existe, aplicar actualización de campos válidos; si el request incluye imagen válida, almacenarla en el directorio de propiedades con nombre interno único UUID + extensión y persistir imageUrl nueva; si no incluye imagen, conservar imageUrl previa; si falla el guardado de imagen, la operación debe ser atómica; mantener semántica de errores consistente con 005 (400 para validaciones, 415 para formato no permitido, archivo vacío o contenido real inválido, 413 para imagen mayor a 5MB, 500 para fallo interno de almacenamiento de imagen, 404 cuando la entidad no exista). La spec debe incluir historias por prioridad (P1/P2/P3) con escenarios independientes: P1 actualización sin imagen, P2 actualización con imagen válida, P3 rechazos por validación y manejo de errores; requisitos funcionales equivalentes a 005; edge cases, incluyendo id inexistente, imagen vacía/real inválido, colisión resuelta por UUID, no alteración de imageUrl cuando no se envía imagen; criterios de éxito medibles, incluyendo latencia p95 en muestra funcional de solicitudes válidas; alineación explícita con arquitectura Vertical Slice existente; compatibilidad con specs previas 001-005 y con gobernanza Spec-Driven del repositorio."

## Escenarios de usuario y pruebas

### Historia de usuario 1 - Actualización de propiedad sin cambiar la imagen actual (Prioridad: P1)

El equipo de producto necesita editar la información de una propiedad sin forzar la re-carga de su imagen actual. Cuando el cliente actualiza campos editables y no incluye archivo, el backend debe conservar la imagen existente y aplicar únicamente los cambios de negocio solicitados.

**Por qué esta prioridad**: Este es el flujo principal del caso de uso y evita romper la experiencia de edición existente, especialmente para propiedades ya publicadas en el catálogo y con contenido visual asociado.

**Prueba independiente**: Se puede verificar enviando una actualización válida sin archivo que modifique datos de negocio y comprobando que la respuesta es correcta y que la propiedad conserva la URL de imagen previa.

**Escenarios de aceptación**:

1. **Dado** que existe una propiedad persistida con `imageUrl` válida, **cuando** el cliente invoca `PUT /api/properties/{id}` con datos válidos y sin imagen, **entonces** la API debe responder con `200 OK` o equivalente de actualización exitosa y actualizar los campos modificados.
2. **Dado** que la actualización no incluye archivo, **cuando** el backend procesa la petición, **entonces** el valor de `imageUrl` persistido debe permanecer exactamente igual al valor previo y no debe convertirse en `null`, vacío ni una ruta nueva.
3. **Dado** que la propiedad existe en base de datos, **cuando** la validación del slice se ejecuta, **entonces** la actualización de campos de negocio válidos debe aplicarse sin cambiar la semántica del dominio ni la estructura de persistencia ya establecida en las specs anteriores.

### Historia de usuario 2 - Actualización con imagen válida y reemplazo de asset público (Prioridad: P2)

El equipo necesita permitir sustituir la imagen asociada a una propiedad cuando el cliente carga un nuevo archivo válido. En ese caso, el backend debe guardar la imagen en el directorio público del sitio, generar un nombre interno único y persistir la nueva URL pública o relativa.

**Por qué esta prioridad**: La imagen es una pieza crítica para la presentación y el catálogo, y su reemplazo debe ser consistente y seguro para evitar referencias rotas o rutas inválidas.

**Prueba independiente**: Se puede verificar enviando un archivo PNG o JPG de 1 MB, comprobando que se almacena bajo `wwwroot/assets/properties`, que la nueva URL se persiste y que la propiedad actualiza su imagen sin dejar artefactos antiguos en el registro.

**Escenarios de aceptación**:

1. **Dado** que existe una propiedad con una imagen previa, **cuando** el cliente envía un archivo PNG o JPG válido usando `PUT /api/properties/{id}`, **entonces** el archivo se almacena en `app/backend/src/RealtorApi/wwwroot/assets/properties` con un nombre generado internamente como `UUID + extensión`.
2. **Dado** que la imagen se guarda correctamente, **cuando** la propiedad se actualiza, **entonces** `imageUrl` debe apuntar a la ruta pública final del asset y no a una ruta local o de infraestructura no servible.
3. **Dado** que la operación es exitosa, **cuando** la base de datos refleja el cambio, **entonces** la nueva imagen debe quedar asociada a la propiedad y la URL anterior debe reemplazarse solo si el archivo nuevo se guardó correctamente.

### Historia de usuario 3 - Rechazos por validación, almacenamiento y manejo de errores (Prioridad: P3)

El sistema debe rechazar intentos de actualización con datos inválidos o archivos no soportados de forma consistente, devolviendo los errores esperados según el tipo de fallo. Esto permite que el cliente corrija la solicitud y evita actualizaciones parciales o estados inconsistentes.

**Por qué esta prioridad**: La gobernanza de errores y validación es imprescindible para mantener la integridad del contenido y la compatibilidad con clientes y pruebas de integración.

**Prueba independiente**: Se puede verificar enviando una actualización con id inexistente, archivo vacío, tipo no soportado, tamaño > 5 MB o contenido inválido y comprobando que la API devuelve `404`, `415`, `413`, `400` o `500` según el caso, sin dejar cambios parciales.

**Escenarios de aceptación**:

1. **Dado** que la propiedad no existe, **cuando** el cliente intenta actualizarla, **entonces** la API responde `404 Not Found` y no modifica ningún dato.
2. **Dado** que el cliente envía un archivo vacío o con contenido real inválido, **cuando** el validator del slice o el almacenamiento lo procesa, **entonces** la API responde `415 Unsupported Media Type` y no actualiza la propiedad ni su imagen.
3. **Dado** que el cliente envía una imagen mayor a 5 MB, **cuando** la actualización llega al endpoint, **entonces** la API responde `413 Payload Too Large` o equivalente de validación y no persiste cambios parciales.
4. **Dado** que el archivo tiene formato no permitido (`.exe`, `.pdf`, etc.), **cuando** el endpoint valida el request, **entonces** responde `415` con un detalle claro indicando que solo se admiten PNG/JPG/JPEG.
5. **Dado** que falla el almacenamiento del archivo durante una actualización con imagen válida, **cuando** la operación intenta guardar el asset, **entonces** la actualización completa debe revertirse o no aplicarse, manteniendo la consistencia del registro y sin dejar `imageUrl` parcialmente actualizada.

## Casos límite

- ¿Qué ocurre si el id no existe? Debe responder `404 Not Found` y no modificar ningún campo ni la imagen asociada.
- ¿Qué ocurre si la nueva imagen es vacía o tiene contenido real inválido? Debe responder `415 Unsupported Media Type` y no aplicar ningún cambio.
- ¿Qué ocurre si el archivo no es PNG/JPG/JPEG? Debe responder `415` y no guardar el archivo ni actualizar la propiedad.
- ¿Qué ocurre si el archivo pesa más de 5 MB? Debe responder `413 Payload Too Large` o equivalente de validación y no persistir cambios parciales.
- ¿Qué ocurre si el cliente no incluye imagen? La propiedad debe actualizarse exitosamente y `imageUrl` debe mantenerse igual al valor previo.
- ¿Qué ocurre si dos archivos coinciden en el nombre base y el sistema intenta guardar simultáneamente? El sistema debe resolver la colisión usando un UUID interno, sin reemplazar accidentalmente el asset de otra propiedad.
- ¿Qué ocurre si el directorio destino no existe? La aplicación debe crear el directorio requerido bajo `wwwroot/assets/properties` antes del almacenamiento o devolver un error de almacenamiento interno reproducible.
- ¿Qué ocurre si el payload combina campos válidos con un archivo corrupto? El backend debe tratar la operación como una actualización inválida y debe no existir un estado parcial en base de datos ni en almacenamiento.
- ¿Qué ocurre si el request actualiza un campo no editable o no permitido por la especificación? Debe rechazarse con `400 Bad Request` y no deben aplicarse cambios no autorizados.

## Requisitos

### Requisitos funcionales

- **FR-001**: El backend DEBE exponer un endpoint público de actualización por id, usando el patrón `PUT /api/properties/{id}` dentro de la arquitectura Minimal API con `ISlice` y auto-descubrimiento ya establecida por la solución.
- **FR-002**: El caso de uso DEBE residir en una feature del backend con la estructura `Features/Properties/UpdateProperty`, con responsabilidad separada en `slice`, `handler`, `mapping` y `validator`, sin lógica de negocio en la capa de mapeo.
- **FR-003**: El endpoint DEBE aceptar un request con los campos editables de la propiedad y un archivo opcional de imagen, manteniendo el mismo enfoque propuesto en la especificación de alta para manejar `multipart/form-data` y campos escalares.
- **FR-004**: Antes de cualquier actualización, el backend DEBE buscar la entidad por id en la base de datos. Si la propiedad no existe, DEBE responder `404 Not Found` sin ejecutar cambios en el registro ni en el almacenamiento de imágenes.
- **FR-005**: Si la propiedad existe y el request no incluye imagen, el backend DEBE actualizar los campos de negocio válidos y DEBE conservar el valor previo de `imageUrl` sin reemplazarlo, vaciarlo ni generar una ruta nueva.
- **FR-006**: Si el request incluye una imagen válida, el backend DEBE almacenar el archivo en `app/backend/src/RealtorApi/wwwroot/assets/properties` usando una estrategia de nombre seguro con UUID y extensión válida.
- **FR-007**: La nueva imagen almacenada DEBE persistirse en la base de datos mediante la URL pública/relativa final del asset, manteniendo la convención de static files del proyecto y la ruta consumible por clientes.
- **FR-008**: El sistema DEBE aceptar únicamente imágenes en formatos `PNG`, `JPG` o `JPEG`, rechazando cualquier otro tipo con código `415` y detalle de validación de archivo.
- **FR-009**: Los archivos vacíos, los payloads con contenido real inválido y los formatos no permitidos DEBEN reportarse como `415 Unsupported Media Type` en lugar de producir actualizaciones parciales.
- **FR-010**: El tamaño máximo de la imagen DEBE ser 5 MB. Si se excede, la operación DEBE devolver `413 Payload Too Large` y no dejar cambios persistidos.
- **FR-011**: La actualización de propiedades DEBE ser atómica desde la perspectiva del archivo y la base de datos: si el almacenamiento de la imagen falla, no debe quedar ninguna actualización parcial inconsistente ni `imageUrl` parcialmente guardada.
- **FR-012**: En caso de validaciones de negocio inválidas (por ejemplo, campos obligatorios o reglas del dominio), la API DEBE responder con `400 Bad Request` o `ProblemDetails` con detalle identificable del error.
- **FR-013**: En caso de error interno del almacenamiento de imagen, la API DEBE responder `500 Internal Server Error` o su equivalente de `ProblemDetails`, sin comprometer la consistencia del dato previamente persistido.
- **FR-014**: La respuesta exitosa DEBE devolver la entidad actualizada con los campos modificados, incluyendo la URL de imagen actualizada o el valor previo conservado si no se envió archivo.
- **FR-015**: El endpoint DEBE mantener la compatibilidad con la semántica de errores establecida en 005: `400` para validaciones de negocio, `415` para formato no permitido o archivo vacío o contenido real inválido, `413` para imagen mayor a 5 MB, `500` para fallo interno de almacenamiento, `404` cuando la entidad no exista.
- **FR-016**: El caso de uso DEBE utilizar `mapping` explícito por slice y `validator` para cualquier regla de entrada; no debe incluir lógica de negocio en el código de mapeo ni introducir reglas cruzadas ajenas al slice.
- **FR-017**: La funcionalidad DEBE mantener compatibilidad con las specs previas de foundation, persistencia, listado y creación, sin duplicar el modelo del dominio ni romper la semántica de los estados y propiedades ya definidos.
- **FR-018**: La update feature DEBE incluir pruebas funcionales y de integración que cubran: actualización sin imagen, actualización con imagen válida, propiedad inexistente, archivo vacío o inválido, archivo no permitido, archivo sobre tamaño y no alteración del `imageUrl` previo cuando no se envía imagen.

### Reglas de respuesta y errores esperados

- **Respuesta exitosa**: `200 OK` con la entidad actualizada en el cuerpo de la respuesta, o la variante funcional equivalente del proyecto, incluyendo los cambios aplicados e `imageUrl` final.
- **Propiedad inexistente**: `404 Not Found` con detalle del id no localizado y sin cambios en base de datos ni almacenamiento.
- **Sin imagen en request**: actualización exitosa con `imageUrl` preservado exactamente como estaba antes.
- **Validación de negocio inválida**: `400 Bad Request` con ProblemDetails y detalle de los campos inválidos.
- **Formato no permitido / archivo vacío / contenido real inválido**: `415 Unsupported Media Type` sin persistir cambios parciales.
- **Archivo superior a 5MB**: `413 Payload Too Large` o su equivalente de validación, sin guardar ni actualizar parcialmente el registro.
- **Fallo interno de almacenamiento**: `500 Internal Server Error` y la operación debe permanecer atómica.
- **Colisión de nombre**: el nombre del archivo definitivo debe resolverse con UUID para garantizar unicidad y evitar sobrescrituras accidentales.

### Entidades clave

- **Property**: Entidad principal del catálogo inmobiliario que se actualiza y persiste en la base de datos.
- **UpdatePropertyRequest**: Modelo de entrada del caso de uso, con los campos editables y el archivo opcional de imagen.
- **UpdatePropertyResponse**: Modelo de salida del caso de uso que presenta la entidad actualizada y la URL pública o relativa resultante.
- **UploadedImageInfo**: Representación del archivo recibido y validado, con nombre, tipo, tamaño, identificador y ruta de almacenamiento final.
- **PropertyStatus**: Estado existente del dominio reutilizado según la especificación de persistencia y catálogo de propiedades.

## Requisitos no funcionales

- **NFR-001**: El caso de uso DEBE seguir el patrón Vertical Slice del repositorio y respetar la organización por feature, con un slice específico para la actualización y auto-registro mediante `ISlice` e `IHandler`.
- **NFR-002**: La validación del archivo DEBE ejecutarse en el backend sin depender de la lógica de frontend ni de rutas locales del entorno.
- **NFR-003**: El endpoint DEBE usar logging estructurado y manejo de errores consistente con `ProblemDetails` para registrar fallos de validación y almacenamiento.
- **NFR-004**: La funcionalidad DEBE ser compatible con la infraestructura de `AppDbContext`, dominio previo y file serving del backend, así como con la gobernanza Spec-Driven del repositorio.
- **NFR-005**: El tiempo de respuesta en solicitudes válidas DEBE mantener una latencia p95 razonable para un entorno local de pruebas funcionales; la muestra de validación debe demostrar que la operación no introduce retrasos significativos frente a la creación y consumo actual del sistema.

## Criterios de éxito

### Resultados medibles

- **SC-001**: El backend expone un endpoint de actualización por id que admite cambios válidos de propiedad con y sin imagen.
- **SC-002**: En el 100 % de las actualizaciones sin imagen, el valor de `imageUrl` previo se conserva sin cambiar, vaciarse ni reemplazarse.
- **SC-003**: En el 100 % de las actualizaciones con imagen válida, el archivo se guarda bajo `wwwroot/assets/properties` y la URL persistida corresponde al recurso público final.
- **SC-004**: El sistema responde con `404` cuando el id no existe y no produce cambios de persistencia ni almacenamiento.
- **SC-005**: El sistema responde con `415` para archivos vacíos, con contenido real inválido o con formato no soportado, y con `413` cuando el tamaño supera 5 MB.
- **SC-006**: Si falla el guardado de imagen, la operación no deja la propiedad en un estado parcialmente actualizado; la transacción y el almacenamiento permanecen consistentes.
- **SC-007**: El 100 % de las validaciones de negocio fallidas devuelve `400` con detalle de error, y cualquier error interno de almacenamiento devuelve `500` con el ProblemDetails manejado por la infraestructura.
- **SC-008**: La implementación usa el patrón Vertical Slice con `UpdateProperty` dentro de `Features/Properties`, `ISlice`, `IHandler`, `mapping` y `validator`, sin lógica transversa fuera del slice.
- **SC-009**: En una muestra funcional de solicitudes válidas, el p95 de latencia no excede 500 ms para actualizaciones sin imagen y 750 ms para actualizaciones con imagen válida en un entorno local de pruebas, conservando la rapidez esperada del backend.
- **SC-010**: La feature es compatible con las specs 001-005 y no introduce regresiones en el modelo de dominio, la persistencia y el listado ya definidos.

## Compatibilidad con specs previas y gobernanza del repositorio

- **001-realtor-solution-foundation**: La feature respeta la base funcional del backend, el modelo de dominio y la estructura general del proyecto.
- **002-foundation-backend**: La actualización se integra con la arquitectura Minimal API y con la infraestructura de servicio/host que ya existe en la solución.
- **003-properties-persistence-seeding**: La operación de actualización conserva la persistencia de propiedades, la semántica de estados y la estrategia de seed y datos de prueba ya aprobada.
- **004-properties-list-pagination**: El endpoint de actualización no altera la forma en que el listado expone `imageUrl`, ni los contratos públicos ya definidos para la lectura.
- **005-properties-create**: La feature mantiene la misma lógica para validación de imágenes, guardado de asset y rutas públicas, y extiende esa lógica para soportar cambios sobre registros existentes sin romper el patrón de creación.
- **Gobernanza Spec-Driven**: La especificación se mantiene como descripción de la intención de negocio, con alcance claro, pruebas independientes, requisitos testables y dependencia explícita con las features previas.

## Suposiciones

- La entidad `Property` ya existe en el dominio y la persistencia con los campos editables y normativa de estados previamente definidos por las specs de foundation y persistencia.
- El directorio de archivos estáticos del backend ya sirve contenido desde `wwwroot` y se sigue la convención de almacenamiento en `assets/properties` o `file:properties` según la convención vigente del repositorio.
- La operación de actualización es pública y no requiere autenticación ni autorización en esta especificación, salvo que una feature posterior lo defina explícitamente.
- El archivo de imagen es opcional; cuando no se incluye, la operación no debe descartar la imagen actual ni su valor persistido.
- La generación de nombres para archivos debe garantizar unicidad y no basarse únicamente en el nombre original del archivo para evitar colisiones.
- El request de actualización no permitirá modificar campos que no sean editables por el caso de uso, y los cambios no autorizados deben rechazarse con `400 Bad Request`.
