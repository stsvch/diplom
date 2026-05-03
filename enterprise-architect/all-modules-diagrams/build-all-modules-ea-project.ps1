param(
    [string]$OutputPath = "$PSScriptRoot\EduPlatform-All-Modules-Diagrams.qea",
    [string]$BackendModulesPath = "$PSScriptRoot\..\..\backend\src\Modules"
)

$ErrorActionPreference = "Stop"

$eaBase = "C:\Program Files (x86)\Sparx Systems\EA Trial\EABase.qea"
if (-not (Test-Path -LiteralPath $eaBase)) {
    throw "EABase.qea not found at $eaBase"
}

$BackendModulesPath = (Resolve-Path -LiteralPath $BackendModulesPath).Path
$outputDir = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
Copy-Item -LiteralPath $eaBase -Destination $OutputPath -Force

function Get-TypeNameFromPropertyType {
    param([string]$Type)

    $t = $Type.Trim()
    $t = $t -replace '\?', ''
    $t = $t -replace '\[\]', ''

    if ($t -match '^(ICollection|IReadOnlyCollection|IEnumerable|List|HashSet)<(.+)>$') {
        $t = $Matches[2].Trim()
    }

    if ($t.Contains('.')) {
        $parts = $t.Split('.')
        $t = $parts[$parts.Length - 1]
    }

    return $t.Trim()
}

function Test-IsCollectionType {
    param([string]$Type)
    return $Type.Trim() -match '^(ICollection|IReadOnlyCollection|IEnumerable|List|HashSet)<'
}

function Parse-CSharpDomainTypes {
    param([string]$ModuleName, [string]$DomainPath)

    $types = @()
    $files = Get-ChildItem -LiteralPath $DomainPath -Recurse -Filter *.cs |
        Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }

    foreach ($file in $files) {
        $lines = Get-Content -LiteralPath $file.FullName
        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = $lines[$i].Trim()
            if ($line -notmatch '^public\s+(abstract\s+|sealed\s+|partial\s+)?(class|record|interface|enum)\s+([A-Za-z_][A-Za-z0-9_]*)(?:\s*:\s*([^{]+))?') {
                continue
            }

            $modifier = ""
            if ($Matches[1]) {
                $modifier = $Matches[1].Trim()
            }
            $kind = $Matches[2]
            $name = $Matches[3]
            $baseRaw = ""
            if ($Matches[4]) {
                $baseRaw = $Matches[4].Trim()
            }
            $bases = @()
            if (-not [string]::IsNullOrWhiteSpace($baseRaw)) {
                $bases = $baseRaw.Split(',') | ForEach-Object { $_.Trim() -replace '<.*$', '' } | Where-Object { $_ }
            }

            $bodyLines = New-Object System.Collections.Generic.List[string]
            $braceDepth = 0
            $started = $false

            for ($j = $i; $j -lt $lines.Count; $j++) {
                $current = $lines[$j]
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
            }

            $properties = @()
            $enumValues = @()

            if ($kind -eq 'enum') {
                foreach ($bodyLine in $bodyLines) {
                    $v = $bodyLine.Trim()
                    if ($v -eq '' -or $v.StartsWith('{') -or $v.StartsWith('}') -or $v.StartsWith('//')) {
                        continue
                    }
                    $v = $v.Split('//')[0].Trim()
                    $v = $v.TrimEnd(',')
                    if ($v.Contains('=')) {
                        $v = $v.Split('=')[0].Trim()
                    }
                    if ($v -match '^[A-Za-z_][A-Za-z0-9_]*$') {
                        $enumValues += $v
                    }
                }
            }
            else {
                foreach ($bodyLine in $bodyLines) {
                    $p = $bodyLine.Trim()
                    if ($p -match '^public\s+(.+?)\s+([A-Za-z_][A-Za-z0-9_]*)\s*\{\s*get;\s*set;\s*\}') {
                        $properties += [pscustomobject]@{
                            Name = $Matches[2]
                            Type = $Matches[1].Trim()
                        }
                    }
                }
            }

            $types += [pscustomobject]@{
                Module = $ModuleName
                Name = $name
                Kind = $kind
                IsAbstract = $modifier -match 'abstract'
                Bases = $bases
                Properties = $properties
                EnumValues = $enumValues
                File = $file.FullName
            }
        }
    }

    return $types
}

$repo = New-Object -ComObject EA.Repository

function Add-AttributeToElement {
    param($Element, [string]$Name, [string]$Type)
    $attr = $Element.Attributes.AddNew($Name, $Type)
    $attr.Visibility = "Public"
    $attr.Update() | Out-Null
    $Element.Attributes.Refresh()
}

function Add-Element {
    param($Package, $TypeInfo)

    $eaKind = switch ($TypeInfo.Kind) {
        'enum' { 'Enumeration' }
        'interface' { 'Interface' }
        default { 'Class' }
    }

    $el = $Package.Elements.AddNew($TypeInfo.Name, $eaKind)
    if ($TypeInfo.IsAbstract) {
        $el.Abstract = "1"
    }
    $el.Update() | Out-Null

    if ($TypeInfo.Kind -eq 'enum') {
        foreach ($value in $TypeInfo.EnumValues) {
            Add-AttributeToElement -Element $el -Name $value -Type ""
        }
    }
    else {
        foreach ($prop in $TypeInfo.Properties) {
            Add-AttributeToElement -Element $el -Name $prop.Name -Type $prop.Type
        }
    }

    $Package.Elements.Refresh()
    return $el
}

function Add-Connector {
    param(
        $Client,
        $Supplier,
        [string]$Type,
        [string]$Name = "",
        [string]$ClientCardinality = "",
        [string]$SupplierCardinality = "",
        [int]$ClientAggregation = 0
    )

    $connector = $Client.Connectors.AddNew($Name, $Type)
    $connector.SupplierID = $Supplier.ElementID
    if ($ClientCardinality) { $connector.ClientEnd.Cardinality = $ClientCardinality }
    if ($SupplierCardinality) { $connector.SupplierEnd.Cardinality = $SupplierCardinality }
    if ($ClientAggregation -gt 0) { $connector.ClientEnd.Aggregation = $ClientAggregation }
    $connector.Update() | Out-Null
    $Client.Connectors.Refresh()
}

function Add-DiagramObject {
    param($Diagram, $Element, [int]$Left, [int]$Top, [int]$Right, [int]$Bottom)
    $obj = $Diagram.DiagramObjects.AddNew("l=$Left;r=$Right;t=$Top;b=$Bottom;", "")
    $obj.ElementID = $Element.ElementID
    $obj.Update() | Out-Null
    $Diagram.DiagramObjects.Refresh()
}

try {
    $repo.OpenFile($OutputPath) | Out-Null

    while ($repo.Models.Count -gt 0) {
        $repo.Models.DeleteAt(0, $false)
        $repo.Models.Refresh()
    }

    $root = $repo.Models.AddNew("EduPlatform", "Package")
    $root.Update() | Out-Null
    $repo.Models.Refresh()

    $sharedPkg = $root.Packages.AddNew("Shared", "Package")
    $sharedPkg.Update() | Out-Null
    $root.Packages.Refresh()

    $baseTypeInfo = [pscustomobject]@{
        Name = "BaseEntity"
        Kind = "class"
        IsAbstract = $true
        Bases = @()
        Properties = @([pscustomobject]@{ Name = "Id"; Type = "Guid" })
        EnumValues = @()
    }
    $auditableTypeInfo = [pscustomobject]@{
        Name = "IAuditableEntity"
        Kind = "interface"
        IsAbstract = $false
        Bases = @()
        Properties = @(
            [pscustomobject]@{ Name = "CreatedAt"; Type = "DateTime" },
            [pscustomobject]@{ Name = "UpdatedAt"; Type = "DateTime?" }
        )
        EnumValues = @()
    }

    $globalElements = @{}
    $globalElements["BaseEntity"] = Add-Element -Package $sharedPkg -TypeInfo $baseTypeInfo
    $globalElements["IAuditableEntity"] = Add-Element -Package $sharedPkg -TypeInfo $auditableTypeInfo

    $moduleDirs = Get-ChildItem -LiteralPath $BackendModulesPath -Directory | Sort-Object Name
    foreach ($moduleDir in $moduleDirs) {
        $moduleName = $moduleDir.Name
        $domainPath = Join-Path $moduleDir.FullName "$moduleName.Domain"
        if (-not (Test-Path -LiteralPath $domainPath)) {
            continue
        }

        $types = @(Parse-CSharpDomainTypes -ModuleName $moduleName -DomainPath $domainPath)
        if ($types.Count -eq 0) {
            continue
        }

        $pkg = $root.Packages.AddNew($moduleName, "Package")
        $pkg.Update() | Out-Null
        $root.Packages.Refresh()

        $elementMap = @{}
        foreach ($typeInfo in ($types | Sort-Object Kind, Name)) {
            if ($elementMap.ContainsKey($typeInfo.Name)) {
                continue
            }
            $elementMap[$typeInfo.Name] = Add-Element -Package $pkg -TypeInfo $typeInfo
        }

        $connectorKeys = New-Object 'System.Collections.Generic.HashSet[string]'

        foreach ($typeInfo in $types) {
            if (-not $elementMap.ContainsKey($typeInfo.Name)) { continue }
            $client = $elementMap[$typeInfo.Name]

            foreach ($base in $typeInfo.Bases) {
                $baseName = Get-TypeNameFromPropertyType $base
                $supplier = $null
                if ($elementMap.ContainsKey($baseName)) {
                    $supplier = $elementMap[$baseName]
                }
                elseif ($globalElements.ContainsKey($baseName)) {
                    $supplier = $globalElements[$baseName]
                }

                if ($null -eq $supplier) { continue }

                $connectorType = if ($baseName.StartsWith("I")) { "Realisation" } else { "Generalization" }
                $key = "$($client.ElementID)-$connectorType-$($supplier.ElementID)"
                if ($connectorKeys.Add($key)) {
                    Add-Connector -Client $client -Supplier $supplier -Type $connectorType
                }
            }

            foreach ($prop in $typeInfo.Properties) {
                $targetName = Get-TypeNameFromPropertyType $prop.Type
                if ($targetName -eq $typeInfo.Name) { continue }

                if ($elementMap.ContainsKey($targetName)) {
                    $supplier = $elementMap[$targetName]
                    $isCollection = Test-IsCollectionType $prop.Type
                    $supplierCardinality = if ($isCollection) { "0..*" } elseif ($prop.Type.Contains("?")) { "0..1" } else { "1" }
                    $key = "$($client.ElementID)-Association-$($supplier.ElementID)-$($prop.Name)"
                    if ($connectorKeys.Add($key)) {
                        Add-Connector -Client $client -Supplier $supplier -Type "Association" -Name $prop.Name -ClientCardinality "1" -SupplierCardinality $supplierCardinality
                    }
                }
                elseif ($types.Where({ $_.Name -eq $targetName -and $_.Kind -eq 'enum' }).Count -gt 0) {
                    $supplier = $elementMap[$targetName]
                    $key = "$($client.ElementID)-Dependency-$($supplier.ElementID)-$($prop.Name)"
                    if ($connectorKeys.Add($key)) {
                        Add-Connector -Client $client -Supplier $supplier -Type "Dependency"
                    }
                }
            }
        }

        $diagram = $pkg.Diagrams.AddNew("$moduleName Domain Class Diagram", "Class")
        $diagram.Update() | Out-Null
        $pkg.Diagrams.Refresh()

        $diagramElements = New-Object System.Collections.Generic.List[object]
        $usesBase = $types.Where({ $_.Bases -contains "BaseEntity" }).Count -gt 0
        $usesAuditable = $types.Where({ $_.Bases -contains "IAuditableEntity" }).Count -gt 0

        if ($usesBase) { $diagramElements.Add($globalElements["BaseEntity"]) }
        if ($usesAuditable) { $diagramElements.Add($globalElements["IAuditableEntity"]) }

        foreach ($typeInfo in ($types | Sort-Object @{Expression={ if ($_.Kind -eq 'enum') { 2 } elseif ($_.Kind -eq 'interface') { 0 } else { 1 } }}, Name)) {
            if ($elementMap.ContainsKey($typeInfo.Name)) {
                $diagramElements.Add($elementMap[$typeInfo.Name])
            }
        }

        $cols = if ($diagramElements.Count -gt 28) { 5 } elseif ($diagramElements.Count -gt 16) { 4 } else { 3 }
        $xGap = 360
        $yGap = 300
        $startX = 80
        $startY = 80
        $width = 300

        for ($idx = 0; $idx -lt $diagramElements.Count; $idx++) {
            $el = $diagramElements[$idx]
            $row = [math]::Floor($idx / $cols)
            $col = $idx % $cols
            $left = $startX + ($col * $xGap)
            $top = $startY + ($row * $yGap)
            $attrCount = $el.Attributes.Count
            $height = [Math]::Min(420, [Math]::Max(110, 70 + ($attrCount * 18)))
            Add-DiagramObject -Diagram $diagram -Element $el -Left $left -Top $top -Right ($left + $width) -Bottom ($top + $height)
        }

        $diagram.Update() | Out-Null
        $repo.SaveDiagram($diagram.DiagramID) | Out-Null
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
