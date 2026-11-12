# Tareas: Actualización del estado de una propiedad

**Entrada**: Documentos de diseño de `/specs/009-update-status/`

**Prerrequisitos**: `plan.md`, `spec.md` aprobada expresamente, `research.md`, `data-model.md`, `quickstart.md` y `contracts/backend-properties-status.openapi.yaml`.

**Gate de aprobación**: No comenzar ninguna tarea de implementación hasta que la persona usuaria apruebe explícitamente `spec.md` y su campo `Estado` se actualice a `Aprobada`. La generación de `plan.md` y `tasks.md` no constituye aprobación.

**Pruebas**: Se incluyen unit tests porque la constitución exige TDD y verificación real para cambios funcionales. No se incluyen pruebas de integración ni Testcontainers; la verificación HTTP se realiza mediante `RealtorApi.http`.

**Organización**: Las tareas están agrupadas por historia de usuario y respetan el orden test-primero.

## Formato

- `[P]`: puede ejecutarse en paralelo con tareas de archivos distintos y sin dependencias pendientes.
- `[US1]`, `[US2]` y `[US3]`: historia de usuario de la spec a la que pertenece la tarea.
- Cada tarea indica la ruta exacta de los archivos que debe crear, modificar o validar.

## Fase 1: Preparación

**Propósito**: Preparar los contratos de entrada y salida compartidos por el slice.

- [X] T001 [P] Definir en `app/backend/src/RealtorApi/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatus.Request.cs` el DTO público `UpdatePropertyStatusBody` con solo `Status`, separado del objeto local de binding para el id de ruta y errores de lectura.
- [X] T002 [P] Crear el contrato de respuesta pública con solo `Id` y `Status` en `app/backend/src/RealtorApi/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatus.Response.cs`.

## Fase 2: Fundamentos

**Propósito**: Validar el request común antes de que las historias implementen la mutación y los resultados HTTP.

- [X] T003 [P] Escribir primero y ejecutar pruebas unitarias para UUID canónico y estados admitidos en `app/backend/tests/RealtorApiTests/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatusValidatorTests.cs`.
- [X] T004 Implementar la validación del identificador y de los tres estados permitidos en `app/backend/src/RealtorApi/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatus.Validator.cs`.
- [X] T005 [P] Escribir primero y ejecutar pruebas unitarias de binding del id y del body JSON válido con solo `status` en `app/backend/tests/RealtorApiTests/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatusRequestTests.cs`.
- [X] T006 Implementar el binding local de ruta y body para crear el request validable en `app/backend/src/RealtorApi/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatus.Request.cs`.

**Punto de control**: El request válido llega al validator como DTO explícito, sin cambios a la configuración global ni a `Program.cs`.

## Fase 3: Historia de usuario 1 - Actualizar el estado existente (Prioridad: P1) - MVP

**Objetivo**: Actualizar únicamente `Property.Status`, devolver `{id,status}` y preservar todos los demás datos persistidos.

**Prueba independiente**: Enviar un estado válido a una propiedad existente, comprobar `200 OK` con exactamente `id` y `status`, y consultar el detalle para verificar que los otros campos, `ImageUrl`, `CreatedAt` y `UpdatedAt` siguen iguales.

### Pruebas para la historia de usuario 1

- [X] T007 [P] [US1] Escribir primero y ejecutar pruebas unitarias que comprueben que el handler cambia solo `Status`, conserva los demás campos, registra `PropertyId`/`Outcome=updated` y propaga cancelación en `app/backend/tests/RealtorApiTests/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatusHandlerTests.cs`.
- [X] T008 [P] [US1] Escribir primero y ejecutar pruebas unitarias para la respuesta con solo `Id` y `Status` y los valores canónicos del enum en `app/backend/tests/RealtorApiTests/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatusMappingTests.cs`.

### Implementación de la historia de usuario 1

- [X] T009 [US1] Implementar el mapping explícito del estado validado y de la respuesta pública acotada en `app/backend/src/RealtorApi/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatus.Mapping.cs`.
- [X] T010 [US1] Implementar el handler asíncrono que localiza la propiedad, usa el mapping de T009, asigna exclusivamente `Status`, persiste con `CancellationToken` y registra `PropertyId` con `Outcome=updated` en `app/backend/src/RealtorApi/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatus.Handler.cs`.
- [X] T011 [US1] Implementar el endpoint público `PATCH /api/properties/{id}/status`, delegar al handler, declarar `.Accepts<UpdatePropertyStatusBody>("application/json")` y metadata de respuesta `200` en `app/backend/src/RealtorApi/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatus.Endpoint.cs`.
- [X] T012 [US1] Agregar el ejemplo manual de actualización válida y consulta posterior de detalle para verificar la preservación de datos en `app/backend/src/RealtorApi/RealtorApi.http`.

**Punto de control**: La historia P1 debe responder `200` y cambiar exclusivamente el estado.

## Fase 4: Historia de usuario 2 - Propiedad inexistente (Prioridad: P2)

**Objetivo**: Responder `404 ProblemDetails` para un UUID canónico sin propiedad asociada y no persistir cambios.

**Prueba independiente**: Enviar un UUID canónico no registrado y comprobar `404`, `ProblemDetails`, ausencia de inserciones y logging con `Outcome=notFound`.

### Pruebas para la historia de usuario 2

- [X] T013 [US2] Escribir primero y ejecutar una prueba unitaria de propiedad ausente que compruebe resultado no encontrado y logging estructurado sin datos de la propiedad en `app/backend/tests/RealtorApiTests/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatusHandlerTests.cs`.

### Implementación de la historia de usuario 2

- [X] T014 [US2] Completar el camino de ausencia del handler con `Outcome=notFound`, sin agregar ni modificar registros, en `app/backend/src/RealtorApi/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatus.Handler.cs`.
- [X] T015 [US2] Mapear el resultado ausente a `404 ProblemDetails` y declarar las respuestas HTTP correspondientes en `app/backend/src/RealtorApi/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatus.Endpoint.cs`.
- [X] T016 [US2] Agregar el ejemplo manual con UUID canónico inexistente y expectativa `404 ProblemDetails` en `app/backend/src/RealtorApi/RealtorApi.http`.

**Punto de control**: La historia P2 debe distinguir un recurso ausente de un id malformado y no causar escrituras.

## Fase 5: Historia de usuario 3 - Entrada de estado inválida (Prioridad: P3)

**Objetivo**: Rechazar cuerpos e identificadores inválidos con `400 ValidationProblemDetails`, sin modificar la propiedad.

**Prueba independiente**: Probar cuerpo ausente o malformado, estado omitido/nulo/no textual/no admitido, propiedad JSON adicional o duplicada, content type incorrecto e id no canónico; verificar `400` y ausencia de escrituras.

### Pruebas para la historia de usuario 3

- [X] T017 [P] [US3] Escribir primero y ejecutar pruebas unitarias del request para body ausente, JSON malformado, tipo no JSON, propiedades extra/duplicadas y valor `status` de tipo incorrecto en `app/backend/tests/RealtorApiTests/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatusRequestTests.cs`.
- [X] T018 [P] [US3] Escribir primero y ejecutar pruebas unitarias que rechacen estado omitido, nulo, con casing no canónico o fuera de la allowlist, UUID no canónico y errores de binding conservando sus claves en `app/backend/tests/RealtorApiTests/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatusValidatorTests.cs`.

### Implementación de la historia de usuario 3

- [X] T019 [US3] Completar el binding local para convertir errores de lectura, content type y estructura JSON en errores del request sin ignorar propiedades adicionales en `app/backend/src/RealtorApi/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatus.Request.cs`.
- [X] T020 [US3] Completar las reglas de FluentValidation para errores de binding definidos en T019 y rechazos del estado y del UUID, con claves de error identificables, en `app/backend/src/RealtorApi/Features/Properties/UpdatePropertyStatus/UpdatePropertyStatus.Validator.cs`.
- [X] T021 [US3] Agregar ejemplos manuales para estado omitido, valor no admitido, campo adicional, body inválido e id no canónico, todos con expectativa `400 ValidationProblemDetails`, en `app/backend/src/RealtorApi/RealtorApi.http`.

**Punto de control**: La historia P3 debe rechazar todas las entradas inválidas con `400` y mantener inalterada la propiedad.

## Fase 6: Pulido y aspectos transversales

**Propósito**: Verificar contrato generado, suite, ejemplos manuales y trazabilidad sin ampliar el alcance.

- [X] T022 Regenerar y validar OpenAPI v1 con `support/scripts/generate-openapi-v1.ps1`; comprobar en `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` que el endpoint declara request body requerido `application/json`, un schema con solo `status` y enum `Available`, `Rented`, `Maintenance`, response `200` con exactamente `id` y `status`, errores `400` y `404`, y que conserva los contratos existentes.
- [X] T023 Ejecutar y corregir las pruebas unitarias del backend, incluidas las pruebas de drift de OpenAPI, usando `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`.
- [X] T024 Ejecutar los escenarios de éxito, `400`, `404` y preservación de datos descritos en `specs/009-update-status/quickstart.md` y `app/backend/src/RealtorApi/RealtorApi.http`.

## Dependencias y orden de ejecución

### Dependencias entre fases

- **Preparación (fase 1)**: Sin dependencias; los contratos pueden prepararse en paralelo.
- **Fundamentos (fase 2)**: Depende de la preparación; crea el binding y validación comunes que bloquean la implementación del endpoint.
- **Historia de usuario 1 (fase 3)**: Depende de Fundamentos; entrega el MVP funcional.
- **Historia de usuario 2 (fase 4)**: Depende de US1 porque extiende el handler y endpoint ya creados.
- **Historia de usuario 3 (fase 5)**: Depende de Fundamentos y del endpoint de US1; endurece y verifica los rechazos del request.
- **Pulido (fase 6)**: Depende de las tres historias completas.

### Orden de historias

1. **US1 (P1)**: MVP; debe completarse primero porque crea el flujo válido del slice.
2. **US2 (P2)**: Reutiliza el handler y endpoint de US1 para el resultado 404.
3. **US3 (P3)**: Completa el contrato de validación y la cobertura manual de entradas inválidas.

Las historias no son implementables como endpoints aislados porque comparten una única ruta y sus archivos de slice; su comportamiento sí se verifica por historia una vez integrado el endpoint base.

## Oportunidades de paralelismo

- **Preparación**: T001 y T002 trabajan en archivos distintos.
- **Fundamentos**: T003 y T005 son pruebas en archivos distintos; tras ellas, T004 y T006 pueden implementarse en paralelo en archivos distintos.
- **US1**: T007 y T008 son pruebas en archivos distintos. T009 debe completarse antes de T010 porque el handler usa el mapping para convertir el estado y construir la respuesta.
- **US2**: No hay paralelismo de implementación útil; handler y endpoint tienen dependencias y las pruebas comparten el archivo de pruebas de US1.
- **US3**: T017 y T018 son pruebas en archivos distintos y pueden ejecutarse en paralelo; después, completar T019 antes de T020 porque el validator consume los errores normalizados por el request.

### Ejemplo de ejecución paralela

```text
Fase 1: T001 + T002
Fase 2: T003 + T005; después T004 + T006
US1: T007 + T008; después T009 → T010
US3: T017 + T018; después T019 → T020
```

## Estrategia de implementación

### MVP primero

1. Completar preparación y fundamentos.
2. Completar solo US1: estado válido, respuesta `200`, mutación exclusiva de `Status`, preservación de los demás campos y ejemplo manual exitoso.
3. Ejecutar las pruebas unitarias focalizadas y el flujo manual de US1.
4. Continuar con US2 y US3 en orden porque ambas amplían el mismo endpoint.

### Entrega incremental

1. Preparación + Fundamentos: contratos, binding y validación canónica.
2. US1: actualización exitosa y preservación de datos.
3. US2: propiedad inexistente y `404 ProblemDetails`.
4. US3: errores del request y `400 ValidationProblemDetails`.
5. Pulido: OpenAPI generado, suite del backend y quickstart manual.
