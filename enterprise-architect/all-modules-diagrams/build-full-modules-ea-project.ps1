param(
    [string]$OutputPath = "$PSScriptRoot\EduPlatform-All-Modules-Full-Diagrams.qea",
    [string]$BackendRoot = "$PSScriptRoot\..\..\backend"
)

$ErrorActionPreference = "Stop"

$BackendRoot = (Resolve-Path -LiteralPath $BackendRoot).Path
$ModulesRoot = Join-Path $BackendRoot "src\Modules"
$HostRoot = Join-Path $BackendRoot "src\Host"

$baseGenerator = Join-Path $PSScriptRoot "build-all-modules-ea-project.ps1"
if (-not (Test-Path -LiteralPath $baseGenerator)) {
    throw "Base generator not found: $baseGenerator"
}

& $baseGenerator -OutputPath $OutputPath -BackendModulesPath $ModulesRoot | Out-Null

$ModuleHostFiles = @{
    "Assignments"   = @("Controllers\AssignmentsController.cs")
    "Auth"          = @("Controllers\AuthController.cs", "Controllers\UsersController.cs", "Controllers\AdminUsersController.cs", "Controllers\PlatformSettingsController.cs")
    "Calendar"      = @("Controllers\CalendarController.cs")
    "Content"       = @("Controllers\LessonsController.cs", "Controllers\LessonBlocksController.cs", "Controllers\FilesController.cs", "Controllers\GlossaryController.cs")
    "Courses"       = @("Controllers\CoursesController.cs", "Controllers\ModulesController.cs", "Controllers\DisciplinesController.cs", "Controllers\CourseBuilderController.cs", "Controllers\CourseReviewsController.cs", "Controllers\AdminCoursesController.cs")
    "Grading"       = @("Controllers\GradesController.cs")
    "Messaging"     = @("Controllers\ChatsController.cs", "Controllers\MessagesController.cs")
    "Notifications" = @("Controllers\NotificationsController.cs")
    "Payments"      = @("Controllers\PaymentsController.cs", "Controllers\AdminPaymentsController.cs")
    "Progress"      = @("Controllers\ProgressController.cs", "Controllers\LessonProgressController.cs", "Controllers\ReportsController.cs", "Controllers\AdminStatsController.cs")
    "Scheduling"    = @("Controllers\ScheduleController.cs")
    "Tests"         = @("Controllers\TestsController.cs", "Controllers\QuestionsController.cs", "Controllers\AttemptsController.cs")
    "Tools"         = @()
}

$ModuleHostFolders = @{
    "Courses"  = @("Models\Courses")
    "Payments" = @("Models\Payments")
    "Progress" = @("Models\Reports", "Models\Admin")
}

$ModuleHostServices = @{
    "Courses" = @(
        "Services\CourseBuilderReadService.cs",
        "Services\CourseItemManagementService.cs",
        "Services\CourseItemSyncService.cs",
        "Services\CourseReviewService.cs"
    )
    "Auth" = @("Services\UserDeletionGuard.cs")
}

$ExternalTypes = @(
    "ControllerBase",
    "IMediator",
    "IRequest",
    "IRequestHandler",
    "AbstractValidator",
    "Profile",
    "DbContext",
    "IdentityUser",
    "IdentityRole",
    "IdentityDbContext",
    "UserManager",
    "RoleManager",
    "SignInManager",
    "Hub",
    "IHubContext",
    "MongoClient",
    "IMongoDatabase",
    "StripeClient",
    "SessionService",
    "PaymentIntentService",
    "Result",
    "Result<T>",
    "PagedResult",
    "PagedResult<T>",
    "BaseEntity",
    "IAuditableEntity"
)

function Get-RelativePath {
    param([string]$FullPath)

    if ($FullPath.StartsWith($BackendRoot)) {
        return $FullPath.Substring($BackendRoot.Length).TrimStart('\')
    }

    return $FullPath
}

function Get-CleanTypeName {
    param([string]$TypeText)

    if ([string]::IsNullOrWhiteSpace($TypeText)) {
        return ""
    }

    $t = $TypeText.Trim()
    $t = $t -replace '\?', ''
    $t = $t -replace '\[\]', ''
    $t = $t -replace '\bTask<(.+)>\b', '$1'
    $t = $t -replace '\bValueTask<(.+)>\b', '$1'
    $t = $t -replace '\bIActionResult\b', ''
    $t = $t -replace '\bActionResult<(.+)>\b', '$1'

    while ($t -match '^(ICollection|IReadOnlyCollection|IEnumerable|List|HashSet|DbSet|IQueryable|Result|PagedResult|Task|ValueTask|Nullable)<(.+)>$') {
        $t = $Matches[2].Trim()
    }

    if ($t.Contains(",")) {
        $t = $t.Split(",")[0].Trim()
    }

    if ($t.Contains(".")) {
        $parts = $t.Split(".")
        $t = $parts[$parts.Length - 1]
    }

    $t = $t -replace '<.*$', ''
    return $t.Trim()
}

function Get-TypeReferences {
    param([string]$Text)

    $refs = New-Object System.Collections.Generic.List[string]
    if ([string]::IsNullOrWhiteSpace($Text)) {
        return $refs
    }

    $matches = [regex]::Matches($Text, '[A-Za-z_][A-Za-z0-9_]*(?:<[^<>]+>)?')
    foreach ($m in $matches) {
        $name = Get-CleanTypeName $m.Value
        if (-not [string]::IsNullOrWhiteSpace($name)) {
            $refs.Add($name)
        }
    }

    return $refs
}

function Get-StereotypeFromPath {
    param([string]$FilePath, [string]$Kind, [string]$Name)

    $normalized = $FilePath.Replace('/', '\')
    if ($kind -eq "enum") { return "enum" }
    if ($kind -eq "interface") { return "interface" }
    if ($normalized -match '\\Controllers\\') { return "controller" }
    if ($normalized -match '\\DTOs\\' -or $normalized -match '\\Models\\') { return "dto" }
    if ($normalized -match '\\Commands\\' -and $Name -match 'CommandHandler$') { return "handler" }
    if ($normalized -match '\\Queries\\' -and $Name -match 'QueryHandler$') { return "handler" }
    if ($normalized -match '\\Commands\\' -and $Name -match 'CommandValidator$') { return "validator" }
    if ($normalized -match '\\Queries\\' -and $Name -match 'QueryValidator$') { return "validator" }
    if ($normalized -match '\\Commands\\' -and $Name -match 'Command$') { return "command" }
    if ($normalized -match '\\Queries\\' -and $Name -match 'Query$') { return "query" }
    if ($normalized -match '\\Queries\\' -and $Name -match 'Dto$') { return "dto" }
    if ($normalized -match '\\Interfaces\\') { return "interface" }
    if ($normalized -match '\\Services\\') { return "service" }
    if ($normalized -match '\\Mappings\\') { return "mapping" }
    if ($normalized -match '\\Persistence\\' -and $Name -match 'DbContext$') { return "dbcontext" }
    if ($normalized -match '\\Configuration\\') { return "configuration" }
    if ($normalized -match '\\Entities\\') { return "entity" }
    if ($normalized -match '\\ValueObjects\\') { return "value object" }
    if ($normalized -match '\\Domain\\') { return "domain" }
    if ($normalized -match '\\Infrastructure\\') { return "infrastructure" }
    if ($normalized -match '\\Application\\') { return "application" }
    return "class"
}

function Get-LayerFromPath {
    param([string]$FilePath)

    $normalized = $FilePath.Replace('/', '\')
    if ($normalized -match '\\Controllers\\') { return "API" }
    if ($normalized -match '\\Host\\Models\\' -or $normalized -match '\\Host\\Services\\') { return "API" }
    if ($normalized -match '\\Application\\') { return "Application" }
    if ($normalized -match '\\Domain\\') { return "Domain" }
    if ($normalized -match '\\Infrastructure\\') { return "Infrastructure" }
    return "Other"
}

function Get-TypeBody {
    param([string[]]$Lines, [int]$StartIndex)

    $bodyLines = New-Object System.Collections.Generic.List[string]
    $braceDepth = 0
    $started = $false

    for ($j = $StartIndex; $j -lt $Lines.Count; $j++) {
        $current = $Lines[$j]
        if ($current.Contains('{')) {
            $started = $true
        }

        if ($started) {
            $bodyLines.Add($current)
            $braceDepth += ([regex]::Matches($current, '\{')).Count
            $braceDepth -= ([regex]::Matches($current, '\}')).Count
            if ($braceDepth -eq 0) {
                break
            }
        }
        elseif ($current.Trim().EndsWith(";")) {
            $bodyLines.Add($current)
            break
        }
    }

    return $bodyLines
}

function Get-RecordConstructorParameters {
    param([string[]]$Lines, [int]$StartIndex)

    $text = ""
    for ($i = $StartIndex; $i -lt [Math]::Min($Lines.Count, $StartIndex + 20); $i++) {
        $text += " " + $Lines[$i].Trim()
        if ($text -match '\)\s*(?:;|:|\{|$)') {
            break
        }
    }

    $properties = @()
    if ($text -match 'record\s+[A-Za-z_][A-Za-z0-9_]*\s*\((.*)\)') {
        $params = $Matches[1]
        foreach ($part in ($params -split ',')) {
            $p = $part.Trim()
            if ($p -match '^(?:\[.+?\]\s*)?([A-Za-z_][A-Za-z0-9_<>,\?\[\]\. ]*)\s+([A-Za-z_][A-Za-z0-9_]*)$') {
                $properties += [pscustomobject]@{
                    Name = $Matches[2].Trim()
                    Type = $Matches[1].Trim()
                }
            }
        }
    }

    return $properties
}

function Parse-CSharpTypes {
    param(
        [string]$ModuleName,
        [System.IO.FileInfo[]]$Files
    )

    $types = @()
    foreach ($file in $Files) {
        if ($file.FullName -match '\\(bin|obj|Migrations)\\') { continue }
        if ($file.Name -match '\.Designer\.cs$' -or $file.Name -match 'ModelSnapshot\.cs$') { continue }

        $lines = Get-Content -LiteralPath $file.FullName
        $content = [string]::Join("`n", $lines)
        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = $lines[$i].Trim()
            if ($line -notmatch '^public\s+(?:(?:abstract|sealed|partial|static)\s+)*(class|record|interface|enum)\s+([A-Za-z_][A-Za-z0-9_]*)(?:\s*(?:\([^)]*\))?\s*:\s*([^{;]+))?') {
                continue
            }

            $kind = $Matches[1]
            $name = $Matches[2]
            $baseRaw = ""
            if ($Matches.Count -gt 3 -and $Matches[3]) {
                $baseRaw = $Matches[3].Trim()
            }

            $bodyLines = @(Get-TypeBody -Lines $lines -StartIndex $i)
            $properties = @()
            $enumValues = @()
            $methods = @()
            $bases = @()
            $references = New-Object System.Collections.Generic.HashSet[string]

            if (-not [string]::IsNullOrWhiteSpace($baseRaw)) {
                foreach ($refName in Get-TypeReferences $baseRaw) {
                    if ($refName -ne $name) {
                        $bases += $refName
                        $references.Add($refName) | Out-Null
                    }
                }
            }

            if ($kind -eq "record") {
                $properties += @(Get-RecordConstructorParameters -Lines $lines -StartIndex $i)
            }

            if ($kind -eq "enum") {
                foreach ($bodyLine in $bodyLines) {
                    $v = $bodyLine.Trim()
                    if ($v -eq "" -or $v.StartsWith("{") -or $v.StartsWith("}") -or $v.StartsWith("//")) { continue }
                    $v = $v.Split("//")[0].Trim().TrimEnd(",")
                    if ($v.Contains("=")) { $v = $v.Split("=")[0].Trim() }
                    if ($v -match '^[A-Za-z_][A-Za-z0-9_]*$') { $enumValues += $v }
                }
            }
            else {
                foreach ($bodyLine in $bodyLines) {
                    $p = $bodyLine.Trim()
                    if ($p -match '^public\s+(.+?)\s+([A-Za-z_][A-Za-z0-9_]*)\s*\{\s*get;\s*(?:init;|set;|private\s+set;)?') {
                        $properties += [pscustomobject]@{
                            Name = $Matches[2]
                            Type = $Matches[1].Trim()
                        }
                        foreach ($refName in Get-TypeReferences $Matches[1]) {
                            $references.Add($refName) | Out-Null
                        }
                    }
                    elseif ($p -match '^public\s+(?:async\s+)?(?:static\s+)?[A-Za-z_][A-Za-z0-9_<>,\?\[\]\. ]+\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(') {
                        $methodName = $Matches[1]
                        if ($methodName -ne $name -and $methods -notcontains $methodName) {
                            $methods += $methodName
                        }
                    }
                }
            }

            foreach ($refName in Get-TypeReferences $content) {
                $references.Add($refName) | Out-Null
            }

            $stereotype = Get-StereotypeFromPath -FilePath $file.FullName -Kind $kind -Name $name
            $types += [pscustomobject]@{
                Module = $ModuleName
                Name = $name
                Kind = $kind
                Stereotype = $stereotype
                Layer = Get-LayerFromPath $file.FullName
                Bases = @($bases | Select-Object -Unique)
                Properties = $properties
                EnumValues = $enumValues
                Methods = $methods
                References = @($references | Select-Object -Unique)
                File = $file.FullName
                RelativeFile = Get-RelativePath $file.FullName
            }
        }
    }

    return $types
}

function Get-ModuleFiles {
    param([string]$ModuleName)

    $files = New-Object System.Collections.Generic.List[System.IO.FileInfo]
    $modulePath = Join-Path $ModulesRoot $ModuleName
    if (Test-Path -LiteralPath $modulePath) {
        Get-ChildItem -LiteralPath $modulePath -Recurse -Filter *.cs |
            Where-Object { $_.FullName -notmatch '\\(bin|obj|Migrations)\\' -and $_.Name -notmatch '\.Designer\.cs$' -and $_.Name -notmatch 'ModelSnapshot\.cs$' } |
            ForEach-Object { $files.Add($_) }
    }

    if ($ModuleHostFiles.ContainsKey($ModuleName)) {
        foreach ($relative in $ModuleHostFiles[$ModuleName]) {
            $path = Join-Path $HostRoot $relative
            if (Test-Path -LiteralPath $path) {
                $files.Add((Get-Item -LiteralPath $path))
            }
        }
    }

    if ($ModuleHostFolders.ContainsKey($ModuleName)) {
        foreach ($relative in $ModuleHostFolders[$ModuleName]) {
            $path = Join-Path $HostRoot $relative
            if (Test-Path -LiteralPath $path) {
                Get-ChildItem -LiteralPath $path -Recurse -Filter *.cs | ForEach-Object { $files.Add($_) }
            }
        }
    }

    if ($ModuleHostServices.ContainsKey($ModuleName)) {
        foreach ($relative in $ModuleHostServices[$ModuleName]) {
            $path = Join-Path $HostRoot $relative
            if (Test-Path -LiteralPath $path) {
                $files.Add((Get-Item -LiteralPath $path))
            }
        }
    }

    return @($files | Sort-Object FullName -Unique)
}

$repo = New-Object -ComObject EA.Repository

function Get-PackageByName {
    param($ParentPackage, [string]$Name)

    for ($i = 0; $i -lt $ParentPackage.Packages.Count; $i++) {
        $pkg = $ParentPackage.Packages.GetAt($i)
        if ($pkg.Name -eq $Name) { return $pkg }
    }
    return $null
}

function Get-OrCreatePackage {
    param($ParentPackage, [string]$Name)

    $pkg = Get-PackageByName -ParentPackage $ParentPackage -Name $Name
    if ($null -ne $pkg) { return $pkg }

    $pkg = $ParentPackage.Packages.AddNew($Name, "Package")
    $pkg.Update() | Out-Null
    $ParentPackage.Packages.Refresh()
    return $pkg
}

function Get-OrCreateElement {
    param(
        $Package,
        [string]$Name,
        [string]$Type = "Class",
        [string]$Stereotype = "",
        [string]$Notes = ""
    )

    for ($i = 0; $i -lt $Package.Elements.Count; $i++) {
        $existing = $Package.Elements.GetAt($i)
        if ($existing.Name -eq $Name) {
            if ($Stereotype -and [string]::IsNullOrWhiteSpace($existing.Stereotype)) {
                $existing.Stereotype = $Stereotype
                $existing.Update() | Out-Null
            }
            return $existing
        }
    }

    $el = $Package.Elements.AddNew($Name, $Type)
    if ($Stereotype) { $el.Stereotype = $Stereotype }
    if ($Notes) { $el.Notes = $Notes }
    $el.Update() | Out-Null
    $Package.Elements.Refresh()
    return $el
}

function Add-AttributeIfMissing {
    param($Element, [string]$Name, [string]$Type)

    if ([string]::IsNullOrWhiteSpace($Name)) { return }
    for ($i = 0; $i -lt $Element.Attributes.Count; $i++) {
        $attr = $Element.Attributes.GetAt($i)
        if ($attr.Name -eq $Name) { return }
    }

    $newAttr = $Element.Attributes.AddNew($Name, $Type)
    $newAttr.Visibility = "Public"
    $newAttr.Update() | Out-Null
    $Element.Attributes.Refresh()
}

function Add-MethodIfMissing {
    param($Element, [string]$Name)

    if ([string]::IsNullOrWhiteSpace($Name)) { return }
    for ($i = 0; $i -lt $Element.Methods.Count; $i++) {
        $method = $Element.Methods.GetAt($i)
        if ($method.Name -eq $Name) { return }
    }

    $newMethod = $Element.Methods.AddNew($Name, "")
    $newMethod.Visibility = "Public"
    $newMethod.Update() | Out-Null
    $Element.Methods.Refresh()
}

function Add-ConnectorIfMissing {
    param(
        $Client,
        $Supplier,
        [string]$Type,
        [string]$Name = "",
        [string]$ClientCardinality = "",
        [string]$SupplierCardinality = ""
    )

    if ($null -eq $Client -or $null -eq $Supplier) { return }
    if ($Client.ElementID -eq $Supplier.ElementID) { return }

    for ($i = 0; $i -lt $Client.Connectors.Count; $i++) {
        $existing = $Client.Connectors.GetAt($i)
        if ($existing.SupplierID -eq $Supplier.ElementID -and $existing.Type -eq $Type -and $existing.Name -eq $Name) {
            return
        }
    }

    $connector = $Client.Connectors.AddNew($Name, $Type)
    $connector.SupplierID = $Supplier.ElementID
    if ($ClientCardinality) { $connector.ClientEnd.Cardinality = $ClientCardinality }
    if ($SupplierCardinality) { $connector.SupplierEnd.Cardinality = $SupplierCardinality }
    $connector.Update() | Out-Null
    $Client.Connectors.Refresh()
}

function Delete-DiagramIfExists {
    param($Package, [string]$Name)

    for ($i = $Package.Diagrams.Count - 1; $i -ge 0; $i--) {
        $diagram = $Package.Diagrams.GetAt($i)
        if ($diagram.Name -eq $Name) {
            $Package.Diagrams.DeleteAt($i, $false)
            $Package.Diagrams.Refresh()
        }
    }
}

function Add-DiagramObject {
    param($Diagram, $Element, [int]$Left, [int]$Top, [int]$Right, [int]$Bottom)

    $obj = $Diagram.DiagramObjects.AddNew("l=$Left;r=$Right;t=$Top;b=$Bottom;", "")
    $obj.ElementID = $Element.ElementID
    $obj.Update() | Out-Null
    $Diagram.DiagramObjects.Refresh()
}

function Get-EATypeFromKind {
    param([string]$Kind)

    switch ($Kind) {
        "enum" { return "Enumeration" }
        "interface" { return "Interface" }
        default { return "Class" }
    }
}

function Add-TypeToPackage {
    param($Package, $TypeInfo)

    $element = Get-OrCreateElement -Package $Package -Name $TypeInfo.Name -Type (Get-EATypeFromKind $TypeInfo.Kind) -Stereotype $TypeInfo.Stereotype -Notes $TypeInfo.RelativeFile

    if ($TypeInfo.Kind -eq "enum") {
        foreach ($value in $TypeInfo.EnumValues) {
            Add-AttributeIfMissing -Element $element -Name $value -Type ""
        }
    }
    else {
        foreach ($property in $TypeInfo.Properties) {
            Add-AttributeIfMissing -Element $element -Name $property.Name -Type $property.Type
        }
        foreach ($method in $TypeInfo.Methods) {
            Add-MethodIfMissing -Element $element -Name $method
        }
    }

    return $element
}

function Add-GroupToDiagram {
    param(
        $Diagram,
        [object[]]$Elements,
        [int]$StartX,
        [int]$StartY,
        [int]$Cols,
        [int]$Width = 300
    )

    if ($Elements.Count -eq 0) { return }

    $xGap = $Width + 70
    $yGap = 210
    for ($idx = 0; $idx -lt $Elements.Count; $idx++) {
        $el = $Elements[$idx]
        $row = [Math]::Floor($idx / $Cols)
        $col = $idx % $Cols
        $left = $StartX + ($col * $xGap)
        $top = $StartY + ($row * $yGap)
        $visibleMembers = $el.Attributes.Count + $el.Methods.Count
        $height = [Math]::Min(360, [Math]::Max(100, 80 + ($visibleMembers * 16)))
        Add-DiagramObject -Diagram $Diagram -Element $el -Left $left -Top $top -Right ($left + $Width) -Bottom ($top + $height)
    }
}

function Select-ElementsByStereotype {
    param($ElementMap, [string[]]$Stereotypes)

    $result = @()
    foreach ($key in $ElementMap.Keys) {
        $entry = $ElementMap[$key]
        if ($Stereotypes -contains $entry.TypeInfo.Stereotype) {
            $result += $entry.Element
        }
    }
    return @($result | Sort-Object Name)
}

try {
    $repo.OpenFile($OutputPath) | Out-Null
    $root = $repo.Models.GetAt(0)
    $sharedPkg = Get-OrCreatePackage -ParentPackage $root -Name "Shared"

    $moduleDirs = Get-ChildItem -LiteralPath $ModulesRoot -Directory | Sort-Object Name
    foreach ($moduleDir in $moduleDirs) {
        $moduleName = $moduleDir.Name
        $modulePackage = Get-OrCreatePackage -ParentPackage $root -Name $moduleName

        $files = @(Get-ModuleFiles -ModuleName $moduleName)
        $types = @(Parse-CSharpTypes -ModuleName $moduleName -Files $files)
        if ($types.Count -eq 0) { continue }

        $elementMap = @{}
        foreach ($typeInfo in ($types | Sort-Object Layer, Stereotype, Name)) {
            if ($elementMap.ContainsKey($typeInfo.Name)) { continue }
            $element = Add-TypeToPackage -Package $modulePackage -TypeInfo $typeInfo
            $elementMap[$typeInfo.Name] = [pscustomobject]@{
                Element = $element
                TypeInfo = $typeInfo
            }
        }

        $externalMap = @{}
        foreach ($externalName in $ExternalTypes) {
            if ($elementMap.ContainsKey($externalName)) { continue }
            $allRefs = $types | Where-Object { $_.References -contains $externalName -or $_.Bases -contains $externalName }
            if ($allRefs.Count -gt 0) {
                $externalElement = Get-OrCreateElement -Package $modulePackage -Name $externalName -Type "Class" -Stereotype "external"
                $externalMap[$externalName] = $externalElement
            }
        }

        $allElementsByName = @{}
        foreach ($key in $elementMap.Keys) { $allElementsByName[$key] = $elementMap[$key].Element }
        foreach ($key in $externalMap.Keys) { $allElementsByName[$key] = $externalMap[$key] }

        foreach ($typeInfo in $types) {
            if (-not $elementMap.ContainsKey($typeInfo.Name)) { continue }
            $client = $elementMap[$typeInfo.Name].Element

            foreach ($baseName in $typeInfo.Bases) {
                if (-not $allElementsByName.ContainsKey($baseName)) { continue }
                $supplier = $allElementsByName[$baseName]
                $connectorType = if ($baseName.StartsWith("I") -or $baseName -eq "AbstractValidator") { "Realisation" } else { "Generalization" }
                Add-ConnectorIfMissing -Client $client -Supplier $supplier -Type $connectorType
            }

            foreach ($property in $typeInfo.Properties) {
                $targetName = Get-CleanTypeName $property.Type
                if ($targetName -eq $typeInfo.Name) { continue }
                if ($allElementsByName.ContainsKey($targetName)) {
                    $supplier = $allElementsByName[$targetName]
                    Add-ConnectorIfMissing -Client $client -Supplier $supplier -Type "Association" -Name $property.Name
                }
            }

            foreach ($refName in $typeInfo.References) {
                if ($refName -eq $typeInfo.Name) { continue }
                if ($allElementsByName.ContainsKey($refName)) {
                    $supplier = $allElementsByName[$refName]
                    $supplierStereotype = "external"
                    if ($elementMap.ContainsKey($refName)) {
                        $supplierStereotype = $elementMap[$refName].TypeInfo.Stereotype
                    }

                    $shouldDepend = $false
                    if ($typeInfo.Stereotype -eq "controller" -and @("command", "query", "api request", "dto", "external") -contains $supplierStereotype) {
                        $shouldDepend = $true
                    }
                    elseif ($typeInfo.Stereotype -eq "handler" -and @("command", "query", "interface", "service", "dbcontext", "dto", "entity", "value object", "external") -contains $supplierStereotype) {
                        $shouldDepend = $true
                    }
                    elseif ($typeInfo.Stereotype -eq "validator" -and @("command", "query", "external") -contains $supplierStereotype) {
                        $shouldDepend = $true
                    }
                    elseif (@("service", "dbcontext", "configuration", "infrastructure") -contains $typeInfo.Stereotype -and @("interface", "entity", "value object", "domain", "external") -contains $supplierStereotype) {
                        $shouldDepend = $true
                    }
                    elseif ($typeInfo.Stereotype -eq "mapping" -and @("dto", "entity", "value object", "domain", "external") -contains $supplierStereotype) {
                        $shouldDepend = $true
                    }

                    if ($shouldDepend) {
                        Add-ConnectorIfMissing -Client $client -Supplier $supplier -Type "Dependency"
                    }
                }
            }

            if ($typeInfo.Name -match '^(.*)(Command|Query)Handler$') {
                $requestName = $Matches[1] + $Matches[2]
                if ($allElementsByName.ContainsKey($requestName)) {
                    Add-ConnectorIfMissing -Client $client -Supplier $allElementsByName[$requestName] -Type "Dependency" -Name "handles"
                }
            }
            elseif ($typeInfo.Name -match '^(.*)(Command|Query)Validator$') {
                $requestName = $Matches[1] + $Matches[2]
                if ($allElementsByName.ContainsKey($requestName)) {
                    Add-ConnectorIfMissing -Client $client -Supplier $allElementsByName[$requestName] -Type "Dependency" -Name "validates"
                }
            }
        }

        $diagramName = "$moduleName Full Class Diagram"
        Delete-DiagramIfExists -Package $modulePackage -Name $diagramName
        $diagram = $modulePackage.Diagrams.AddNew($diagramName, "Class")
        $diagram.Update() | Out-Null
        $modulePackage.Diagrams.Refresh()

        $api = Select-ElementsByStereotype -ElementMap $elementMap -Stereotypes @("controller", "api request")
        $cqrs = Select-ElementsByStereotype -ElementMap $elementMap -Stereotypes @("command", "query", "handler", "validator")
        $app = Select-ElementsByStereotype -ElementMap $elementMap -Stereotypes @("dto", "interface", "service", "mapping", "application")
        $domain = Select-ElementsByStereotype -ElementMap $elementMap -Stereotypes @("entity", "value object", "domain", "enum")
        $infra = Select-ElementsByStereotype -ElementMap $elementMap -Stereotypes @("dbcontext", "configuration", "infrastructure")
        $other = Select-ElementsByStereotype -ElementMap $elementMap -Stereotypes @("class")
        $external = @($externalMap.Values | Sort-Object Name)

        Add-GroupToDiagram -Diagram $diagram -Elements $api -StartX 60 -StartY 80 -Cols 3 -Width 320
        Add-GroupToDiagram -Diagram $diagram -Elements $cqrs -StartX 60 -StartY 950 -Cols 5 -Width 320
        Add-GroupToDiagram -Diagram $diagram -Elements $app -StartX 60 -StartY 2850 -Cols 5 -Width 320
        Add-GroupToDiagram -Diagram $diagram -Elements $domain -StartX 60 -StartY 4600 -Cols 5 -Width 320
        Add-GroupToDiagram -Diagram $diagram -Elements $infra -StartX 60 -StartY 6800 -Cols 5 -Width 320
        Add-GroupToDiagram -Diagram $diagram -Elements $other -StartX 60 -StartY 7900 -Cols 5 -Width 320
        Add-GroupToDiagram -Diagram $diagram -Elements $external -StartX 60 -StartY 8900 -Cols 5 -Width 320

        $diagram.Update() | Out-Null
        $repo.SaveDiagram($diagram.DiagramID) | Out-Null
        Write-Output "${moduleName}: full diagram objects=$($api.Count + $cqrs.Count + $app.Count + $domain.Count + $infra.Count + $other.Count + $external.Count)"
    }

    $repo.SaveAllDiagrams() | Out-Null
    $repo.CloseFile()
    Write-Output "Created: $OutputPath"
}
finally {
    if ($null -ne $repo) {
        $repo.Exit()
    }
}
