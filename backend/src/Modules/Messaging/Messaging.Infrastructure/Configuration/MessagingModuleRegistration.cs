using EduPlatform.Shared.Application.Contracts;
using Messaging.Application.Interfaces;
using Messaging.Infrastructure.Repositories;
using Messaging.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Messaging.Infrastructure.Configuration;

public static class MessagingModuleRegistration
{
    public static IServiceCollection AddMessagingModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB")
            ?? "mongodb://localhost:27017";

        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase("eduplatform");
        });
        services.AddSingleton<IChatConnectionTracker, ChatConnectionTracker>();

        services.AddScoped<IMessagingRepository, MongoMessagingRepository>();
        services.AddScoped<IChatBroadcaster, SignalRChatBroadcaster>();
        services.AddScoped<IChatAdmin, ChatAdminService>();

        return services;
    }
}
