using Inventory.Core.Abstraction;
using Inventory.Core.Entities;
using Inventory.Core.Services;
using Inventory.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories
{

    public sealed class InventoryAdminReader : IInventoryAdminReader
    {
        private readonly InventoryDbContext _db;
        public InventoryAdminReader(InventoryDbContext db) => _db = db;

        public async Task<PagedResult<InventorySku>> ListSkusAsync(
            string? q, string? sortBy, string? sortDir, int page, int pageSize, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;

            var query = _db.Skus.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(x => EF.Functions.ILike(x.Sku, $"%{term}%"));
            }

            var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy?.ToLowerInvariant()) switch
            {
                "available" => descending ? query.OrderByDescending(x => x.Available).ThenBy(x => x.Sku)
                                          : query.OrderBy(x => x.Available).ThenBy(x => x.Sku),
                "reserved" => descending ? query.OrderByDescending(x => x.Reserved).ThenBy(x => x.Sku)
                                          : query.OrderBy(x => x.Reserved).ThenBy(x => x.Sku),
                "updatedat" => descending ? query.OrderByDescending(x => x.UpdatedAt).ThenBy(x => x.Sku)
                                          : query.OrderBy(x => x.UpdatedAt).ThenBy(x => x.Sku),
                _ => descending ? query.OrderByDescending(x => x.Sku)
                                          : query.OrderBy(x => x.Sku)
            };

            var total = await query.LongCountAsync(ct);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return new PagedResult<InventorySku> { Items = items, Page = page, PageSize = pageSize, Total = total };
        }

    }

    //reservation reader need to move to reservation rep
}
