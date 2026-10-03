# Tasks: Persistencia y seeding inicial de propiedades

**Input**: Design documents from `/specs/003-properties-persistence-seeding/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Las pruebas están explícitamente solicitadas por la spec para validar configuración EF, migración, seeding idempotente, assets externos y ruta pública de imagen.

**Organization**: Las tareas están agrupadas por user story para permitir implementación y validación independiente.

## Formato

- [ ] `T001` ...
- [ ] `T010 [P] [US1] ...`

- **[P]**: puede ejecutarse en paralelo cuando no depende de otra tarea activa.
- **[US1]**: etiqueta la tarea con la historia de usuario correspondiente.
- La descripción incluye siempre una ruta exacta del archivo a tocar.

## Phase 1: Setup (Infraestructura compartida)

**Propósito**: Preparar la base técnica y de estructura para la feature.

- [X] T001 Crear la estructura de dominio y persistencia para propiedades en `app/backend/src/RealtorApi/Domain/Properties/` y `app/backend/src/RealtorApi/Infrastructure/Persistence/`
- [X] T002 [P] Confirmar y ajustar el proyecto backend y los paquetes EF Core/Npgsql necesarios en `app/backend/src/RealtorApi/RealtorApi.csproj`
- [X] T003 [P] Preparar el proyecto de pruebas de persistencia en `app/backend/tests/RealtorApiTests/` con casos de configuración EF y seed

---

## Phase 2: Fundacional (bloquea todas las historias)

**Propósito**: Completar la base persistente y de arranque antes de la implementación funcional por historia.

- [X] T004 Implementar la entidad `PropertyStatus` en `app/backend/src/RealtorApi/Domain/Properties/PropertyStatus.cs`
- [X] T005 Implementar la entidad `Property` en `app/backend/src/RealtorApi/Domain/Properties/Property.cs`
- [X] T006 [P] Implementar la configuración EF Core en `app/backend/src/RealtorApi/Infrastructure/Persistence/Configurations/PropertyConfiguration.cs`
- [X] T007 [P] Implementar `AppDbContext` con `DbSet` y `ApplyConfigurationsFromAssembly` en `app/backend/src/RealtorApi/Infrastructure/Persistence/AppDbContext.cs`
- [X] T008 Implementar la extensión de migración y el flujo de arranque con `MigrateAsync()` en `app/backend/src/RealtorApi/Infrastructure/Persistence/MigrationExtensions.cs` y `app/backend/src/RealtorApi/Program.cs`
- [X] T009 Implementar la lógica base del seed y la lectura del manifest en `app/backend/src/RealtorApi/Infrastructure/Persistence/DatabaseSeeder.cs`

**Checkpoint**: La base de dominio y persistencia debe estar lista para que las historias de usuario puedan ejecutarse de forma independiente.

---

## Phase 3: User Story 1 - Persistencia del catálogo inicial de propiedades (Prioridad: P1) 🎯 MVP

**Goal**: Dejar definido el modelo persistente de propiedades y estados con configuración EF y migración coherente.

**Independent Test**: Verificar que la entidad se configura correctamente, que el valor de status se persiste como string y que la migración contiene el esquema esperado.

### Tests para User Story 1

- [X] T010 [P] [US1] Crear prueba de configuración EF para conversión enum-string en `app/backend/tests/RealtorApiTests/PropertyConfigurationTests.cs`
- [X] T011 [P] [US1] Crear prueba de migración para validar esquema esperado en `app/backend/tests/RealtorApiTests/PropertyMigrationTests.cs`

### Implementation for User Story 1

- [X] T012 [US1] Finalizar configuración del modelo persistente en `app/backend/src/RealtorApi/Infrastructure/Persistence/Configurations/PropertyConfiguration.cs`
- [X] T013 [US1] Confirmar restricciones, longitudes, precisión y defaults en el modelo de dominio y configuración en `app/backend/src/RealtorApi/Domain/Properties/Property.cs`
- [X] T014 [US1] Generar y revisar la migración única coherente en `app/backend/src/RealtorApi/Infrastructure/Migrations/`
- [X] T015 [US1] Verificar snapshot, `Up` y `Down` de la migración y dejar evidencia de coherencia funcional en la tarea

**Checkpoint**: User Story 1 debe quedar funcional y verificable de forma aislada.

---

## Phase 4: User Story 2 - Seeding automático y material externo integrado (Prioridad: P1)

**Goal**: Habilitar el seed idempotente a partir del manifest central y los assets externos del repositorio.

**Independent Test**: Ejecutar el arranque con la base de datos vacía y verificar que el seed carga estados y propiedades sin duplicar registros.

### Tests para User Story 2

- [X] T016 [P] [US2] Crear prueba de idempotencia del seeder en `app/backend/tests/RealtorApiTests/DatabaseSeederTests.cs`
- [X] T017 [P] [US2] Crear prueba de lectura de JSON y manifest en `app/backend/tests/RealtorApiTests/SeedManifestTests.cs`

### Implementation for User Story 2

- [X] T018 [P] [US2] Añadir los assets de seed y el manifiesto central en `support/seed-data/seed-manifest.json`, `support/seed-data/properties.json` y `support/seed-data/status.json`
- [X] T019 [P] [US2] Integrar los assets y las imágenes en `app/backend/src/RealtorApi/RealtorApi.csproj` para build y publish
- [X] T020 [US2] Implementar la resolución de rutas y la sincronización de imágenes en `app/backend/src/RealtorApi/Infrastructure/Persistence/DatabaseSeeder.cs`
- [X] T021 [US2] Asegurar que `ImageUrl` persiste la ruta pública final y no una ruta física de `support` en `app/backend/src/RealtorApi/Domain/Properties/Property.cs` y seeding logic
- [X] T022 [US2] Validar la idempotencia del seed y la consistencia con el manifest en la implementación final

**Checkpoint**: User Story 2 debe ser funcional sin depender de ejecuciones manuales ni rutas hardcodeadas.

---

## Phase 5: User Story 3 - Validación de migración y consistencia de arranque (Prioridad: P2)

**Goal**: Asegurar que el arranque de la aplicación ejecuta migración y seed de forma automática y sin duplicados.

**Independent Test**: Iniciar la API y validar que `MigrateAsync()` se ejecuta antes de `Run` y que `UseSeeding` + `UseAsyncSeeding` invocan el seed incluso sin migraciones pendientes.

### Tests para User Story 3

- [X] T023 [P] [US3] Crear prueba de arranque con seeding async en `app/backend/tests/RealtorApiTests/StartupSeedingTests.cs`
- [X] T024 [P] [US3] Crear prueba de consistencia de `ImageUrl` y copia de imágenes en `app/backend/tests/RealtorApiTests/PropertyImageUrlTests.cs`

### Implementation for User Story 3

- [X] T025 [US3] Configurar `UseSeeding` y `UseAsyncSeeding` en `app/backend/src/RealtorApi/Infrastructure/Persistence/AppDbContext.cs`
- [X] T026 [US3] Ajustar el flujo de startup en `app/backend/src/RealtorApi/Program.cs` para ejecutar migración antes de ejecutar la app
- [X] T027 [US3] Revisión final del flujo de bootstrap y validación de la ruta final de imagen con el manifest
- [X] T028 [US3] Ejecutar la verificación del arranque y confirmar que el proceso es idempotente en reinicios consecutivos

**Checkpoint**: User Story 3 debe quedar validada como parte del arranque real de la aplicación.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Propósito**: Revisar integración global y preparación para cierre de feature.

- [X] T029 [P] Revisar el conjunto final de archivos del feature en `specs/003-properties-persistence-seeding/` para confirmar trazabilidad y coherencia con el plan
- [X] T030 [P] Ejecutar la suite final del backend en `app/backend/tests/RealtorApiTests/` y confirmar que no hay regresiones en infraestructura
- [X] T031 Ejecutar la validación de quickstart en `specs/003-properties-persistence-seeding/quickstart.md` y confirmar que el inicio de la API es consistente

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sin dependencias; puede arrancar inmediatamente.
- **Fundacional (Phase 2)**: depende del Setup; bloquea todas las historias.
- **User Story 1**: depende del Fundacional y prepara el modelo persistente base.
- **User Story 2**: depende del Fundacional y de la persistencia de la historia 1.
- **User Story 3**: depende del Fundacional y del resultado de las historias 1 y 2.
- **Polish (Phase 6)**: depende de que todas las historias relevantes estén completas.

### User Story Dependencies

- **US1**: puede arrancar después del Fundacional; no depende de otras historias.
- **US2**: requiere el modelo persistente de US1 para validar el seed end-to-end.
- **US3**: requiere validación real de migración, seeding y rutas de imagen generadas por US1 y US2.

### Parallel Opportunities

- `T002` y `T003` pueden ejecutarse en paralelo.
- `T006` y `T007` pueden ejecutarse en paralelo.
- Las pruebas de cada story (`T010`, `T011`, `T016`, `T017`, `T023`, `T024`) pueden ejecutarse en paralelo dentro del mismo story.
- Los tasks de implementación de cada story también pueden desarrollarse en paralelo siempre que no compartan el mismo fichero.

---

## Parallel Example: User Story 1

```bash
# Ejecutar pruebas del User Story 1 en paralelo
Task: "Crear prueba de configuración EF para conversión enum-string en app/backend/tests/RealtorApiTests/PropertyConfigurationTests.cs"
Task: "Crear prueba de migración para validar esquema esperado en app/backend/tests/RealtorApiTests/PropertyMigrationTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Completar Phase 1: Setup
2. Completar Phase 2: Fundacional
3. Completar Phase 3: User Story 1
4. Validar la migración y la configuración EF antes de continuar
5. Solo entonces avanzar al seed y al arranque

### Incremental Delivery

1. Setup + Fundacional → base persistente lista
2. User Story 1 → validar esquema y migración
3. User Story 2 → validar seed e imágenes
4. User Story 3 → validar arranque automático y idempotencia
5. Cierre con revisión cross-cutting

### Parallel Team Strategy

Con varios desarrolladores:

1. Equipo completa Setup + Fundacional juntos.
2. Tras el Fundacional:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. Cada story se valida de forma independiente antes del cierre global.

---

## Notes

- Las tareas marcadas con `[P]` apuntan a archivos diferentes o a validaciones que no dependen de trabajo previo en el mismo fichero.
- Las tareas de testing deben ir primero en cada historia para cumplir la práctica de TDD y la validación real del comportamiento.
- No se permiten tareas vagas ni tareas sin ruta exacta.
- Cada story debe poder validarse de forma independiente antes de pasar a la siguiente prioridad.
