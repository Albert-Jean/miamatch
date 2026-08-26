using Microsoft.Extensions.Configuration;
using Notifications.Application.Abstractions;
using WebPush;
using DomainPushSubscription = Notifications.Domain.Entities.PushSubscription;

namespace Notifications.Infrastructure.Client;

public class WebPushNotificationSender : IPushNotificationSender
{
    private readonly VapidDetails _vapidDetails;
    private readonly WebPushClient _webPushClient;

    public WebPushNotificationSender(IConfiguration configuration)
    {
        _vapidDetails = new VapidDetails(
            configuration["Vapid:Subject"],
            configuration["Vapid:PublicKey"],
            configuration["Vapid:PrivateKey"]);

        _webPushClient = new WebPushClient();
    }

    public async Task SendAsync(DomainPushSubscription subscription, string payload)
    {
        var webPushSubscription = new PushSubscription(
            subscription.Endpoint,
            subscription.P256dhKey,
            subscription.AuthKey);

        await _webPushClient.SendNotificationAsync(webPushSubscription, payload, _vapidDetails);
    }
}
