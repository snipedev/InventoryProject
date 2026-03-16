using Inventory.Core.Abstraction;
using Inventory.Core.Entities;

namespace Inventory.Models
{

    public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, long Total)
    {
        public static PagedResponse<T> From<TDomain>(PagedResult<TDomain> r, Func<TDomain, T> map) =>
            new(r.Items.Select(map).ToList(), r.Page, r.PageSize, r.Total);
    }

    public sealed record InventoryListItemDto(string Sku, long Available, long Reserved, long Version, DateTimeOffset UpdatedAt)
    {
        public static InventoryListItemDto FromDomain(InventorySku s) =>
            new(s.Sku, s.Available, s.Reserved, s.Version, s.UpdatedAt);
    }

    public sealed record ReservationDto(Guid Id, string OrderId, string Sku, long Qty, string Status, DateTimeOffset ExpiresAt, DateTimeOffset CreatedAt)
    {
        public static ReservationDto FromDomain(InventoryReservation r) =>
            new(r.Id, r.OrderId, r.Sku, r.Qty, r.Status.ToString(), r.ExpiresAt, r.CreatedAt);
    }

}
