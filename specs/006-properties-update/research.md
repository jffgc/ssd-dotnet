# Investigación de diseño: actualización de propiedades con imagen opcional

## Decisiones

### Decisión 1: endpoint y recuperación de la entidad
- El caso de uso se expondrá como `PUT /api/properties/{id}` dentro del patrón Minimal API actual del repositorio.
- El handler debe recuperar la entidad por id sobre `AppDbContext` antes de aplicar cambios.
- Si la propiedad no existe, la operación debe responder `404` y no guardar cambios ni escribir archivos.

**Racional**: La especificación exige una búsqueda previa y una respuesta no destructiva en caso de entidad inexistente, además de mantener el comportamiento esperado por las specs previas.

**Alternativas consideradas**:
- Actualizar primero y luego verificar existencia: descartado porque viola la regla de no cambios si no existe la entidad.
- Hacer la actualización sin consultar el id: descartado porque rompe la semántica base del caso de uso y la integridad del dominio.

### Decisión 2: comportamiento cuando no se envía imagen
- Si el request no incluye archivo, el sistema debe conservar el `imageUrl` previo.
- Los campos editables actuales deben actualizarse sin tocar la imagen ya persistida.

**Racional**: Esta regla evita pérdida de contenido visual y mantiene la compatibilidad con propiedades ya publicadas en la base.

**Alternativas consideradas**:
- Borrar `imageUrl` al actualizar sin archivo: descartado porque no cumple la especificación y produce regresiones.
- Reemplazar la imagen con un valor vacío o `null`: descartado por los requisitos funcionales y la semántica de negocio.

### Decisión 3: almacenamiento y nombre de archivo
- Si el request incluye una imagen válida, el backend guardará el archivo en la carpeta pública del proyecto para el asset de propiedades.
- El nombre del archivo deberá resolverse con un valor único generado por UUID más la extensión permitida.
- La ruta persistida debe ser la URL pública o relativa final, compatible con el `UseStaticFiles` del backend.

**Racional**: La especificación exige evitar colisiones de nombre y usar la misma lógica de `005` para las rutas públicas de la imagen.

**Alternativas consideradas**:
- Usar el nombre original del archivo: descartado por riesgo de colisiones y sobrescrituras accidentales.
- Guardar el asset fuera de la ruta pública: descartado porque rompe la ruta consumible por clientes y la convención del repositorio.

### Decisión 4: atomicidad de la actualización
- El almacenamiento de la nueva imagen y la actualización de la entidad deben comportarse como una sola operación lógica.
- Si el guardado o la escritura del archivo falla, se debe abortar la actualización y no dejar la base de datos con un `imageUrl` parcial o inconsistente.

**Racional**: La especificación exige que la operación sea atómica, aunque el almacenamiento esté fuera de la base de datos.

**Alternativas consideradas**:
- Actualizar primero la base de datos y luego guardar la imagen: descartado porque rompe la consistencia en fallos intermedios.
- Dejar `imageUrl` actualizado aunque falle el archivo: descartado por la obligación de no dejar estado parcial.

### Decisión 5: validación de upload y semántica de errores
- El validator del slice validará: archivo vacío, tipo no soportado, tamaño > 5MB y payload real inválido.
- El backend mantendrá la semántica de errores establecida en 005: `400`, `415`, `413` y `500` según el tipo de fallo.
- La ausencia de imagen será tratada como camino válido y no como error.

**Racional**: La feature requiere compatibilidad con la lógica de validación ya definida y con la API contract establecida en features anteriores.

**Alternativas consideradas**:
- Hacer la imagen obligatoria: descartado por no cumplir la especificación.
- Tratar todos los fallos de archivo como `500`: descartado porque rompe la contract y la semántica de errores esperada.

## Requisitos de diseño derivados

- La feature debe quedar en `Features/Properties/UpdateProperty` y reutilizar el dominio de propiedades ya definido.
- El `UpdatePropertyHandler` debe recuperar la entidad, aplicar cambios válidos y persistir la actualización en `AppDbContext` con `SaveChangesAsync`.
- El `UpdatePropertyEndpoint` debe mapear el route y delegar la validación y la ejecución del caso de uso.
- El almacenamiento del archivo debe hacerse de forma explícita dentro del handler o un servicio local del slice, sin mezclar la lógica de archivo con el mapeo.
- Las pruebas deben cubrir el flujo real con `AppDbContext` y una muestra funcional de validación del archivo.

## Riesgos y mitigación

- Propiedad inexistente: se valida primero la búsqueda por id y se responde con `404` sin escritura.
- Imagen inválida o corrupta: se rechaza con `415` antes de actualizar la entidad.
- Validación de tamaño: el `validator` debe rechazar archivos > 5MB sin guardar nada.
- Ruta pública inconsistente: se construye con la raíz pública del backend y no con rutas físicas internas.
- Estado parcialmente actualizado: se requiere almacenamiento atómico y validación previa antes de persistir los cambios de dominio.
