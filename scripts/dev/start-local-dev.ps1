param(
    [switch]$NoBrowser,
    [switch]$SkipDocker,
    [switch]$SkipFrontend,
    [switch]$SkipStripe,
    [switch]$SkipBackend,
    [switch]$SkipStripeCliInstall,
    [int]$ReadyTimeoutSeconds = 120
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$stateDir = Join-Path $repoRoot '.tmp\local-dev'
$logsDir = Join-Path $stateDir 'logs'
$statePath = Join-Path $stateDir 'state.json'
$envPath = Join-Path $repoRoot '.env'
$frontendDir = Join-Path $repoRoot 'frontend'
$hostLocalConfigPath = Join-Path $repoRoot 'backend\src\Host\appsettings.Development.Local.json'

New-Item -ItemType Directory -Force -Path $logsDir | Out-Null

function Write-Step {
    param([string]$Message)
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Load-DotEnv {
    param([string]$Path)

    $result = [ordered]@{}
    if (-not (Test-Path $Path)) {
        return $result
    }

    foreach ($line in Get-Content $Path) {
        $trimmed = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed) -or $trimmed.StartsWith('#')) {
            continue
        }

        $separatorIndex = $trimmed.IndexOf('=')
        if ($separatorIndex -lt 1) {
            continue
        }

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

function Get-CommandPath {
    param([string]$Name)

    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if ($null -eq $command) {
        return $null
    }

    return $command.Source
}

function Wait-ForRegexInFiles {
    param(
        [string[]]$Paths,
        [string]$Pattern,
        [int]$TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        foreach ($path in $Paths) {
            if (-not (Test-Path $path)) {
                continue
            }

            $content = Get-Content $path -Raw -ErrorAction SilentlyContinue
            if ([string]::IsNullOrWhiteSpace($content)) {
                continue
            }

            $match = [regex]::Match($content, $Pattern)
            if ($match.Success) {
                return $match.Value
            }
        }

        Start-Sleep -Milliseconds 500
    }

    return $null
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
            if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) {
                return
            }
        } catch {
            Start-Sleep -Seconds 2
            continue
        }
    }

    throw "Сервис не стал доступен по адресу $Url за $TimeoutSeconds сек."
}

function Write-HostLocalStripeConfig {
    param(
        [string]$Path,
        [string]$SecretKey,
        [string]$WebhookSecret,
        [string]$Country
    )

    $config = [ordered]@{
        Stripe = [ordered]@{
            SecretKey = $SecretKey
            WebhookSecret = $WebhookSecret
            Country = $Country
        }
    }

    $json = $config | ConvertTo-Json -Depth 4
    Set-Content -Path $Path -Value $json -Encoding UTF8
}

function Stop-ExistingProcesses {
    param([string]$StateFile)

    if (-not (Test-Path $StateFile)) {
        return
    }

    try {
        $state = Get-Content $StateFile -Raw | ConvertFrom-Json
        foreach ($processInfo in $state.processes) {
            if ($null -eq $processInfo.pid) {
                continue
            }

            $running = Get-Process -Id $processInfo.pid -ErrorAction SilentlyContinue
            if ($null -ne $running) {
                Write-Step "Останавливаю предыдущий процесс $($processInfo.name) (PID $($processInfo.pid))"
                & taskkill /PID $processInfo.pid /T /F | Out-Null
            }
        }
    } catch {
        Write-Warning "Не удалось корректно прочитать прошлое состояние launcher: $($_.Exception.Message)"
    }

    Remove-Item $StateFile -Force -ErrorAction SilentlyContinue
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
        if (Test-Path $path) {
            Remove-Item $path -Force
        }
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

function Ensure-DockerReady {
    $dockerPath = Get-CommandPath 'docker'
    if ([string]::IsNullOrWhiteSpace($dockerPath)) {
        throw 'Docker CLI не найден. Установи Docker Desktop.'
    }

    & $dockerPath info *> $null
    if ($LASTEXITCODE -eq 0) {
        return $dockerPath
    }

    $dockerDesktopPath = Join-Path $env:ProgramFiles 'Docker\Docker\Docker Desktop.exe'
    if (Test-Path $dockerDesktopPath) {
        Write-Step 'Запускаю Docker Desktop'
        Start-Process -FilePath $dockerDesktopPath | Out-Null
        $deadline = (Get-Date).AddMinutes(2)
        while ((Get-Date) -lt $deadline) {
            Start-Sleep -Seconds 3
            & $dockerPath info *> $null
            if ($LASTEXITCODE -eq 0) {
                return $dockerPath
            }
        }
    }

    throw 'Docker daemon недоступен. Запусти Docker Desktop и повтори.'
}

function Ensure-StripeCli {
    param([switch]$AllowInstall)

    $stripePath = Get-CommandPath 'stripe'
    if (-not [string]::IsNullOrWhiteSpace($stripePath)) {
        return $stripePath
    }

    if (-not $AllowInstall) {
        throw 'Stripe CLI не найден. Разреши автоустановку или установи его один раз вручную.'
    }

    $wingetPath = Get-CommandPath 'winget'
    if ([string]::IsNullOrWhiteSpace($wingetPath)) {
        throw 'Stripe CLI не найден, а winget недоступен для автоустановки.'
    }

    Write-Step 'Устанавливаю Stripe CLI через winget'
    & $wingetPath install --id Stripe.StripeCLI -e --accept-package-agreements --accept-source-agreements
    if ($LASTEXITCODE -ne 0) {
        throw 'Не удалось установить Stripe CLI через winget.'
    }

    $stripePath = Get-CommandPath 'stripe'
    if ([string]::IsNullOrWhiteSpace($stripePath)) {
        throw 'Stripe CLI установлен, но не найден в PATH. Перезапусти терминал и повтори.'
    }

    return $stripePath
}

$dotenv = Load-DotEnv -Path $envPath

$backendUrl = if ($dotenv.Contains('BACKEND_URL')) { $dotenv['BACKEND_URL'] } else { 'http://localhost:5000' }
$frontendUrl = if ($dotenv.Contains('FRONTEND_URL')) { $dotenv['FRONTEND_URL'] } else { 'http://localhost:4200' }
$stripeSecretKey = if ($env:Stripe__SecretKey) {
    $env:Stripe__SecretKey
} elseif ($env:STRIPE_SECRET_KEY) {
    $env:STRIPE_SECRET_KEY
} elseif ($dotenv.Contains('STRIPE_SECRET_KEY')) {
    $dotenv['STRIPE_SECRET_KEY']
} else {
    $null
}
$stripeCountry = if ($dotenv.Contains('STRIPE_COUNTRY')) { $dotenv['STRIPE_COUNTRY'] } else { 'US' }

$managedProcesses = New-Object System.Collections.Generic.List[object]

Stop-ExistingProcesses -StateFile $statePath

if (-not $SkipDocker) {
    Write-Step 'Проверяю Docker'
    $dockerPath = Ensure-DockerReady
    Write-Step 'Поднимаю инфраструктуру через docker compose'
    Push-Location $repoRoot
    try {
        & $dockerPath compose up -d
        if ($LASTEXITCODE -ne 0) {
            throw 'docker compose up -d завершился с ошибкой.'
        }
    } finally {
        Pop-Location
    }
}

if (-not $SkipFrontend) {
    $nodeModulesPath = Join-Path $frontendDir 'node_modules'
    if (-not (Test-Path $nodeModulesPath)) {
        Write-Step 'Ставлю frontend-зависимости через npm ci'
        $npmInstallPath = Get-CommandPath 'npm'
        if ([string]::IsNullOrWhiteSpace($npmInstallPath)) {
            throw 'npm не найден. Установи Node.js.'
        }

        Push-Location $frontendDir
        try {
            & $npmInstallPath ci
            if ($LASTEXITCODE -ne 0) {
                throw 'npm ci завершился с ошибкой.'
            }
        } finally {
            Pop-Location
        }
    }
}

$stripeWebhookSecret = $null
if (-not $SkipStripe) {
    if ([string]::IsNullOrWhiteSpace($stripeSecretKey)) {
        throw "Не найден STRIPE_SECRET_KEY. Добавь его в .env или в переменные окружения."
    }

    $stripePath = Ensure-StripeCli -AllowInstall:(-not $SkipStripeCliInstall)
    $stripeStdOut = Join-Path $logsDir 'stripe.out.log'
    $stripeStdErr = Join-Path $logsDir 'stripe.err.log'
    $webhookUrl = '{0}/api/payments/webhooks/stripe' -f $backendUrl.TrimEnd('/')

    Write-Step "Запускаю Stripe CLI forwarding на $webhookUrl"
    $stripeProcess = Start-ManagedProcess `
        -Name 'stripe-listen' `
        -FilePath $stripePath `
        -ArgumentList @('listen', '--api-key', $stripeSecretKey, '--forward-to', $webhookUrl) `
        -WorkingDirectory $repoRoot `
        -EnvironmentOverrides @{} `
        -StdOutPath $stripeStdOut `
        -StdErrPath $stripeStdErr

    $stripeWebhookSecret = Wait-ForRegexInFiles `
        -Paths @($stripeStdOut, $stripeStdErr) `
        -Pattern 'whsec_[A-Za-z0-9]+' `
        -TimeoutSeconds 30

    if ([string]::IsNullOrWhiteSpace($stripeWebhookSecret)) {
        try {
            & taskkill /PID $stripeProcess.Id /T /F | Out-Null
        } catch {
        }

        throw "Stripe CLI не выдал webhook secret. Проверь логи: $stripeStdOut и $stripeStdErr"
    }

    Write-HostLocalStripeConfig `
        -Path $hostLocalConfigPath `
        -SecretKey $stripeSecretKey `
        -WebhookSecret $stripeWebhookSecret `
        -Country $stripeCountry

    $managedProcesses.Add([pscustomobject]@{
            name = 'stripe-listen'
            pid = $stripeProcess.Id
            stdout = $stripeStdOut
            stderr = $stripeStdErr
        }) | Out-Null
}

$sharedEnvironment = @{}
foreach ($key in $dotenv.Keys) {
    $sharedEnvironment[$key] = $dotenv[$key]
}

if (-not $SkipBackend) {
    $dotnetPath = Get-CommandPath 'dotnet'
    if ([string]::IsNullOrWhiteSpace($dotnetPath)) {
        throw '.NET SDK не найден.'
    }

    $backendStdOut = Join-Path $logsDir 'backend.out.log'
    $backendStdErr = Join-Path $logsDir 'backend.err.log'
    $backendEnv = @{}
    foreach ($key in $sharedEnvironment.Keys) {
        $backendEnv[$key] = $sharedEnvironment[$key]
    }

    $backendEnv['ASPNETCORE_ENVIRONMENT'] = 'Development'
    if (-not [string]::IsNullOrWhiteSpace($stripeSecretKey)) {
        $backendEnv['Stripe__SecretKey'] = $stripeSecretKey
    }
    if (-not [string]::IsNullOrWhiteSpace($stripeWebhookSecret)) {
        $backendEnv['Stripe__WebhookSecret'] = $stripeWebhookSecret
    }
    if (-not [string]::IsNullOrWhiteSpace($stripeCountry)) {
        $backendEnv['Stripe__Country'] = $stripeCountry
    }

    Write-Step 'Запускаю backend на http://localhost:5000'
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

    Wait-ForHttpReady -Url ('{0}/swagger/index.html' -f $backendUrl.TrimEnd('/')) -TimeoutSeconds $ReadyTimeoutSeconds
}

if (-not $SkipFrontend) {
    $npmPath = Get-CommandPath 'npm'
    if ([string]::IsNullOrWhiteSpace($npmPath)) {
        throw 'npm не найден. Установи Node.js.'
    }

    $frontendStdOut = Join-Path $logsDir 'frontend.out.log'
    $frontendStdErr = Join-Path $logsDir 'frontend.err.log'

    Write-Step 'Запускаю frontend на http://localhost:4200'
    $frontendProcess = Start-ManagedProcess `
        -Name 'frontend' `
        -FilePath $npmPath `
        -ArgumentList @('start') `
        -WorkingDirectory $frontendDir `
        -EnvironmentOverrides $sharedEnvironment `
        -StdOutPath $frontendStdOut `
        -StdErrPath $frontendStdErr

    $managedProcesses.Add([pscustomobject]@{
            name = 'frontend'
            pid = $frontendProcess.Id
            stdout = $frontendStdOut
            stderr = $frontendStdErr
        }) | Out-Null

    Wait-ForHttpReady -Url $frontendUrl -TimeoutSeconds $ReadyTimeoutSeconds
}

$state = [pscustomobject]@{
    startedAt = (Get-Date).ToString('o')
    backendUrl = $backendUrl
    frontendUrl = $frontendUrl
    webhookUrl = if ($SkipStripe) { $null } else { '{0}/api/payments/webhooks/stripe' -f $backendUrl.TrimEnd('/') }
    stripeWebhookSecret = $stripeWebhookSecret
    processes = $managedProcesses
}

$state | ConvertTo-Json -Depth 6 | Set-Content -Path $statePath -Encoding UTF8

Write-Host ''
Write-Host 'Локальный dev-стек запущен.' -ForegroundColor Green
Write-Host "Frontend: $frontendUrl"
Write-Host "Backend:  $backendUrl"
if (-not [string]::IsNullOrWhiteSpace($stripeWebhookSecret)) {
    Write-Host "Stripe webhook secret подхвачен автоматически и прокинут в backend."
    Write-Host "Локальный конфиг для IDE/Aspire: $hostLocalConfigPath"
}
Write-Host "Логи:     $logsDir"
Write-Host "Остановка: .\\stop-local-dev.cmd"

if (-not $NoBrowser -and -not $SkipFrontend) {
    Start-Process $frontendUrl | Out-Null
}
