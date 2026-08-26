using System;
using System.Collections.Generic;
using System.Text;
using Notifications.Application.Abstractions;

namespace Notifications.Application.Notifications
{
    public class SendMatchNotificationHandler
    {
        private readonly IHouseholdMembersClient _householdMembersClient;
        private readonly IPushSubscriptionRepository _pushSubscriptionRepository;
        private readonly IPushNotificationSender _notificationSender;
        public SendMatchNotificationHandler(IHouseholdMembersClient householdMembersClient, IPushSubscriptionRepository pushSubscriptionRepository, IPushNotificationSender pushNotificationSender)
        {
            _householdMembersClient = householdMembersClient;
            _pushSubscriptionRepository = pushSubscriptionRepository;
            _notificationSender = pushNotificationSender;
        }

        public async Task ExecuteAsync(Guid householdId, Guid recipeId)
        {
            var userIds = await _householdMembersClient.GetMembersAsync(householdId);
            var payloadObject = new PushPayload("Nouveau match !", "Vous avez trouvé une recette en commun.");
            var payload = System.Text.Json.JsonSerializer.Serialize(payloadObject);
            foreach ( var userId in userIds)
            {
                var subscriptions = await _pushSubscriptionRepository.GetForUserAsync(userId);
                foreach( var subscription in subscriptions)
                {
                    try
                    {
                        await _notificationSender.SendAsync(subscription, payload);
                    }
                    catch (Exception)
                    {
                        // abonnement probablement expiré/invalide, on continue avec les autres
                    }
                }
            }
        }
    }
    public sealed record PushPayload(string Title, string Body);
}
