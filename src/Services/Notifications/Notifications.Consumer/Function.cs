using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Application.Abstractions;
using Notifications.Application.Notifications;
using Notifications.Infrastructure.Client;
using Notifications.Infrastructure.Clients;
using Notifications.Infrastructure.Persistence;
using Notifications.Infrastructure.Repositories;
[assembly: Amazon.Lambda.Core.LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Notifications.Consumer;

public class Function
{
    private static readonly IServiceProvider _serviceProvider = BuildServiceProvider();

    private static IServiceProvider BuildServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("NotificationsDb")));
        services.AddScoped<IPushSubscriptionRepository, PushSubscriptionRepository>();
        services.AddHttpClient<IHouseholdMembersClient, HouseholdMembersClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["Services:UsersApiBaseUrl"]!);
        });
        services.AddSingleton<IPushNotificationSender, WebPushNotificationSender>();
        services.AddScoped<SendMatchNotificationHandler>();

        return services.BuildServiceProvider();
    }

    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        foreach (var message in sqsEvent.Records)
        {
            var matchCreatedMessage = JsonSerializer.Deserialize<MatchCreatedMessage>(message.Body);
            if (matchCreatedMessage is null)
            {
                continue;
            }

            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<SendMatchNotificationHandler>();
            await handler.ExecuteAsync(matchCreatedMessage.HouseholdId, matchCreatedMessage.RecipeId);
        }
    }
}

public sealed record MatchCreatedMessage(Guid MatchId, Guid HouseholdId, Guid RecipeId, Guid DeckId, DateTime MatchedAt);
