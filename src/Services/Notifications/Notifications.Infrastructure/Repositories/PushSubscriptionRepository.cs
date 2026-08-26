using System;
using System.Collections.Generic;
using System.Text;
using Notifications.Application.Abstractions;
using Notifications.Domain.Entities;
using Notifications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Notifications.Infrastructure.Repositories
{
    public class PushSubscriptionRepository: IPushSubscriptionRepository
    {
        private readonly NotificationsDbContext _context;
        public PushSubscriptionRepository(NotificationsDbContext context)
        {
            _context = context;
        }
        public async Task<IReadOnlyCollection<PushSubscription>> GetForUserAsync(Guid userId)
        {
            var result = await _context.PushSubscriptions.Where(p => p.UserId == userId).ToListAsync();
            return result;
        }
        public async Task AddAsync(PushSubscription subscription)
        {
            await _context.AddAsync(subscription);
            await _context.SaveChangesAsync();
        }
    }
}
