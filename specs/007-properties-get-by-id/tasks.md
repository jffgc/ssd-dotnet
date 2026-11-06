# Tareas: Consulta de propiedad por identificador

**Entrada**: Documentos de diseño en `specs/007-properties-get-by-id/`

**Prerequisitos**: `plan.md`, `spec.md`, `research.md`, `data-model.md` y `contracts/`

**Pruebas**: Se incluyen pruebas unitarias porque la constitución exige TDD para cambios funcionales y el plan limita las pruebas automatizadas a xUnit unitario. La validación HTTP completa se conserva como paso manual.

**Organización**: Las tareas funcionales están agrupadas por historia de usuario para permitir implementación incremental y verificación independiente.

## Fase 1: Setup

**Propósito**: El proyecto backend, el SDK fijado y las dependencias de FluentValidation, EF Core y xUnit ya existen. No se requieren tareas de inicialización ni paquetes nuevos.

## Fase 2: Fundamentos compartidos

**Propósito**: Extraer y probar la construcción de URL pública compartida con el listado antes de que los slices la consuman.

- [X] T001 Añadir pruebas unitarias de URL absoluta por esquema/host y rechazo de nombres de archivo inseguros en `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/ListPropertiesMappingTests.cs`
- [X] T002 Implementar el helper compartido de construcción segura de URL de imagen en `app/backend/src/RealtorApi/Features/Properties/PropertyImageUrlBuilder.cs`
- [X] T003 Refactorizar el mapping del listado para usar el helper compartido sin alterar su contrato en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesMapping.cs`

**Checkpoint**: El helper y el listado conservan las mismas reglas de URL absoluta y nombre seguro antes de iniciar el nuevo slice.

## Fase 3: Historia de usuario 1 - Consultar una propiedad existente (Prioridad: P1)

**Objetivo**: Devolver una propiedad existente con los diez campos públicos, sin paginación ni entidad EF expuesta.

**Prueba independiente**: Consultar un UUID existente y comparar los diez campos de la respuesta 200 con los datos conocidos; verificar que no hay propiedades de paginación.

### Preparación de contratos y firmas para TDD

- [X] T004 [US1] Definir request, query/proyección y DTO de respuesta explícitos para el detalle en `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdContracts.cs`
- [X] T005 [US1] Crear las firmas mínimas del handler y mapping sin lógica de consulta ni transformación para permitir que las pruebas de comportamiento compilen y fallen antes de su implementación en `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdHandler.cs` y `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdMapping.cs`

### Pruebas de la historia de usuario 1

- [X] T006 [P] [US1] Crear pruebas unitarias del handler para la proyección de una propiedad existente, propagación de cancelación y logging estructurado; comprobar que fallan por comportamiento antes de implementar el handler en `app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdHandlerTests.cs`
- [X] T007 [P] [US1] Crear pruebas unitarias del mapping para los diez campos públicos, estado textual, `imageUrl` nullable y ausencia de metadatos de paginación; comprobar que fallan por comportamiento antes de implementar el mapping en `app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdMappingTests.cs`

### Prueba anticipada de US2 requerida por TDD

- [X] T008 [US2] Añadir prueba unitaria de propiedad ausente y resultado `notFound` al handler; comprobar que falla antes de implementar la rama de ausencia en `app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdHandlerTests.cs`

### Implementación de la historia de usuario 1

- [X] T009 [P] [US1] Implementar consulta `AsNoTracking` por `Id`, proyección de solo lectura, propagación de `CancellationToken`, resultado nullable y logging estructurado `found`/`notFound` en `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdHandler.cs`
- [X] T010 [P] [US1] Implementar mapping explícito de los diez campos y delegar la imagen al helper compartido con el esquema y host de la solicitud en `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdMapping.cs`
- [X] T011 [US1] Mapear `GET /api/properties/{id}` como Minimal API auto-descubierta, enlazar el request de ruta y devolver la respuesta 200 del slice en `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdSlice.cs`

**Checkpoint**: La consulta de un UUID existente devuelve el contrato de detalle, propaga cancelación y registra el resultado sin exponer datos descriptivos en logs.

## Fase 4: Historia de usuario 2 - Distinguir ID inválido de propiedad inexistente (Prioridad: P2)

**Objetivo**: Rechazar formatos distintos al UUID canónico con 400 y devolver 404 ProblemDetails para UUID canónicos sin registro.

**Prueba independiente**: Validar un UUID canónico y rechazar un identificador sin guiones aunque `Guid.TryParse` pueda interpretarlo; consultar un UUID canónico no registrado y verificar 404 ProblemDetails.

### Pruebas de la historia de usuario 2

- [X] T012 [P] [US2] Crear pruebas unitarias del validator para UUID canónico válido e identificadores vacíos, malformados, sin guiones, entre llaves o paréntesis en `app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdValidatorTests.cs`

### Implementación de la historia de usuario 2

- [X] T013 [US2] Implementar FluentValidation usando `Guid.TryParseExact` con formato `D` y asociar el error al campo `id` en `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdValidator.cs`
- [X] T014 [US2] Convertir resultado nullable en HTTP 404 ProblemDetails, declarar respuestas 200/400/404 y mantener el 400 del filtro de validación en `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdSlice.cs`

**Checkpoint**: Las entradas no canónicas reciben 400 antes de consultar; UUID canónicos sin fila reciben 404 ProblemDetails.

## Fase 5: Historia de usuario 3 - Mantener URL de imagen según solicitud (Prioridad: P3)

**Objetivo**: Confirmar que la URL de imagen es absoluta y refleja esquema, host y puerto actuales, y que la ausencia de imagen produce `null`.

**Prueba independiente**: Verificar el DTO con una propiedad sin imagen y con imagen bajo solicitudes HTTP/HTTPS y hosts distintos; comprobar que `imageUrl` es `null` o una URL pública absoluta correspondiente a cada solicitud.

### Pruebas e implementación de la historia de usuario 3

- [X] T015 [P] [US3] Ampliar pruebas de mapping con imagen para esquemas HTTP/HTTPS, hosts y puertos distintos, y verificar `imageUrl: null` sin imagen en `app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdMappingTests.cs`
- [X] T016 [P] [US3] Añadir solicitudes manuales para propiedad con imagen, sin imagen y variación de esquema/host en `app/backend/src/RealtorApi/RealtorApi.http`

**Checkpoint**: La URL de imagen es consumible desde clientes externos y no contiene rutas relativas ni físicas.

## Fase 6: Pulido y verificación

**Propósito**: Verificar la regresión del listado, el slice nuevo y los escenarios manuales documentados.

- [X] T017 Ejecutar las pruebas unitarias del backend y corregir regresiones del slice nuevo o del listado en `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`
- [ ] T018 Ejecutar manualmente los casos de propiedad existente, UUID no registrado, ID no canónico, propiedad sin imagen y host/esquema variables siguiendo `specs/007-properties-get-by-id/quickstart.md` y `app/backend/src/RealtorApi/RealtorApi.http`

## Dependencias y orden de ejecución

### Dependencias entre fases

- **Setup (Fase 1)**: No requiere cambios; SDK, proyecto y paquetes ya existen.
- **Fundamentos (Fase 2)**: T001 → T002 → T003. El refactor del listado debe conservar su comportamiento antes de que el detalle comparta el helper.
- **US1 (Fase 3)**: Depende de los fundamentos; T004 define los contratos y T005 prepara firmas mínimas. T006 y T007 prueban los casos existentes; T008 prueba la ausencia requerida por US2 antes de T009 y T010 implementan handler y mapping. T011 registra el endpoint después de completar handler y mapping.
- **US2 (Fase 4)**: La prueba de handler T008 se ejecuta en TDD antes de implementar `notFound`; después de US1, T012 prueba el validator antes de T013 y T014 completan validación y errores públicos.
- **US3 (Fase 5)**: Depende del mapping y endpoint de US1. T015 y T016 pueden realizarse en paralelo después de US1; sus verificaciones cubren el contrato bajo hosts y esquemas variables.
- **Pulido (Fase 6)**: Depende de las tres historias; ejecutar pruebas y quickstart solo después de completar las implementaciones.

### Oportunidades de paralelismo

- Las tareas `T006` y `T007` pueden desarrollarse en paralelo porque modifican archivos de prueba distintos.
- Las tareas `T009` y `T010` pueden desarrollarse en paralelo después de T006-T008 porque implementan handler y mapping en archivos distintos.
- Las tareas `T012` y `T015` pueden desarrollarse en paralelo después de US1 porque cubren validator y mapping en archivos de prueba distintos.
- Las tareas `T015` y `T016` pueden desarrollarse en paralelo después de US1 porque modifican el archivo de pruebas de mapping y el archivo HTTP, respectivamente.
- Las historias US2 y US3 pueden avanzar en paralelo una vez que US1 haya establecido el request, handler, mapping y endpoint; evitar cambios simultáneos sobre los mismos archivos de slice.

### Ejemplos de ejecución paralela por historia

US1, después de completar las tareas T001-T005:

```text
Task: T006 - Pruebas unitarias del handler
Task: T007 - Pruebas unitarias del mapping
```

US2 y US3, después de completar US1:

```text
Task: T012 - Pruebas unitarias del validator
Task: T015 - Pruebas de mapping con esquema y host variables
```

US3, después de completar US1:

```text
Task: T015 - Pruebas de mapping con esquema y host variables
Task: T016 - Solicitudes HTTP manuales
```

## Estrategia de implementación

### MVP primero

1. Completar T001-T003 para extraer el helper sin cambiar el contrato existente del listado.
1. Completar los contratos y firmas de US1; escribir T006-T008 y verificar que fallen por comportamiento.
2. Implementar handler, mapping y endpoint con T009-T011; validar el caso 200 de forma independiente.
3. Completar US2 con T012-T014 para fijar la distinción 400/404.
4. Completar US3 y el quickstart para verificar imagen nullable y URL pública bajo variaciones de esquema/host.
5. Ejecutar T017 y T018 antes de declarar terminada la implementación.

### Entrega incremental

- US1 entrega el caso principal de lectura con proyección y contrato explícitos.
- US2 agrega los errores públicos deterministas para formato inválido y recurso ausente.
- US3 verifica los casos de imagen y la composición de URL en distintos entornos.
- Las pruebas y la verificación manual de cada historia deben pasar antes de marcar completadas sus tareas.

## Supuestos y alcance

- No se crea una nueva infraestructura de validación, registro de slices ni acceso a datos; se reutilizan los mecanismos actuales.
- La ruta `/assets/properties` ya se sirve desde los archivos estáticos configurados y esta feature solo reutiliza el helper que construye su URL pública.
- No se actualizan `Program.cs`, entidades, migraciones, seeds, paquetes ni proyectos frontend.
- Las pruebas automatizadas permanecen unitarias; los códigos HTTP reales se comprueban manualmente conforme a la constitución y al plan.