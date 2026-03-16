using System.Runtime.CompilerServices;
using Inventory.Core.Services;
using Inventory.Core.ValueObjects;
using Inventory.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Endpoints
{
    public static class ReservationEndpoints
    {

        public static IEndpointRouteBuilder MapReservationEndpoints(this IEndpointRouteBuilder app)
        {
            // You can mount this group under a versioned prefix (e.g., /api/v1)
            var group = app.MapGroup("/inventory/reservations")
                           .WithTags("Reservations");

            // POST /inventory/reservations/{orderId}/reserve
            group.MapPost("/{orderId}/reserve", ReserveAsync)
                 .WithName("ReserveInventory")
                 .WithSummary("Create a temporary reservation (PENDING) for an order")
                 .WithDescription("Reserves stock for the given order with a TTL. Emits InventoryReserved or InventoryRejected via outbox.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces<string>(StatusCodes.Status400BadRequest);

            // POST /inventory/reservations/{orderId}/commit
            group.MapPost("/{orderId}/commit", CommitAsync)
                 .WithName("CommitReservation")
                 .WithSummary("Commit a reservation after payment success")
                 .WithDescription("Deducts available, reduces reserved, and marks reservations COMMITTED. Emits InventoryCommitted via outbox.")
                 .Produces(StatusCodes.Status200OK);

            // POST /inventory/reservations/{orderId}/release
            group.MapPost("/{orderId}/release", ReleaseAsync)
                 .WithName("ReleaseReservation")
                 .WithSummary("Release a reservation after payment failure or cancellation")
                 .WithDescription("Frees reserved units and marks reservations RELEASED. Emits InventoryReleased via outbox.")
                 .Produces(StatusCodes.Status200OK);

            return app;
        }

        /// Handlers ///
        /// 


        private static async Task<Results<Ok, BadRequest<string>>> ReserveAsync(
                string orderId,
                [FromBody] ReservationRequest req,
                [FromServices] IReservationWriter writer,
                CancellationToken ct)
        {
            // basic input validation
            if (string.IsNullOrWhiteSpace(orderId))
                return TypedResults.BadRequest("INVALID_ORDER_ID");

            if (req is null || req.Items is null || req.Items.Count == 0)
                return TypedResults.BadRequest("INVALID_INPUT");

            if (req.TtlMinutes < 0)
                return TypedResults.BadRequest("INVALID_TTL");

            foreach (var item in req.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Sku))
                    return TypedResults.BadRequest("INVALID_SKU");
                if (item.Qty <= 0)
                    return TypedResults.BadRequest("INVALID_QTY");
            }

            var ttl = TimeSpan.FromMinutes(req.TtlMinutes <= 0 ? 15 : req.TtlMinutes);
            var items = req.Items.Select(i => (sku: new Sku(i.Sku), qty: i.Qty));
            var result = await writer.ReserveAsync(orderId, items, ttl, ct);

            return result.IsSuccess
                ? TypedResults.Ok()
                : TypedResults.BadRequest(result.Error ?? "RESERVE_FAILED");
        }

        private static async Task<Ok> CommitAsync(
            string orderId,
            [FromServices] IReservationWriter writer,
            CancellationToken ct)
        {
            // Idempotent: if nothing is pending, writer returns Ok
            await writer.CommitAsync(orderId, ct);
            return TypedResults.Ok();
        }

        private static async Task<Ok> ReleaseAsync(
            string orderId,
            [FromServices] IReservationWriter writer,
            CancellationToken ct)
        {
            // Idempotent: if nothing is pending, writer returns Ok
            await writer.ReleaseAsync(orderId, ct);
            return TypedResults.Ok();
        }
    }


}
