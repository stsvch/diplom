param(
    [switch]$StopDocker
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$statePath = Join-Path $repoRoot '.tmp\local-dev\state.json'

function Write-Step {
    param([string]$Message)
    Write-Host "==> $Message" -ForegroundColor Cyan
}

if (Test-Path $statePath) {
    $state = Get-Content $statePath -Raw | ConvertFrom-Json
    foreach ($processInfo in $state.processes) {
        if ($null -eq $processInfo.pid) {
            continue
        }

        $running = Get-Process -Id $processInfo.pid -ErrorAction SilentlyContinue
        if ($null -eq $running) {
            continue
        }

        Write-Step "Останавливаю $($processInfo.name) (PID $($processInfo.pid))"
        & taskkill /PID $processInfo.pid /T /F | Out-Null
    }

    Remove-Item $statePath -Force -ErrorAction SilentlyContinue
} else {
    Write-Step 'Файл состояния launcher не найден, управляемые процессы не зарегистрированы.'
}

if ($StopDocker) {
    Write-Step 'Останавливаю инфраструктуру через docker compose down'
    Push-Location $repoRoot
    try {
        & docker compose down
        if ($LASTEXITCODE -ne 0) {
            throw 'docker compose down завершился с ошибкой.'
        }
    } finally {
        Pop-Location
    }
}

Write-Host 'Локальный dev-стек остановлен.' -ForegroundColor Green
