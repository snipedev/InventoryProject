using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Inventory.Core.Abstraction;
using Inventory.Core.Entities;
using Inventory.Core.Errors;
using Inventory.Core.Services;
using Inventory.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories
{
    #region reservationreader
    public sealed class ReservationReader : IReservationreader
    {
        private readonly InventoryDbContext _db;
        public ReservationReader(InventoryDbContext db) => _db = db;

        public async Task<PagedResult<InventoryReservation>> ListByOrderAsync(
            string orderId, string? status, int page, int pageSize, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;

            var query = _db.Set<InventoryReservation>().AsNoTracking().Where(r => r.OrderId == orderId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                var st = status.Trim().ToUpperInvariant();
                query = query.Where(r => r.Status.ToString() == st);
            }

            query = query.OrderByDescending(r => r.CreatedAt);

            var total = await query.LongCountAsync(ct);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return new PagedResult<InventoryReservation> { Items = items, Page = page, PageSize = pageSize, Total = total };
        }

        public async Task<PagedResult<InventoryReservation>> ListBySkuAsync(
            Sku sku, string? status, int page, int pageSize, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;

            var query = _db.Set<InventoryReservation>().AsNoTracking().Where(r => r.Sku == sku);

            if (!string.IsNullOrWhiteSpace(status))
            {
                var st = status.Trim().ToUpperInvariant();
                query = query.Where(r => r.Status.ToString() == st);
            }

            query = query.OrderByDescending(r => r.CreatedAt);

            var total = await query.LongCountAsync(ct);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return new PagedResult<InventoryReservation> { Items = items, Page = page, PageSize = pageSize, Total = total };
        }

        public async Task<PagedResult<InventoryReservation>> ListPendingExpiringBeforeAsync(
            DateTimeOffset before, int page, int pageSize, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;

            var query = _db.Set<InventoryReservation>().AsNoTracking()
                           .Where(r => r.Status == ReservationStatus.PENDING && r.ExpiresAt <= before)
                           .OrderBy(r => r.ExpiresAt);

            var total = await query.LongCountAsync(ct);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return new PagedResult<InventoryReservation> { Items = items, Page = page, PageSize = pageSize, Total = total };
        }

    }
    #endregion

    #region reservationwriter

    public sealed class ReservationWriter : IReservationWriter
    {
        private readonly InventoryDbContext _db;
        private readonly IDateTimeProvider _clock;

        public ReservationWriter(InventoryDbContext db, IDateTimeProvider clock)
        {
            _db = db;
            _clock = clock;
        }

        public async Task<Result> ReserveAsync(string orderId, IEnumerable<(Sku sku, long qty)> items, TimeSpan ttl, CancellationToken ct = default)
        {
            var list = items?.ToList() ?? new();
            if (string.IsNullOrWhiteSpace(orderId) || list.Count == 0) return Result.Fail(ErrorCodes.InvalidInput);

            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                foreach (var (sku, qty) in list)
                {
                    var skuRow = await _db.Skus.FirstOrDefaultAsync(x => x.Sku == sku, ct);
                    if (skuRow is null) return Result.Fail(ErrorCodes.SkuNotFound);

                    // Ensure capacity
                    if ((skuRow.Available - skuRow.Reserved) < qty)
                    {
                        await AddOutboxAsync("reservation", orderId, "InventoryRejected", new
                        {
                            eventId = Guid.NewGuid().ToString(),
                            orderId,
                            reason = "INSUFFICIENT_STOCK",
                            failingSku = (string)sku
                        }, ct);

                        await _db.SaveChangesAsync(ct);
                        await tx.CommitAsync(ct);
                        return Result.Fail(ErrorCodes.InsufficientAvailable);
                    }

                    // Increment reserved
                    skuRow.SetReserved(skuRow.Reserved + qty,_clock.UtcNow);

                    // Insert reservation row (idempotent by unique (order_id, sku))
                    var entity = new InventoryReservation(
                        id: Guid.NewGuid(),
                        orderId: orderId,
                        sku: sku,
                        qty: qty,
                        status: ReservationStatus.PENDING,
                        expiresAt: _clock.UtcNow.Add(ttl),
                        createdAt: _clock.UtcNow
                    );

                    // ON CONFLICT in DB will protect duplicates, but EF path: try add and ignore duplicate errors as success
                    _db.Add(entity);
                }

                await AddOutboxAsync("reservation", orderId, "InventoryReserved", new
                {
                    eventId = Guid.NewGuid().ToString(),
                    orderId,
                    reserved = list.Select(i => new { sku = (string)i.sku, qty = i.qty }),
                    expiresAt = _clock.UtcNow.Add(ttl)
                }, ct);

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return Result.Ok();
            }
            catch (DbUpdateException ex)
            {
                // Log the actual exception to see what's failing
                await tx.RollbackAsync(ct);
                
                // Check if it's truly a duplicate key constraint (23505 in PostgreSQL)
                if (ex.InnerException?.Message?.Contains("23505") == true || 
                    ex.InnerException?.Message?.Contains("duplicate key") == true)
                {
                    // Idempotent success for duplicate constraint
                    return Result.Ok();
                }
                
                // Otherwise, this is a real error - don't hide it
                throw;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<Result> CommitAsync(string orderId, CancellationToken ct = default)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var reservations = await _db.Set<InventoryReservation>()
                    .Where(r => r.OrderId == orderId && r.Status == ReservationStatus.PENDING)
                    .ToListAsync(ct);

                if (reservations.Count == 0)
                {
                    await tx.CommitAsync(ct);
                    return Result.Ok(); // idempotent
                }

                foreach (var r in reservations)
                {
                    var skuRow = await _db.Skus.FirstAsync(x => x.Sku == r.Sku, ct);
                    skuRow.AdjustAvailable(-r.Qty,_clock.UtcNow);
                    skuRow.SetReserved(CalculateReserved(skuRow, r), _clock.UtcNow);

                    r.MarkCommitted();
                }

                await AddOutboxAsync("reservation", orderId, "InventoryCommitted", new
                {
                    eventId = Guid.NewGuid().ToString(),
                    orderId,
                    committed = reservations.Select(i => new { sku = (string)i.Sku, qty = i.Qty })
                }, ct);

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return Result.Ok();
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<Result> ReleaseAsync(string orderId, CancellationToken ct = default)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var reservations = await _db.Set<InventoryReservation>()
                    .Where(r => r.OrderId == orderId && r.Status == ReservationStatus.PENDING)
                    .ToListAsync(ct);

                if (reservations.Count == 0)
                {
                    await tx.CommitAsync(ct);
                    return Result.Ok();
                }

                foreach (var r in reservations)
                {
                    var skuRow = await _db.Skus.FirstAsync(x => x.Sku == r.Sku, ct);
                    var reservedQuantity = CalculateReserved(skuRow, r);
                    skuRow.SetReserved(reservedQuantity, _clock.UtcNow);

                    r.MarkReleased();
                }

                await AddOutboxAsync("reservation", orderId, "InventoryReleased", new
                {
                    eventId = Guid.NewGuid().ToString(),
                    orderId,
                    released = reservations.Select(i => new { sku = (string)i.Sku, qty = i.Qty }),
                    reason = "TIMEOUT_OR_PAYMENT_FAIL"
                }, ct);

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return Result.Ok();
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<int> ReleaseExpiredAsync(CancellationToken ct = default)
        {
            var now = _clock.UtcNow;

            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var expired = await _db.Set<InventoryReservation>()
                    .Where(r => r.Status == ReservationStatus.PENDING && r.ExpiresAt <= now)
                    .ToListAsync(ct);

                foreach (var r in expired)
                {
                    var skuRow = await _db.Skus.FirstAsync(x => x.Sku == r.Sku, ct);
                    var reservedQuantity = CalculateReserved(skuRow,r);
                    skuRow.SetReserved(reservedQuantity, _clock.UtcNow);

                    r.MarkReleased();

                    await AddOutboxAsync("reservation", r.OrderId, "InventoryReleased", new
                    {
                        eventId = Guid.NewGuid().ToString(),
                        orderId = r.OrderId,
                        released = new[] { new { sku = (string)r.Sku, qty = r.Qty } },
                        reason = "EXPIRED"
                    }, ct);
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return expired.Count;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        private async Task AddOutboxAsync(string aggregateType, string aggregateId, string eventType, object payload, CancellationToken ct)
        {
            var evt = new OutboxEvent
            {
                Id = Guid.NewGuid(),
                AggregateType = aggregateType,
                AggregateId = aggregateId,
                EventType = eventType,
                Payload = JsonSerializer.SerializeToDocument(payload),
                CreatedAt = _clock.UtcNow
            };
            await _db.AddAsync(evt, ct);
        }

        private long CalculateReserved(InventorySku sku, InventoryReservation r) 
        {
            var reservedQuantity = sku.Reserved - r.Qty;
            if (reservedQuantity < 0) 
            {
                return 0;
            }
            return reservedQuantity;
        }
    }

    #endregion

}
