using System;
using System.Collections.Generic;
using System.Text;
using Inventory.Core.Entities;
using Inventory.Infrastructure.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.EntityTypeConfig
{

    public class InventoryReservationConfig : IEntityTypeConfiguration<InventoryReservation>
    {

        public void Configure(EntityTypeBuilder<InventoryReservation> r)
        {
            r.ToTable("inventory_reservations");
            r.HasKey(x => x.Id);

            r.Property(x => x.Id).HasColumnName("id").IsRequired();
            r.Property(x => x.OrderId).HasColumnName("order_id").IsRequired();

            // Sku value object -> text
            r.Property(x => x.Sku)
             .HasConversion(new SkuConverter())
             .HasColumnName("sku")
             .HasMaxLength(100)
             .IsRequired();

            r.Property(x => x.Qty).HasColumnName("qty").IsRequired();

            // Enum -> text
            r.Property(x => x.Status)
             .HasConversion<string>()
             .HasColumnName("status")
             .IsRequired();

            r.Property(x => x.ExpiresAt).HasColumnName("expires_at").IsRequired();
            r.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();

            // Matches UNIQUE (order_id, sku)
            r.HasIndex(x => new { x.OrderId, x.Sku }).IsUnique();
        }
    }


    public class OutboxConfiguration : IEntityTypeConfiguration<OutboxEvent>
    {
        public void Configure(EntityTypeBuilder<OutboxEvent> e)
        {
            e.ToTable("outbox");

            e.HasKey(x => x.Id);
            e.Property(x => x.Id)
             .HasColumnName("id")
             .IsRequired();

            e.Property(x => x.AggregateType)
             .HasColumnName("aggregate_type")
             .IsRequired();

            e.Property(x => x.AggregateId)
             .HasColumnName("aggregate_id")
             .IsRequired();

            e.Property(x => x.EventType)
             .HasColumnName("event_type")
             .IsRequired();

            e.Property(x => x.Payload)
             .HasColumnName("payload")
             .HasColumnType("jsonb")
             .IsRequired();

            e.Property(x => x.CreatedAt)
             .HasColumnName("created_at")
             .HasDefaultValueSql("now()")
             .IsRequired();

            e.Property(x => x.PublishedAt)
             .HasColumnName("published_at");

            // Matches: CREATE INDEX idx_outbox_unpublished ON outbox (created_at) WHERE published_at IS NULL;
            // EF can't create a filtered index portably across providers in code here; but we keep table DDL as-is (you already created it).
            // If you ever add migrations, you can add HasIndex with .HasFilter(...) for PostgreSQL.
        }
    }

    }
