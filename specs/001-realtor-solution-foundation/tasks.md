# Tasks: Base de la solución Realtor

**Input**: Design documents from `/specs/001-realtor-solution-foundation/`

**Prerequisites**: `spec.md`, `plan.md`, `research.md`, `data-model.md` and `quickstart.md` are complete. The repository already contains the authoritative SDK defined in `global.json`.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the repository structure needed to host the solution and its base projects.

- [X] T001 Create the app directory structure in `app/`, `app/backend/`, `app/frontend/`, `app/backend/src/`, `app/backend/tests/`, `app/frontend/src/`, and `app/frontend/test/`
- [X] T002 Create the solution entry file `app/Realtor.sln` and register the base projects in the solution
- [X] T003 [P] Create the backend project skeleton in `app/backend/src/RealtorApi/` using ASP.NET Core Minimal APIs
- [X] T004 [P] Create the backend test project skeleton in `app/backend/tests/RealtorApiTests/`
- [X] T005 [P] Create the frontend project skeleton in `app/frontend/src/RealtorWeb/` using Blazor Web App
- [X] T006 [P] Create the frontend test project skeleton in `app/frontend/test/RealtorWeb/`

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish the minimal runtime wiring for the base solution without introducing business logic.

- [X] T007 Configure the backend entry point in `app/backend/src/RealtorApi/Program.cs` with base services, base middleware, and an initial minimal endpoint mapping
- [X] T008 Configure the frontend entry point in `app/frontend/src/RealtorWeb/Program.cs` with the base Blazor app services and root routing setup
- [X] T009 Validate there are no controllers, domain entities, or business logic implementations in `app/backend/src/RealtorApi/` or `app/frontend/src/RealtorWeb/`
- [X] T010 Validate the .NET SDK version is sourced from `global.json` and that `global.json` remains unchanged in this initiative

## Phase 3: User Story 1 - Base operativa de la solución (Priority: P1)

**Goal**: Deliver a working foundation for the repository to host backend and frontend work in a single solution.

**Independent Test**: The story is complete when the solution loads and compiles without business logic or feature code.

### Implementation for User Story 1

- [X] T011 [US1] Create the backend project contract and structure under `app/backend/src/RealtorApi/` for the minimal ASP.NET Core host
- [X] T012 [P] [US1] Create the frontend project structure under `app/frontend/src/RealtorWeb/` for the Blazor Web App shell
- [X] T013 [US1] Confirm the solution includes both projects and their test projects under `app/Realtor.sln`
- [X] T014 [US1] Ensure the backend app configuration in `app/backend/src/RealtorApi/Program.cs` stays limited to foundation setup only
- [X] T015 [US1] Ensure the frontend app configuration in `app/frontend/src/RealtorWeb/Program.cs` stays limited to foundation setup only

**Checkpoint**: User Story 1 should be independently buildable and ready for future feature work.

## Phase 4: User Story 2 - Preparación de la base para evolución (Priority: P2)

**Goal**: Leave the repository in a clean, canonical state that future initiatives can extend safely.

**Independent Test**: The story is complete when the solution satisfies the architectural rules and the repo remains neutral for the next feature phase.

### Implementation for User Story 2

- [X] T016 [P] [US2] Verify the repository respects the canonical paths in `app/backend/src/RealtorApi/`, `app/backend/tests/RealtorApiTests/`, `app/frontend/src/RealtorWeb/`, and `app/frontend/test/RealtorWeb/`
- [X] T017 [US2] Review the generated foundation files in `specs/001-realtor-solution-foundation/` to confirm they document the base solution only
- [X] T018 [US2] Run a final compilation check for `app/Realtor.sln` to confirm the foundation remains buildable without business logic
- [X] T019 [US2] Validate the foundation is not introducing domain entities or feature implementations in `app/backend/src/RealtorApi/` or `app/frontend/src/RealtorWeb/`

**Checkpoint**: User Story 2 is complete when the foundation is stable, documented, and ready for the next initiative.

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final repo hygiene and quality review for the foundation release.

- [X] T020 [P] Review the documentation under `specs/001-realtor-solution-foundation/` for alignment with the repository constitution
- [X] T021 [P] Check that all generated paths use the canonical solution layout and no parallel structures were introduced
- [X] T022 Verify the final foundation state is a neutral base and does not include business logic, domain models, or feature implementation

## Dependencies & Execution Order

- Phase 1 must complete before Phase 2 starts.
- Phase 2 must complete before User Story 1 and User Story 2 implementation begins.
- User Story 1 and User Story 2 can be developed in parallel after the foundation is ready.
- Phase 5 depends on all user story work being complete.

## Parallel Opportunities

- T003, T004, T005, and T006 can be completed in parallel.
- T012 and T016 are parallelizable once their respective project structures exist.
- T020 and T021 are parallelizable during final review.

## Implementation Strategy

### MVP First

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Finish User Story 1 to establish the minimal solution shell.
4. Finish User Story 2 to confirm the foundation is ready for future work.
5. Stop and validate before moving to any feature-specific initiative.
