using Inventory.Core.Entities;

namespace Inventory.Models
{
    public sealed record CreateSkuRequest(string Sku, long InitialAvailable);
    public sealed record AdjustRequest(long DeltaAvailable);


    public sealed record InventoryDto(
        string Sku,
        long Available,
        long Reserved,
        long Version,
        DateTimeOffset UpdatedAt)
    {
        public static InventoryDto FromDomain(InventorySku e)
            => new(e.Sku, e.Available, e.Reserved, e.Version, e.UpdatedAt);
    }

}
