Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot | Split-Path -Parent
$projectPath = Join-Path $repoRoot 'app/backend/src/RealtorApi/RealtorApi.csproj'
$projectDirectory = Split-Path -Parent $projectPath
$assemblyPath = Join-Path $projectDirectory 'bin/Debug/net11.0/RealtorApi.dll'
$outputPath = Join-Path $projectDirectory 'wwwroot/openapi/v1.json'
$redoclyConfigPath = Join-Path $repoRoot '.redocly.yaml'
$nswagConfigPath = Join-Path $repoRoot 'support/scripts/openapi-v1.nswag.json'
$clientOutputFile = Join-Path $repoRoot 'artifacts/openapi-client-smoke/RealtorApiClient.cs'

function Assert-CommandAvailable {
    param([Parameter(Mandatory = $true)][string]$Command)

    if (-not (Get-Command $Command -ErrorAction SilentlyContinue)) {
        throw "Prerequisite missing: '$Command' is not available in PATH."
    }
}

function Invoke-HttpGet {
    param(
        [Parameter(Mandatory = $true)][System.Net.Http.HttpClient]$Client,
        [Parameter(Mandatory = $true)][string]$Uri
    )

    return $Client.GetAsync($Uri).GetAwaiter().GetResult()
}

Push-Location $repoRoot
$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
$process = $null
Add-Type -AssemblyName System.Net.Http
$httpClient = [System.Net.Http.HttpClient]::new()
$httpClient.Timeout = [TimeSpan]::FromSeconds(3)
try {
    Assert-CommandAvailable -Command 'dotnet'
    Assert-CommandAvailable -Command 'node'
    Assert-CommandAvailable -Command 'npm'
    Assert-CommandAvailable -Command 'npx'

    $nodeVersion = (node --version).TrimStart('v')
    if ([version]$nodeVersion -lt [version]'20.0') {
        throw "Node.js 20+ is required; found $nodeVersion."
    }
    if (-not (Test-Path $redoclyConfigPath)) {
        throw "Required Redocly configuration is missing: $redoclyConfigPath"
    }
    if (-not (Test-Path $nswagConfigPath)) {
        throw "Required NSwag configuration is missing: $nswagConfigPath"
    }

    Write-Host '[1/5] Building RealtorApi and generating OpenAPI v1...'
    dotnet build $projectPath --nologo
    if ($LASTEXITCODE -ne 0) {
        throw 'RealtorApi build or OpenAPI generation failed.'
    }
    if (-not (Test-Path $outputPath)) {
        throw "OpenAPI document was not generated: $outputPath"
    }

    $document = Get-Content $outputPath -Raw | ConvertFrom-Json
    if (-not $document.openapi -or -not $document.paths) {
        throw "Generated OpenAPI document is invalid or has no paths: $outputPath"
    }

    Write-Host '[2/5] Checking the static HTTP route and UI exclusions...'
    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, 0)
    $listener.Start()
    $port = $listener.LocalEndpoint.Port
    $listener.Stop()
    $baseUrl = "http://127.0.0.1:$port"
    $env:ASPNETCORE_ENVIRONMENT = 'OpenApiGeneration'
    $process = Start-Process -FilePath 'dotnet' `
        -ArgumentList "`"$assemblyPath`" --urls $baseUrl" `
        -WorkingDirectory $projectDirectory `
        -PassThru -NoNewWindow

    $openApiResponse = $null
    for ($attempt = 0; $attempt -lt 40; $attempt++) {
        if ($process.HasExited) {
            throw 'RealtorApi exited before the OpenAPI smoke test could run.'
        }
        try {
            $openApiResponse = Invoke-HttpGet -Client $httpClient -Uri "$baseUrl/openapi/v1.json"
            if ($openApiResponse.StatusCode -eq [System.Net.HttpStatusCode]::OK) {
                break
            }
            $openApiResponse.Dispose()
            $openApiResponse = $null
        }
        catch {
        }
        Start-Sleep -Milliseconds 500
    }
    if ($null -eq $openApiResponse) {
        throw "GET $baseUrl/openapi/v1.json did not return a response."
    }
    if ($openApiResponse.Content.Headers.ContentType.MediaType -ne 'application/json') {
        throw "OpenAPI route returned unexpected content type: $($openApiResponse.Content.Headers.ContentType)"
    }
    $openApiResponse.Dispose()

    foreach ($uiPath in @('/swagger', '/swagger/index.html', '/redoc', '/scalar/v1')) {
        $uiResponse = Invoke-HttpGet -Client $httpClient -Uri "$baseUrl$uiPath"
        $uiStatus = $uiResponse.StatusCode
        $uiResponse.Dispose()
        if ($uiStatus -ne [System.Net.HttpStatusCode]::NotFound) {
            throw "Interactive documentation route '$uiPath' is unexpectedly exposed ($uiStatus)."
        }
    }

    Write-Host '[3/5] Validating the generated document with Redocly...'
    & npx --yes @redocly/cli lint --config $redoclyConfigPath $outputPath
    if ($LASTEXITCODE -ne 0) {
        throw 'Redocly validation failed.'
    }

    Write-Host '[4/5] Restoring and verifying the local NSwag tool...'
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet tool restore failed.'
    }
    $toolList = (dotnet tool list --local) -join "`n"
    if ($LASTEXITCODE -ne 0 -or $toolList -notmatch 'nswag\.consolecore') {
        throw 'NSwag.ConsoleCore is not available from the local tool manifest.'
    }

    Write-Host '[5/5] Generating a typed client with NSwag...'
    & dotnet tool run nswag run $nswagConfigPath
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path $clientOutputFile)) {
        throw "NSwag failed to generate the smoke client: $clientOutputFile"
    }

    Write-Host 'OpenAPI v1 generated and validated successfully.'
    Write-Host "Document: $outputPath"
    Write-Host "Smoke client: $clientOutputFile"
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
    $httpClient.Dispose()
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
    Pop-Location
}