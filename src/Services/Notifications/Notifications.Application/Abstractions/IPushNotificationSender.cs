using System;
using System.Collections.Generic;
using System.Text;
using Notifications.Domain.Entities;

namespace Notifications.Application.Abstractions
{
    public interface IPushNotificationSender
    {
       Task SendAsync(PushSubscription subscription, string payload);
    }
}
