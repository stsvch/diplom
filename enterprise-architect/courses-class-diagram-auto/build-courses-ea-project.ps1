param(
    [string]$OutputPath = "$PSScriptRoot\EduPlatform-Courses-Diagram.qea"
)

$ErrorActionPreference = "Stop"

$eaBase = "C:\Program Files (x86)\Sparx Systems\EA Trial\EABase.qea"
if (-not (Test-Path -LiteralPath $eaBase)) {
    throw "EABase.qea not found at $eaBase"
}

$outputDir = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
Copy-Item -LiteralPath $eaBase -Destination $OutputPath -Force

$repo = New-Object -ComObject EA.Repository

function Add-Attribute {
    param($Element, [string]$Name, [string]$Type)

    $attr = $Element.Attributes.AddNew($Name, $Type)
    $attr.Visibility = "Public"
    $attr.Update() | Out-Null
    $Element.Attributes.Refresh()
}

function Add-Class {
    param($Package, [string]$Name, [string[]]$Attributes = @(), [string]$Kind = "Class", [bool]$Abstract = $false)

    $el = $Package.Elements.AddNew($Name, $Kind)
    if ($Abstract) {
        $el.Abstract = "1"
    }
    $el.Update() | Out-Null
    foreach ($line in $Attributes) {
        $parts = $line.Split(":", 2)
        Add-Attribute -Element $el -Name $parts[0].Trim() -Type $parts[1].Trim()
    }
    $Package.Elements.Refresh()
    return $el
}

function Add-Enum {
    param($Package, [string]$Name, [string[]]$Values)

    $el = $Package.Elements.AddNew($Name, "Enumeration")
    $el.Update() | Out-Null
    foreach ($value in $Values) {
        Add-Attribute -Element $el -Name $value -Type ""
    }
    $Package.Elements.Refresh()
    return $el
}

function Add-Generalization {
    param($Child, $Parent)

    $connector = $Child.Connectors.AddNew("", "Generalization")
    $connector.SupplierID = $Parent.ElementID
    $connector.Update() | Out-Null
    $Child.Connectors.Refresh()
}

function Add-Realization {
    param($Client, $Supplier)

    $connector = $Client.Connectors.AddNew("", "Realisation")
    $connector.SupplierID = $Supplier.ElementID
    $connector.Update() | Out-Null
    $Client.Connectors.Refresh()
}

function Add-Association {
    param(
        $Client,
        $Supplier,
        [string]$Name,
        [string]$ClientCardinality,
        [string]$SupplierCardinality,
        [int]$ClientAggregation = 0
    )

    $connector = $Client.Connectors.AddNew($Name, "Association")
    $connector.SupplierID = $Supplier.ElementID
    $connector.ClientEnd.Cardinality = $ClientCardinality
    $connector.SupplierEnd.Cardinality = $SupplierCardinality
    $connector.ClientEnd.Aggregation = $ClientAggregation
    $connector.Update() | Out-Null
    $Client.Connectors.Refresh()
}

function Add-Dependency {
    param($Client, $Supplier)

    $connector = $Client.Connectors.AddNew("", "Dependency")
    $connector.SupplierID = $Supplier.ElementID
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

    $model = $repo.Models.AddNew("EduPlatform", "Package")
    $model.Update() | Out-Null
    $repo.Models.Refresh()

    $pkg = $model.Packages.AddNew("Courses Module", "Package")
    $pkg.Update() | Out-Null
    $model.Packages.Refresh()

    $baseEntity = Add-Class -Package $pkg -Name "BaseEntity" -Kind "Class" -Abstract $true -Attributes @(
        "Id: Guid"
    )

    $auditable = Add-Class -Package $pkg -Name "IAuditableEntity" -Kind "Interface" -Attributes @(
        "CreatedAt: DateTime",
        "UpdatedAt: DateTime?"
    )

    $courseLevel = Add-Enum -Package $pkg -Name "CourseLevel" -Values @("Beginner", "Intermediate", "Advanced")
    $courseOrderType = Add-Enum -Package $pkg -Name "CourseOrderType" -Values @("Sequential", "Free")
    $enrollmentStatus = Add-Enum -Package $pkg -Name "EnrollmentStatus" -Values @("Active", "Completed", "Dropped")
    $courseItemType = Add-Enum -Package $pkg -Name "CourseItemType" -Values @("Lesson", "Test", "Assignment", "LiveSession", "Resource", "ExternalLink")
    $courseItemStatus = Add-Enum -Package $pkg -Name "CourseItemStatus" -Values @("Draft", "NeedsContent", "Ready", "Published", "Archived")
    $lessonLayout = Add-Enum -Package $pkg -Name "LessonLayout" -Values @("Scroll", "Stepper")

    $discipline = Add-Class -Package $pkg -Name "Discipline" -Attributes @(
        "Name: string",
        "Description: string?",
        "ImageUrl: string?",
        "CreatedAt: DateTime",
        "UpdatedAt: DateTime?"
    )

    $course = Add-Class -Package $pkg -Name "Course" -Attributes @(
        "DisciplineId: Guid",
        "TeacherId: string",
        "TeacherName: string",
        "Title: string",
        "Description: string",
        "Price: decimal?",
        "IsFree: bool",
        "IsPublished: bool",
        "IsArchived: bool",
        "ArchiveReason: string?",
        "ArchivedBy: string?",
        "OrderType: CourseOrderType",
        "HasGrading: bool",
        "HasCertificate: bool",
        "Deadline: DateTime?",
        "ImageUrl: string?",
        "Level: CourseLevel",
        "Tags: string?",
        "RatingAverage: double?",
        "RatingCount: int",
        "ReviewsCount: int",
        "CreatedAt: DateTime",
        "UpdatedAt: DateTime?"
    )

    $courseModule = Add-Class -Package $pkg -Name "CourseModule" -Attributes @(
        "CourseId: Guid",
        "Title: string",
        "Description: string?",
        "OrderIndex: int",
        "IsPublished: bool"
    )

    $lesson = Add-Class -Package $pkg -Name "Lesson" -Attributes @(
        "ModuleId: Guid",
        "Title: string",
        "Description: string?",
        "OrderIndex: int",
        "IsPublished: bool",
        "Duration: int?",
        "Layout: LessonLayout"
    )

    $courseItem = Add-Class -Package $pkg -Name "CourseItem" -Attributes @(
        "CourseId: Guid",
        "ModuleId: Guid?",
        "Type: CourseItemType",
        "SourceId: Guid",
        "Title: string",
        "Description: string?",
        "Url: string?",
        "AttachmentId: Guid?",
        "ResourceKind: string?",
        "OrderIndex: int",
        "Status: CourseItemStatus",
        "IsRequired: bool",
        "Points: decimal?",
        "AvailableFrom: DateTime?",
        "Deadline: DateTime?",
        "CreatedAt: DateTime",
        "UpdatedAt: DateTime?"
    )
    $courseItem.Notes = "CourseItem is a universal Course Builder item. Type + SourceId identify source entity. Source can be Lesson, Test, Assignment, LiveSession, Resource or ExternalLink. Unique index: { Type, SourceId }."
    $courseItem.Update() | Out-Null

    $courseEnrollment = Add-Class -Package $pkg -Name "CourseEnrollment" -Attributes @(
        "CourseId: Guid",
        "StudentId: string",
        "EnrolledAt: DateTime",
        "Status: EnrollmentStatus"
    )

    $courseReview = Add-Class -Package $pkg -Name "CourseReview" -Attributes @(
        "CourseId: Guid",
        "StudentId: string",
        "StudentName: string",
        "Rating: int",
        "Comment: string?",
        "CreatedAt: DateTime",
        "UpdatedAt: DateTime?"
    )

    foreach ($el in @($discipline, $course, $courseModule, $lesson, $courseItem, $courseEnrollment, $courseReview)) {
        Add-Generalization -Child $el -Parent $baseEntity
    }

    foreach ($el in @($discipline, $course, $courseItem, $courseReview)) {
        Add-Realization -Client $el -Supplier $auditable
    }

    Add-Association -Client $discipline -Supplier $course -Name "Courses" -ClientCardinality "1" -SupplierCardinality "0..*" -ClientAggregation 1
    Add-Association -Client $course -Supplier $courseModule -Name "Modules" -ClientCardinality "1" -SupplierCardinality "0..*" -ClientAggregation 2
    Add-Association -Client $course -Supplier $courseItem -Name "Items" -ClientCardinality "1" -SupplierCardinality "0..*" -ClientAggregation 2
    Add-Association -Client $course -Supplier $courseEnrollment -Name "Enrollments" -ClientCardinality "1" -SupplierCardinality "0..*" -ClientAggregation 2
    Add-Association -Client $course -Supplier $courseReview -Name "Reviews" -ClientCardinality "1" -SupplierCardinality "0..*" -ClientAggregation 2
    Add-Association -Client $courseModule -Supplier $lesson -Name "Lessons" -ClientCardinality "1" -SupplierCardinality "0..*" -ClientAggregation 2
    Add-Association -Client $courseModule -Supplier $courseItem -Name "Items" -ClientCardinality "0..1" -SupplierCardinality "0..*" -ClientAggregation 1

    Add-Dependency -Client $course -Supplier $courseLevel
    Add-Dependency -Client $course -Supplier $courseOrderType
    Add-Dependency -Client $courseItem -Supplier $courseItemType
    Add-Dependency -Client $courseItem -Supplier $courseItemStatus
    Add-Dependency -Client $courseEnrollment -Supplier $enrollmentStatus
    Add-Dependency -Client $lesson -Supplier $lessonLayout

    $diagram = $pkg.Diagrams.AddNew("Courses Domain Class Diagram", "Class")
    $diagram.Update() | Out-Null
    $pkg.Diagrams.Refresh()

    Add-DiagramObject -Diagram $diagram -Element $baseEntity -Left 450 -Top 80 -Right 650 -Bottom 160
    Add-DiagramObject -Diagram $diagram -Element $auditable -Left 760 -Top 80 -Right 1020 -Bottom 180

    Add-DiagramObject -Diagram $diagram -Element $discipline -Left 80 -Top 310 -Right 330 -Bottom 470
    Add-DiagramObject -Diagram $diagram -Element $course -Left 430 -Top 250 -Right 760 -Bottom 650
    Add-DiagramObject -Diagram $diagram -Element $courseModule -Left 900 -Top 250 -Right 1180 -Bottom 430
    Add-DiagramObject -Diagram $diagram -Element $lesson -Left 1320 -Top 260 -Right 1560 -Bottom 470

    Add-DiagramObject -Diagram $diagram -Element $courseItem -Left 900 -Top 560 -Right 1250 -Bottom 920
    Add-DiagramObject -Diagram $diagram -Element $courseEnrollment -Left 420 -Top 780 -Right 720 -Bottom 950
    Add-DiagramObject -Diagram $diagram -Element $courseReview -Left 80 -Top 760 -Right 340 -Bottom 950

    Add-DiagramObject -Diagram $diagram -Element $courseLevel -Left 80 -Top 1080 -Right 310 -Bottom 1200
    Add-DiagramObject -Diagram $diagram -Element $courseOrderType -Left 350 -Top 1080 -Right 580 -Bottom 1180
    Add-DiagramObject -Diagram $diagram -Element $enrollmentStatus -Left 630 -Top 1080 -Right 860 -Bottom 1220
    Add-DiagramObject -Diagram $diagram -Element $courseItemType -Left 920 -Top 1080 -Right 1200 -Bottom 1270
    Add-DiagramObject -Diagram $diagram -Element $courseItemStatus -Left 1240 -Top 1080 -Right 1500 -Bottom 1240
    Add-DiagramObject -Diagram $diagram -Element $lessonLayout -Left 1580 -Top 1080 -Right 1780 -Bottom 1180

    $diagram.Update() | Out-Null
    $repo.SaveDiagram($diagram.DiagramID) | Out-Null
    $repo.SaveAllDiagrams() | Out-Null
    $repo.CloseFile()

    Write-Output "Created: $OutputPath"
}
finally {
    if ($repo -ne $null) {
        $repo.Exit()
    }
}
