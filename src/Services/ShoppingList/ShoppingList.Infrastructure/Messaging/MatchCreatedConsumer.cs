using System;
using System.Collections.Generic;
using System.Text;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShoppingList.Application.ShoppingListItems;

namespace ShoppingList.Infrastructure.Messaging
{
    public class MatchCreatedConsumer: BackgroundService
    {
        private readonly IAmazonSQS _sqs;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public MatchCreatedConsumer(IAmazonSQS sqs, IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _sqs = sqs;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queueUrl = _configuration["SQS:MatchCreatedQueueUrl"];
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var response = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
                    {
                        QueueUrl = queueUrl,
                        MaxNumberOfMessages = 5,
                        WaitTimeSeconds = 20
                    }, stoppingToken);

                    foreach (var message in response.Messages ?? new List<Amazon.SQS.Model.Message>())
                    {
                        try
                        {
                            var matchCreatedMessage = System.Text.Json.JsonSerializer.Deserialize<MatchCreatedMessage>(message.Body);
                            if (matchCreatedMessage is not null)
                            {
                                using var scope = _scopeFactory.CreateScope();
                                var handler = scope.ServiceProvider.GetRequiredService<AddMatchedRecipeIngredientsHandler>();
                                await handler.ExecuteAsync(matchCreatedMessage.HouseholdId, matchCreatedMessage.RecipeId);
                            }
                            await _sqs.DeleteMessageAsync(queueUrl, message.ReceiptHandle, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[MatchCreatedConsumer] Erreur : {ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MatchCreatedConsumer] Erreur : {ex}");
                }
            }
        }

    }
    public sealed record MatchCreatedMessage
    {
        public Guid MatchId { get; set; }
        public Guid HouseholdId { get; set; }
        public Guid RecipeId { get; set; }
        public Guid DeckId { get; set; }
        public DateTime MatchedAt { get; set; }
    }
}
