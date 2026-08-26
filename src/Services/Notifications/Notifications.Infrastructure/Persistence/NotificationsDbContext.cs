using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Entities;

namespace Notifications.Infrastructure.Persistence
{
    public class NotificationsDbContext: DbContext
    {
        public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) { }
        public DbSet<PushSubscription> PushSubscriptions { get; set; }       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("notifications");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
        }
    }
}
