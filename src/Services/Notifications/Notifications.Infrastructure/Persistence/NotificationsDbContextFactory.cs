using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Notifications.Infrastructure.Persistence
{
    internal class NotificationsDbContextFactory: IDesignTimeDbContextFactory<NotificationsDbContext>
    {
        public NotificationsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NotificationsDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=miamatch;Username=miamatch;Password=miamatch");

            return new NotificationsDbContext(optionsBuilder.Options);
        }
    }        
}
