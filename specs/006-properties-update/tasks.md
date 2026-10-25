# Tasks: Actualización de propiedades con upload opcional de imagen

**Input**: Design documents from `/specs/006-properties-update/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Las pruebas están explícitamente solicitadas por la spec para cubrir actualización sin imagen, actualización con imagen válida, propiedad inexistente, archivo vacío/inh válido, formato no permitido y tamaño máximo.

**Organization**: Las tareas están agrupadas por user story para permitir implementación y validación independientes.

## Formato

- [ ] `T001` ...
- [ ] `T010 [P] [US1] ...`

- **[P]**: puede ejecutarse en paralelo cuando no depende de otra tarea activa.
- **[US1]**: etiqueta la tarea con la historia de usuario correspondiente.
- La descripción incluye siempre la ruta exacta del archivo a tocar.

## Phase 1: Setup (Infraestructura compartida)

**Propósito**: Preparar la base técnica y de estructura para la feature.

- [ ] T001 Crear el directorio y archivos base del slice `UpdateProperty` en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/`
- [ ] T002 [P] Confirmar la configuración del proyecto backend y la ruta pública de assets en `app/backend/src/RealtorApi/RealtorApi.csproj` y `app/backend/src/RealtorApi/Program.cs`
- [ ] T003 [P] Preparar la suite de pruebas del caso de uso en `app/backend/tests/RealtorApiTests/`

---

## Phase 2: Fundacional (bloquea todas las historias)

**Propósito**: Definir la base técnica del slice y la validación del request antes de implementar cada historia.

- [ ] T004 Crear el modelo de entrada del caso de uso en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Request.cs`
- [ ] T005 Crear el modelo de salida del caso de uso en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Response.cs`
- [ ] T006 [P] Implementar el validador del request en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Validator.cs`
- [ ] T007 [P] Implementar el mapeo explícito del request a entidad en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Mapping.cs`
- [ ] T008 Implementar el endpoint público en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Endpoint.cs` con la ruta `PUT /api/properties/{id}`
- [ ] T009 Implementar el handler del caso de uso en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Handler.cs`

**Checkpoint**: La base del slice debe quedar lista para que cada historia pueda ejecutarse de forma aislada.

---

## Phase 3: User Story 1 - Actualización sin imagen (Prioridad: P1) 🎯 MVP

**Goal**: Permitir actualizar una propiedad válida sin exigir un archivo adjunto y conservar `imageUrl` previo.

**Independent Test**: Ejecutar el endpoint con datos válidos sin imagen y comprobar `200 OK` o equivalente y que `imageUrl` se conserva intacto.

### Tests para User Story 1

- [ ] T010 [P] [US1] Crear prueba de actualización exitosa sin imagen en `app/backend/tests/RealtorApiTests/UpdatePropertyWithoutImageTests.cs`
- [ ] T011 [P] [US1] Crear prueba de persistencia de `imageUrl` previo en `app/backend/tests/RealtorApiTests/UpdatePropertyImagePreservedTests.cs`

### Implementation for User Story 1

- [ ] T012 [US1] Definir la lógica del handler para actualización sin imagen en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Handler.cs`
- [ ] T013 [US1] Mapear el request a `Property` conservando `ImageUrl` existente en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Mapping.cs`
- [ ] T014 [US1] Finalizar el endpoint de actualización pública en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Endpoint.cs`
- [ ] T015 [US1] Verificar la respuesta HTTP y el cuerpo de salida en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Response.cs`

**Checkpoint**: User Story 1 debe quedar funcional y verificable de forma aislada.

---

## Phase 4: User Story 2 - Actualización con imagen válida y reemplazo de asset público (Prioridad: P2)

**Goal**: Permitir subir una imagen válida, guardar el archivo con UUID y persistir la URL pública final en `ImageUrl`.

**Independent Test**: Enviar una imagen PNG o JPG válida y verificar que el archivo se salva bajo `wwwroot/assets/properties` y la propiedad guarda la nueva URL pública resultante.

### Tests para User Story 2

- [ ] T016 [P] [US2] Crear prueba de actualización con imagen válida en `app/backend/tests/RealtorApiTests/UpdatePropertyWithImageTests.cs`
- [ ] T017 [P] [US2] Crear prueba de persistencia de `imageUrl` pública en `app/backend/tests/RealtorApiTests/UpdatePropertyImageUrlTests.cs`

### Implementation for User Story 2

- [ ] T018 [P] [US2] Implementar la validación de tipo, tamaño y vaciedad de imagen en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Validator.cs`
- [ ] T019 [US2] Implementar la lógica de guardado del archivo con UUID + extensión en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Handler.cs`
- [ ] T020 [US2] Crear o asegurar el directorio de destino de assets en `app/backend/src/RealtorApi/wwwroot/assets/properties/`
- [ ] T021 [US2] Persistir la URL pública de la nueva imagen en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Mapping.cs`
- [ ] T022 [US2] Validar la compatibilidad de la ruta con el static files del backend en `app/backend/src/RealtorApi/Program.cs`

**Checkpoint**: User Story 2 debe quedar funcional sin depender de ejecuciones manuales ni rutas hardcodeadas.

---

## Phase 5: User Story 3 - Rechazos por validación y manejo de errores (Prioridad: P3)

**Goal**: Rechazar propiedades inexistentes, archivos inválidos y tamaños excesivos sin dejar cambios parciales.

**Independent Test**: Enviar id inexistente, archivo vacío/incorrecto o >5MB y comprobar que la API devuelve `404`, `415` o `413` según el caso, sin actualizar la entidad ni dejar artefactos parciales.

### Tests para User Story 3

- [ ] T023 [P] [US3] Crear prueba de propiedad inexistente en `app/backend/tests/RealtorApiTests/UpdatePropertyNotFoundTests.cs`
- [ ] T024 [P] [US3] Crear prueba de archivo vacío o formato no soportado en `app/backend/tests/RealtorApiTests/UpdatePropertyInvalidImageTests.cs`
- [ ] T025 [P] [US3] Crear prueba de archivo >5MB y `ProblemDetails` en `app/backend/tests/RealtorApiTests/UpdatePropertyOversizeImageTests.cs`

### Implementation for User Story 3

- [ ] T026 [US3] Implementar la validación de `400` para reglas de negocio y `404` para id inexistente en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Validator.cs`
- [ ] T027 [US3] Implementar la validación de `415` para formato no permitido, archivo vacío o contenido real inválido en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Validator.cs`
- [ ] T028 [US3] Implementar la validación de `413` para imagen >5MB en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Validator.cs`
- [ ] T029 [US3] Asegurar atomicidad y reversión de cambios cuando falla el almacenamiento de imagen en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Handler.cs`
- [ ] T030 [US3] Asegurar la respuesta de error consistente con `ProblemDetails` en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Endpoint.cs`

**Checkpoint**: User Story 3 debe quedar validada como comportamiento de negocio real y no solo como validación estática.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Propósito**: Revisión final de la feature y validación del comportamiento global.

- [ ] T031 [P] Ejecutar la suite relevante del backend en `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`
- [ ] T032 [P] Revisar la trazabilidad y el alcance de la feature en `specs/006-properties-update/spec.md` y `specs/006-properties-update/plan.md`
- [ ] T033 Ejecutar la validación del quickstart de la feature en `specs/006-properties-update/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sin dependencias; puede arrancar inmediatamente.
- **Fundacional (Phase 2)**: depende del Setup; bloquea todas las historias.
- **User Story 1 (Phase 3)**: depende del Fundacional; es el MVP de la feature.
- **User Story 2 (Phase 4)**: depende del Fundacional y del comportamiento base del endpoint.
- **User Story 3 (Phase 5)**: depende del Fundacional y de la validación del archivo y del id.
- **Polish (Phase 6)**: depende de que todas las historias relevantes estén completas.

### User Story Dependencies

- **US1**: puede ejecutarse tras Fundacional; no depende de otras historias.
- **US2**: requiere la base del endpoint y la capa del handler para guardar archivo.
- **US3**: requiere validación del id y del archivo, así como la semántica de errores del endpoint.

### Parallel Opportunities

- `T002` y `T003` pueden ejecutarse en paralelo.
- `T006` y `T007` pueden ejecutarse en paralelo dentro del Fundacional.
- Las pruebas de cada story pueden ejecutarse en paralelo.
- Los tasks de implementación dentro de cada story pueden ejecutarse en paralelo cuando no comparten el mismo archivo.

---

## Parallel Example: User Story 1

```bash
# Ejecutar pruebas del User Story 1 en paralelo
Task: "Crear prueba de actualización exitosa sin imagen en app/backend/tests/RealtorApiTests/UpdatePropertyWithoutImageTests.cs"
Task: "Crear prueba de persistencia de imageUrl previo en app/backend/tests/RealtorApiTests/UpdatePropertyImagePreservedTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Completar Phase 1: Setup
2. Completar Phase 2: Fundacional
3. Completar Phase 3: User Story 1
4. Validar la actualización sin imagen antes de continuar con imágenes
5. Probar solo el flujo principal antes de ampliar a validación de archivo

### Incremental Delivery

1. Setup + Fundacional → base del slice lista
2. User Story 1 → validación de actualización básica
3. User Story 2 → validación de upload y almacenamiento real
4. User Story 3 → validación de errores de archivo y entidad inexistente
5. Cierre con revisión final y pruebas cruzadas

### Parallel Team Strategy

Con varios desarrolladores:

1. Equipo completa Setup + Fundacional juntos.
2. Tras el Fundacional:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. Al terminar, se ejecuta la revisión final de progreso y regresiones.

---

## Notes

- Las tareas marcadas con `[P]` apuntan a archivos diferentes o validaciones que no dependen de trabajo previo en el mismo fichero.
- Las tareas de testing deben ir primero en cada historia para cumplir la práctica de TDD y la validación real del comportamiento.
- No se permiten tareas vagas ni tareas sin ruta exacta.
- Cada story debe poder validarse de forma independiente antes de pasar a la siguiente prioridad.
