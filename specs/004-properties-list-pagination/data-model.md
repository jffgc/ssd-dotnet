# Modelo de datos: Listado paginado de propiedades

## Alcance persistente

Este caso de uso es de solo lectura. No crea entidades, tablas, columnas, relaciones, índices, configuraciones ni migraciones. La consulta usa el modelo persistente existente `Property`.

## Datos de entrada

### Solicitud de listado

| Campo | Tipo de contrato | Predeterminado | Validación |
|---|---|---:|---|
| `page` | entero `Int32` | 1 | Entero positivo representable como `Int32`. |
| `pageSize` | entero | 6 | Entero positivo, máximo 100. |

Una omisión usa el valor predeterminado. Un valor no numérico, cero, negativo, `page` mayor que 2,147,483,647 o `pageSize` mayor que 100 produce HTTP 400 con detalles de validación.

## Datos existentes consultados

### `Property` (entidad persistente existente)

| Propiedad | Uso en el contrato | Transformación |
|---|---|---|
| `Id` (`Guid`) | `id` | Sin transformación. |
| `Title` (`string`) | `title` | Orden ascendente; valor proyectado. |
| `Description` (`string`) | `description` | Valor proyectado. |
| `Address` (`string`) | `address` | Valor proyectado. |
| `Price` (`decimal`) | `price` | Valor proyectado. |
| `Status` (`PropertyStatus`) | `status` | Nombre textual del miembro del enum. |
| `BedroomCount` (`int`) | `bedroomCount` | Valor proyectado. |
| `BathroomCount` (`int`) | `bathroomCount` | Valor proyectado. |
| `AreaSquareMeters` (`decimal`) | `areaSquareMeters` | Valor proyectado. |
| `ImageUrl` (`string`) | `imageUrl` | Se obtiene el nombre de archivo y se construye una URL absoluta pública bajo `/assets/properties/`. |

`CreatedAt` y `UpdatedAt` no forman parte del contrato de listado.

## Datos de salida

### Elemento del listado

Contrato explícito e inmutable con los campos `id`, `title`, `description`, `address`, `price`, `status`, `bedroomCount`, `bathroomCount`, `areaSquareMeters` e `imageUrl`. Ninguna entidad de EF Core se serializa directamente.

### Página de propiedades

| Campo | Tipo | Regla |
|---|---|---|
| `items` | lista de elementos del listado | Máximo `pageSize`; vacía si la página no contiene elementos. |
| `page` | entero | Página solicitada, empezando por 1. |
| `pageSize` | entero | Tamaño aplicado, máximo 100. |
| `totalItems` | entero largo | Total de propiedades que coinciden con el listado, antes de paginar. |
| `totalPages` | entero largo | Cero si `totalItems` es cero; de lo contrario, techo de `totalItems / pageSize`. |
| `hasNext` | booleano | Verdadero si `page < totalPages`. |
| `hasPrevious` | booleano | Verdadero si `page > 1` y `totalItems > 0`, incluso si la página solicitada está fuera de rango. |

El handler emite un evento de logging estructurado con los valores `page`, `pageSize` y `totalItems`, sin incluir datos personales ni contenido descriptivo de las propiedades.

## Orden, paginación y mapping

- El orden es ascendente por `Title`. La spec no exige desempate adicional para títulos idénticos.
- La consulta cuenta el conjunto antes de aplicar `Skip`/`Take` y proyecta únicamente las propiedades del contrato.
- El acceso es de solo lectura (`AsNoTracking`) y todas las operaciones asíncronas reciben el token de cancelación.
- La URL de imagen se forma a partir del esquema y host de la solicitud actual y del nombre de archivo de `ImageUrl`; no contiene rutas físicas.
- La ruta `/assets/properties/{fileName}` se sirve desde los archivos estáticos existentes de imágenes de propiedades.

## Estados límite

- Catálogo vacío: `items = []`, `totalItems = 0`, `totalPages = 0`, `hasNext = false`, `hasPrevious = false`.
- Página fuera de rango y catálogo no vacío: `items = []`, total y total de páginas reales; `hasPrevious` sigue la regla definida arriba.
- Parámetros no válidos: no se ejecuta el handler de consulta; se responde 400 con detalles de validación.
