# Investigación de diseño: alta de propiedades con imagen opcional

## Decisiones

### Decisión 1: endpoint público y contrato de entrada
- El caso de uso se expondrá como `POST /api/properties` dentro del patrón Minimal API actual del repositorio.
- El payload de entrada será un modelo explícito del slice, con campos escalares de la propiedad y una imagen opcional.
- El contrato de entrada priorizará `multipart/form-data`, manteniendo un modelo claro de datos del inmueble y un archivo `IFormFile` opcional.

**Racional**: La feature requiere la carga de archivo y el repositorio ya usa Minimal APIs y `ISlice` para exposiciones públicas. El multipart form-data es el mecanismo natural para enviar archivo y metadata en el mismo request.

**Alternativas consideradas**:
- JSON con un campo base64: descartado porque complica la persistencia y la validación del upload y no encaja con la estrategia de `wwwroot`.
- Endpoint separado para subir imagen: descartado porque la feature exige un caso de uso único de alta de propiedad con imagen opcional.

### Decisión 2: manejo de la imagen opcional
- Si no llega archivo, el sistema considera la operación válida y persiste `ImageUrl` como `null`.
- Si llega un archivo válido, se guarda en `wwwroot/assets/properties`, con nombre seguro y con una ruta pública final compatible con el host de la API.

**Racional**: La especificación exige que la imagen sea opcional y que la ruta se persista como URL consumible por clientes.

**Alternativas consideradas**:
- Guardar en rutas físicas de `support`: descartado porque rompe la regla de ruta pública final y la experiencia del cliente.
- Forzar la imagen como obligatoria: descartado porque no cumple la especificación y rompe compatibilidad.

### Decisión 3: validación del upload
- La validación se realizará en el validator del slice con reglas para: archivo presente o no, formato permitido (`png`, `jpg`, `jpeg`) y tamaño máximo 5MB.
- En caso de error, la propiedad no debe persistirse ni guardarse ningún archivo.

**Racional**: El proyecto usa FluentValidation y `ProblemDetails` y la feature exige una validación funcional explícita del upload.

**Alternativas consideradas**:
- Validación solo en el cliente: descartado porque el backend debe asumir responsabilidad del contrato y de la integridad.
- Validación por extensión sin inspección real del archivo: descartado porque la seguridad del backend exige validación más robusta.

### Decisión 4: modelo y mapping
- El request de creación se convertirá a entidad `Property` via mapping explícito del slice.
- El mapping debe ser una transformación pura; la lógica de negocio y la validación del archivo se mantienen en handler y validator.

**Racional**: La constitución del repositorio exige separación de responsabilidades y evita lógica de negocio dentro del mapeo.

**Alternativas consideradas**:
- Incorpora lógica de subida de imagen en `Mapping`: descartado por violar el patrón requerido.
- Generar entidad en el endpoint: descartado por dispersar lógica del caso de uso.

### Decisión 5: respuesta y errores esperados
- La operación exitosa devuelve `201 Created` con la entidad creada.
- Los errores de validación devuelven `ProblemDetails` con detalle claro y identificador del campo o regla fallida.
- Los errores de archivo no soportado o exésis de tamaño se gestionan como validación del caso de uso y no como excepciones técnicas.

**Racional**: El backend ya usa `ProblemDetails` y la feature exige respuestas testables y estandarizadas.

**Alternativas consideradas**:
- Respuestas genéricas `500` o textos libres: descartado por no cumplir la API contract y la especificación.
- Hacer la imagen obligatoria con `415` para sin archivo: descartado porque contradice la especificación.

## Requisitos de diseño derivados

- La feature debe quedar en `Features/Properties/CreateProperty` y reutilizar los modelos de dominio ya existentes de `Property` y `PropertyStatus`.
- El `CreatePropertyHandler` debe crear la entidad y persistirla con `AppDbContext` usando el patrón `IHandler` del repositorio.
- El `CreatePropertyEndpoint` debe mapear el endpoint y delegar la validación y la ejecución del caso de uso.
- El storage del archivo debe ser un comportamiento explícito del handler, no un comportamiento global oculto.
- Las pruebas deben validar el flujo real: sin imagen, con imagen válida y con rechazo por tipo/tamaño.

## Riesgos y mitigación

- Archivo mal formado o no válido: se rechaza con `ProblemDetails` y sin persistencia parcial.
- Ruta pública inconsistente: se construye a partir de la ruta pública final y no de una ubicación local o de `support`.
- Mapeo y validación mezclados: se separan por responsabilidades para respetar la arquitectura de slices.
- Regressions en la base: reutilización del dominio y persistencia previos, pruebas de integración del caso de uso y sin cambios no requeridos.
