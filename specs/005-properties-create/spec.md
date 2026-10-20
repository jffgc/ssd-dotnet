# Especificación de la Funcionalidad: Alta de propiedades con upload opcional de imagen

**Feature Branch**: `005-properties-create`

**Creado**: 2026-10-19

**Estado**: En especificación

**Entrada**: Descripción del usuario: "Crear la especificación 005-properties-create. Objetivo: Definir un nuevo caso de uso de alta de propiedades mediante un slice Minimal API en backend. La especificación debe cubrir un endpoint público de creación que reciba los datos de la propiedad y permita de forma opcional el upload de imagen. Si no se envía imagen, la propiedad debe persistirse correctamente con imageUrl en null. Si se envía imagen válida, debe almacenarse en app/backend/src/RealtorApi/wwwroot/assets/properties y persistirse la URL pública/relativa correspondiente en la propiedad para su consumo posterior por clientes. Incluir criterios de validación funcional del upload (formatos PNG/JPG y tamaño máximo 5MB), reglas de respuesta y errores esperados, escenarios independientes por prioridad (P1/P2/P3), requisitos funcionales testables y criterios de éxito medibles. Mantener alineación con la arquitectura existente de Vertical Slice (slice + handler + mapping + validator), auto-registro por IHandler e ISlice, y mapeos explícitos por slice sin lógica de negocio en mapping. Respetar gobernanza Spec-Driven del repositorio y dependencias con specs previas de foundation, persistencia y listado."

## Escenarios de Usuario y Pruebas

### Historia de usuario 1 - Creación de propiedad sin imagen (Prioridad: P1)

El equipo de productos necesita dar de alta propiedades sin forzar una imagen asociada. El backend debe permitir crear una propiedad con todos los datos obligatorios y dejar `imageUrl` en `null` cuando el cliente no envía archivo. Esto garantiza compatibilidad con propiedades ya conocidas o en proceso de captura en etapas tempranas.

**Por qué esta prioridad**: Es el flujo principal del caso de uso y define la base de escritura del catálogo inmobiliario; además permite no romper clientes existentes ni datos ya persistidos en la base.

**Prueba independiente**: Se puede verificar enviando una petición válida sin archivo y comprobando que la respuesta del endpoint es exitosa y que el registro se persiste con `ImageUrl = null`.

**Escenarios de aceptación**:

1. **Dado** que el cliente invoca `POST /api/properties` con datos válidos de una propiedad, **cuando** no incluye imagen en la petición, **entonces** la API debe crear el registro y devolver `201 Created` con la propiedad creada.
2. **Dado** que la propiedad se guarda en base de datos, **cuando** se inspecciona el valor persistido, **entonces** `ImageUrl` debe quedar en `null` y no debe generarse una URL vacía ni un path físico.
3. **Dado** que la creación incluye el resto de campos obligatorios, **cuando** la validación del slice se ejecuta, **entonces** la propiedad se mantiene consistente con la configuración de dominio y persistencia establecida por la spec previa de propiedades.

### Historia de usuario 2 - Upload de imagen válida y almacenamiento público (Prioridad: P1)

El equipo necesita soportar la carga de imágenes en la creación de propiedades para enriquecer la oferta inmobiliaria. Cuando el cliente envía una imagen válida, el backend debe guardarla en la carpeta pública del sitio y persistir la URL pública o relativa que los clientes puedan consumir directamente.

**Por qué esta prioridad**: La imagen es un dato de negocio clave para la experiencia de listado y detalle; si no se almacena correctamente, la propiedad queda inconsistente para consumidores frontend y clientes externos.

**Prueba independiente**: Se puede verificar subiendo un archivo PNG o JPG de 1MB y comprobando que se guarda bajo `wwwroot/assets/properties`, que la propiedad persiste la URL relativa correcta y que la ruta es públicamente servida por la API.

**Escenarios de aceptación**:

1. **Dado** que el cliente envía un archivo PNG o JPG válido junto a una propiedad, **cuando** la API procesa la petición, **entonces** el archivo se guarda en `app/backend/src/RealtorApi/wwwroot/assets/properties`.
2. **Dado** que la imagen se ha almacenado correctamente, **cuando** se guarda la propiedad en la base de datos, **entonces** `ImageUrl` debe apuntar a la ruta pública final del asset y no a una ruta de soporte o física del repositorio.
3. **Dado** que la API expone contenido estático bajo `wwwroot`, **cuando** un cliente solicita la imagen, **entonces** debe poder consumir el recurso sin requerir rutas internas del servidor ni paths de infraestructura.

### Historia de usuario 3 - Validación de upload y errores esperados (Prioridad: P1)

El sistema debe refutar archivos inválidos con mensajes claros y errores HTTP estándar para que el cliente pueda corregir la carga. La validación debe cubrir formato, tamaño, presencia de archivo y contenido del payload de la petición.

**Por qué esta prioridad**: Las respuestas de error deben ser previsibles y testables para evitar datos inconsistentes y facilitar la integración del cliente con la API.

**Prueba independiente**: Se puede verificar enviando un archivo con extensión no soportada, un peso superior a 5MB o un payload mal formado y comprobando que la API devuelve un `ProblemDetails` con el código y el detalle esperados.

**Escenarios de aceptación**:

1. **Dado** que el cliente envia un archivo en formato no permitido, **cuando** el validator del slice lo analiza, **entonces** la API responde con error de validación de tipo de archivo y no persiste la propiedad ni el asset.
2. **Dado** que el cliente envía un archivo superior a 5MB, **cuando** la petición llega al endpoint, **entonces** la API rechaza la carga con error equivalente a tamaño no permitido y no guarda ni crea datos parciales.
3. **Dado** que el payload no cumple la estructura esperada o el tipo de contenido es incorrecto, **cuando** se procesa la petición, **entonces** la API devuelve un error de validación y el campo responsable queda identificado en la respuesta.

### Historia de usuario 4 - Integración con arquitectura vertical slice y dependencias previas (Prioridad: P2)

La funcionalidad debe encajar en la arquitectura existente del backend: slice por caso de uso, endpoint Minimal API auto-descubierto, handler para negocio, mapping explícito y validator para reglas de entrada. La nueva feature debe reutilizar el dominio, persistencia y el modelo de listado ya definidos en specs anteriores sin duplicar responsabilidades ni romper la estructura funcional.

**Por qué esta prioridad**: Garantiza coherencia tecnica y evita un crecimiento de la base de código basado en capas transversales que no respetan el patrón del repositorio.

**Prueba independiente**: Se puede validar revisando la estructura del slice, la auto-registro del endpoint y una prueba de integración del caso de uso con la base de datos real o de prueba.

**Escenarios de aceptación**:

1. **Dado** que el caso de uso está implementado en `Features/Properties/CreateProperty`, **cuando** la aplicación arranca, **entonces** el endpoint se auto-registra bajo la infraestructura de `ISlice` y `IHandler` sin cambios manuales en `Program.cs`.
2. **Dado** que el modelo de propiedad ya existe en dominio y persistencia, **cuando** el slice se ejecuta, **entonces** el mapping del request a entidad es explícito y no incorpora lógica de negocio, respetando la separación de responsabilidades del proyecto.
3. **Dado** que la feature compone con la base de datos y la lógica existente, **cuando** se ejecutan las pruebas del slice, **entonces** no se introducen regressions ni cambios no requeridos en `HealthSlice` ni en otras features de negocio.

### Historia de usuario 5 - Criterios de calidad y disponibilidad del recurso público (Prioridad: P3)

El equipo necesita garantizar que la URL de imagen persistida sea consumible por clientes de frontend y por consumidores externos, y que la respuesta del endpoint sea estable y documentada. El objetivo es evitar entradas no reproducibles y garantizar que la API no devuelve rutas de sistema no servidas por `wwwroot`.

**Por qué esta prioridad**: Aunque no bloquea la escritura base, esta validación asegura la calidad operativa y la consumibilidad del asset una vez creada la propiedad.

**Prueba independiente**: Se puede comprobar con una integración de cliente o un test que la URL pública devuelta por la API sea accesible y no incluya rutas locales del entorno.

**Escenarios de aceptación**:

1. **Dado** que una propiedad se crea con imagen, **cuando** la API responde, **entonces** la URL devuelta es relativa o pública y compatible con la ruta del aplicativo.
2. **Dado** que la imagen se guarda bajo `wwwroot`, **cuando** se accede con la URL persistida, **entonces** el recurso debe servirse de forma estable y sin depender de un path local del entorno.
3. **Dado** que la funcionalidad se integra con futuros listados, **cuando** el cliente consume las propiedades, **entonces** la imagen se puede presentar sin manipulación adicional ni rutas que rompan en diferentes entornos.

## Casos límite

- ¿Qué ocurre si el cliente no envía imagen? La propiedad debe crearse exitosamente y `imageUrl` debe quedar en `null`.
- ¿Qué ocurre si el archivo no es PNG/JPG? El backend debe rechazar la petición con error de validación y no guardar ningún archivo asociado.
- ¿Qué ocurre si el archivo pesa más de 5MB? La API debe devolver un error de validación/tamaño y no persistir el asset ni la propiedad.
- ¿Qué ocurre si el cliente envía la imagen con un nombre no seguro o una extensión mayúscula? El sistema debe normalizar o validar de forma consistente, y la persistencia debe respetar las reglas de nombres seguros y rutas estables.
- ¿Qué ocurre si la propiedad incluye datos obligatorios faltantes? El endpoint debe devolver `400 Bad Request` (o equivalente de `ProblemDetails`) con el detalle de cada validación fallida.
- ¿Qué ocurre si un cliente intenta subir un archivo con `Content-Type` inconsistente o un payload multipart corrupto? Debe devolverse error de estructura/validación y no generarse artefactos parciales.
- ¿Qué ocurre si la imagen es válida pero el directorio destino no existe? La aplicación debe crear el directorio requerido bajo `wwwroot/assets/properties` antes de guardar el archivo o fallar de forma clara y reproducible.

## Requisitos

### Requisitos funcionales

- **FR-001**: El backend DEBE exponer un endpoint público `POST /api/properties` para dar de alta propiedades dentro del patrón Minimal API y Vertical Slice ya establecido por la solución.
- **FR-002**: El caso de uso DEBE vivir bajo la estructura de Feature `Features/Properties/CreateProperty`, con la responsabilidad separada en slice, handler, mapping y validator, sin lógica de negocio en la capa de mapeo.
- **FR-003**: El endpoint DEBE admitir la carga de la propiedad con sus datos básicos y una imagen opcional como parte del payload de creación, priorizando un contrato multipart/form-data para el archivo y campos escalares para el resto de atributos.
- **FR-004**: Si no se envía imagen, la propiedad DEBE persistirse correctamente y `ImageUrl` DEBE quedar en `null` sin generar rutas inválidas ni marcas vacías.
- **FR-005**: Si se envía una imagen válida, el backend DEBE guardarla en `app/backend/src/RealtorApi/wwwroot/assets/properties` usando una estrategia de nombre estable y seguridad mínima para evitar colisiones innecesarias.
- **FR-006**: La propiedad persistida DEBE guardar la URL pública o relativa del asset en `ImageUrl`, con una ruta consumible por clientes y compatible con la estrategia de static files del backend.
- **FR-007**: El sistema DEBE aceptar únicamente imágenes en formato `PNG` o `JPG`/`JPEG` y rechazar cualquier otro tipo de archivo con error de validación funcional.
- **FR-008**: El archivo adjunto DEBE cumplir un tamaño máximo de 5MB. Si excede el límite, la petición DEBE rechazarse sin persistir la propiedad ni el archivo.
- **FR-009**: El endpoint DEBE devolver una respuesta exitosa con status `201 Created` y el recurso creado en el cuerpo de respuesta, incluyendo los datos persistidos y la URL de imagen cuando exista.
- **FR-010**: En caso de error de validación, el endpoint DEBE responder con un `ProblemDetails` consistente con la infraestructura del backend y con mensajes claros para el cliente.
- **FR-011**: Los errores esperados DEBEN incluir, al menos, validación de campos obligatorios, tipo de archivo no soportado, imagen demasiado grande y payload mal formado o incoherente.
- **FR-012**: La funcionalidad DEBE respetar la arquitectura existente de `ISlice` e `IHandler`, con auto-registro del endpoint en la aplicación y sin registrar handlers o endpoints de forma manual en `Program.cs`.
- **FR-013**: Los mappings DEBEN ser explícitos por slice y no encapsular lógica de negocio o reglas transversales; cualquier validación de negocio debe permanecer en el validator o en el handler.
- **FR-014**: El caso de uso DEBE mantenerse alineado con la estructura previa del dominio y la persistencia de propiedades, reutilizando los contratos de la spec de foundation/persistencia/listado y evitando cambios no requeridos sobre el dominio base.
- **FR-015**: La solución DEBE trabajar con la ruta pública final del asset y no con path físico del proyecto ni rutas de soporte, asegurando compatibilidad entre entornos de ejecución y consumo por clientes.
- **FR-016**: El backend DEBE incluir pruebas funcionales y de integración que cubran: creación sin imagen, creación con imagen válida, rechazo por tipo de archivo, rechazo por tamaño, persistencia de `imageUrl` y respuesta del endpoint con `ProblemDetails`.

### Reglas de respuesta y errores esperados

- **Respuesta exitosa**: `201 Created` con la entidad creada, incluyendo el identificador, datos del inmueble y `imageUrl` cuando exista.
- **Sin imagen**: `201 Created` y `imageUrl` = `null`.
- **Validación de campos obligatorios**: error de validación con `400 Bad Request` o su equivalente `ProblemDetails`.
- **Imagen inválida o formato no soportado**: error de validación con detalle indicando que solo se admiten PNG/JPG/JPEG.
- **Archivo demasiado grande**: error de validación o `413 Payload Too Large` cuando la infraestructura lo soporte; la propiedad no debe persistirse.
- **Payload mal formado**: `400 Bad Request` con descripción del error y, de ser posible, el campo que falló.
- **Archivo no cargado**: se considera una creación válida sin archivo; no debe producirse fallo si la imagen es opcional.

## Entidades clave

- **Property**: Entidad principal del catálogo inmobiliario utilizada en la creación y persistencia de propiedades.
- **CreatePropertyRequest**: Modelo de entrada del caso de uso, definido por el slice para recibir los datos de la propiedad y el archivo opcional.
- **CreatePropertyResponse**: Modelo de salida del caso de uso para devolver la propiedad creada con `imageUrl` y metadatos mínimos.
- **UploadedImageInfo**: Representación del archivo recibido y validado, con nombre, tipo, tamaño y ruta final de almacenamiento.
- **PropertyStatus**: Estado existente en el dominio de propiedades, reutilizado desde la spec previa de persistencia para mantener consistencia del catálogo.

## Requisitos no funcionales

- **NFR-001**: El caso de uso DEBE seguir el patrón de Vertical Slice del repositorio y respetar la estructura de features y capas transversales ya definidas.
- **NFR-002**: La validación del archivo DEBE ejecutarse en el stack del backend sin depender de lógica en frontend ni de paths manuales del entorno local.
- **NFR-003**: La API DEBE usar logging estructurado y manejo de errores consistente con `ProblemDetails` para los errores esperados.
- **NFR-004**: La solución DEBE ser compatible con los requisitos de foundation y persistencia previamente definidos para evitar inconsistencias en el ámbito del dominio inmobiliario.

## Criterios de éxito

### Resultados medibles

- **SC-001**: El backend expone un endpoint público de creación que permite dar de alta una propiedad con y sin imagen.
- **SC-002**: Una propiedad creada sin imagen persiste con `ImageUrl = null` y sin generar un valor vacío o relativo incorrecto.
- **SC-003**: Una propiedad creada con imagen válida guarda el archivo en `wwwroot/assets/properties` y persiste la URL pública o relativa correspondiente.
- **SC-004**: La validación rechaza archivos con tipo no soportado o tamaño mayor a 5MB, y devuelve respuestas consistentes con `ProblemDetails`.
- **SC-005**: La implementación usa el patrón Vertical Slice explícito con `CreateProperty` dentro de `Features/Properties`, `ISlice`, `IHandler`, `mapping` y `validator` y sin lógica transversa fuera de la feature.
- **SC-006**: Las pruebas del caso de uso cubren la creación exitosa sin imagen, creación exitosa con imagen, rechazo por archivo inválido y rechazo por tamaño excedido.
- **SC-007**: La funcionalidad es compatible con la arquitectura y los datos previos de foundation, persistencia y listado, y no introducen regresiones en la base del sistema.

## Suposiciones

- La infraestructura previa de `AppDbContext`, dominio, persistencia y enumeración de estados ya existe o se consolida según la spec de persistencia.
- La API ya sirve contenido estático desde `wwwroot` y la ruta pública de assets se resuelve acorde al hosting actual del backend.
- El caso de uso de alta es público y no exige autenticación ni autorización en esta especificación, salvo que la feature posterior las defina explícitamente.
- El archivo de imagen se considera opcional; la propiedad sin imagen es un caso válido y no debe fallar la creación.
- Los nombres de archivo y la resolución final de la imagen deben ser deterministas y compatibles con múltiples arranques y despliegues del backend.
