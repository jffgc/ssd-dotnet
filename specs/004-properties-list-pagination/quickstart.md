# Guía de validación: Listado paginado de propiedades

## Prerrequisitos

- SDK fijado por `global.json` instalado.
- PostgreSQL de desarrollo disponible con la cadena `DefaultConnection` configurada para RealtorApi.
- Imágenes de propiedades disponibles en los archivos estáticos incorporados por el proyecto.
- Puerto base tomado del perfil de ejecución activo; los ejemplos suponen `http://localhost:5023`.

## Ejecutar pruebas unitarias

Desde la raíz del repositorio:

```powershell
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj
```

**Resultado esperado**: finalización exitosa de las pruebas del proyecto, incluidas las pruebas unitarias de validación y paginación del slice.

## Iniciar la API

Desde la raíz del repositorio:

```powershell
dotnet run --project app/backend/src/RealtorApi/RealtorApi.csproj
```

**Resultado esperado**: RealtorApi inicia en el perfil configurado, aplica el flujo existente de inicialización de base de datos y expone el endpoint en el host de desarrollo.

## Verificar el listado y las páginas

En `app/backend/src/RealtorApi/RealtorApi.http`, ejecutar las solicitudes:

```http
GET http://localhost:5023/api/properties
```

```http
GET http://localhost:5023/api/properties?page=2&pageSize=6
```

**Resultado esperado**:

- La primera solicitud aplica `page = 1`, `pageSize = 6` y devuelve hasta seis elementos por `title` ascendente.
- La segunda devuelve el segmento de página correspondiente y metadatos coherentes con el conjunto completo.
- Cada elemento contiene los campos del contrato y una `imageUrl` absoluta con el host actual y el prefijo `/assets/properties/`.

## Verificar validación

Enviar solicitudes con `page=0`, `pageSize=-1`, `pageSize=101`, `page=2147483648` y un valor no numérico. Cada solicitud debe responder HTTP 400 con un cuerpo `ProblemDetails` o `ValidationProblemDetails` que identifique el error.

## Verificar el recurso de imagen

Tomar el `imageUrl` de un elemento devuelto y solicitarlo directamente.

**Resultado esperado**: la ruta `/assets/properties/{fileName}` devuelve el archivo de imagen estático; no revela una ruta física del servidor.

## Alcance de esta guía

La comprobación HTTP es manual contra una instancia en ejecución. No se agrega una suite de integración ni un contenedor de PostgreSQL sin autorización explícita en los artefactos normativos.
