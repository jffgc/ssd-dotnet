# Guía rápida de validación: OpenAPI v1 de RealtorApi

## Requisitos previos

Antes de implementar o validar la feature, se debe comprobar que el entorno cumple las precondiciones documentadas en la spec:

1. `node --version` >= 20
2. `npm --version`
3. `npx @redocly/cli --version`
4. `dotnet tool restore`
5. `dotnet tool list` para confirmar que `NSwag.ConsoleCore` está disponible

## Verificación del entorno

```powershell
node --version
npm --version
npx @redocly/cli --version
dotnet tool restore
dotnet tool list
```

Si falla cualquiera de estas comprobaciones, la validación debe detenerse con un error claro y no continuar con la generación del contrato.

## Generación y validación reproducible

El siguiente flujo debe ejecutarse desde la raíz del repositorio:

```powershell
# 1) build del backend para generar el documento OpenAPI
# 2) validación de schema con Redocly
# 3) smoke test de consumidor con NSwag
# 4) comprobación del artefacto resultado
```

La implementación real usa el script desde PowerShell en la raíz:

```powershell
.\support\scripts\generate-openapi-v1.ps1
```

## Validación funcional esperada

### 1. Documento generado

```powershell
Get-Content .\app\backend\src\RealtorApi\wwwroot\openapi\v1.json
```

Se espera un JSON válido con el documento OpenAPI v1.

### 2. Ruta HTTP accesible

```powershell
dotnet run --project .\app\backend\src\RealtorApi\RealtorApi.csproj
```

Y luego verificar:

```http
GET /openapi/v1.json
Accept: application/json
```

Resultado esperado:
- HTTP `200 OK`
- `Content-Type: application/json`

### 3. Validación Redocly

```powershell
npx @redocly/cli lint .\app\backend\src\RealtorApi\wwwroot\openapi\v1.json --config .\.redocly.yaml
```

Resultado esperado: sin errores de validación, salvo warnings permitidos por la configuración.

### 4. Smoke test con NSwag

```powershell
dotnet tool run nswag run support/scripts/openapi-v1.nswag.json
```

Resultado esperado: cliente C# tipado generado sin correcciones manuales del contrato.

## Resultado de éxito

La feature se considera validada si:
- `v1.json` existe y se sirve en `GET /openapi/v1.json`.
- la validación Redocly pasa con la configuración del repositorio.
- la prueba de drift no detecta desalineación.
- el cliente generado por NSwag es consumible sin ajustes manuales.
- los endpoints interactivos de Swagger, ReDoc y Scalar no están disponibles en runtime.
