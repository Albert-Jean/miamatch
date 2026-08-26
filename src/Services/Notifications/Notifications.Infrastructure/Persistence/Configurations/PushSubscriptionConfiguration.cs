using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notifications.Domain.Entities;

namespace Notifications.Infrastructure.Persistence.Configurations
{
    public class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
    {
        public void Configure(EntityTypeBuilder<PushSubscription> builder)
        {
            builder.ToTable("push_subscriptions");
            builder.HasKey(p=>p.Id);
            builder.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(p => p.Endpoint).HasColumnName("endpoint").IsRequired();
            builder.Property(p => p.P256dhKey).HasColumnName("p256dh_key").IsRequired();
            builder.Property(p => p.AuthKey).HasColumnName("auth_key").IsRequired();
            builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        }       
    }
}
