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
RUN dotnet restore

COPY . .
RUN dotnet publish src/Host/EduPlatform.Host.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "EduPlatform.Host.dll"]
