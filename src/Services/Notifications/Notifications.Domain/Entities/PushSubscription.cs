using System;
using System.Collections.Generic;
using System.Text;

namespace Notifications.Domain.Entities
{
    public class PushSubscription
    {
        public Guid Id { get; }
        public Guid UserId { get; }
        public string Endpoint { get; }
        public string P256dhKey { get; }
        public string AuthKey { get; }
        public DateTime CreatedAt { get; }

        private PushSubscription(Guid id, Guid userId, string endpoint, string p256dhKey, string authKey, DateTime createdAt)
        {
            Id = id;
            UserId = userId;
            Endpoint = endpoint;
            P256dhKey = p256dhKey;
            AuthKey = authKey;
            CreatedAt = createdAt;
        }

        public static PushSubscription Create(Guid userId, string endpoint, string p256dhKey, string authKey)
        {
            Guid id = Guid.NewGuid();
            DateTime createdAt = DateTime.UtcNow;
            return new PushSubscription(id, userId, endpoint, p256dhKey, authKey, createdAt);
        }        
    }
}
