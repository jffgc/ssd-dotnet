# Inicio rápido: consulta manual de propiedad por identificador

## Preparación

1. Iniciar el backend conforme al procedimiento local de la solución y anotar su URL base, incluyendo esquema y puerto, por ejemplo `https://localhost:7023`.
2. Elegir un GUID de una propiedad existente en los datos locales.
3. Elegir también un GUID bien formado que no esté registrado.

En los ejemplos siguientes, sustituir `{{baseUrl}}` y los identificadores por valores del entorno local.

## Consultar una propiedad existente

```http
GET {{baseUrl}}/api/properties/9c2fcbcb-f94a-4c70-b04f-b24488b89ad1
Accept: application/json
```

Resultado esperado: `200 OK` y un objeto con exactamente `id`, `title`, `description`, `address`, `price`, `status`, `bedroomCount`, `bathroomCount`, `areaSquareMeters` e `imageUrl`. La respuesta no contiene campos de paginación.

Si la propiedad tiene imagen, comprobar que `imageUrl` es una URL absoluta que usa el mismo esquema, host y puerto que `{{baseUrl}}`, con ruta `/assets/properties/{fileName}`. Si no tiene imagen, comprobar que el campo está presente y vale `null`.

## Comprobar errores del identificador

GUID válido sin registro:

```http
GET {{baseUrl}}/api/properties/00000000-0000-0000-0000-000000000001
Accept: application/json
```

Resultado esperado: `404 Not Found` con `ProblemDetails`.

Segmento que no es un GUID:

```http
GET {{baseUrl}}/api/properties/not-a-guid
Accept: application/json
```

Resultado esperado: `400 Bad Request` con `ProblemDetails` o `ValidationProblemDetails`; cuando se devuelvan errores de validación, debe existir una entrada para `id`.

GUID interpretable por .NET, pero sin el formato canónico con guiones:

```http
GET {{baseUrl}}/api/properties/9c2fcbcbf94a4c70b04fb24488b89ad1
Accept: application/json
```

Resultado esperado: `400 Bad Request` con el error asociado a `id`; el endpoint solo acepta el formato UUID canónico con guiones.

## Comprobar esquema y host variables

Repetir una consulta exitosa con las URLs base HTTP y HTTPS disponibles y, si el entorno expone más de un host, con cada host. Para una propiedad con imagen, verificar que el esquema, host y puerto de `imageUrl` coinciden con la URL base de esa solicitud. No aceptar una ruta relativa ni una ruta local del sistema.
