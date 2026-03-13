using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Inventory.Infrastructure.Design
{

    public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
    {
        public InventoryDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();

            // Use env var or fallback; EF needs *some* connection string at design time.
            var conn = "Host=localhost;Port=5432;Database=inventory;Username=postgres;Password=postgres";
            optionsBuilder.UseNpgsql(conn);

            return new InventoryDbContext(optionsBuilder.Options);
        }
    }

}
