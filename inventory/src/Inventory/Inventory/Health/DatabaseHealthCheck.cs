using Inventory.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Inventory.Health;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly InventoryDbContext _context;

    public DatabaseHealthCheck(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to execute a simple query to check database connectivity
            await _context.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy("Database connection is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}
