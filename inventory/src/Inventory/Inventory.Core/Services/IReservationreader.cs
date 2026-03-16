using System;
using System.Collections.Generic;
using System.Text;
using Inventory.Core.Abstraction;
using Inventory.Core.Entities;
using Inventory.Core.ValueObjects;

namespace Inventory.Core.Services
{
    public interface IReservationreader
    {

        Task<PagedResult<InventoryReservation>> ListByOrderAsync(
                string orderId, string? status, int page, int pageSize, CancellationToken ct = default);

        Task<PagedResult<InventoryReservation>> ListBySkuAsync(
            Sku sku, string? status, int page, int pageSize, CancellationToken ct = default);

        Task<PagedResult<InventoryReservation>> ListPendingExpiringBeforeAsync(
            DateTimeOffset before, int page, int pageSize, CancellationToken ct = default);

    }

    public interface IReservationWriter
    {
        Task<Result> ReserveAsync(string orderId, IEnumerable<(Sku sku, long qty)> items, TimeSpan ttl, CancellationToken ct = default);

        Task<Result> CommitAsync(string orderId, CancellationToken ct = default);


        Task<Result> ReleaseAsync(string orderId, CancellationToken ct = default);

        /// Release expired PENDING reservations whose expires_at <= now. Returns number released.
        Task<int> ReleaseExpiredAsync(CancellationToken ct = default);

    }


    public interface IInventoryAdminReader
    {
        Task<PagedResult<InventorySku>> ListSkusAsync(
            string? q,
            string? sortBy,   // "sku" | "available" | "reserved" | "updatedAt"
            string? sortDir,  // "asc" | "desc"
            int page,
            int pageSize,
            CancellationToken ct = default);
    }

}
