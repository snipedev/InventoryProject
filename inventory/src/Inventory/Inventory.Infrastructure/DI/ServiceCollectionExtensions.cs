using Inventory.Core.Abstraction;
using Inventory.Core.Services;
using Inventory.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure.DI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var conn = config.GetConnectionString("Db") ?? throw new InvalidOperationException("Connectionstring: Db not configured");

            services.AddDbContext<InventoryDbContext>(opt => {
                opt.UseNpgsql(conn, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(2), null);
                });
            });


            services.AddSingleton<IDateTimeProvider, SystemDateTiemProvider>();

            // Repositories / Services
            services.AddScoped<IInventoryReader, InventoryRepository>();
            services.AddScoped<IInventoryWriter, InventoryRepository>();


            return services;
        }
    }
}
