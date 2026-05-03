param(
    [switch]$NoBrowser,
    [switch]$StartBackend,
    [switch]$StartFrontend,
    [int]$ReadyTimeoutSeconds = 180
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$stateDir = Join-Path $repoRoot '.tmp\app'
$logsDir = Join-Path $stateDir 'logs'
$statePath = Join-Path $stateDir 'state.json'
$composeFile = Join-Path $repoRoot 'docker-compose.local.yml'
$hostLocalConfigPath = Join-Path $repoRoot 'backend\src\Host\appsettings.Development.Local.json'
$frontendDir = Join-Path $repoRoot 'frontend'
$tmpDir = Join-Path $repoRoot 'tmp'

New-Item -ItemType Directory -Force -Path $logsDir | Out-Null
New-Item -ItemType Directory -Force -Path $tmpDir | Out-Null

function Write-Step {
    param([string]$Message)
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Get-CommandPath {
    param([string]$Name)
    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if ($null -eq $command) { return $null }
    return $command.Source
}

function Load-DotEnv {
    param([string]$Path)
    $result = [ordered]@{}
    if (-not (Test-Path $Path)) { return $result }

    foreach ($line in Get-Content $Path) {
        $trimmed = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed) -or $trimmed.StartsWith('#')) { continue }
        $separatorIndex = $trimmed.IndexOf('=')
        if ($separatorIndex -lt 1) { continue }
        $key = $trimmed.Substring(0, $separatorIndex).Trim()
        $value = $trimmed.Substring($separatorIndex + 1).Trim()
        if ($value.Length -ge 2) {
            if (($value.StartsWith('"') -and $value.EndsWith('"')) -or ($value.StartsWith("'") -and $value.EndsWith("'"))) {
                $value = $value.Substring(1, $value.Length - 2)
            }
        }
        $result[$key] = $value
    }

    return $result
}

function Ensure-DockerReady {
    $dockerPath = Get-CommandPath 'docker'
    if ([string]::IsNullOrWhiteSpace($dockerPath)) {
        throw 'Docker CLI не найден. Установи Docker Desktop.'
    }

    & $dockerPath info *> $null
    if ($LASTEXITCODE -eq 0) { return $dockerPath }

    $dockerDesktopPath = Join-Path $env:ProgramFiles 'Docker\Docker\Docker Desktop.exe'
    if (Test-Path $dockerDesktopPath) {
        Write-Step 'Запускаю Docker Desktop'
        Start-Process -FilePath $dockerDesktopPath | Out-Null
        $deadline = (Get-Date).AddMinutes(3)
        while ((Get-Date) -lt $deadline) {
            Start-Sleep -Seconds 3
            & $dockerPath info *> $null
            if ($LASTEXITCODE -eq 0) { return $dockerPath }
        }
    }

    throw 'Docker daemon недоступен. Запусти Docker Desktop и повтори.'
}

function Stop-ExistingProcesses {
    if (-not (Test-Path $statePath)) { return }

    try {
        $state = Get-Content $statePath -Raw | ConvertFrom-Json
        foreach ($processInfo in $state.processes) {
            if ($null -eq $processInfo.pid) { continue }
            $running = Get-Process -Id $processInfo.pid -ErrorAction SilentlyContinue
            if ($null -ne $running) {
                Write-Step "Останавливаю предыдущий процесс $($processInfo.name)"
                & taskkill /PID $processInfo.pid /T /F | Out-Null
            }
        }
    } catch {
        Write-Warning "Не удалось прочитать прошлое состояние launcher: $($_.Exception.Message)"
    }

    Remove-Item $statePath -Force -ErrorAction SilentlyContinue
}

function Wait-ForFilePattern {
    param(
        [string]$Path,
        [string]$Pattern,
        [int]$TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if (Test-Path $Path) {
            $content = Get-Content $Path -Raw -ErrorAction SilentlyContinue
            if ($content -match $Pattern) { return }
        }
        Start-Sleep -Milliseconds 500
    }

    throw "Файл $Path не появился или не содержит нужную конфигурацию за $TimeoutSeconds сек."
}

function Wait-ForHttpReady {
    param(
        [string]$Url,
        [int]$TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        try {
            $response = Invoke-WebRequest -Uri $Url -Method Get -UseBasicParsing -TimeoutSec 5
            if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) { return }
        } catch {
            Start-Sleep -Seconds 2
        }
    }

    throw "Сервис не стал доступен по адресу $Url за $TimeoutSeconds сек."
}

function Start-ManagedProcess {
    param(
        [string]$Name,
        [string]$FilePath,
        [string[]]$ArgumentList,
        [string]$WorkingDirectory,
        [hashtable]$EnvironmentOverrides,
        [string]$StdOutPath,
        [string]$StdErrPath
    )

    foreach ($path in @($StdOutPath, $StdErrPath)) {
        if (Test-Path $path) { Remove-Item $path -Force }
    }

    $process = Start-Process `
        -FilePath $FilePath `
        -ArgumentList $ArgumentList `
        -WorkingDirectory $WorkingDirectory `
        -Environment $EnvironmentOverrides `
        -RedirectStandardOutput $StdOutPath `
        -RedirectStandardError $StdErrPath `
        -PassThru `
        -WindowStyle Hidden

    if ($null -eq $process) {
        throw "Не удалось запустить процесс $Name."
    }

    return $process
}

$dotenv = Load-DotEnv -Path (Join-Path $repoRoot '.env')
if (-not $dotenv.Contains('STRIPE_SECRET_KEY') -or [string]::IsNullOrWhiteSpace($dotenv['STRIPE_SECRET_KEY'])) {
    throw 'В .env не задан STRIPE_SECRET_KEY. Для запуска со Stripe нужен тестовый sk_test_... ключ.'
}

Stop-ExistingProcesses

Write-Step 'Проверяю Docker'
$dockerPath = Ensure-DockerReady

Write-Step 'Поднимаю PostgreSQL, MongoDB, MinIO и Stripe listener в Docker'
Push-Location $repoRoot
try {
    if (Test-Path $hostLocalConfigPath) {
        Remove-Item $hostLocalConfigPath -Force
    }

    & $dockerPath compose -f $composeFile up -d postgres mongo minio
    if ($LASTEXITCODE -ne 0) { throw 'Не удалось поднять Docker-инфраструктуру.' }

    & $dockerPath compose -f $composeFile up -d --force-recreate stripe-listen
    if ($LASTEXITCODE -ne 0) { throw 'Не удалось запустить Stripe listener.' }
} finally {
    Pop-Location
}

Write-Step 'Жду Stripe webhook secret'
try {
    Wait-ForFilePattern -Path $hostLocalConfigPath -Pattern 'whsec_[A-Za-z0-9]+' -TimeoutSeconds 60
} catch {
    Write-Host ''
    Write-Host 'Последние логи Stripe listener:' -ForegroundColor Yellow
    & $dockerPath compose -f $composeFile logs --tail=40 stripe-listen
    throw
}

$managedProcesses = New-Object System.Collections.Generic.List[object]

if ($StartBackend) {
    $dotnetPath = Get-CommandPath 'dotnet'
    if ([string]::IsNullOrWhiteSpace($dotnetPath)) {
        throw '.NET SDK не найден. Для локального backend нужен .NET SDK 10.'
    }

    $backendStdOut = Join-Path $logsDir 'backend.out.log'
    $backendStdErr = Join-Path $logsDir 'backend.err.log'
    $backendEnv = @{
        ASPNETCORE_ENVIRONMENT = 'Development'
        TEMP = $tmpDir
        TMP = $tmpDir
    }

    Write-Step 'Запускаю backend'
    $backendProcess = Start-ManagedProcess `
        -Name 'backend' `
        -FilePath $dotnetPath `
        -ArgumentList @('run', '--project', 'backend/src/Host/EduPlatform.Host.csproj', '--launch-profile', 'http') `
        -WorkingDirectory $repoRoot `
        -EnvironmentOverrides $backendEnv `
        -StdOutPath $backendStdOut `
        -StdErrPath $backendStdErr

    $managedProcesses.Add([pscustomobject]@{
            name = 'backend'
            pid = $backendProcess.Id
            stdout = $backendStdOut
            stderr = $backendStdErr
        }) | Out-Null

    Wait-ForHttpReady -Url 'http://localhost:5000/swagger/index.html' -TimeoutSeconds $ReadyTimeoutSeconds
}

if ($StartFrontend) {
    $npmPath = Get-CommandPath 'npm'
    if ([string]::IsNullOrWhiteSpace($npmPath)) {
        throw 'npm не найден. Для локального frontend нужен Node.js.'
    }

    if (-not (Test-Path (Join-Path $frontendDir 'node_modules'))) {
        Write-Step 'Ставлю frontend-зависимости'
        Push-Location $frontendDir
        try {
            & $npmPath ci
            if ($LASTEXITCODE -ne 0) { throw 'npm ci завершился с ошибкой.' }
        } finally {
            Pop-Location
        }
    }

    $frontendStdOut = Join-Path $logsDir 'frontend.out.log'
    $frontendStdErr = Join-Path $logsDir 'frontend.err.log'

    Write-Step 'Запускаю frontend'
    $frontendProcess = Start-ManagedProcess `
        -Name 'frontend' `
        -FilePath $npmPath `
        -ArgumentList @('start') `
        -WorkingDirectory $frontendDir `
        -EnvironmentOverrides @{} `
        -StdOutPath $frontendStdOut `
        -StdErrPath $frontendStdErr

    $managedProcesses.Add([pscustomobject]@{
            name = 'frontend'
            pid = $frontendProcess.Id
            stdout = $frontendStdOut
            stderr = $frontendStdErr
        }) | Out-Null

    Wait-ForHttpReady -Url 'http://localhost:4200' -TimeoutSeconds $ReadyTimeoutSeconds
}

$state = [pscustomobject]@{
    startedAt = (Get-Date).ToString('o')
    frontendUrl = 'http://localhost:4200'
    backendUrl = 'http://localhost:5000'
    composeFile = $composeFile
    processes = $managedProcesses
}
$state | ConvertTo-Json -Depth 6 | Set-Content -Path $statePath -Encoding UTF8

Write-Host ''
Write-Host 'Docker-сервисы и Stripe listener запущены.' -ForegroundColor Green
Write-Host 'Stripe config записан в backend/src/Host/appsettings.Development.Local.json'
Write-Host 'Теперь можно вручную запускать backend и frontend.'
Write-Host 'Backend:  .\start-backend.cmd'
Write-Host 'Frontend: .\start-frontend.cmd'
Write-Host ''
Write-Host 'После ручного запуска:'
Write-Host 'Frontend: http://localhost:4200'
Write-Host 'Swagger:  http://localhost:5000/swagger'
Write-Host 'MinIO:    http://localhost:9101'
Write-Host "Логи:     $logsDir"
Write-Host 'Остановка: .\stop-app.cmd'

if (-not $NoBrowser -and $StartFrontend) {
    Start-Process 'http://localhost:4200' | Out-Null
}
