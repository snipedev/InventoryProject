using Inventory.Endpoints;
using Inventory.Health;
using Inventory.Infrastructure.DI;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)    // picks Serilog from appsettings.json
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .WriteTo.Console() // fallback to console if no config
            .CreateLogger();

        builder.Host.UseSerilog();


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

        //----------versioning----------------//
        var v1 = app.MapGroup("/api/v1");
        var v2 = app.MapGroup("/api/v2");

        v1.MapInventoryEndpoints();
        // need to add reservation endpoints

        v2.MapInventoryEndpoints();
        v2.MapAdminV2Endpoints();
        v2.MapReservationEndpoints();

        app.MapInventoryEndpoints();


        app.UseSerilogRequestLogging(opts =>
        {
            opts.EnrichDiagnosticContext = (diagCtx, httpCtx) =>
            {
                diagCtx.Set("ClientIP", httpCtx.Connection.RemoteIpAddress?.ToString());
                diagCtx.Set("UserAgent", httpCtx.Request.Headers.UserAgent.ToString());
            };
        });


        app.MapHealthChecks("/health");

        app.Run();
    }
}