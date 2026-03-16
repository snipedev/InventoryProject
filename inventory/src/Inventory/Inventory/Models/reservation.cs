using Inventory.Core.Entities;
using Inventory.Core.ValueObjects;

namespace Inventory.Models
{
    /// <summary>
    /// request for reservation class
    /// 
    /// </summary>
    public record ReservationRequest
    {
        public int TtlMinutes { get; set; }

        public ICollection<Items> Items { get; set; }
    }

    public record Items
    {
        public Sku Sku { get; set; }
        public long Qty { get; set; }
    }
}
