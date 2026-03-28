FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY EduPlatform.sln .
COPY src/Host/EduPlatform.Host.csproj src/Host/
COPY src/Shared/EduPlatform.Shared.Domain/EduPlatform.Shared.Domain.csproj src/Shared/EduPlatform.Shared.Domain/
COPY src/Shared/EduPlatform.Shared.Application/EduPlatform.Shared.Application.csproj src/Shared/EduPlatform.Shared.Application/
COPY src/Shared/EduPlatform.Shared.Infrastructure/EduPlatform.Shared.Infrastructure.csproj src/Shared/EduPlatform.Shared.Infrastructure/
COPY src/Modules/Auth/Auth.Domain/Auth.Domain.csproj src/Modules/Auth/Auth.Domain/
COPY src/Modules/Auth/Auth.Application/Auth.Application.csproj src/Modules/Auth/Auth.Application/
COPY src/Modules/Auth/Auth.Infrastructure/Auth.Infrastructure.csproj src/Modules/Auth/Auth.Infrastructure/
COPY src/Modules/Courses/Courses.Domain/Courses.Domain.csproj src/Modules/Courses/Courses.Domain/
COPY src/Modules/Courses/Courses.Application/Courses.Application.csproj src/Modules/Courses/Courses.Application/
COPY src/Modules/Courses/Courses.Infrastructure/Courses.Infrastructure.csproj src/Modules/Courses/Courses.Infrastructure/
COPY src/Modules/Content/Content.Domain/Content.Domain.csproj src/Modules/Content/Content.Domain/
COPY src/Modules/Content/Content.Application/Content.Application.csproj src/Modules/Content/Content.Application/
COPY src/Modules/Content/Content.Infrastructure/Content.Infrastructure.csproj src/Modules/Content/Content.Infrastructure/
COPY src/Modules/Tests/Tests.Domain/Tests.Domain.csproj src/Modules/Tests/Tests.Domain/
COPY src/Modules/Tests/Tests.Application/Tests.Application.csproj src/Modules/Tests/Tests.Application/
COPY src/Modules/Tests/Tests.Infrastructure/Tests.Infrastructure.csproj src/Modules/Tests/Tests.Infrastructure/
COPY src/Modules/Assignments/Assignments.Domain/Assignments.Domain.csproj src/Modules/Assignments/Assignments.Domain/
COPY src/Modules/Assignments/Assignments.Application/Assignments.Application.csproj src/Modules/Assignments/Assignments.Application/
COPY src/Modules/Assignments/Assignments.Infrastructure/Assignments.Infrastructure.csproj src/Modules/Assignments/Assignments.Infrastructure/
COPY src/Modules/Grading/Grading.Domain/Grading.Domain.csproj src/Modules/Grading/Grading.Domain/
COPY src/Modules/Grading/Grading.Application/Grading.Application.csproj src/Modules/Grading/Grading.Application/
COPY src/Modules/Grading/Grading.Infrastructure/Grading.Infrastructure.csproj src/Modules/Grading/Grading.Infrastructure/
COPY src/Modules/Progress/Progress.Domain/Progress.Domain.csproj src/Modules/Progress/Progress.Domain/
COPY src/Modules/Progress/Progress.Application/Progress.Application.csproj src/Modules/Progress/Progress.Application/
COPY src/Modules/Progress/Progress.Infrastructure/Progress.Infrastructure.csproj src/Modules/Progress/Progress.Infrastructure/
COPY src/Modules/Notifications/Notifications.Domain/Notifications.Domain.csproj src/Modules/Notifications/Notifications.Domain/
COPY src/Modules/Notifications/Notifications.Application/Notifications.Application.csproj src/Modules/Notifications/Notifications.Application/
COPY src/Modules/Notifications/Notifications.Infrastructure/Notifications.Infrastructure.csproj src/Modules/Notifications/Notifications.Infrastructure/
COPY src/Modules/Calendar/Calendar.Domain/Calendar.Domain.csproj src/Modules/Calendar/Calendar.Domain/
COPY src/Modules/Calendar/Calendar.Application/Calendar.Application.csproj src/Modules/Calendar/Calendar.Application/
COPY src/Modules/Calendar/Calendar.Infrastructure/Calendar.Infrastructure.csproj src/Modules/Calendar/Calendar.Infrastructure/
COPY src/Modules/Messaging/Messaging.Domain/Messaging.Domain.csproj src/Modules/Messaging/Messaging.Domain/
COPY src/Modules/Messaging/Messaging.Application/Messaging.Application.csproj src/Modules/Messaging/Messaging.Application/
COPY src/Modules/Messaging/Messaging.Infrastructure/Messaging.Infrastructure.csproj src/Modules/Messaging/Messaging.Infrastructure/
COPY src/Modules/Scheduling/Scheduling.Domain/Scheduling.Domain.csproj src/Modules/Scheduling/Scheduling.Domain/
COPY src/Modules/Scheduling/Scheduling.Application/Scheduling.Application.csproj src/Modules/Scheduling/Scheduling.Application/
COPY src/Modules/Scheduling/Scheduling.Infrastructure/Scheduling.Infrastructure.csproj src/Modules/Scheduling/Scheduling.Infrastructure/
RUN dotnet restore

COPY . .
RUN dotnet publish src/Host/EduPlatform.Host.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "EduPlatform.Host.dll"]
