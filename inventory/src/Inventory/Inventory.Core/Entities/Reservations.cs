using System;
using System.Collections.Generic;
using System.Text;
using Inventory.Core.ValueObjects;

namespace Inventory.Core.Entities
{

    public enum ReservationStatus
    {
        PENDING,
        COMMITTED,
        RELEASED
    }


    public class InventoryReservation
    {
        // Primary key
        public Guid Id { get; private set; }

        /// <summary>External order identifier this reservation belongs to.</summary>
        public string OrderId { get; private set; } = default!;

        /// <summary>The SKU reserved.</summary>
        public Sku Sku { get; private set; }

        /// <summary>Quantity reserved (strictly positive).</summary>
        public long Qty { get; private set; }

        /// <summary>Reservation status (PENDING, COMMITTED, RELEASED).</summary>
        public ReservationStatus Status { get; private set; }

        /// <summary>Reservation expiry instant (UTC).</summary>
        public DateTimeOffset ExpiresAt { get; private set; }

        /// <summary>Creation instant (UTC).</summary>
        public DateTimeOffset CreatedAt { get; private set; }

        // For EF Core / serializers
        private InventoryReservation() { }

        public InventoryReservation(
            Guid id,
            string orderId,
            Sku sku,
            long qty,
            ReservationStatus status,
            DateTimeOffset expiresAt,
            DateTimeOffset createdAt)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new ArgumentException("orderId is required", nameof(orderId));
            if (qty <= 0)
                throw new ArgumentOutOfRangeException(nameof(qty), "qty must be > 0");

            Id = id;
            OrderId = orderId;
            Sku = sku;
            Qty = qty;
            Status = status;
            ExpiresAt = expiresAt;
            CreatedAt = createdAt;
        }

        public void MarkCommitted()
        {
            if (Status != ReservationStatus.PENDING) return;
            Status = ReservationStatus.COMMITTED;
        }

        public void MarkReleased()
        {
            if (Status == ReservationStatus.RELEASED) return;
            Status = ReservationStatus.RELEASED;
        }
    }


    }
