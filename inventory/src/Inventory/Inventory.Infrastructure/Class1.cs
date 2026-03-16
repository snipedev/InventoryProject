using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure
{
    public class InventoryDbContext : DbContext
    {
        public DbSet<InventorySku> Skus => Set<InventorySku>();
        public DbSet<InventoryReservation> InventoryReservations => Set<InventoryReservation>();
        public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

        public InventoryDbContext(DbContextOptions<InventoryDbContext> opts) : base(opts) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
