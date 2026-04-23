var builder = DistributedApplication.CreateBuilder(args);

// Backend запускается как проектный ресурс Aspire.
// Инфраструктура (PostgreSQL, MongoDB, MinIO) поднимается отдельно через docker-compose,
// а бэкенд подключается к ней по connection strings из appsettings.json (localhost:5432 и т.д.).
// Фиксированный порт 5000 — совпадает с docker-compose и с launchSettings.json,
// чтобы фронт всегда обращался на один и тот же адрес.
builder.AddProject<Projects.EduPlatform_Host>("eduplatform-host")
    .WithEndpoint("http", endpoint => endpoint.IsProxied = false);

builder.Build().Run();
