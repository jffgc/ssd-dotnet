---
name: 009-update-status
agent: speckit.specify
---
/speckit.specify

Quiero crear una nueva spec llamada 009-update-status para el backend de Realtor API, basada en el estilo de la spec 006-properties-update, pero con un alcance mucho más pequeño.

Objetivo:
Crear un endpoint público para actualizar únicamente el estado de una propiedad existente identificada por su id.

Alcance:

Solo se puede actualizar el campo status.
No se pueden modificar otros campos como title, description, address, price, bedroomCount, bathroomCount, areaSquareMeters ni imageUrl.
No debe incluir imagen ni subida de archivos.
Debe seguir el patrón de Minimal API + Vertical Slice usado en el proyecto.
El endpoint debe ser público, sin autenticación.
Debe incluir ejemplo de prueba manual en el archivo .http del proyecto.
Debe incluir contrato OpenAPI, quickstart, tasks y documentación en español.
Comportamiento esperado:

Usar PATCH, por ejemplo /api/properties/{id}/status.
Si la propiedad no existe, responder 404 Not Found.
Si el status viene omitido o inválido, responder 400 Bad Request.
Si la actualización es exitosa, responder 200 OK.
La respuesta debe devolver la propiedad actualizada, conservando intactos los demás datos persistidos, incluyendo imageUrl.
Criterios mínimos:

Actualización correcta del status por id.
Rechazo con 404 si no existe la propiedad.
Rechazo con 400 si el body no trae status o si el valor no es válido.
Verificación manual desde .http.
Mantener consistencia con ProblemDetails y con el estilo de las specs anteriores.
Importante:

No implementar actualización parcial de otros campos.
No incluir imagen.
No cambiar la semántica de endpoints existentes.
Mantener el alcance alineado con la constitución y el flujo Speckit del repositorio.