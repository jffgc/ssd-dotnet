# Tareas: Listado paginado de propiedades

**Entrada**: Documentos de diseño en `specs/004-properties-list-pagination/`

**Prerrequisitos**: `plan.md` y `spec.md`; decisiones complementarias en `research.md`, `data-model.md`, `contracts/` y `quickstart.md`.

**Organización**: Las tareas están agrupadas por historia de usuario y ordenadas para implementar y verificar cada incremento.

## Fase 1: Preparación

**Propósito**: Preparar los proyectos necesarios para el trabajo.

Los proyectos `app/backend/src/RealtorApi/RealtorApi.csproj` y `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`, el SDK y las dependencias requeridas ya existen. No se requieren tareas de inicialización, cambios de paquetes ni configuración nueva.

---

## Fase 2: Fundamentos

**Propósito**: Resolver requisitos compartidos que bloqueen las historias.

El registro auto-descubierto de `ISlice` e `IHandler`, el filtro de validación FluentValidation y el middleware de archivos estáticos ya existen. No se requieren cambios fundacionales compartidos; cada adición se realizará dentro de la historia que la necesita.

---

## Fase 3: Historia de usuario 1 - Consultar y recorrer el listado de propiedades (Prioridad: P1)

**Objetivo**: Ofrecer `GET /api/properties` con valores predeterminados, resultados ordenados y paginados, metadatos completos y URLs públicas de imágenes.

**Prueba independiente**: Ejecutar las pruebas unitarias del handler y del mapping para el orden, los segmentos, los metadatos, los casos vacío/fuera de rango y la URL absoluta. Después, usar las solicitudes válidas de `app/backend/src/RealtorApi/RealtorApi.http` contra la API local para verificar los valores predeterminados, una página explícita, los campos del contrato y el acceso configurado a la ruta de imágenes, sin agregar pruebas HTTP de integración.

### Pruebas de la historia

- [X] T001 [P] [US1] Escribir pruebas unitarias del handler para orden por título, proyección, paginación, conteo y metadatos, catálogo vacío, página fuera de rango, desplazamiento extremo y cancelación; verificar además que el logging estructurado contiene `Page`, `PageSize` y `TotalItems` sin descripciones ni otros datos de propiedades, en `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/ListPropertiesHandlerTests.cs`.
- [X] T002 [P] [US1] Escribir pruebas unitarias del mapping para todos los campos del contrato, estado textual y URL absoluta derivada del esquema, host y nombre de archivo en `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/ListPropertiesMappingTests.cs`.
- [X] T003 [P] [US1] Escribir una prueba unitaria de configuración que compruebe que las opciones de archivos estáticos usan `/assets/properties` y el directorio físico `wwwroot/images/properties`, sin iniciar un host HTTP, en `app/backend/tests/RealtorApiTests/PropertiesStaticFilesConfigurationTests.cs`.
- [X] T004 [P] [US1] Escribir una prueba unitaria de registro del endpoint que verifique el método GET y la ruta `/api/properties` a partir de `ListPropertiesSlice.AddEndpoint`, sin iniciar un host HTTP ni usar `WebApplicationFactory`, en `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/ListPropertiesSliceTests.cs`.

### Implementación de la historia

- [X] T005 [US1] Definir en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesContracts.cs` el DTO explícito de solicitud que conserva como texto nullable los valores de consulta para permitir validar entradas no enteras, junto con los contratos inmutables de elemento y página; representar las omisiones para que se normalicen a `page = 1` y `pageSize = 6`.
- [X] T006 [P] [US1] Implementar el handler de lectura en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesHandler.cs` con `AsNoTracking`, conteo previo a la paginación, proyección solo de los campos requeridos, orden ascendente por título, operaciones asíncronas con `CancellationToken`, cálculo del desplazamiento con aritmética amplia para devolver una página vacía si excede el rango aceptado por `Skip`, y un evento de `ILogger` estructurado con `Page`, `PageSize` y `TotalItems` que no registre contenido de propiedades, cumpliendo FR-012.
- [X] T007 [P] [US1] Implementar el mapping explícito en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesMapping.cs`, convirtiendo el estado a su nombre textual y formando `imageUrl` como URL absoluta con el esquema y host actuales y el nombre de archivo bajo `/assets/properties/`; calcular `totalPages` como cero para un total vacío o como el techo de `totalItems / pageSize`, `hasNext` cuando `page < totalPages` y `hasPrevious` exactamente cuando `page > 1 && totalItems > 0`, incluso para páginas fuera de rango.
- [X] T008 [US1] Implementar `ListPropertiesSlice` en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesSlice.cs` como `ISlice` auto-descubierto que registre `GET /api/properties`, coordine la solicitud normalizada con el handler y propague el token de cancelación.
- [X] T009 [P] [US1] Configurar en `app/backend/src/RealtorApi/Program.cs` las opciones de archivos estáticos del alias `/assets/properties` hacia `wwwroot/images/properties`, preservando el middleware y la ruta `/images/properties` existentes; exponer la construcción de opciones mediante `Program.CreatePropertiesAssetsOptions(string webRootPath)` para su verificación unitaria.
- [X] T010 [US1] Añadir a `app/backend/src/RealtorApi/RealtorApi.http` las solicitudes manuales requeridas para `GET /api/properties` y `GET /api/properties?page=1&pageSize=6`.

**Punto de control**: La historia queda funcional cuando las pruebas unitarias pasan y las solicitudes manuales devuelven una página válida con los campos, metadatos y URL pública especificados.

---

## Fase 4: Historia de usuario 2 - Recibir errores claros para parámetros inválidos (Prioridad: P2)

**Objetivo**: Responder con HTTP 400 y detalles `ValidationProblemDetails` ante entradas inválidas, incluidos valores no enteros que no deben fallar durante el model binding.

**Prueba independiente**: Ejecutar las pruebas unitarias del validador para parámetros omitidos, valores no numéricos, cero, negativos, `page` mayor que 2,147,483,647 y `pageSize` mayor que 100; después enviar las solicitudes inválidas documentadas en el archivo `.http` y comprobar el estado 400 y el campo identificado. No se requieren pruebas de integración automatizadas.

### Pruebas de la historia

- [X] T011 [P] [US2] Escribir pruebas unitarias FluentValidation para solicitudes omitidas (válidas y normalizadas a 1 y 6), `page` y `pageSize` no numéricos, valores cero o negativos, `page=2147483647` (válido), `page=2147483648` (inválido), `pageSize=100` (válido) y `pageSize=101` (inválido), comprobando errores asociados al campo en `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/ListPropertiesValidatorTests.cs`; agregar en `app/backend/tests/RealtorApiTests/InfrastructureTests.cs` una prueba unitaria del filtro central que confirme que una solicitud inválida produce `ValidationProblemDetails` con HTTP 400.
- [X] T012 [P] [US2] Añadir en `app/backend/src/RealtorApi/RealtorApi.http` ejemplos manuales inválidos para `page=0`, `page=2147483648`, `pageSize=-1`, `pageSize=101` y un valor no entero de cada parámetro.

### Implementación de la historia

- [X] T013 [US2] Implementar `ListPropertiesValidator` en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesValidator.cs` con FluentValidation: aceptar omisiones como 1 y 6, comprobar que ambos valores puedan interpretarse como enteros `Int32` positivos, rechazar `page` mayor que 2,147,483,647 y `pageSize` mayor que 100, con errores que identifiquen cada parámetro.
- [X] T014 [US2] Conectar el filtro global de FluentValidation al mapeo de endpoints en `app/backend/src/RealtorApi/Infrastructure/Endpoints/ISlice.cs` y `app/backend/src/RealtorApi/Infrastructure/Validation/ValidationFilterFactory.cs`, ya que `MapSliceEndpoints` actualmente no lo aplica; resolver validators scoped por solicitud, sin resolverlos durante el mapeo ni validar manualmente en el endpoint. Integrar el request del slice para conservar los valores no enteros hasta FluentValidation y normalizar omisiones/entradas válidas antes de invocar el handler. Todas las entradas inválidas deben producir HTTP 400 con detalles de validación y no ejecutar la consulta.

**Punto de control**: La historia queda funcional cuando las pruebas del validador pasan y los ejemplos manuales inválidos devuelven HTTP 400 con detalles para el parámetro correspondiente.

---

## Fase 5: Verificación y cierre

**Propósito**: Verificar el incremento completo sin ampliar su alcance.

- [X] T015 Ejecutar `dotnet build app/backend/src/RealtorApi/RealtorApi.csproj` y corregir errores de compilación del endpoint y su configuración como verificación de US1 (FR-001–FR-012).
- [X] T016 Ejecutar `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj` y confirmar las pruebas unitarias nuevas y existentes como verificación de US1/US2 (FR-001–FR-012, SC-001–SC-005).
- [ ] T017 Verificar manualmente los escenarios de `specs/004-properties-list-pagination/quickstart.md` con las solicitudes de `app/backend/src/RealtorApi/RealtorApi.http`, incluida la obtención de la URL de imagen bajo `/assets/properties/`, como verificación de US1/US2 (FR-001–FR-010, SC-001–SC-004); no crear `WebApplicationFactory`, pruebas HTTP automatizadas, Testcontainers, migraciones ni cambios de persistencia.

---

## Dependencias y orden de ejecución

### Dependencias entre fases

- **Preparación (Fase 1)**: No requiere cambios; los proyectos y las dependencias ya están disponibles.
- **Fundamentos (Fase 2)**: No requiere cambios; la infraestructura existente permite iniciar las historias.
- **Historia de usuario 1 (Fase 3)**: Puede comenzar de inmediato. Establece el contrato y el endpoint base.
- **Historia de usuario 2 (Fase 4)**: Se completa después de la historia 1 porque endurece el enlace de parámetros y añade validación al endpoint existente. Sus reglas de validador son comprobables unitariamente de forma aislada.
- **Verificación y cierre (Fase 5)**: Depende de ambas historias; el build, la suite unitaria y la comprobación manual se ejecutan al final.

### Dependencias entre tareas

- En US1, T001, T002, T003 y T004 son pruebas independientes y pueden empezar en paralelo. T005 define los contratos que usan T006 y T007; T006 y T007 pueden implementarse en paralelo tras T005. T008 integra handler y mapping en el slice y completa la prueba de registro T004. T009 implementa la configuración comprobada por T003 y puede avanzar en paralelo con T005-T008. T010 requiere el endpoint de T008.
- En US2, T011 y T012 pueden prepararse en paralelo una vez que exista el contrato de US1. T013 sigue a T011. T014 requiere T013 y el slice de T008 para integrar la validación antes de llamar al handler.
- T015, T016 y T017 se realizan después de que ambas historias estén completas. La verificación del alias estática automatizada se limita a la prueba unitaria de opciones T003; T017 comprueba manualmente el recurso servido, sin crear una prueba de integración.

### Oportunidades de ejecución paralela

**Historia de usuario 1**

```text
En paralelo: T001, T002, T003, T004
Tras T005: T006 y T007 en paralelo
En paralelo con T005-T008: T009
```

**Historia de usuario 2**

```text
En paralelo: T011 y T012
Después de T011: T013
Después de T013 y T008: T014
```

## Estrategia de implementación

### MVP primero

El alcance mínimo de entrega es la **Historia de usuario 1 (P1)**: finalizar las pruebas y la implementación de T001-T010, compilar y ejecutar las pruebas unitarias correspondientes, y validar de forma manual el listado y el alias de imágenes. Esto entrega el listado ordenado y paginado con valores predeterminados y metadatos.

### Entrega incremental

1. Completar US1 y verificar de forma independiente el listado, los contratos, la paginación y las imágenes públicas.
2. Completar US2 y verificar los errores 400 para valores numéricos fuera de rango y entradas no enteras.
3. Ejecutar el build, la suite unitaria y la guía manual antes de cerrar el incremento.

## Límites de alcance

- No modificar entidades, esquema, seeds, migraciones, dependencias ni persistencia.
- No agregar controllers, filtros u ordenamientos adicionales, frontend ni abstracciones de repositorio.
- No agregar pruebas de integración, `WebApplicationFactory` ni Testcontainers.
- Mantener cada tarea pendiente hasta que se implemente y verifique; la generación de esta lista no marca tareas como completadas.
