using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShoppingList.Application.Abstractions;
using ShoppingList.Application.ShoppingListItems;
using ShoppingList.Infrastructure;
using ShoppingList.Infrastructure.Configuration;
using ShoppingList.Infrastructure.Persistence;
using ShoppingList.Infrastructure.Repositories;
[assembly: Amazon.Lambda.Core.LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace ShoppingList.Consumer;

public class Function
{
    private static readonly IServiceProvider _serviceProvider = BuildServiceProvider();

    private static IServiceProvider BuildServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddMiamMatchSecrets()
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddDbContext<ShoppingListDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ShoppingListDb")));
        services.AddScoped<IShoppingListItemRepository, ShoppingListItemRepository>();
        services.AddHttpClient<IRecipeClient, RecipeClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["Services:RecipesApiBaseUrl"]!);
        });
        services.AddScoped<AddMatchedRecipeIngredientsHandler>();

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
            var handler = scope.ServiceProvider.GetRequiredService<AddMatchedRecipeIngredientsHandler>();
            await handler.ExecuteAsync(matchCreatedMessage.HouseholdId, matchCreatedMessage.RecipeId);
        }
    }
}

public sealed record MatchCreatedMessage(Guid MatchId, Guid HouseholdId, Guid RecipeId, Guid DeckId, DateTime MatchedAt);