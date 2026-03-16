using Inventory.Core.Services;
using Inventory.Core.ValueObjects;
using Inventory.Core.Errors;
using Microsoft.AspNetCore.Http.HttpResults;
using Inventory.Models;
using Asp.Versioning;
using Asp.Versioning.Builder;

namespace Inventory.Endpoints;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("/inventory")
            .WithTags("Inventory");

        // GET /v1/inventory/{sku}
        group.MapGet("/{sku}", async Task<Results<Ok<InventoryDto>, NotFound<string>>> (
            string sku, IInventoryReader reader, CancellationToken ct) =>
        {
            var entity = await reader.GetSkuAsync(new Sku(sku), ct);
            return entity is null
                ? TypedResults.NotFound(ErrorCodes.SkuNotFound)
                : TypedResults.Ok(InventoryDto.FromDomain(entity));
        });

        // POST /v1/inventory
        group.MapPost("/", async Task<Results<Created<InventoryDto>, Conflict<string>, BadRequest<string>>> (
            CreateSkuRequest req, IInventoryWriter writer, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Sku) || req.InitialAvailable < 0)
                return TypedResults.BadRequest(ErrorCodes.InvalidInput);

            var result = await writer.CreateSkuAsync(new Sku(req.Sku), req.InitialAvailable, ct);
            if (!result.IsSuccess)
            {
                return result.Error switch
                {
                    ErrorCodes.SkuAlreadyExists => TypedResults.Conflict(ErrorCodes.SkuAlreadyExists),
                    _ => TypedResults.BadRequest(result.Error ?? ErrorCodes.InvalidInput)
                };
            }

            var dto = InventoryDto.FromDomain(result.Value!);
            return TypedResults.Created($"/v1/inventory/{dto.Sku}", dto);
        });

        // POST /v1/inventory/{sku}/adjust
        group.MapPost("/{sku}/adjust", async Task<Results<Ok<InventoryDto>, NotFound<string>, BadRequest<string>>> (
            string sku, AdjustRequest req, IInventoryWriter writer, CancellationToken ct) =>
        {
            // delta can be negative, but cannot make available < 0 (enforced by domain)
            var result = await writer.AdjustAvailableAsync(new Sku(sku), req.DeltaAvailable, ct);

            if (!result.IsSuccess)
            {
                return result.Error switch
                {
                    ErrorCodes.SkuNotFound => TypedResults.NotFound(ErrorCodes.SkuNotFound),
                    ErrorCodes.InsufficientAvailable => TypedResults.BadRequest(ErrorCodes.InsufficientAvailable),
                    "CONCURRENCY_CONFLICT" => TypedResults.BadRequest("CONCURRENCY_CONFLICT"),
                    _ => TypedResults.BadRequest(result.Error ?? ErrorCodes.InvalidInput)
                };
            }

            return TypedResults.Ok(InventoryDto.FromDomain(result.Value!));
        });

        return app;
    }
}