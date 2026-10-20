# Especificación de la funcionalidad: Listado paginado de propiedades

**Feature Branch**: `004-properties-list-pagination`

**Creado**: 2026-10-18

**Estado**: Aprobada para implementación (2026-10-19)

**Entrada**: Descripción del usuario: “Definir un listado paginado de propiedades mediante `GET /api/properties`, con `page` y `pageSize` (predeterminados a 1 y 6), orden ascendente por título, respuesta con los campos de propiedad y metadatos de paginación, URL absoluta pública de imagen, validación 400 con ProblemDetails, cancelación propagada, mapping explícito y proyección de lectura. Respetar Minimal API con ISlice y el modelo persistente actual, no usar controllers, y añadir ejemplos manuales en el archivo `.http`.”

## Clarifications

### Session 2026-10-18

- Q: ¿Cómo debe calcularse `hasPrevious` si se solicita una página fuera del rango disponible? → A: `hasPrevious` es verdadero si `page > 1` y `totalItems > 0`; de lo contrario, es falso.
- Q: ¿Debe `pageSize` tener un límite máximo? → A: `pageSize` tiene un máximo de 100; los valores mayores reciben 400 con detalles de validación.

### Session 2026-10-19

- Q: ¿La spec está aprobada para implementación? → A: Sí, el usuario confirma su aprobación para implementación.
- Q: ¿Cómo debe responderse a un `page` numérico mayor que el máximo entero de 32 bits? → A: Rechazar con HTTP 400 y detalles de validación.

## Escenarios de usuario y pruebas

### Historia de usuario 1 - Consultar y recorrer el listado de propiedades (Prioridad: P1)

Una persona que consulta las propiedades necesita recibir un conjunto ordenado y acotado de resultados y conocer su posición dentro del catálogo, para poder revisar las propiedades disponibles y continuar a otras páginas.

**Por qué esta prioridad**: El listado paginado es la capacidad principal de esta funcionalidad y da acceso al catálogo sin exigir que el consumidor cargue todas las propiedades a la vez.

**Prueba independiente**: Consultar el endpoint con y sin parámetros, contrastar los elementos con un conjunto conocido de propiedades y verificar el orden, los campos de cada propiedad y los metadatos de página.

**Escenarios de aceptación**:

1. **Dado** que hay propiedades iniciales, **cuando** se consulta `GET /api/properties` sin parámetros, **entonces** se devuelve la página 1 con un máximo de 6 propiedades en orden ascendente por título.
2. **Dado** que hay suficientes propiedades para más de una página, **cuando** se consulta `GET /api/properties?page=2&pageSize=6`, **entonces** se devuelve el segundo segmento de resultados y metadatos coherentes con el total y la página solicitada.
3. **Dado** que el resultado contiene propiedades con imágenes persistidas, **cuando** se consulta el listado, **entonces** cada elemento incluye una URL absoluta pública correspondiente a su imagen.
4. **Dado** que la solicitud usa `http://localhost:5023`, **cuando** se devuelve una propiedad con imagen, **entonces** su URL sigue el patrón `http://localhost:5023/assets/properties/{fileName}`.

### Historia de usuario 2 - Recibir errores claros para parámetros inválidos (Prioridad: P2)

Una persona que consulta el listado necesita recibir una respuesta de validación comprensible cuando indica una página o un tamaño de página no válido, en lugar de recibir resultados engañosos.

**Por qué esta prioridad**: La validación mantiene predecible la navegación y permite al consumidor corregir la solicitud.

**Prueba independiente**: Enviar valores de página o tamaño de página iguales o menores que cero y comprobar que cada solicitud inválida recibe el estado 400 y detalles de validación.

**Escenarios de aceptación**:

1. **Dado** que `page` o `pageSize` es menor o igual que cero, **cuando** se consulta el listado, **entonces** se responde con 400 y ProblemDetails o ValidationProblemDetails que identifica el error de validación.
2. **Dado** que `pageSize` es mayor que 100, **cuando** se consulta el listado, **entonces** se responde con 400 y detalles de validación.
3. **Dado** que `page` es mayor que 2,147,483,647, **cuando** se consulta el listado, **entonces** se responde con 400 y detalles de validación.

### Casos límite

- Si no hay propiedades, el listado devuelve una colección vacía, `totalItems` y `totalPages` iguales a cero, y ambos indicadores de navegación en falso.
- Si la página solicitada excede el número de páginas disponibles, se devuelve una colección vacía y los metadatos siguen reflejando el total real y la página solicitada; no se transforma el caso en un error. `hasPrevious` es verdadero si `page > 1` y `totalItems > 0`, incluso cuando la página solicitada está fuera de rango.
- Si `page` o `pageSize` no se puede interpretar como entero positivo, la solicitud se trata como inválida y recibe 400 con detalles de validación.
- `page` debe caber en un entero positivo de 32 bits; valores mayores que 2,147,483,647 reciben 400 con detalles de validación.
- Si el catálogo contiene títulos repetidos, todos los elementos deben respetar el orden ascendente por título; no se exige un orden entre elementos con el mismo título.
- Se presupone que cada propiedad que se incluye en el catálogo tiene una imagen persistida servible. Definir una imagen alternativa para propiedades sin imagen queda fuera del alcance.

## Requisitos

### Requisitos funcionales

- **FR-001**: El backend DEBE ofrecer el listado en `GET /api/properties`, siguiendo el patrón Minimal API basado en ISlice y el auto-descubrimiento de endpoints existente. No se deben introducir controllers.
- **FR-002**: El listado DEBE aceptar los parámetros de consulta `page` y `pageSize`; cuando se omitan, sus valores serán respectivamente 1 y 6.
- **FR-003**: `page` y `pageSize` DEBEN ser enteros positivos representables como `Int32`; además, `pageSize` DEBE ser menor o igual que 100. Si alguno no cumple estas condiciones, el endpoint DEBE responder con estado 400 y ProblemDetails o ValidationProblemDetails con información de validación.
- **FR-004**: La respuesta DEBE contener como máximo `pageSize` propiedades y los resultados DEBEN estar ordenados por título ascendente.
- **FR-005**: Cada propiedad de la respuesta DEBE exponer, como mínimo, `id`, `title`, `description`, `address`, `price`, `status`, `bedroomCount`, `bathroomCount`, `areaSquareMeters` e `imageUrl`. La respuesta DEBE usar un contrato explícito y no exponer directamente entidades de persistencia.
- **FR-006**: `imageUrl` DEBE ser una URL absoluta pública con el patrón `http(s)://{host}/assets/properties/{fileName}`, derivada del esquema y host de la solicitud actual. No puede ser relativa ni revelar rutas físicas internas.
- **FR-007**: La respuesta DEBE incluir `page`, `pageSize`, `totalItems`, `totalPages`, `hasNext` y `hasPrevious`. Estos valores DEBEN ser coherentes con el conjunto completo y la página solicitada. `hasPrevious` será verdadero únicamente cuando `page > 1` y `totalItems > 0`, incluso si la página solicitada está fuera de rango; para un conjunto vacío, `totalPages` será cero y ambos indicadores serán falsos.
- **FR-008**: La lectura DEBE proyectar directamente los campos requeridos desde el modelo persistente existente, sin seguimiento de cambios, y el mapping de respuesta DEBE ser explícito. La lógica del caso de uso debe pertenecer al handler del slice y el endpoint debe limitarse a coordinar la solicitud.
- **FR-009**: El flujo de solicitud DEBE propagar la cancelación mediante `CancellationToken`.
- **FR-010**: El archivo `app/backend/src/RealtorApi/RealtorApi.http` DEBE incluir ejemplos manuales para `GET /api/properties` y `GET /api/properties?page=1&pageSize=6`.
- **FR-011**: La funcionalidad DEBE limitarse al listado paginado y mantener compatibilidad con el modelo persistente y las convenciones ISlice existentes. No debe introducir ordenamientos alternativos, filtros, mapeadores automáticos ni otras capacidades no solicitadas.
- **FR-012**: El handler DEBE emitir logging estructurado que registre la página, el tamaño solicitado y el total de resultados, sin incluir datos personales ni contenido descriptivo de las propiedades.

### Entidades clave

- **Propiedad del listado**: Propiedad existente del catálogo que se presenta con sus datos descriptivos, precio, estado, características, área e imagen pública.
- **Página del listado**: Resultado de una consulta que relaciona las propiedades devueltas con la página solicitada, el tamaño de página, el total de propiedades, el total de páginas y los indicadores de navegación.

## Criterios de éxito

### Resultados medibles

- **SC-001**: En todas las verificaciones de la solicitud sin parámetros, el sistema devuelve la página 1, no más de 6 propiedades y orden ascendente por título.
- **SC-002**: En todas las verificaciones con varias páginas, cada página devuelve el segmento solicitado del catálogo ordenado por título y sus seis metadatos coinciden con el conjunto de prueba.
- **SC-003**: El 100 % de las solicitudes con `page` o `pageSize` no positivo o no entero, con `page` mayor que 2,147,483,647, o con `pageSize` mayor que 100, reciben estado 400 y detalles de validación.
- **SC-004**: El 100 % de las propiedades devueltas con imagen tienen una URL absoluta pública que conserva el esquema y host de la solicitud y apunta a `/assets/properties/{fileName}`.
- **SC-005**: En el 100 % de las respuestas exitosas, cada elemento contiene los diez campos documentados y la página contiene los seis metadatos requeridos; `hasNext` y `hasPrevious` reflejan las reglas especificadas para las páginas de primera, intermedia, última, vacía y fuera de rango.

## Supuestos

- El modelo persistente existente y los datos iniciales proporcionan los campos requeridos para el listado, incluidas las imágenes servibles.
- La representación de `page` y `pageSize` en la entrada es compatible con enteros `Int32`.
- Una página fuera del rango disponible produce una colección vacía con metadatos válidos, no un error 404.
- La dirección pública de las imágenes se forma con el esquema y host de la solicitud actual, conforme al contrato indicado.
- El alcance no incluye filtros, ordenamientos alternativos, consultas de detalle ni operaciones de escritura.
