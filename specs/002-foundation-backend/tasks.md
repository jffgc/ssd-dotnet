# Tasks: Base de infraestructura del backend Realtor

**Input**: Design documents from `/specs/002-foundation-backend/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

## Phase 1: Setup y estructura base

- [X] T001 Crear estructura de infraestructura bajo `app/backend/src/RealtorApi/Infrastructure` y `Features/Health`
- [X] T002 Configurar paquetes necesarios para validación y tests del backend
- [X] T003 [P] Preparar el proyecto de pruebas unitarias para la infraestructura transversal

## Phase 2: Fundamentos del backend

**⚠️ CRÍTICO**: La infraestructura base debe quedar completa antes de verificar el endpoint de salud.

- [X] T004 Implementar `ISlice` y `RegisterSlices` con discovery por assembly scanning sin duplicados
- [X] T005 Implementar `MapSliceEndpoints` para mapear endpoints transversales sin registro manual
- [X] T006 Implementar `IHandler` y `RegisterHandlers` para auto-registro de handlers
- [X] T007 Implementar `ValidationFilterFactory` con detección automática de `IValidator<T>`
- [X] T008 Implementar `Result` y mapeo a `ProblemDetails` con status codes consistentes
- [X] T009 Migrar `/health` al patrón ISlice y dejar `Program.cs` centralizado

## Phase 3: Validación y cierre

- [X] T010 [P] Escribir pruebas de infraestructura para slices, validators y errors
- [X] T011 Ejecutar la suite de prueba del backend y corregir desviaciones
- [X] T012 Verificar compilación de la solución y confirmar que no hay lógica de negocio en esta feature

## Dependencias

- T001 → T004/T005/T006/T007/T008/T009
- T010 depende de T004-T009
- T011 depende de T010
- T012 depende de T011
