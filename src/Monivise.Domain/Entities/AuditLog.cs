using System;
using System.Collections.Generic;
using System.Text;

namespace Monivise.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public string Action { get; private set; } = string.Empty;
        public string EntityType { get; private set; } = string.Empty;
        public Guid? EntityId { get; private set; }
        public string Payload { get; private set; } = string.Empty;
        public string IpAddress { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        protected AuditLog() { }

        public static AuditLog Create(Guid userId, string action, string entityType,
            Guid? entityId, string payload, string ipAddress = "")
        {
            return new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Payload = payload,
                IpAddress = ipAddress
            };
        }
    }
}
