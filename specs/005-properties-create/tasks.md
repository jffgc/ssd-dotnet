# Tasks: Alta de propiedades con upload opcional de imagen

**Input**: Design documents from `/specs/005-properties-create/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Las pruebas están explícitamente solicitadas por la spec para cubrir creación sin imagen, creación con imagen válida, rechazo por tipo de archivo y rechazo por tamaño máximo.

**Organization**: Las tareas están agrupadas por user story para permitir implementación y validación independientes.

## Formato

- [ ] `T001` ...
- [ ] `T010 [P] [US1] ...`

- **[P]**: puede ejecutarse en paralelo cuando no depende de otra tarea activa.
- **[US1]**: etiqueta la tarea con la historia de usuario correspondiente.
- La descripción incluye siempre una ruta exacta del archivo a tocar.

## Phase 1: Setup (Infraestructura compartida)

**Propósito**: Preparar la base técnica y de estructura para la feature.

- [ ] T001 Crear la estructura del slice `CreateProperty` en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/`
- [ ] T002 [P] Confirmar el proyecto backend y la configuración existente en `app/backend/src/RealtorApi/RealtorApi.csproj` para el nuevo caso de uso sin cambiar el stack base
- [ ] T003 [P] Preparar la suite de pruebas del caso de uso en `app/backend/tests/RealtorApiTests/`

---

## Phase 2: Fundacional (bloquea todas las historias)

**Propósito**: Definir la base técnica del slice y la validación del request antes de implementar cada historia.

- [ ] T004 Crear el modelo de entrada para la propiedad en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Request.cs`
- [ ] T005 Crear el modelo de salida del caso de uso en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Response.cs`
- [ ] T006 [P] Implementar el validador del request en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Validator.cs`
- [ ] T007 [P] Implementar el mapeo explícito del request a entidad en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Mapping.cs`
- [ ] T008 Implementar el endpoint público en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Endpoint.cs` y registrar la ruta `POST /api/properties`
- [ ] T009 Implementar el handler del caso de uso en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Handler.cs`

**Checkpoint**: La base del slice debe quedar lista para que cada historia pueda ejecutarse de forma aislada.

---

## Phase 3: User Story 1 - Creación de propiedad sin imagen (Prioridad: P1) 🎯 MVP

**Goal**: Permitir crear una propiedad válida sin exigir un archivo adjunto y guardar `ImageUrl` como `null`.

**Independent Test**: Ejecutar el endpoint con datos válidos sin imagen y comprobar `201 Created` y `ImageUrl = null`.

### Tests para User Story 1

- [ ] T010 [P] [US1] Crear prueba de creación exitosa sin imagen en `app/backend/tests/RealtorApiTests/CreatePropertyWithoutImageTests.cs`
- [ ] T011 [P] [US1] Crear prueba de persistencia de `ImageUrl = null` en `app/backend/tests/RealtorApiTests/CreatePropertyPersistenceTests.cs`

### Implementation for User Story 1

- [ ] T012 [US1] Definir la lógica del handler para creación sin imagen en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Handler.cs`
- [ ] T013 [US1] Mapear el request a `Property` con `ImageUrl = null` en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Mapping.cs`
- [ ] T014 [US1] Finalizar el endpoint de creación pública en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Endpoint.cs`
- [ ] T015 [US1] Verificar la respuesta HTTP y el cuerpo de salida en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Response.cs`

**Checkpoint**: User Story 1 debe quedar funcional y verificable de forma aislada.

---

## Phase 4: User Story 2 - Upload de imagen válida y almacenamiento público (Prioridad: P1)

**Goal**: Permitir subir una imagen válida, guardarla en `wwwroot/assets/properties` y persistir la URL pública final en `ImageUrl`.

**Independent Test**: Enviar una imagen PNG o JPG válida y verificar que el archivo se salva bajo `wwwroot/assets/properties` y la propiedad guarda la URL pública resultante.

### Tests para User Story 2

- [ ] T016 [P] [US2] Crear prueba de upload válido PNG/JPG en `app/backend/tests/RealtorApiTests/CreatePropertyWithImageTests.cs`
- [ ] T017 [P] [US2] Crear prueba de persistencia de `ImageUrl` pública en `app/backend/tests/RealtorApiTests/CreatePropertyImageUrlTests.cs`

### Implementation for User Story 2

- [ ] T018 [P] [US2] Implementar la validación de tipo y tamaño de imagen en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Validator.cs`
- [ ] T019 [US2] Implementar la lógica de guardado del archivo en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Handler.cs`
- [ ] T020 [US2] Asegurar la creación del directorio de destino en `app/backend/src/RealtorApi/wwwroot/assets/properties/`
- [ ] T021 [US2] Persistir la URL pública de la imagen en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Mapping.cs`
- [ ] T022 [US2] Validar la compatibilidad de `ImageUrl` con la API servida desde `wwwroot` y no desde una ruta física de soporte

**Checkpoint**: User Story 2 debe quedar funcional sin depender de ejecuciones manuales ni de rutas hardcodeadas.

---

## Phase 5: User Story 3 - Validación de upload y errores esperados (Prioridad: P1)

**Goal**: Rechazar imágenes con formato no soportado o tamaño superior a 5MB y responder con `ProblemDetails` coherente.

**Independent Test**: Enviar archivo inválido o demasiado grande y comprobar que la API devuelve un error de validación útil sin crear registros parciales.

### Tests para User Story 3

- [ ] T023 [P] [US3] Crear prueba de tipo de archivo no soportado en `app/backend/tests/RealtorApiTests/CreatePropertyInvalidFileTypeTests.cs`
- [ ] T024 [P] [US3] Crear prueba de sobrepeso de archivo (>5MB) en `app/backend/tests/RealtorApiTests/CreatePropertyOversizeFileTests.cs`
- [ ] T025 [P] [US3] Crear prueba de `ProblemDetails` para errores de validación en `app/backend/tests/RealtorApiTests/CreatePropertyProblemDetailsTests.cs`

### Implementation for User Story 3

- [ ] T026 [US3] Finalizar la validación de error de archivo no permitido en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Validator.cs`
- [ ] T027 [US3] Finalizar la validación de tamaño máximo en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Validator.cs`
- [ ] T028 [US3] Ajustar la respuesta HTTP y el tratamiento de errores en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Endpoint.cs`
- [ ] T029 [US3] Asegurar que no se persiste ni se guarda archivo cuando una imagen inválida falla la validación en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Handler.cs`

**Checkpoint**: User Story 3 debe quedar validada como comportamiento de negocio real y no solo como validación estática.

---

## Phase 6: User Story 4 - Integración con arquitectura vertical slice y dependencias previas (Prioridad: P2)

**Goal**: Garantizar que el slice encaja con la infraestructura existente sin introducir cambios no requeridos.

**Independent Test**: Verificar que el slice se auto-registra con `ISlice` e `IHandler` y que no se rompe ninguna parte previa del backend.

### Tests para User Story 4

- [ ] T030 [P] [US4] Crear prueba de auto-registro del slice en `app/backend/tests/RealtorApiTests/CreatePropertySliceRegistrationTests.cs`
- [ ] T031 [P] [US4] Crear prueba de integración con dominio previo en `app/backend/tests/RealtorApiTests/CreatePropertyIntegrationTests.cs`

### Implementation for User Story 4

- [ ] T032 [US4] Confirmar la estructura del slice y la convención `ISlice` en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/`
- [ ] T033 [US4] Comprobar que no se introduce lógica de negocio en `CreateProperty.Mapping.cs`
- [ ] T034 [US4] Revalidar que `Program.cs` mantiene el auto-registro de slices y handlers sin cambios manuales del endpoint
- [ ] T035 [US4] Ejecutar la verificación de no regresión en `app/backend/src/RealtorApi/Features/Health/HealthSlice.cs`

**Checkpoint**: User Story 4 debe validar la integración del diseño con la estructura ya existente.

---

## Phase 7: User Story 5 - Calidad operativa del recurso público (Prioridad: P3)

**Goal**: Confirmar que la URL de imagen persistida es consumible por clientes y estable en distintos entornos.

**Independent Test**: Verificar que la `ImageUrl` devuelta es pública o relativa y sigue siendo accesible a través de `wwwroot`.

### Tests para User Story 5

- [ ] T036 [P] [US5] Crear prueba de URL pública final en `app/backend/tests/RealtorApiTests/CreatePropertyPublicUrlTests.cs`
- [ ] T037 [P] [US5] Crear prueba de consumo del recurso desde `wwwroot` en `app/backend/tests/RealtorApiTests/CreatePropertyStaticAssetTests.cs`

### Implementation for User Story 5

- [ ] T038 [US5] Ajustar la ruta pública generada para la imagen en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Mapping.cs`
- [ ] T039 [US5] Validar la compatibilidad con el hosting de static files de `app/backend/src/RealtorApi/Program.cs`

**Checkpoint**: User Story 5 debe confirmar que el asset queda listo para consumo por frontend o clientes externos.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Propósito**: Revisión final de la feature y validación del comportamiento global.

- [ ] T040 [P] Ejecutar la suite relevante del backend en `app/backend/tests/RealtorApiTests/`
- [ ] T041 [P] Revisar la trazabilidad y el alcance de la feature en `specs/005-properties-create/`
- [ ] T042 Ejecutar la validación del quickstart de la feature en `specs/005-properties-create/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sin dependencias; puede arrancar inmediatamente.
- **Fundacional (Phase 2)**: depende del Setup; bloquea todas las historias.
- **User Story 1**: depende del Fundacional; MVP inicial.
- **User Story 2**: depende del Fundacional y del comportamiento correcto del endpoint base.
- **User Story 3**: depende del Fundacional y del comportamiento de validación del upload.
- **User Story 4**: depende de la integración del slice con el backend existente.
- **User Story 5**: depende del contenido público de la imagen y del endpoint correcto.
- **Polish (Phase 8)**: depende de que todas las historias relevantes estén completas.

### User Story Dependencies

- **US1**: puede ejecutarse tras Fundacional; no depende de otras historias.
- **US2**: requiere la creación base del endpoint y del modelo de entidad.
- **US3**: requiere validación del upload y del error flow del request.
- **US4**: requiere revisión de infraestructura y auto-registro.
- **US5**: requiere la creación correcta del archivo y la persistencia del valor público.

### Parallel Opportunities

- `T002` y `T003` pueden ejecutarse en paralelo.
- `T006` y `T007` pueden ejecutarse en paralelo dentro del Fundacional.
- Las pruebas de cada story pueden ejecutarse en paralelo.
- Los tasks de implementación dentro de cada story pueden ejecutarse en paralelo cuando no comparten el mismo archivo.

---

## Parallel Example: User Story 1

```bash
# Ejecutar pruebas del User Story 1 en paralelo
Task: "Crear prueba de creación exitosa sin imagen en app/backend/tests/RealtorApiTests/CreatePropertyWithoutImageTests.cs"
Task: "Crear prueba de persistencia de ImageUrl = null en app/backend/tests/RealtorApiTests/CreatePropertyPersistenceTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Completar Phase 1: Setup
2. Completar Phase 2: Fundacional
3. Completar Phase 3: User Story 1
4. Validar la creación sin imagen antes de continuar con imágenes
5. Probar solo el flujo principal antes de ampliar a validación de archivo

### Incremental Delivery

1. Setup + Fundacional → base del slice lista
2. User Story 1 → validación de creación básica
3. User Story 2 → validación de upload y almacenamiento real
4. User Story 3 → validación de errores de archivo
5. User Story 4 + 5 → integración y consistencia operativa
6. Cierre con revisión final y pruebas cruzadas

### Parallel Team Strategy

Con varios desarrolladores:

1. Equipo completa Setup + Fundacional juntos.
2. Tras el Fundacional:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. Después de integración funcional, User Story 4 y 5 pueden cerrarse con revisión conjunta.

---

## Notes

- Las tareas marcadas con `[P]` apuntan a archivos diferentes o validaciones que no dependen de trabajo previo en el mismo fichero.
- Las tareas de testing deben ir primero en cada historia para cumplir la práctica de TDD y la validación real del comportamiento.
- No se permiten tareas vagas ni tareas sin ruta exacta.
- Cada story debe poder validarse de forma independiente antes de pasar a la siguiente prioridad.
