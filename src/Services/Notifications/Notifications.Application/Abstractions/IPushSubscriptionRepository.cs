using System;
using System.Collections.Generic;
using System.Text;
using Notifications.Domain.Entities;
namespace Notifications.Application.Abstractions
{
    public interface IPushSubscriptionRepository
    {
        Task<IReadOnlyCollection<PushSubscription>> GetForUserAsync(Guid userId);
        Task AddAsync(PushSubscription subscription);

    }
}
