using Inventory.Core.Services;
using Inventory.Core.ValueObjects;
using Inventory.Infrastructure.Repositories;
using Inventory.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Endpoints
{

    public static class AdminV2Endpoints
    {
        public static IEndpointRouteBuilder MapAdminV2Endpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/admin").WithTags("Admin");

            // GET /api/v2/admin/inventory
            group.MapGet("/inventory", async Task<Ok<PagedResponse<InventoryListItemDto>>> (
                string? q, string? sortBy, string? sortDir, int? page, int? pageSize,
                [FromServices]IInventoryAdminReader reader, CancellationToken ct) =>
            {
                var result = await reader.ListSkusAsync(q, sortBy, sortDir, page ?? 1, pageSize ?? 50, ct);
                var dto = PagedResponse<InventoryListItemDto>.From(result, InventoryListItemDto.FromDomain);
                return TypedResults.Ok(dto);
            })
            .WithSummary("List SKUs with pagination, search, sorting");

            // GET /api/v2/admin/reservations/by-order/{orderId}
            group.MapGet("/reservations/by-order/{orderId}", async Task<Ok<PagedResponse<ReservationDto>>> (
                string orderId, string? status, int? page, int? pageSize,
                [FromServices]IReservationreader reader, CancellationToken ct) =>
            {
                var result = await reader.ListByOrderAsync(orderId, status, page ?? 1, pageSize ?? 50, ct);
                var dto = PagedResponse<ReservationDto>.From(result, ReservationDto.FromDomain);
                return TypedResults.Ok(dto);
            })
            .WithSummary("List reservations for an order");

            // GET /api/v2/admin/reservations/by-sku/{sku}
            group.MapGet("/reservations/by-sku/{sku}", async Task<Ok<PagedResponse<ReservationDto>>> (
                string sku, string? status, int? page, int? pageSize,
                [FromServices]IReservationreader reader, CancellationToken ct) =>
            {
                var result = await reader.ListBySkuAsync(new Sku(sku), status, page ?? 1, pageSize ?? 50, ct);
                var dto = PagedResponse<ReservationDto>.From(result, ReservationDto.FromDomain);
                return TypedResults.Ok(dto);
            })
            .WithSummary("List reservations for a SKU");

            // GET /api/v2/admin/reservations/pending
            group.MapGet("/reservations/pending", async Task<Results<Ok<PagedResponse<ReservationDto>>, BadRequest<string>>> (
                DateTimeOffset? expiresBefore, int? olderThanMinutes, int? page, int? pageSize,
                [FromServices] IReservationreader reader, CancellationToken ct) =>
            {
                DateTimeOffset cutoff;
                if (expiresBefore.HasValue) cutoff = expiresBefore.Value;
                else if (olderThanMinutes.HasValue) cutoff = DateTimeOffset.UtcNow.AddMinutes(olderThanMinutes.Value);
                else return TypedResults.BadRequest("Provide either expiresBefore or olderThanMinutes");

                var result = await reader.ListPendingExpiringBeforeAsync(cutoff, page ?? 1, pageSize ?? 50, ct);
                var dto = PagedResponse<ReservationDto>.From(result, ReservationDto.FromDomain);
                return TypedResults.Ok(dto);
            })
            .WithSummary("List pending reservations expiring before a time");

            return app;
        }
    }

}
