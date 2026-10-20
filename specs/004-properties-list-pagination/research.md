# Investigación: Listado paginado de propiedades

## Contexto del repositorio

- API backend con Minimal APIs, slices auto-descubiertos mediante `ISlice`, handlers con `IHandler` registrados por reflexión y filtro global de validación FluentValidation.
- La entidad persistente `RealtorApi.Domain.Properties.Property` contiene `Id`, `Title`, `Description`, `Address`, `Price`, `Status`, `BedroomCount`, `BathroomCount`, `AreaSquareMeters` e `ImageUrl`.
- `AppDbContext` expone `DbSet<Property>` y descubre configuraciones desde el assembly.
- `Program.cs` ya habilita `UseStaticFiles`; los archivos de imagen de soporte se copian bajo `wwwroot/images/properties`.
- Los datos semilla usan rutas públicas `/images/properties/{fileName}`. El contrato aprobado exige `/assets/properties/{fileName}`.
- Los tests usan xUnit. No existe archivo `RealtorApi.http` en el proyecto actualmente.

## Decisiones

### 1. Implementar el endpoint como slice auto-descubierto

**Decisión**: Añadir un slice stateless en `Features/Properties/ListProperties`. El endpoint delega al handler scoped a través de servicios de la solicitud; el handler realiza la consulta a `AppDbContext`.

**Justificación**: Sigue los patrones `ISlice` e `IHandler` ya presentes y evita modificar el registro central de endpoints o añadir dependencias scoped a un slice singleton.

**Alternativas consideradas**:

- Registrar el endpoint directamente desde `Program.cs`: rechazado por la convención `ISlice` y el auto-descubrimiento existentes.
- Añadir controllers o un registry manual: prohibido por la spec y las instrucciones del backend.
- Añadir un repositorio dedicado: innecesario para una única proyección de EF Core y contrario a evitar abstracciones triviales.

### 2. Validar entrada con FluentValidation y emitir detalles de validación

**Decisión**: Definir un request específico del slice que conserve los valores de consulta como texto hasta validarlos. El validator aplica valores por defecto 1/6, exige enteros `Int32` positivos y `pageSize <= 100`. El repositorio sí tiene `ValidationFilterFactory`, pero `MapSliceEndpoints` no lo aplica actualmente; T014 conectará el filtro al mapeo central y resolverá validators scoped desde los servicios de cada solicitud para responder 400 con `ValidationProblemDetails`.

**Justificación**: Completa el patrón de validación documentado sin resolver servicios scoped al mapear endpoints y mantiene la validación fuera del handler de consulta.

**Alternativas consideradas**:

- Validación manual dentro del endpoint: rechazada por el patrón centralizado y porque agranda el endpoint.
- Limitar silenciosamente el tamaño máximo: rechazado; la clarificación aprobada exige 400.
- Confiar únicamente en el model binding para valores no numéricos: no garantiza el detalle de validación requerido para cada parámetro; el request/validator debe conservar una respuesta 400 con información del campo.
- Resolver un validator scoped durante el mapeo de slices: rechazado porque los slices se registran como singleton y el provider de mapeo no tiene el lifetime de una solicitud.

### 3. Proyectar y paginar directamente con EF Core

**Decisión**: Obtener el total y los resultados con operaciones EF Core asíncronas y `CancellationToken`. Aplicar `AsNoTracking`, ordenar por `Title`, proyectar solo los campos del contrato y usar `Skip`/`Take` con el tamaño ya validado. Calcular el desplazamiento con aritmética amplia antes de convertirlo al argumento de EF; si excede el rango que acepta `Skip`, devolver página vacía con metadatos calculados del total.

**Justificación**: La lectura no necesita entidades completas. La validación limita `page` a un entero `Int32` positivo. Evitar desbordamiento del producto `(page - 1) * pageSize` mantiene predecible el caso extremo: si el desplazamiento válido excede el rango de `Skip`, se devuelve una página vacía con metadatos calculados del total.

**Alternativas consideradas**:

- Cargar primero todas las propiedades y paginar en memoria: rechazado por coste y transferencia innecesarios.
- Añadir un repositorio o una abstracción genérica de paginación: rechazado por no aportar reutilización requerida.
- Aceptar páginas fuera del rango de `Int32`: rechazado por decisión aprobada; se responden como entradas inválidas con HTTP 400 y detalles.

### Logging estructurado del handler

**Decisión**: Inyectar `ILogger<ListPropertiesHandler>` en el handler y emitir un evento estructurado con `Page`, `PageSize` y `TotalItems` al completar la lectura. Las pruebas unitarias del handler verifican que se registre el evento y no se incluyen datos personales ni descripciones.

**Justificación**: Satisface el requisito constitucional de logging estructurado sin registrar contenido descriptivo del catálogo.

### 4. Definir metadatos según la aclaración aprobada

**Decisión**: `totalPages` es cero cuando `totalItems` es cero; en otro caso es el techo de `totalItems / pageSize`. `hasNext` indica que el número de página solicitado es menor que `totalPages`. `hasPrevious` es verdadero exactamente cuando `page > 1 && totalItems > 0`, incluso para una página solicitada fuera de rango.

**Justificación**: Conserva la regla explícita acordada y refleja fielmente el estado del conjunto sin transformar una página fuera de rango en error.

**Alternativas consideradas**:

- Hacer `hasPrevious` falso para páginas mayores que `totalPages`: incompatible con la decisión del usuario.
- Devolver 404 para una página vacía: incompatible con los casos límite de la spec.

### 5. Exponer el estado como texto y usar contratos explícitos

**Decisión**: El DTO de salida usa tipos explícitos e inmutables, con `status` representado por el nombre del estado (`Available`, `Rented` o `Maintenance`) y `imageUrl` como URL absoluta. El mapping se mantiene en el slice y no se cambia la serialización JSON global.

**Justificación**: Los consumidores reciben un estado legible y estable frente a reordenamientos del enum; una conversión local evita efectos colaterales sobre otros contratos.

**Alternativas consideradas**:

- Serializar el valor numérico del enum: rechazado por menor legibilidad y estabilidad externa.
- Configurar un conversor de enum global: rechazado porque la funcionalidad no debe alterar otros endpoints.
- Devolver la entidad EF: prohibido por la spec.

### 6. Cumplir la ruta pública de imágenes conservando compatibilidad

**Decisión**: Mantener el middleware estático actual y agregar una configuración estática de `RequestPath` `/assets/properties` que sirva el directorio físico existente de imágenes de propiedades. El mapping construye `imageUrl` con el esquema y host de la solicitud y el nombre de archivo tomado del `ImageUrl` persistido.

**Justificación**: Hace que el URL requerido sea servible sin cambiar seeds, datos persistidos ni retirar `/images/properties`.

**Alternativas consideradas**:

- Devolver `/images/properties`: incompatible con el contrato aprobado.
- Cambiar los valores persistidos y seeds: introduce cambios de persistencia innecesarios y fuera del alcance.
- Devolver una URL absoluta que apunta a un path sin mapeo estático: no satisface que la imagen sea servible.
- Crear un endpoint dinámico para transferir archivos: innecesario dado que ya se usa middleware estático.

### 7. Limitar las pruebas a pruebas unitarias más verificación manual

**Decisión**: Añadir pruebas unitarias para validación, metadatos, orden/paginación del handler y mapping. Agregar solicitudes manuales `.http` y una guía de ejecución. No añadir `WebApplicationFactory`, Testcontainers ni PostgreSQL de pruebas.

**Justificación**: La constitución permite unit tests por defecto; las pruebas de integración requieren justificación explícita en spec, plan y tasks, que no forma parte del alcance aprobado.

**Alternativas consideradas**:

- Crear pruebas de integración HTTP o con PostgreSQL: excluidas por la regla constitucional sin aprobación explícita.
- Depender solo de pruebas manuales: insuficiente para reglas deterministas de paginación y validación.

## Supuestos y límites

- Los registros que se listan tienen una imagen persistida con nombre de archivo válido, como establece la spec.
- Los archivos físicos de las imágenes están disponibles en el directorio que actualmente se sirve bajo `wwwroot/images/properties`.
- No se actualizarán las referencias del SDK ni paquetes del backend como parte de la funcionalidad.
- La carpeta `Features/Properties` se crea si todavía no existe.
- El `.http` utiliza el host local de desarrollo; el puerto puede ajustarse al perfil activo.

## Investigación pendiente

No quedan decisiones de arquitectura pendientes que bloqueen la planificación. La validación futura deberá confirmar la ruta física resuelta por `StaticFileOptions` en el perfil de ejecución real; cualquier variación de rutas por entorno debe mantenerse dentro del mecanismo existente de archivos estáticos, sin alterar el contrato público.
