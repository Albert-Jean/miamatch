using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Matching.Application.Abstractions;
using Matching.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Matching.Infrastructure.Persistence.Messaging
{
    public class MatchEventPublisher : IMatchEventPublisher
    {
        private readonly IConfiguration _configuration;
        private readonly IAmazonSimpleNotificationService _sns;        

        public MatchEventPublisher(IAmazonSimpleNotificationService sns, IConfiguration configuration)
        {
            _sns = sns;
            _configuration = configuration;
        }

        public async Task PublishMatchCreatedAsync(Match match)
        {
            MatchCreatedMessage message = new MatchCreatedMessage
            {
                MatchId = match.Id,
                HouseholdId = match.HouseholdId,
                RecipeId = match.RecipeId,
                DeckId = match.DeckId,
                MatchedAt = match.MatchedAt
            };
            var json = System.Text.Json.JsonSerializer.Serialize(message);
            await _sns.PublishAsync(new PublishRequest
            {
                TopicArn = _configuration["Sns:MatchCreatedTopicArn"],
                Message = json
            });
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
