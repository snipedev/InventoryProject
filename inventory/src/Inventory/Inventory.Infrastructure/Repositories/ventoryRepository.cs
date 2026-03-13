using System;
using System.Collections.Generic;
using System.Text;
using Inventory.Core.Abstraction;
using Inventory.Core.Entities;
using Inventory.Core.Errors;
using Inventory.Core.Services;
using Inventory.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories
{
    public sealed class InventoryRepository : IInventoryReader, IInventoryWriter
    {
        private readonly InventoryDbContext _db;
        private readonly IDateTimeProvider _clock;


        public InventoryRepository(InventoryDbContext db, IDateTimeProvider clock)
        {
            _db = db;
            _clock = clock;
        }


        public async Task<InventorySku?> GetSkuAsync(Sku sku, CancellationToken ct = default)
                => await _db.Skus.AsNoTracking().FirstOrDefaultAsync(x => x.Sku == sku, ct);


        public async Task<Result<InventorySku>> CreateSkuAsync(Sku sku, long initialAvailable, CancellationToken ct = default)
        {
            // quick exist check (optional)
            var exists = await _db.Skus.AsNoTracking().AnyAsync(x => x.Sku == sku, ct);
            if (exists)
                return Result<InventorySku>.Fail(ErrorCodes.SkuAlreadyExists);

            try
            {
                var entity = InventorySku.Create(sku, initialAvailable, _clock.UtcNow);
                _db.Skus.Add(entity);
                await _db.SaveChangesAsync(ct);
                return Result<InventorySku>.Ok(entity);
            }
            catch (DbUpdateException)
            {
                // in case of race (unique PK violation) fallback to fail
                return Result<InventorySku>.Fail(ErrorCodes.SkuAlreadyExists);
            }
        }

        public async Task<Result<InventorySku>> AdjustAvailableAsync(Sku sku, long delta, CancellationToken ct = default)
        {
            var entity = await _db.Skus.FirstOrDefaultAsync(x => x.Sku == sku, ct);
            if (entity is null)
                return Result<InventorySku>.Fail(ErrorCodes.SkuNotFound);

            try
            {
                entity.AdjustAvailable(delta, _clock.UtcNow);
                await _db.SaveChangesAsync(ct);
                return Result<InventorySku>.Ok(entity);
            }
            catch (DbUpdateConcurrencyException)
            {
                // someone updated the row; caller may retry
                return Result<InventorySku>.Fail("CONCURRENCY_CONFLICT");
            }
            catch (InvalidOperationException ex) when (ex.Message == "INSUFFICIENT_AVAILABLE")
            {
                return Result<InventorySku>.Fail(ErrorCodes.InsufficientAvailable);
            }
        }
    }
}
