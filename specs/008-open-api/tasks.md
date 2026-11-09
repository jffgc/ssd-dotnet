# Tasks: OpenAPI v1 de RealtorApi

**Input**: Design documents from `/specs/008-open-api/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Configure the tooling and build pipeline needed to generate, validate and smoke-test the OpenAPI contract.

- [X] T001 [P] Create Redocly validation configuration in .redocly.yaml
- [X] T002 [P] Register NSwag.ConsoleCore in .config/dotnet-tools.json
- [X] T003 [P] Create the reproducible generation script in support/scripts/generate-openapi-v1.ps1
- [X] T004 [P] Add OpenAPI generation metadata to app/backend/src/RealtorApi/RealtorApi.csproj

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Prepare the backend runtime and folder layout so the OpenAPI artifact can be generated and served without modifying business logic.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T005 Configure the static output folder for generated docs in app/backend/src/RealtorApi/wwwroot/openapi/
- [X] T006 Configure static file serving and the stable GET /openapi/v1.json route in app/backend/src/RealtorApi/Program.cs
- [X] T007 Ensure the OpenAPI v1 artifact is built from the runtime endpoints and stored at app/backend/src/RealtorApi/wwwroot/openapi/v1.json
- [X] T008 [P] Expose shared ProblemDetails and HttpValidationProblemDetails schemas through generated endpoint metadata in app/backend/src/RealtorApi/
- [X] T009 Verify the app still serves only static JSON and no Swagger UI or ReDoc UI runtime endpoint in app/backend/src/RealtorApi/Program.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel.

---

## Phase 3: User Story 1 - Contrato público derivado del backend real (Priority: P1) 🎯 MVP

**Goal**: Ensure the OpenAPI document reflects the actual Minimal API runtime for the public Realtor API.

**Independent Test**: Generate the OpenAPI JSON and confirm all implemented endpoints are represented without manual editing.

### Tests for User Story 1

- [X] T010 [P] [US1] Add a runtime endpoint inventory test in app/backend/tests/RealtorApiTests/OpenApiContractTests.cs
- [X] T011 [P] [US1] Add a Redocly lint validation test in app/backend/tests/RealtorApiTests/OpenApiContractTests.cs

### Implementation for User Story 1

- [X] T012 [US1] Add explicit endpoint metadata to the health endpoint in app/backend/src/RealtorApi/Features/Health/HealthSlice.cs
- [X] T013 [US1] Ensure the list properties endpoint exposes the canonical contract in app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesSlice.cs
- [X] T014 [US1] Ensure the create property endpoint exposes request and response metadata in app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreateProperty.Endpoint.cs
- [X] T015 [US1] Ensure the update property endpoint exposes request and response metadata and normalizes route constraints in app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdateProperty.Endpoint.cs
- [X] T016 [US1] Configure shared schema exposure for PropertyStatus and ProblemDetails in the generated OpenAPI output under app/backend/src/RealtorApi/
- [X] T017 [US1] Generate the v1.json artifact and validate the model against the real endpoint inventory at app/backend/src/RealtorApi/wwwroot/openapi/v1.json

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently.

---

## Phase 4: User Story 2 - Publicación estática y exclusión explícita de UI (Priority: P2)

**Goal**: Publish a stable JSON document at a fixed URL and confirm the application does not expose interactive documentation viewers.

**Independent Test**: Request GET /openapi/v1.json and verify the response is 200 OK with JSON content while no Swagger or ReDoc UI is served.

### Tests for User Story 2

- [X] T018 [P] [US2] Add the HTTP smoke check for GET /openapi/v1.json in app/backend/tests/RealtorApiTests/OpenApiSmokeTests.cs
- [X] T019 [P] [US2] Add a negative assertion for no Swagger UI or ReDoc UI in app/backend/tests/RealtorApiTests/OpenApiSmokeTests.cs

### Implementation for User Story 2

- [X] T020 [P] [US2] Publish the generated JSON file from app/backend/src/RealtorApi/wwwroot/openapi/v1.json
- [X] T021 [US2] Configure the static route to serve the document without adding UI middleware in app/backend/src/RealtorApi/Program.cs
- [X] T022 [US2] Document that @redocly/cli and NSwag are CLI-only validation tools and not part of the runtime surface in specs/008-open-api/spec.md
- [X] T023 [US2] Confirm the app exposes only the static JSON contract and not any interactive documentation route in app/backend/src/RealtorApi/Program.cs

**Checkpoint**: At this point, User Stories 1 and 2 should both work independently.

---

## Phase 5: User Story 3 - Verificación, validación y protección ante drift (Priority: P3)

**Goal**: Guarantee the contract remains aligned with the current backend through automated validation and drift detection.

**Independent Test**: Run the generation script and the drift tests to ensure a mismatch between runtime endpoints and the contract is detected and fails the suite.

### Tests for User Story 3

- [X] T024 [P] [US3] Add a drift comparison between runtime EndpointDataSource and documented paths in app/backend/tests/RealtorApiTests/OpenApiContractTests.cs
- [X] T025 [P] [US3] Add a NSwag smoke-test generation step in app/backend/tests/RealtorApiTests/OpenApiSmokeTests.cs
- [X] T026 [P] [US3] Add a Redocly lint execution check in app/backend/tests/RealtorApiTests/OpenApiContractTests.cs

### Implementation for User Story 3

- [X] T027 [US3] Implement the path normalization rule for route constraints in app/backend/tests/RealtorApiTests/OpenApiContractTests.cs
- [X] T028 [US3] Add the reproducible validation flow in support/scripts/generate-openapi-v1.ps1 with build, lint, NSwag smoke test and success reporting
- [X] T029 [US3] Fail explicitly when the generated document is missing or any validation step fails in support/scripts/generate-openapi-v1.ps1
- [X] T030 [US3] Document the preflight tooling checks in specs/008-open-api/quickstart.md and specs/008-open-api/spec.md
- [X] T031 [US3] Confirm the generated v1 schema is reusable by clients without manual corrections in artifacts/openapi-client-smoke/

**Checkpoint**: All user stories should now be independently functional.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final reliability checks, documentation cleanup, and cross-feature validation for the OpenAPI contract.

- [X] T032 [P] Review and align the generated JSON output with the endpoint inventory and schema requirements in app/backend/src/RealtorApi/wwwroot/openapi/v1.json
- [X] T033 [P] Verify the contract still matches real runtime endpoints after the build and smoke test in app/backend/tests/RealtorApiTests/
- [X] T034 [P] Validate the feature documentation and quickstart steps are consistent across specs/008-open-api/spec.md, specs/008-open-api/plan.md and specs/008-open-api/quickstart.md
- [X] T035 Run the full OpenAPI generation and validation workflow via support/scripts/generate-openapi-v1.ps1
- [X] T036 Verify only the authorized read-only GET by id was added and no interactive documentation UI endpoints are exposed

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately.
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories.
- **User Stories (Phase 3+)**: Depend on Foundational completion.
- **Polish (Phase 6)**: Depends on all user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational - no dependency on other stories.
- **User Story 2 (P2)**: Can start after Foundational - can be implemented independently.
- **User Story 3 (P3)**: Can start after Foundational - can be implemented independently.

### Within Each User Story

- Tests (if included) must be written and fail before implementation.
- Endpoint metadata before final generation and smoke validation.
- Validation before drift detection or release readiness.

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel.
- All Foundational tasks marked [P] can run in parallel.
- User stories 1, 2, and 3 can be worked in parallel once the Foundational phase is complete.
- Test tasks inside each story marked [P] can run in parallel.

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "Add a runtime endpoint inventory test in app/backend/tests/RealtorApiTests/OpenApiContractTests.cs"
Task: "Add a Redocly lint validation test in app/backend/tests/RealtorApiTests/OpenApiContractTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: User Story 1.
4. Validate the generated contract independently.
5. Stop and confirm the runtime contract is complete before continuing.

### Incremental Delivery

1. Setup + Foundational -> ready to generate and serve the contract.
2. Add User Story 1 -> confirm endpoint coverage and schema exposure.
3. Add User Story 2 -> confirm runtime publication and no UI exposure.
4. Add User Story 3 -> confirm drift detection and reproducible validation.
5. Final polish and full run of the generation script.

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together.
2. Once foundational work is done:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. Final validation happens after all stories complete.

---

## Notes

- [P] tasks = different files, no dependencies.
- [Story] label maps each task to the corresponding story for traceability.
- Each user story remains independently testable and demonstrable.
- Validate the docs and test expectations before implementation changes.
- Avoid broad refactors; keep the feature limited to the OpenAPI contract and validation pipeline.

## Authorized Contract Completion

- [X] T037 [P] [US1] Add unit tests for the authorized get-by-id query in app/backend/tests/RealtorApiTests/GetPropertyByIdTests.cs
- [X] T038 [US1] Implement the read-only GetPropertyById slice in app/backend/src/RealtorApi/Features/Properties/GetPropertyById/
