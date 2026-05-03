param(
    [switch]$KeepDocker
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$statePath = Join-Path $repoRoot '.tmp\app\state.json'
$composeFile = Join-Path $repoRoot 'docker-compose.local.yml'

function Write-Step {
    param([string]$Message)
    Write-Host "==> $Message" -ForegroundColor Cyan
}

if (Test-Path $statePath) {
    $state = Get-Content $statePath -Raw | ConvertFrom-Json
    foreach ($processInfo in $state.processes) {
        if ($null -eq $processInfo.pid) { continue }
        $running = Get-Process -Id $processInfo.pid -ErrorAction SilentlyContinue
        if ($null -eq $running) { continue }

        Write-Step "Останавливаю $($processInfo.name)"
        & taskkill /PID $processInfo.pid /T /F | Out-Null
    }

    Remove-Item $statePath -Force -ErrorAction SilentlyContinue
} else {
    Write-Step 'Локальные процессы launcher не зарегистрированы.'
}

if (-not $KeepDocker) {
    Write-Step 'Останавливаю Docker-сервисы'
    Push-Location $repoRoot
    try {
        & docker compose -f $composeFile down
        if ($LASTEXITCODE -ne 0) {
            throw 'docker compose down завершился с ошибкой.'
        }
    } finally {
        Pop-Location
    }
}

Write-Host 'Приложение остановлено.' -ForegroundColor Green
