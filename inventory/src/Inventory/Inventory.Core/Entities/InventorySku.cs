using System;
using System.Collections.Generic;
using System.Text;
using Inventory.Core.ValueObjects;

namespace Inventory.Core.Entities
{
    public class InventorySku
    {

        public Sku Sku { get; private set; }
        public long Available { get; private set; }
        public long Reserved { get; private set; }
        public long Version { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        // EF / serialization constructor
        private InventorySku() { }

        private InventorySku(Sku sku, long available, long reserved, long version, DateTimeOffset updatedAt)
        {
            Sku = sku;
            Available = available;
            Reserved = reserved;
            Version = version;
            UpdatedAt = updatedAt;
            EnsureInvariants();
        }

        public static InventorySku Create(Sku sku, long initialAvailable, DateTimeOffset now)
        {
            if (initialAvailable < 0)
                throw new ArgumentOutOfRangeException(nameof(initialAvailable), "Initial available must be >= 0.");

            return new InventorySku(sku, initialAvailable, reserved: 0, version: 0, updatedAt: now);
        }

        /// <summary>
        /// Adjusts available stock by delta (can be negative). Cannot go below zero.
        /// Increments version and updates timestamp when successful.
        /// </summary>
        public void AdjustAvailable(long delta, DateTimeOffset now)
        {
            var newAvailable = Available + delta;
            if (newAvailable < 0)
                throw new InvalidOperationException("INSUFFICIENT_AVAILABLE");

            Available = newAvailable;
            Touch(now);
        }

        /// <summary>
        /// Reserved cannot be negative. Provided for future reservation features.
        /// </summary>
        public void SetReserved(long newReserved, DateTimeOffset now)
        {
            if (newReserved < 0)
                throw new InvalidOperationException("RESERVED_NEGATIVE");
            Reserved = newReserved;
            Touch(now);
        }

        private void Touch(DateTimeOffset now)
        {
            Version += 1;
            UpdatedAt = now;
            EnsureInvariants();
        }

        private void EnsureInvariants()
        {
            if (Available < 0) throw new InvalidOperationException("AVAILABLE_NEGATIVE");
            if (Reserved < 0) throw new InvalidOperationException("RESERVED_NEGATIVE");
        }

    }
}
