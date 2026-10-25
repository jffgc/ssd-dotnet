# Plan de Implementación: Actualización de propiedades con upload opcional de imagen

**Branch**: `006-properties-update` | **Fecha**: 2026-10-25 | **Spec**: [spec.md](spec.md)

**Entrada**: Especificación de la funcionalidad en `/specs/006-properties-update/spec.md`

## Resumen

La iniciativa define un caso de uso de actualización de propiedades para el backend de Realtor mediante un slice Minimal API que actualiza una propiedad existente por id, conserva `imageUrl` cuando no se envía archivo y reemplaza la imagen pública cuando el cliente proporciona un archivo válido. La solución debe mantener la Arquitectura Vertical Slice ya prevista por el repositorio, respetar el dominio e infraestructura previos y garantizar que cualquier error de almacenamiento no deje una propiedad parcialmente actualizada. La validación de imágenes y la respuesta a errores se alinearán con la semántica definida en la spec 005 y con la gobernanza Spec-Driven del proyecto.

## Contexto técnico

**Idioma/Versión**: .NET 11, según `global.json` del repositorio.

**Dependencias principales**: ASP.NET Core Minimal APIs, EF Core, Npgsql, PostgreSQL, FluentValidation, ProblemDetails, xUnit.

**Persistencia**: PostgreSQL con EF Core y entidades `Property`/`PropertyStatus` ya integradas en la base del backend.

**Testing**: Proyecto `app/backend/tests/RealtorApiTests` con xUnit para validar endpoint, validación de request, persistencia y manejo de imágenes.

**Plataforma objetivo**: Backend ASP.NET Core ejecutándose sobre .NET 11 y sirviendo contenido estático desde `wwwroot`.

**Tipo de proyecto**: Web service / backend con caso de uso de escritura inmobiliaria.

**Objetivos de rendimiento**: actualización de propiedades con latencia aceptable para pruebas locales, manteniendo el manejo de imágenes pequeño y seguro.

**Restricciones**: no se permiten controllers, no se agregan capas transversales paralelas, `imageUrl` es opcional en la actualización y no debe alterarse sin archivo, y la validación de archivo debe ocurrir en backend con respuestas `ProblemDetails` estandarizadas.

**Escala/alcance**: caso de uso `UpdateProperty` para propiedades existentes con y sin imagen, respetando el dominio previo y sin introducir regresiones en la base del sistema.

## Verificación de la constitución

Se valida que la iniciativa cumple la constitución del repositorio:

- Solución única y compartida: la feature amplía el backend existente y no introduce duplicación de arquitectura ni capas paralelas.
- Spec-driven development: la funcionalidad está documentada en [spec.md](spec.md) y el alcance queda acotado al caso de actualización por id.
- Arquitectura canónica: se mantiene Vertical Slice Architecture, `ISlice` auto-registro e `IHandler` para la lógica del caso de uso; no se introduce controller ni lógica de negocio en mapping.
- Stack obligatorio: se usa .NET 11, ASP.NET Core Minimal APIs, EF Core, PostgreSQL/Npgsql, FluentValidation y `ProblemDetails`.
- Calidad de dominio y entrega: se valida la lógica de negocio en handler y validator; la actualización debe ser atómica y las pruebas verifican el comportamiento real y no mocks de capa.

Resultado: la iniciativa cumple la constitución y no requiere una excepción arquitectónica ni una justificación adicional.

## Investigación de diseño

Se resuelven las decisiones técnicas necesarias para esta feature:

- El caso de uso `UpdateProperty` será un slice bajo `Features/Properties/UpdateProperty` con `ISlice`, validación, handler y mapping explícitos.
- El endpoint público será `PUT /api/properties/{id}` o equivalentement en el patrón actual del proyecto; el identificador se toma del path y los datos editables entran como request explícito.
- Antes de actualizar, el handler debe consultar la entidad en `AppDbContext` por id. Si no existe, debe responder `404` sin cambios.
- Si el request no incluye imagen, el handler debe conservar `ImageUrl` anterior; si incluye una imagen válida, se debe guardar en `wwwroot/assets/properties` (o la convención vigente del proyecto) con un nombre generado como `UUID + extensión`.
- El `imageUrl` persistido debe ser la ruta pública o relativa consumible por clientes y compatible con el static files del backend.
- La validación del upload se realizará en el validator del slice: tipos permitidos `png`, `jpg`, `jpeg`, tamaño máximo 5MB, archivo vacío y contenido real inválido.
- La actualización se debe tratar como una operación atómica: se guardará la imagen primero y solo se persistirá la nueva entidad cuando la operación de almacenamiento haya terminado correctamente o se devolverá error sin cambios parciales.
- La semántica de respuesta y errores debe seguir la misma convención de 005: `400` para negocio, `415` para tipo/formato/archivo inválido, `413` para archivo grande, `500` para fallo interno de almacenamiento y `404` cuando la entidad no exista.
- Las pruebas del slice cubrirán: actualización sin imagen, actualización con imagen válida, propiedad inexistente, archivo vacío o inválido, archivo no permitido, sobre tamaño y no alteración del `ImageUrl` previo.

## Estructura del proyecto

### Documentación de esta feature

```text
specs/006-properties-update/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── README.md
└── tasks.md               # creado luego por /speckit.tasks
```

### Código fuente (raíz del repositorio)

```text
app/
├── Realtor.sln
├── backend/
│   ├── src/
│   │   └── RealtorApi/
│   │       ├── Domain/
│   │       │   └── Properties/
│   │       │       ├── Property.cs
│   │       │       └── PropertyStatus.cs
│   │       ├── Features/
│   │       │   └── Properties/
│   │       │       └── UpdateProperty/
│   │       │           ├── UpdateProperty.Endpoint.cs
│   │       │           ├── UpdateProperty.Handler.cs
│   │       │           ├── UpdateProperty.Mapping.cs
│   │       │           ├── UpdateProperty.Request.cs
│   │       │           ├── UpdateProperty.Response.cs
│   │       │           └── UpdateProperty.Validator.cs
│   │       ├── Infrastructure/
│   │       │   ├── Endpoints/
│   │       │   ├── Handlers/
│   │       │   ├── Persistence/
│   │       │   └── Validation/
│   │       ├── Program.cs
│   │       └── wwwroot/assets/properties/
│   └── tests/
│       └── RealtorApiTests/
├── frontend/
└── support/
```

**Decisión de estructura**: se adopta la organización canónica del backend con el slice `UpdateProperty` dentro de `Features/Properties`, reutilizando el dominio y la persisitencia actuales y evitando cambios no requeridos fuera del alcance de la feature.

## Complejidad y seguimiento

No se identifican violaciones de la constitución ni excepciones que deban justificarse en esta iniciativa.
