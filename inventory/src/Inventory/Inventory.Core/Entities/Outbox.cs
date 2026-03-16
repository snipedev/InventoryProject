using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Inventory.Core.Entities
{

    public class OutboxEvent
    {
        public Guid Id { get; set; }                           // DB PK
        public string AggregateType { get; set; } = default!;
        public string AggregateId { get; set; } = default!;
        public string EventType { get; set; } = default!;
        public JsonDocument Payload { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
    }

}
