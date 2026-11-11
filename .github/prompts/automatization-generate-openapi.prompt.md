---
name: automatization-generate-openapi
description: "Configura gobernanza y CI para regenerar/validar OpenAPI v1 en cada endpoint nuevo"
---


# Objetivo

Garantizar que, cada vez que se agregue o modifique un endpoint en
`app/backend/src/RealtorApi`, el documento `wwwroot/openapi/v1.json` se regenere,
se valide y no quede desincronizado. Esto se logra con dos capas:

1. Disciplina local trazable (tarea estándar en el template de tasks).
2. Barrera remota automática (workflow de GitHub Actions que bloquea merges).

Esto es configuración de gobernanza/infraestructura, NO una funcionalidad de la app.
NO crees una spec nueva para esto.

# Contexto ya existente (reutilizar, no reinventar)

- `RealtorApi.csproj` tiene `OpenApiGenerateDocuments=true`: cada `dotnet build`
  regenera `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` desde el código real.
- `support/scripts/generate-openapi-v1.ps1` hace build + Redocly lint + smoke NSwag.
- `OpenApiDriftTests` (en `app/backend/tests/RealtorApiTests`) falla si el runtime y
  el documento se desincronizan.
- SDK requerido: ver `global.json` (`11.0.100-preview.5.26302.115`).
- Tool local NSwag declarado en `dotnet-tools.json` (se restaura con `dotnet tool restore`).
- El script requiere `dotnet`, `node`, `npx` (Redocly vía `npx @redocly/cli`) y `.redocly.yaml`.

# Tarea 1 — Estandarizar la tarea en el template de tasks

Edita `.specify/templates/tasks-template.md`:

- En la fase final "Polish & Cross-Cutting Concerns", agrega una tarea obligatoria:
  `- [ ] TXXX Regenerar y validar contrato OpenAPI v1: ejecutar support/scripts/generate-openapi-v1.ps1 y versionar app/backend/src/RealtorApi/wwwroot/openapi/v1.json (obligatorio si la spec crea o modifica endpoints)`
- Agrega una nota breve, en español, aclarando que esta tarea es obligatoria en toda
  spec que toque el backend y que el cierre de la spec exige que `dotnet test` pase en
  verde (incluye `OpenApiDriftTests` como red de seguridad anti-drift).
- Respeta el estilo y el idioma español del template existente.

# Tarea 2 — Crear el workflow de GitHub Actions

Crea `.github/workflows/openapi-contract.yml` con estas características:

- Se dispara en `pull_request` hacia `main` y en `push` a `main`, más `workflow_dispatch`.
- Runner `ubuntu-latest` (pwsh ya viene preinstalado).
- Pasos:
  1. `actions/checkout`.
  2. `actions/setup-dotnet` usando `global-json-file: global.json` (SDK preview exacto).
  3. `actions/setup-node` (versión LTS, p. ej. 20) para que `npx @redocly/cli` funcione.
  4. `dotnet tool restore` (restaura NSwag).
  5. Ejecutar el script existente: `pwsh support/scripts/generate-openapi-v1.ps1`.
     Este paso valida build + Redocly + smoke NSwag.
  6. Ejecutar las pruebas: `dotnet test app/Realtor.sln` (incluye el drift test).
  7. Verificar que `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` no tenga cambios
     sin commitear tras la regeneración (fallar si `git diff --exit-code` detecta drift),
     para garantizar que el contrato versionado esté actualizado en el PR.
- El job debe fallar (y bloquear el merge) si cualquier paso falla.
- Comentarios del YAML en español.

# Restricciones

- Respeta la constitución del proyecto (`.specify/memory/constitution.md`).
- Todo archivo `.md` debe estar en español; los YAML pueden llevar comentarios en español.
- No modifiques la lógica del backend, ni el script `generate-openapi-v1.ps1`, ni las pruebas.
- No agregues frameworks ni dependencias nuevas.
- El workflow NO despliega nada: solo valida. El deploy queda fuera de alcance.

# Criterio de aceptación

1. `.specify/templates/tasks-template.md` contiene la tarea obligatoria de regeneración y
   validación de OpenAPI v1, con nota aclaratoria en español.
2. Existe `.github/workflows/openapi-contract.yml` que:
   - Se dispara en PR a `main`, push a `main` y manualmente.
   - Configura .NET desde `global.json` y Node.
   - Ejecuta `generate-openapi-v1.ps1`, `dotnet test` y verifica ausencia de drift en
     `v1.json` (git diff limpio).
   - Bloquea el merge si algo falla.
3. No se tocó código de aplicación, el script ni las pruebas.
4. Explica al final, en 3-5 líneas, cómo activar la protección de rama en GitHub para que
   este workflow sea un check requerido (Settings → Branches → Branch protection rules).