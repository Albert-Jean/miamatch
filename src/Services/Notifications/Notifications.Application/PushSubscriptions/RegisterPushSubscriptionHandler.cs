using System;
using System.Collections.Generic;
using System.Text;
using Notifications.Application.Abstractions;
using Notifications.Domain.Entities;
namespace Notifications.Application.PushSubscriptions
{
    public class RegisterPushSubscriptionHandler
    {
        private readonly IPushSubscriptionRepository _pushSubscriptionRepository;
        public RegisterPushSubscriptionHandler(IPushSubscriptionRepository pushSubscriptionRepository)
        {
            _pushSubscriptionRepository= pushSubscriptionRepository;
        }
        public async Task ExecuteAsync(Guid userId,string endpoint,string p256dhKey, string authKey)
        {
            var pushSubscription = PushSubscription.Create(userId,endpoint,p256dhKey,authKey);
            await _pushSubscriptionRepository.AddAsync(pushSubscription);
        }
    }
}
