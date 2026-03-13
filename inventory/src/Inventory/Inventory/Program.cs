using Inventory.Health;
using Inventory.Infrastructure;
using Inventory.Infrastructure.DI;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Inventory.Endpoints;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        //add all necessary domain models
        builder.Services.AddInventoryInfrastructure(config: builder.Configuration);

        builder.Services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("db", HealthStatus.Degraded);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        var app = builder.Build();

        //await app.Services.ApplyMigrationsAsync();

// Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapInventoryEndpoints();

        app.MapHealthChecks("/health");

        app.Run();
    }
}