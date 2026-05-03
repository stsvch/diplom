param(
    [string]$SourceProject = "$PSScriptRoot\EduPlatform-All-Modules-Diagrams.qea",
    [string]$OutputProject = "$PSScriptRoot\EduPlatform-All-Modules-Diagrams-Extended.qea"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $SourceProject)) {
    throw "Source project not found: $SourceProject"
}

Copy-Item -LiteralPath $SourceProject -Destination $OutputProject -Force

$repo = New-Object -ComObject EA.Repository

function Get-PackageByName {
    param($ParentPackage, [string]$Name)

    for ($i = 0; $i -lt $ParentPackage.Packages.Count; $i++) {
        $pkg = $ParentPackage.Packages.GetAt($i)
        if ($pkg.Name -eq $Name) {
            return $pkg
        }
    }
    return $null
}

function Get-OrCreateElement {
    param(
        $Package,
        [string]$Name,
        [string]$Type = "Class",
        [string]$Stereotype = "",
        [string[]]$Attributes = @()
    )

    for ($i = 0; $i -lt $Package.Elements.Count; $i++) {
        $existing = $Package.Elements.GetAt($i)
        if ($existing.Name -eq $Name) {
            return $existing
        }
    }

    $el = $Package.Elements.AddNew($Name, $Type)
    if ($Stereotype) {
        $el.Stereotype = $Stereotype
    }
    $el.Update() | Out-Null

    foreach ($line in $Attributes) {
        $parts = $line.Split(":", 2)
        $attrName = $parts[0].Trim()
        $attrType = ""
        if ($parts.Count -gt 1) {
            $attrType = $parts[1].Trim()
        }
        $attr = $el.Attributes.AddNew($attrName, $attrType)
        $attr.Visibility = "Public"
        $attr.Update() | Out-Null
    }
    $el.Attributes.Refresh()
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
    if ($ClientAggregation -gt 0) { $connector.ClientEnd.Aggregation = $ClientAggregation }
    $connector.Update() | Out-Null
    $Client.Connectors.Refresh()
}

function Add-Operations {
    param(
        $Element,
        [string[]]$Operations = @()
    )

    foreach ($operationName in $Operations) {
        $exists = $false
        for ($i = 0; $i -lt $Element.Methods.Count; $i++) {
            $method = $Element.Methods.GetAt($i)
            if ($method.Name -eq $operationName) {
                $exists = $true
                break
            }
        }

        if (-not $exists) {
            $op = $Element.Methods.AddNew($operationName, "")
            $op.Visibility = "Public"
            $op.Update() | Out-Null
        }
    }

    $Element.Methods.Refresh()
}

function Add-DiagramObject {
    param($Diagram, $Element, [int]$Left, [int]$Top, [int]$Right, [int]$Bottom)

    $obj = $Diagram.DiagramObjects.AddNew("l=$Left;r=$Right;t=$Top;b=$Bottom;", "")
    $obj.ElementID = $Element.ElementID
    $obj.Update() | Out-Null
    $Diagram.DiagramObjects.Refresh()
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

try {
    $repo.OpenFile($OutputProject) | Out-Null
    $model = $repo.Models.GetAt(0)
    $authPackage = Get-PackageByName -ParentPackage $model -Name "Auth"
    if ($null -eq $authPackage) {
        throw "Auth package not found in $OutputProject"
    }

    $applicationUser = Get-OrCreateElement -Package $authPackage -Name "ApplicationUser" -Type "Class"
    $refreshToken = Get-OrCreateElement -Package $authPackage -Name "RefreshToken" -Type "Class"
    $platformSetting = Get-OrCreateElement -Package $authPackage -Name "PlatformSetting" -Type "Class"
    $userRole = Get-OrCreateElement -Package $authPackage -Name "UserRole" -Type "Enumeration"

    $identityUser = Get-OrCreateElement -Package $authPackage -Name "IdentityUser" -Type "Class" -Stereotype "external" -Attributes @(
        "Id: string",
        "Email: string?",
        "UserName: string?",
        "SecurityStamp: string?",
        "LockoutEnd: DateTimeOffset?"
    )
    $identityRole = Get-OrCreateElement -Package $authPackage -Name "IdentityRole" -Type "Class" -Stereotype "external"
    $identityDbContext = Get-OrCreateElement -Package $authPackage -Name "IdentityDbContext<ApplicationUser>" -Type "Class" -Stereotype "external"
    $userManager = Get-OrCreateElement -Package $authPackage -Name "UserManager<ApplicationUser>" -Type "Class" -Stereotype "external"
    $roleManager = Get-OrCreateElement -Package $authPackage -Name "RoleManager<IdentityRole>" -Type "Class" -Stereotype "external"
    $signInManager = Get-OrCreateElement -Package $authPackage -Name "SignInManager<ApplicationUser>" -Type "Class" -Stereotype "external"
    $mediator = Get-OrCreateElement -Package $authPackage -Name "IMediator" -Type "Interface" -Stereotype "external"
    $result = Get-OrCreateElement -Package $authPackage -Name "Result<T>" -Type "Class" -Stereotype "shared"
    $pagedResult = Get-OrCreateElement -Package $authPackage -Name "PagedResult<T>" -Type "Class" -Stereotype "shared"
    $profile = Get-OrCreateElement -Package $authPackage -Name "Profile" -Type "Class" -Stereotype "external"
    $jwtBearer = Get-OrCreateElement -Package $authPackage -Name "JwtBearer" -Type "Class" -Stereotype "external"

    $authController = Get-OrCreateElement -Package $authPackage -Name "AuthController" -Type "Class" -Stereotype "controller" -Attributes @(
        "_mediator: IMediator",
        "_env: IWebHostEnvironment"
    )
    Add-Operations -Element $authController -Operations @(
        "Register",
        "ConfirmEmail",
        "Login",
        "Refresh",
        "Logout",
        "ForgotPassword",
        "ResetPassword"
    )

    $usersController = Get-OrCreateElement -Package $authPackage -Name "UsersController" -Type "Class" -Stereotype "controller" -Attributes @(
        "_mediator: IMediator"
    )
    Add-Operations -Element $usersController -Operations @(
        "GetProfile",
        "UpdateProfile",
        "Search",
        "ChangePassword"
    )

    $adminUsersController = Get-OrCreateElement -Package $authPackage -Name "AdminUsersController" -Type "Class" -Stereotype "controller" -Attributes @(
        "_mediator: IMediator"
    )
    Add-Operations -Element $adminUsersController -Operations @(
        "GetAll",
        "Create",
        "Block",
        "Unblock",
        "ChangeRole",
        "Delete"
    )

    $platformSettingsController = Get-OrCreateElement -Package $authPackage -Name "PlatformSettingsController" -Type "Class" -Stereotype "controller" -Attributes @(
        "_mediator: IMediator"
    )
    Add-Operations -Element $platformSettingsController -Operations @(
        "GetPublic",
        "Get",
        "Update"
    )

    $authDbContext = Get-OrCreateElement -Package $authPackage -Name "AuthDbContext" -Type "Class" -Stereotype "dbcontext"
    $authModuleRegistration = Get-OrCreateElement -Package $authPackage -Name "AuthModuleRegistration" -Type "Class" -Stereotype "configuration"
    $authMappingProfile = Get-OrCreateElement -Package $authPackage -Name "AuthMappingProfile" -Type "Class" -Stereotype "mapping"

    $iAuthDbContext = Get-OrCreateElement -Package $authPackage -Name "IAuthDbContext" -Type "Interface"
    $iTokenService = Get-OrCreateElement -Package $authPackage -Name "ITokenService" -Type "Interface" -Attributes @(
        "GenerateAccessToken(user, roles): string",
        "GenerateRefreshToken(): string",
        "GetPrincipalFromExpiredToken(token): ClaimsPrincipal"
    )
    $iEmailService = Get-OrCreateElement -Package $authPackage -Name "IEmailService" -Type "Interface" -Attributes @(
        "SendEmailAsync(to, subject, body): Task"
    )

    $jwtTokenService = Get-OrCreateElement -Package $authPackage -Name "JwtTokenService" -Type "Class" -Stereotype "service"
    $smtpEmailService = Get-OrCreateElement -Package $authPackage -Name "SmtpEmailService" -Type "Class" -Stereotype "service"

    $authResponseDto = Get-OrCreateElement -Package $authPackage -Name "AuthResponseDto" -Type "Class" -Stereotype "dto" -Attributes @(
        "AccessToken: string",
        "Email: string",
        "FirstName: string",
        "LastName: string",
        "Role: string"
    )
    $loginResultDto = Get-OrCreateElement -Package $authPackage -Name "LoginResultDto" -Type "Class" -Stereotype "dto" -Attributes @(
        "AuthResponse: AuthResponseDto",
        "RefreshToken: string"
    )
    $userProfileDto = Get-OrCreateElement -Package $authPackage -Name "UserProfileDto" -Type "Class" -Stereotype "dto"
    $platformSettingsDto = Get-OrCreateElement -Package $authPackage -Name "PlatformSettingsDto" -Type "Class" -Stereotype "dto"
    $adminUserDto = Get-OrCreateElement -Package $authPackage -Name "AdminUserDto" -Type "Class" -Stereotype "dto" -Attributes @(
        "Id: string",
        "Email: string",
        "FullName: string",
        "Role: string",
        "IsBlocked: bool",
        "EmailConfirmed: bool",
        "CreatedAt: DateTime"
    )
    $userSummaryDto = Get-OrCreateElement -Package $authPackage -Name "UserSummaryDto" -Type "Class" -Stereotype "dto" -Attributes @(
        "Id: string",
        "FullName: string",
        "Email: string",
        "Role: string"
    )
    $userStatsDto = Get-OrCreateElement -Package $authPackage -Name "UserStatsDto" -Type "Class" -Stereotype "dto" -Attributes @(
        "Total: int",
        "Students: int",
        "Teachers: int",
        "Admins: int",
        "Blocked: int",
        "UnconfirmedEmail: int",
        "NewLast7Days: int"
    )

    $registerRequest = Get-OrCreateElement -Package $authPackage -Name "RegisterRequest" -Type "Class" -Stereotype "api request"
    $refreshAccessTokenRequest = Get-OrCreateElement -Package $authPackage -Name "RefreshAccessTokenRequest" -Type "Class" -Stereotype "api request"
    $updateProfileRequest = Get-OrCreateElement -Package $authPackage -Name "UpdateProfileRequest" -Type "Class" -Stereotype "api request"
    $changePasswordRequest = Get-OrCreateElement -Package $authPackage -Name "ChangePasswordRequest" -Type "Class" -Stereotype "api request"
    $createUserRequest = Get-OrCreateElement -Package $authPackage -Name "CreateUserRequest" -Type "Class" -Stereotype "api request"
    $changeRoleRequest = Get-OrCreateElement -Package $authPackage -Name "ChangeRoleRequest" -Type "Class" -Stereotype "api request"
    $updatePlatformSettingsRequest = Get-OrCreateElement -Package $authPackage -Name "UpdatePlatformSettingsRequest" -Type "Class" -Stereotype "api request"

    $registerCommand = Get-OrCreateElement -Package $authPackage -Name "RegisterCommand" -Type "Class" -Stereotype "command"
    $loginCommand = Get-OrCreateElement -Package $authPackage -Name "LoginCommand" -Type "Class" -Stereotype "command"
    $refreshTokenCommand = Get-OrCreateElement -Package $authPackage -Name "RefreshTokenCommand" -Type "Class" -Stereotype "command"
    $logoutCommand = Get-OrCreateElement -Package $authPackage -Name "LogoutCommand" -Type "Class" -Stereotype "command"
    $confirmEmailCommand = Get-OrCreateElement -Package $authPackage -Name "ConfirmEmailCommand" -Type "Class" -Stereotype "command"
    $forgotPasswordCommand = Get-OrCreateElement -Package $authPackage -Name "ForgotPasswordCommand" -Type "Class" -Stereotype "command"
    $resetPasswordCommand = Get-OrCreateElement -Package $authPackage -Name "ResetPasswordCommand" -Type "Class" -Stereotype "command"
    $changePasswordCommand = Get-OrCreateElement -Package $authPackage -Name "ChangePasswordCommand" -Type "Class" -Stereotype "command"
    $updateProfileCommand = Get-OrCreateElement -Package $authPackage -Name "UpdateProfileCommand" -Type "Class" -Stereotype "command"
    $updatePlatformSettingsCommand = Get-OrCreateElement -Package $authPackage -Name "UpdatePlatformSettingsCommand" -Type "Class" -Stereotype "command"
    $createUserCommand = Get-OrCreateElement -Package $authPackage -Name "CreateUserCommand" -Type "Class" -Stereotype "command"
    $blockUserCommand = Get-OrCreateElement -Package $authPackage -Name "BlockUserCommand" -Type "Class" -Stereotype "command"
    $unblockUserCommand = Get-OrCreateElement -Package $authPackage -Name "UnblockUserCommand" -Type "Class" -Stereotype "command"
    $changeUserRoleCommand = Get-OrCreateElement -Package $authPackage -Name "ChangeUserRoleCommand" -Type "Class" -Stereotype "command"
    $deleteUserCommand = Get-OrCreateElement -Package $authPackage -Name "DeleteUserCommand" -Type "Class" -Stereotype "command"

    $getProfileQuery = Get-OrCreateElement -Package $authPackage -Name "GetProfileQuery" -Type "Class" -Stereotype "query"
    $searchUsersQuery = Get-OrCreateElement -Package $authPackage -Name "SearchUsersQuery" -Type "Class" -Stereotype "query"
    $getPlatformSettingsQuery = Get-OrCreateElement -Package $authPackage -Name "GetPlatformSettingsQuery" -Type "Class" -Stereotype "query"
    $getAllUsersQuery = Get-OrCreateElement -Package $authPackage -Name "GetAllUsersQuery" -Type "Class" -Stereotype "query"
    $getUserStatsQuery = Get-OrCreateElement -Package $authPackage -Name "GetUserStatsQuery" -Type "Class" -Stereotype "query"

    $registerHandler = Get-OrCreateElement -Package $authPackage -Name "RegisterCommandHandler" -Type "Class" -Stereotype "handler"
    $loginHandler = Get-OrCreateElement -Package $authPackage -Name "LoginCommandHandler" -Type "Class" -Stereotype "handler"
    $refreshTokenHandler = Get-OrCreateElement -Package $authPackage -Name "RefreshTokenCommandHandler" -Type "Class" -Stereotype "handler"
    $logoutHandler = Get-OrCreateElement -Package $authPackage -Name "LogoutCommandHandler" -Type "Class" -Stereotype "handler"
    $confirmEmailHandler = Get-OrCreateElement -Package $authPackage -Name "ConfirmEmailCommandHandler" -Type "Class" -Stereotype "handler"
    $forgotPasswordHandler = Get-OrCreateElement -Package $authPackage -Name "ForgotPasswordCommandHandler" -Type "Class" -Stereotype "handler"
    $resetPasswordHandler = Get-OrCreateElement -Package $authPackage -Name "ResetPasswordCommandHandler" -Type "Class" -Stereotype "handler"
    $changePasswordHandler = Get-OrCreateElement -Package $authPackage -Name "ChangePasswordCommandHandler" -Type "Class" -Stereotype "handler"
    $updateProfileHandler = Get-OrCreateElement -Package $authPackage -Name "UpdateProfileCommandHandler" -Type "Class" -Stereotype "handler"
    $updatePlatformSettingsHandler = Get-OrCreateElement -Package $authPackage -Name "UpdatePlatformSettingsCommandHandler" -Type "Class" -Stereotype "handler"
    $createUserHandler = Get-OrCreateElement -Package $authPackage -Name "CreateUserCommandHandler" -Type "Class" -Stereotype "handler"
    $blockUserHandler = Get-OrCreateElement -Package $authPackage -Name "BlockUserCommandHandler" -Type "Class" -Stereotype "handler"
    $unblockUserHandler = Get-OrCreateElement -Package $authPackage -Name "UnblockUserCommandHandler" -Type "Class" -Stereotype "handler"
    $changeUserRoleHandler = Get-OrCreateElement -Package $authPackage -Name "ChangeUserRoleCommandHandler" -Type "Class" -Stereotype "handler"
    $deleteUserHandler = Get-OrCreateElement -Package $authPackage -Name "DeleteUserCommandHandler" -Type "Class" -Stereotype "handler"

    $getProfileHandler = Get-OrCreateElement -Package $authPackage -Name "GetProfileQueryHandler" -Type "Class" -Stereotype "handler"
    $searchUsersHandler = Get-OrCreateElement -Package $authPackage -Name "SearchUsersQueryHandler" -Type "Class" -Stereotype "handler"
    $getPlatformSettingsHandler = Get-OrCreateElement -Package $authPackage -Name "GetPlatformSettingsQueryHandler" -Type "Class" -Stereotype "handler"
    $getAllUsersHandler = Get-OrCreateElement -Package $authPackage -Name "GetAllUsersQueryHandler" -Type "Class" -Stereotype "handler"
    $getUserStatsHandler = Get-OrCreateElement -Package $authPackage -Name "GetUserStatsQueryHandler" -Type "Class" -Stereotype "handler"

    $loginValidator = Get-OrCreateElement -Package $authPackage -Name "LoginCommandValidator" -Type "Class" -Stereotype "validator"
    $registerValidator = Get-OrCreateElement -Package $authPackage -Name "RegisterCommandValidator" -Type "Class" -Stereotype "validator"
    $resetPasswordValidator = Get-OrCreateElement -Package $authPackage -Name "ResetPasswordCommandValidator" -Type "Class" -Stereotype "validator"
    $changePasswordValidator = Get-OrCreateElement -Package $authPackage -Name "ChangePasswordCommandValidator" -Type "Class" -Stereotype "validator"

    Add-Connector -Client $applicationUser -Supplier $identityUser -Type "Generalization"
    Add-Connector -Client $authDbContext -Supplier $identityDbContext -Type "Generalization"
    Add-Connector -Client $authDbContext -Supplier $iAuthDbContext -Type "Realisation"
    Add-Connector -Client $jwtTokenService -Supplier $iTokenService -Type "Realisation"
    Add-Connector -Client $smtpEmailService -Supplier $iEmailService -Type "Realisation"
    Add-Connector -Client $refreshToken -Supplier $applicationUser -Type "Association" -Name "User" -ClientCardinality "0..*" -SupplierCardinality "0..1"
    Add-Connector -Client $authMappingProfile -Supplier $profile -Type "Generalization"

    Add-Connector -Client $authController -Supplier $mediator -Type "Dependency" -Name "sends commands"
    Add-Connector -Client $usersController -Supplier $mediator -Type "Dependency" -Name "sends commands and queries"
    Add-Connector -Client $adminUsersController -Supplier $mediator -Type "Dependency" -Name "admin commands"
    Add-Connector -Client $platformSettingsController -Supplier $mediator -Type "Dependency" -Name "settings commands"

    Add-Connector -Client $authController -Supplier $registerCommand -Type "Dependency"
    Add-Connector -Client $authController -Supplier $loginCommand -Type "Dependency"
    Add-Connector -Client $authController -Supplier $refreshTokenCommand -Type "Dependency"
    Add-Connector -Client $authController -Supplier $logoutCommand -Type "Dependency"
    Add-Connector -Client $authController -Supplier $confirmEmailCommand -Type "Dependency"
    Add-Connector -Client $authController -Supplier $forgotPasswordCommand -Type "Dependency"
    Add-Connector -Client $authController -Supplier $resetPasswordCommand -Type "Dependency"
    Add-Connector -Client $authController -Supplier $registerRequest -Type "Dependency"
    Add-Connector -Client $authController -Supplier $refreshAccessTokenRequest -Type "Dependency"

    Add-Connector -Client $usersController -Supplier $getProfileQuery -Type "Dependency"
    Add-Connector -Client $usersController -Supplier $updateProfileCommand -Type "Dependency"
    Add-Connector -Client $usersController -Supplier $searchUsersQuery -Type "Dependency"
    Add-Connector -Client $usersController -Supplier $changePasswordCommand -Type "Dependency"
    Add-Connector -Client $usersController -Supplier $updateProfileRequest -Type "Dependency"
    Add-Connector -Client $usersController -Supplier $changePasswordRequest -Type "Dependency"

    Add-Connector -Client $adminUsersController -Supplier $getAllUsersQuery -Type "Dependency"
    Add-Connector -Client $adminUsersController -Supplier $createUserCommand -Type "Dependency"
    Add-Connector -Client $adminUsersController -Supplier $blockUserCommand -Type "Dependency"
    Add-Connector -Client $adminUsersController -Supplier $unblockUserCommand -Type "Dependency"
    Add-Connector -Client $adminUsersController -Supplier $changeUserRoleCommand -Type "Dependency"
    Add-Connector -Client $adminUsersController -Supplier $deleteUserCommand -Type "Dependency"
    Add-Connector -Client $adminUsersController -Supplier $createUserRequest -Type "Dependency"
    Add-Connector -Client $adminUsersController -Supplier $changeRoleRequest -Type "Dependency"

    Add-Connector -Client $platformSettingsController -Supplier $getPlatformSettingsQuery -Type "Dependency"
    Add-Connector -Client $platformSettingsController -Supplier $updatePlatformSettingsCommand -Type "Dependency"
    Add-Connector -Client $platformSettingsController -Supplier $updatePlatformSettingsRequest -Type "Dependency"

    Add-Connector -Client $registerHandler -Supplier $registerCommand -Type "Dependency"
    Add-Connector -Client $loginHandler -Supplier $loginCommand -Type "Dependency"
    Add-Connector -Client $refreshTokenHandler -Supplier $refreshTokenCommand -Type "Dependency"
    Add-Connector -Client $logoutHandler -Supplier $logoutCommand -Type "Dependency"
    Add-Connector -Client $confirmEmailHandler -Supplier $confirmEmailCommand -Type "Dependency"
    Add-Connector -Client $forgotPasswordHandler -Supplier $forgotPasswordCommand -Type "Dependency"
    Add-Connector -Client $resetPasswordHandler -Supplier $resetPasswordCommand -Type "Dependency"
    Add-Connector -Client $changePasswordHandler -Supplier $changePasswordCommand -Type "Dependency"
    Add-Connector -Client $updateProfileHandler -Supplier $updateProfileCommand -Type "Dependency"
    Add-Connector -Client $updatePlatformSettingsHandler -Supplier $updatePlatformSettingsCommand -Type "Dependency"
    Add-Connector -Client $createUserHandler -Supplier $createUserCommand -Type "Dependency"
    Add-Connector -Client $blockUserHandler -Supplier $blockUserCommand -Type "Dependency"
    Add-Connector -Client $unblockUserHandler -Supplier $unblockUserCommand -Type "Dependency"
    Add-Connector -Client $changeUserRoleHandler -Supplier $changeUserRoleCommand -Type "Dependency"
    Add-Connector -Client $deleteUserHandler -Supplier $deleteUserCommand -Type "Dependency"
    Add-Connector -Client $getProfileHandler -Supplier $getProfileQuery -Type "Dependency"
    Add-Connector -Client $searchUsersHandler -Supplier $searchUsersQuery -Type "Dependency"
    Add-Connector -Client $getPlatformSettingsHandler -Supplier $getPlatformSettingsQuery -Type "Dependency"
    Add-Connector -Client $getAllUsersHandler -Supplier $getAllUsersQuery -Type "Dependency"
    Add-Connector -Client $getUserStatsHandler -Supplier $getUserStatsQuery -Type "Dependency"

    foreach ($handler in @($registerHandler, $loginHandler, $refreshTokenHandler, $logoutHandler, $confirmEmailHandler, $forgotPasswordHandler, $resetPasswordHandler, $changePasswordHandler, $updateProfileHandler, $createUserHandler, $blockUserHandler, $unblockUserHandler, $changeUserRoleHandler, $deleteUserHandler, $getProfileHandler, $searchUsersHandler, $getAllUsersHandler, $getUserStatsHandler)) {
        Add-Connector -Client $handler -Supplier $userManager -Type "Dependency"
    }
    foreach ($handler in @($createUserHandler, $changeUserRoleHandler, $getAllUsersHandler, $getUserStatsHandler, $searchUsersHandler)) {
        Add-Connector -Client $handler -Supplier $roleManager -Type "Dependency"
    }

    Add-Connector -Client $loginHandler -Supplier $signInManager -Type "Dependency"
    Add-Connector -Client $loginHandler -Supplier $iTokenService -Type "Dependency"
    Add-Connector -Client $refreshTokenHandler -Supplier $iTokenService -Type "Dependency"
    Add-Connector -Client $registerHandler -Supplier $iEmailService -Type "Dependency"
    Add-Connector -Client $forgotPasswordHandler -Supplier $iEmailService -Type "Dependency"
    Add-Connector -Client $updatePlatformSettingsHandler -Supplier $iAuthDbContext -Type "Dependency"
    Add-Connector -Client $getPlatformSettingsHandler -Supplier $iAuthDbContext -Type "Dependency"
    Add-Connector -Client $authModuleRegistration -Supplier $authDbContext -Type "Dependency"
    Add-Connector -Client $authModuleRegistration -Supplier $jwtBearer -Type "Dependency"
    Add-Connector -Client $authModuleRegistration -Supplier $jwtTokenService -Type "Dependency"
    Add-Connector -Client $authModuleRegistration -Supplier $smtpEmailService -Type "Dependency"
    Add-Connector -Client $authModuleRegistration -Supplier $identityRole -Type "Dependency"
    Add-Connector -Client $authModuleRegistration -Supplier $roleManager -Type "Dependency"

    Add-Connector -Client $registerCommand -Supplier $result -Type "Dependency"
    Add-Connector -Client $loginCommand -Supplier $result -Type "Dependency"
    Add-Connector -Client $refreshTokenCommand -Supplier $result -Type "Dependency"
    Add-Connector -Client $getAllUsersQuery -Supplier $pagedResult -Type "Dependency"

    Add-Connector -Client $loginHandler -Supplier $loginResultDto -Type "Dependency"
    Add-Connector -Client $loginResultDto -Supplier $authResponseDto -Type "Association"
    Add-Connector -Client $updateProfileHandler -Supplier $userProfileDto -Type "Dependency"
    Add-Connector -Client $updatePlatformSettingsHandler -Supplier $platformSettingsDto -Type "Dependency"
    Add-Connector -Client $getProfileHandler -Supplier $userProfileDto -Type "Dependency"
    Add-Connector -Client $searchUsersHandler -Supplier $userSummaryDto -Type "Dependency"
    Add-Connector -Client $getAllUsersHandler -Supplier $adminUserDto -Type "Dependency"
    Add-Connector -Client $getUserStatsHandler -Supplier $userStatsDto -Type "Dependency"
    Add-Connector -Client $createUserHandler -Supplier $userSummaryDto -Type "Dependency"
    Add-Connector -Client $platformSettingsDto -Supplier $platformSetting -Type "Dependency"
    Add-Connector -Client $authMappingProfile -Supplier $applicationUser -Type "Dependency"
    Add-Connector -Client $authMappingProfile -Supplier $userProfileDto -Type "Dependency"
    Add-Connector -Client $authMappingProfile -Supplier $platformSettingsDto -Type "Dependency"
    Add-Connector -Client $applicationUser -Supplier $userRole -Type "Dependency"

    Add-Connector -Client $loginValidator -Supplier $loginCommand -Type "Dependency"
    Add-Connector -Client $registerValidator -Supplier $registerCommand -Type "Dependency"
    Add-Connector -Client $resetPasswordValidator -Supplier $resetPasswordCommand -Type "Dependency"
    Add-Connector -Client $changePasswordValidator -Supplier $changePasswordCommand -Type "Dependency"

    Delete-DiagramIfExists -Package $authPackage -Name "Auth Full Class Diagram"
    $diagram = $authPackage.Diagrams.AddNew("Auth Full Class Diagram", "Class")
    $diagram.Update() | Out-Null
    $authPackage.Diagrams.Refresh()

    Add-DiagramObject -Diagram $diagram -Element $authController -Left 40 -Top 80 -Right 360 -Bottom 260
    Add-DiagramObject -Diagram $diagram -Element $usersController -Left 40 -Top 320 -Right 360 -Bottom 500
    Add-DiagramObject -Diagram $diagram -Element $adminUsersController -Left 40 -Top 560 -Right 360 -Bottom 760
    Add-DiagramObject -Diagram $diagram -Element $platformSettingsController -Left 40 -Top 820 -Right 360 -Bottom 980
    Add-DiagramObject -Diagram $diagram -Element $mediator -Left 40 -Top 1060 -Right 320 -Bottom 1160

    Add-DiagramObject -Diagram $diagram -Element $registerCommand -Left 470 -Top 40 -Right 730 -Bottom 120
    Add-DiagramObject -Diagram $diagram -Element $loginCommand -Left 470 -Top 140 -Right 730 -Bottom 220
    Add-DiagramObject -Diagram $diagram -Element $refreshTokenCommand -Left 470 -Top 240 -Right 730 -Bottom 320
    Add-DiagramObject -Diagram $diagram -Element $logoutCommand -Left 470 -Top 340 -Right 730 -Bottom 420
    Add-DiagramObject -Diagram $diagram -Element $confirmEmailCommand -Left 470 -Top 440 -Right 730 -Bottom 520
    Add-DiagramObject -Diagram $diagram -Element $forgotPasswordCommand -Left 470 -Top 540 -Right 730 -Bottom 620
    Add-DiagramObject -Diagram $diagram -Element $resetPasswordCommand -Left 470 -Top 640 -Right 730 -Bottom 720
    Add-DiagramObject -Diagram $diagram -Element $changePasswordCommand -Left 470 -Top 740 -Right 730 -Bottom 820
    Add-DiagramObject -Diagram $diagram -Element $updateProfileCommand -Left 470 -Top 840 -Right 730 -Bottom 920
    Add-DiagramObject -Diagram $diagram -Element $updatePlatformSettingsCommand -Left 470 -Top 940 -Right 730 -Bottom 1020
    Add-DiagramObject -Diagram $diagram -Element $createUserCommand -Left 470 -Top 1040 -Right 730 -Bottom 1120
    Add-DiagramObject -Diagram $diagram -Element $blockUserCommand -Left 470 -Top 1140 -Right 730 -Bottom 1220
    Add-DiagramObject -Diagram $diagram -Element $unblockUserCommand -Left 470 -Top 1240 -Right 730 -Bottom 1320
    Add-DiagramObject -Diagram $diagram -Element $changeUserRoleCommand -Left 470 -Top 1340 -Right 730 -Bottom 1420
    Add-DiagramObject -Diagram $diagram -Element $deleteUserCommand -Left 470 -Top 1440 -Right 730 -Bottom 1520
    Add-DiagramObject -Diagram $diagram -Element $getProfileQuery -Left 470 -Top 1580 -Right 730 -Bottom 1660
    Add-DiagramObject -Diagram $diagram -Element $searchUsersQuery -Left 470 -Top 1680 -Right 730 -Bottom 1760
    Add-DiagramObject -Diagram $diagram -Element $getPlatformSettingsQuery -Left 470 -Top 1780 -Right 730 -Bottom 1860
    Add-DiagramObject -Diagram $diagram -Element $getAllUsersQuery -Left 470 -Top 1880 -Right 730 -Bottom 1960
    Add-DiagramObject -Diagram $diagram -Element $getUserStatsQuery -Left 470 -Top 1980 -Right 730 -Bottom 2060

    Add-DiagramObject -Diagram $diagram -Element $registerHandler -Left 820 -Top 40 -Right 1120 -Bottom 120
    Add-DiagramObject -Diagram $diagram -Element $loginHandler -Left 820 -Top 140 -Right 1120 -Bottom 220
    Add-DiagramObject -Diagram $diagram -Element $refreshTokenHandler -Left 820 -Top 240 -Right 1120 -Bottom 320
    Add-DiagramObject -Diagram $diagram -Element $logoutHandler -Left 820 -Top 340 -Right 1120 -Bottom 420
    Add-DiagramObject -Diagram $diagram -Element $confirmEmailHandler -Left 820 -Top 440 -Right 1120 -Bottom 520
    Add-DiagramObject -Diagram $diagram -Element $forgotPasswordHandler -Left 820 -Top 540 -Right 1120 -Bottom 620
    Add-DiagramObject -Diagram $diagram -Element $resetPasswordHandler -Left 820 -Top 640 -Right 1120 -Bottom 720
    Add-DiagramObject -Diagram $diagram -Element $changePasswordHandler -Left 820 -Top 740 -Right 1120 -Bottom 820
    Add-DiagramObject -Diagram $diagram -Element $updateProfileHandler -Left 820 -Top 840 -Right 1120 -Bottom 920
    Add-DiagramObject -Diagram $diagram -Element $updatePlatformSettingsHandler -Left 820 -Top 940 -Right 1120 -Bottom 1020
    Add-DiagramObject -Diagram $diagram -Element $createUserHandler -Left 820 -Top 1040 -Right 1120 -Bottom 1120
    Add-DiagramObject -Diagram $diagram -Element $blockUserHandler -Left 820 -Top 1140 -Right 1120 -Bottom 1220
    Add-DiagramObject -Diagram $diagram -Element $unblockUserHandler -Left 820 -Top 1240 -Right 1120 -Bottom 1320
    Add-DiagramObject -Diagram $diagram -Element $changeUserRoleHandler -Left 820 -Top 1340 -Right 1120 -Bottom 1420
    Add-DiagramObject -Diagram $diagram -Element $deleteUserHandler -Left 820 -Top 1440 -Right 1120 -Bottom 1520
    Add-DiagramObject -Diagram $diagram -Element $getProfileHandler -Left 820 -Top 1580 -Right 1120 -Bottom 1660
    Add-DiagramObject -Diagram $diagram -Element $searchUsersHandler -Left 820 -Top 1680 -Right 1120 -Bottom 1760
    Add-DiagramObject -Diagram $diagram -Element $getPlatformSettingsHandler -Left 820 -Top 1780 -Right 1120 -Bottom 1860
    Add-DiagramObject -Diagram $diagram -Element $getAllUsersHandler -Left 820 -Top 1880 -Right 1120 -Bottom 1960
    Add-DiagramObject -Diagram $diagram -Element $getUserStatsHandler -Left 820 -Top 1980 -Right 1120 -Bottom 2060

    Add-DiagramObject -Diagram $diagram -Element $iAuthDbContext -Left 1220 -Top 80 -Right 1480 -Bottom 170
    Add-DiagramObject -Diagram $diagram -Element $iTokenService -Left 1220 -Top 230 -Right 1540 -Bottom 390
    Add-DiagramObject -Diagram $diagram -Element $iEmailService -Left 1220 -Top 450 -Right 1500 -Bottom 560

    Add-DiagramObject -Diagram $diagram -Element $authDbContext -Left 1640 -Top 70 -Right 1940 -Bottom 190
    Add-DiagramObject -Diagram $diagram -Element $jwtTokenService -Left 1640 -Top 250 -Right 1940 -Bottom 360
    Add-DiagramObject -Diagram $diagram -Element $smtpEmailService -Left 1640 -Top 460 -Right 1940 -Bottom 570
    Add-DiagramObject -Diagram $diagram -Element $authModuleRegistration -Left 1640 -Top 700 -Right 1980 -Bottom 820
    Add-DiagramObject -Diagram $diagram -Element $authMappingProfile -Left 1220 -Top 700 -Right 1520 -Bottom 820

    Add-DiagramObject -Diagram $diagram -Element $applicationUser -Left 1220 -Top 900 -Right 1530 -Bottom 1090
    Add-DiagramObject -Diagram $diagram -Element $refreshToken -Left 1220 -Top 1140 -Right 1520 -Bottom 1350
    Add-DiagramObject -Diagram $diagram -Element $platformSetting -Left 1220 -Top 1400 -Right 1520 -Bottom 1610
    Add-DiagramObject -Diagram $diagram -Element $userRole -Left 1220 -Top 1660 -Right 1480 -Bottom 1820

    Add-DiagramObject -Diagram $diagram -Element $identityUser -Left 1640 -Top 860 -Right 1980 -Bottom 980
    Add-DiagramObject -Diagram $diagram -Element $identityDbContext -Left 1640 -Top 1020 -Right 1980 -Bottom 1120
    Add-DiagramObject -Diagram $diagram -Element $userManager -Left 1640 -Top 1160 -Right 1980 -Bottom 1260
    Add-DiagramObject -Diagram $diagram -Element $signInManager -Left 1640 -Top 1300 -Right 1980 -Bottom 1400
    Add-DiagramObject -Diagram $diagram -Element $roleManager -Left 1640 -Top 1440 -Right 1980 -Bottom 1540
    Add-DiagramObject -Diagram $diagram -Element $identityRole -Left 1640 -Top 1580 -Right 1980 -Bottom 1680
    Add-DiagramObject -Diagram $diagram -Element $jwtBearer -Left 1640 -Top 1720 -Right 1980 -Bottom 1820

    Add-DiagramObject -Diagram $diagram -Element $authResponseDto -Left 80 -Top 2360 -Right 420 -Bottom 2520
    Add-DiagramObject -Diagram $diagram -Element $loginResultDto -Left 500 -Top 2360 -Right 820 -Bottom 2480
    Add-DiagramObject -Diagram $diagram -Element $userProfileDto -Left 900 -Top 2360 -Right 1200 -Bottom 2480
    Add-DiagramObject -Diagram $diagram -Element $platformSettingsDto -Left 1280 -Top 2360 -Right 1580 -Bottom 2480
    Add-DiagramObject -Diagram $diagram -Element $adminUserDto -Left 80 -Top 2580 -Right 420 -Bottom 2740
    Add-DiagramObject -Diagram $diagram -Element $userSummaryDto -Left 500 -Top 2580 -Right 820 -Bottom 2700
    Add-DiagramObject -Diagram $diagram -Element $userStatsDto -Left 900 -Top 2580 -Right 1240 -Bottom 2740
    Add-DiagramObject -Diagram $diagram -Element $result -Left 1280 -Top 2580 -Right 1580 -Bottom 2680
    Add-DiagramObject -Diagram $diagram -Element $pagedResult -Left 1640 -Top 2580 -Right 1940 -Bottom 2680

    Add-DiagramObject -Diagram $diagram -Element $registerRequest -Left 80 -Top 2860 -Right 360 -Bottom 2940
    Add-DiagramObject -Diagram $diagram -Element $refreshAccessTokenRequest -Left 420 -Top 2860 -Right 760 -Bottom 2940
    Add-DiagramObject -Diagram $diagram -Element $updateProfileRequest -Left 820 -Top 2860 -Right 1120 -Bottom 2940
    Add-DiagramObject -Diagram $diagram -Element $changePasswordRequest -Left 1180 -Top 2860 -Right 1500 -Bottom 2940
    Add-DiagramObject -Diagram $diagram -Element $createUserRequest -Left 80 -Top 3000 -Right 360 -Bottom 3080
    Add-DiagramObject -Diagram $diagram -Element $changeRoleRequest -Left 420 -Top 3000 -Right 700 -Bottom 3080
    Add-DiagramObject -Diagram $diagram -Element $updatePlatformSettingsRequest -Left 760 -Top 3000 -Right 1120 -Bottom 3080

    Add-DiagramObject -Diagram $diagram -Element $loginValidator -Left 80 -Top 3240 -Right 360 -Bottom 3320
    Add-DiagramObject -Diagram $diagram -Element $registerValidator -Left 420 -Top 3240 -Right 700 -Bottom 3320
    Add-DiagramObject -Diagram $diagram -Element $resetPasswordValidator -Left 760 -Top 3240 -Right 1060 -Bottom 3320
    Add-DiagramObject -Diagram $diagram -Element $changePasswordValidator -Left 1120 -Top 3240 -Right 1420 -Bottom 3320

    $diagram.Update() | Out-Null
    $repo.SaveDiagram($diagram.DiagramID) | Out-Null
    $repo.SaveAllDiagrams() | Out-Null
    $repo.CloseFile()

    Write-Output "Created: $OutputProject"
}
finally {
    if ($repo -ne $null) {
        $repo.Exit()
    }
}
